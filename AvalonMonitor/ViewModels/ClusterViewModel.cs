using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Akka.Actor;
using Akka.Cluster;
using Akka.Util.Internal;
using Avalonia.Metadata;
using AvalonMonitor.Actors;
using ReactiveUI;

namespace AvalonMonitor.ViewModels
{
    public interface IProcessClusterItems
    {
        void Process(Member member);
        void RemoveByKey(string key);
        void ChangeClusterLeader(Address? leader);
        void ChangeRoleLeader(Address? leader);
        IEnumerable<string> Addresses { get; }
    }

    public class ClusterViewModel : ReactiveObject, IProcessClusterItems
    {
        ClusterViewItem _selectedItem;

        public ClusterViewModel()
        {
            Items = new ObservableCollection<ClusterViewItem>();
        }

        public void Down()
        {
            SystemActors.ClusterManagerActor.Tell(new Messages.MemberDown(SelectedItem.Address));
        }
        [DependsOn(nameof(SelectedItem))]
        bool CanDown(object _)
        {
            return SelectedItem != null;
        }
        
        public void Leave()
        {
            SystemActors.ClusterManagerActor.Tell(new Messages.MemberLeave(SelectedItem.Address));
        }
        [DependsOn(nameof(SelectedItem))]
        bool CanLeave(object _)
        {
            return SelectedItem != null;
        }


        public ObservableCollection<ClusterViewItem> Items { get; }

        public ClusterViewItem SelectedItem
        {
            get => _selectedItem;
            set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
        }

        public void Process(Member member)
        {
            var created = false;
            var key = member.Address.ToString();
            var item = Items.FirstOrDefault(x => x.Address == key);
            if (item == null)
            {
                item = new ClusterViewItem();
                created = true;
            }
            item.Roles = member.Roles.Join(",");
            item.Status = member.Status.ToString();
            item.Address = key;
            item.TimeStamp = DateTime.Now;
            item.IsClusterLeader = false;
            item.IsRoleLeader = false;
            if(created)
                Items.Add(item);
        }

        public void RemoveByKey(string key)
        {
            var item = Items.FirstOrDefault(x => x.Address == key);
            if (item != null)
                Items.Remove(item);
        }

        public void ChangeClusterLeader(Address? leader)
        {
            var comp = leader != null ? leader.ToString() : string.Empty;
            foreach (var item in Items)
                item.IsClusterLeader = item.Address == comp;
        }

        public void ChangeRoleLeader(Address? leader)
        {
            var comp = leader != null ? leader.ToString() : string.Empty;
            foreach (var item in Items)
                item.IsRoleLeader = item.Address == comp;
        }

        public IEnumerable<string> Addresses => Items.Select(x => x.Address);
    }
}
