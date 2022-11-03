using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.Event;
using Akka.Util.Internal;
using AvalonMonitor.ViewModels;
using SharedLibrary.Messages;
using SharedLibrary.PubSub;

namespace AvalonMonitor.Actors
{
    public class ClusterStatusActor : ReceiveActor
    {
        readonly ILoggingAdapter _logger = Context.GetLogger();
        readonly IProcessLoggerItems _loggerProcessor;
        readonly IProcessClusterItems _clusterProcessor;
        readonly IProcessSeenByItems _seenByProcessor;
        readonly IProcessUnreachableItems _unreachableProcessor;

        ICancelable _currentClusertStateTeller;
        Cluster Cluster = Cluster.Get(Context.System);
        Dictionary<string, Member> Members;

        public ClusterStatusActor(IProcessLoggerItems loggerProcessor, IProcessClusterItems clusterProcessor, IProcessSeenByItems seenByProcessor, IProcessUnreachableItems unreachableProcessor)
        {
            _loggerProcessor = loggerProcessor;
            _clusterProcessor = clusterProcessor;
            _seenByProcessor = seenByProcessor;
            _unreachableProcessor = unreachableProcessor;
            var self = Self;
            Members = new Dictionary<string, Member>();
            SystemActors.Mediator.Tell(new Subscribe(Topics.Status, self));
            Receives();
        }

        protected override void PreStart()
        {
            Cluster.Subscribe(Self, ClusterEvent.InitialStateAsEvents, new []{typeof(ClusterEvent.IMemberEvent), typeof(ClusterEvent.IReachabilityEvent)});
            _currentClusertStateTeller = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(60), Self, new Messages.GetCurrentClusterState(), Self);
        }

        protected override void PostStop()
        {
            var self = Self;
            Cluster.Unsubscribe(Self);
            _currentClusertStateTeller?.Cancel(false);
            SystemActors.Mediator.Tell(new Unsubscribe(Topics.Status, self));
        }

        void UpdateClusterListView(Member member)
        {
            var key = member.Address.ToString();
            if (!Members.ContainsKey(key))
                Members.Add(key, member);
            else
                Members[key] = member;
            _clusterProcessor.Process(member);
        }

        void Receives()
        {
            Receive<SignalRMessage>(ic =>
            {
                _loggerProcessor.Process($"{ic.System} {ic.Actor} {ic.Message}");
            });

            Receive<Messages.GetCurrentClusterState>(dowhat =>
            {
                _loggerProcessor.Process("Updating Cluster State");
                var state = Cluster.State;
                
                // Clusterlist                 
                foreach(var member in state.Members)
                    UpdateClusterListView(member);
                var removeMembers = new List<string>();
                foreach (var addr in _clusterProcessor.Addresses)
                {
                    var exists = state.Members.FirstOrDefault(x => x.Address.ToString() == addr);
                    if (exists!=null) continue;
                    removeMembers.Add(addr);
                    Members.Remove(addr);
                }
                foreach(var removeMember in removeMembers)
                    _clusterProcessor.RemoveByKey(removeMember);
                
                // Unreachable
                var removeUnreachables = new List<string>();
                foreach (var addr in _unreachableProcessor.Addresses)
                {
                    var exists = state.Unreachable.FirstOrDefault(x => x.Address.ToString() == addr);
                    if(exists==null)
                        removeUnreachables.Add(addr);
                }
                foreach(var removeUnreachable in removeUnreachables)
                    _unreachableProcessor.RemoveByKey(removeUnreachable);
                
                // Seenby
                var removeSeenBys = new List<string>();
                foreach (var addr in _seenByProcessor.Addresses)
                {
                    var exists = state.SeenBy.FirstOrDefault(x => x.ToString() == addr);
                    if(exists==null)
                        removeSeenBys.Add(addr);
                }
                foreach(var removeSeenBy in removeSeenBys)
                    _seenByProcessor.RemoveByKey(removeSeenBy);
                foreach (var address in state.SeenBy)
                {
                    var member = state.Members.FirstOrDefault(x => x.Address == address);
                    _seenByProcessor.Process(member);
                }
                
                _clusterProcessor.ChangeClusterLeader(state.Leader);
                //_clusterProcessor.ChangeRoleLeader();
            });

            Receive<ClusterEvent.MemberUp>(mem => {
                var roles = mem.Member.Roles.Join(",");
                _loggerProcessor.Process($"MemberUp: {mem.Member}, Role: {roles}");
                UpdateClusterListView(mem.Member);
            });

            Receive<ClusterEvent.MemberExited>(mem =>
            {
                var roles = mem.Member.Roles.Join(",");
                _loggerProcessor.Process($"MemberExited: {mem.Member}, Role: {roles}");
                UpdateClusterListView(mem.Member);
            });

            Receive<ClusterEvent.UnreachableMember>(mem =>
            {
                var roles = mem.Member.Roles.Join(",");
                _loggerProcessor.Process($"UnreachableMember: {mem.Member}, Role: {roles}");
                UpdateClusterListView(mem.Member);
                _unreachableProcessor.Process(mem.Member);
            });

            Receive<ClusterEvent.ReachableMember>(mem =>
            {
                var roles = mem.Member.Roles.Join(",");
                _loggerProcessor.Process($"UnreachableMember: {mem.Member}, Role: {roles}");
                UpdateClusterListView(mem.Member);
                _unreachableProcessor.RemoveByKey(mem.Member.Address.ToString());
            });

            Receive<ClusterEvent.MemberRemoved>(mem =>
            {
                var roles = mem.Member.Roles.Join(",");
                _loggerProcessor.Process($"MemberRemoved: {mem.Member}, Role: {roles}");
                var key = mem.Member.Address.ToString();
                if (Members.ContainsKey(key))
                    _clusterProcessor.RemoveByKey(key);
            });

            Receive<ClusterEvent.IMemberEvent>(mem =>
            {
                var roles = mem.Member.Roles.Join(",");
                _loggerProcessor.Process($"IMemberEvent: {mem.Member}, Role: {roles}");
                UpdateClusterListView(mem.Member);
            });

            Receive<Messages.MemberDown>(key =>
            {
                if (!Members.ContainsKey(key.Address)) return;
                _loggerProcessor.Process($"Down Member: {key.Address}");
                var member = Members[key.Address];
                Cluster.Down(member.Address);
            });

            Receive<Messages.MemberLeave>(key =>
            {
                if (!Members.ContainsKey(key.Address)) return;
                _loggerProcessor.Process($"Ask member to leave: {key.Address}");
                var member = Members[key.Address];
                Cluster.Leave(member.Address);
            });

            Receive<Terminated>(terminated =>
            {
                _loggerProcessor.Process($"Terminated: {terminated.AddressTerminated}");
            });

            Receive<ClusterEvent.LeaderChanged>(leader =>
            {
                var ll = leader.Leader != null ? leader.Leader.ToString() : "Missing Leader Value";
                _loggerProcessor.Process($"Cluster leader changed: {ll}");
                _clusterProcessor.ChangeClusterLeader(leader.Leader);
            });

            Receive<ClusterEvent.RoleLeaderChanged>(leader =>
            {
                var ll = leader.Leader != null ? leader.Leader.ToString() : "Missing Leader Value";
                var role = leader.Role.ToString();
                _loggerProcessor.Process($"Role leader changed: {ll}, Role {role}");
                _clusterProcessor.ChangeRoleLeader(leader.Leader);
            });
        }
    }
}
