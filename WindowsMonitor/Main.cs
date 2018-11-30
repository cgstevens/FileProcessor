using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using SharedLibrary.Actors;
using WinForms.Actors;

namespace WinForms
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (SystemActors.ClusterManagerActor != null)
            {
                //_clusterManagerActor.Tell(new ClusterManager.UnSubscribeFromManager());
            }
            Thread.Sleep(3000); // Give time to leave before we close out everything.
            base.OnClosing(e);
        }

        private void Main_Load(object sender, EventArgs e)
        {
            clusterListView.View = View.Details;
            clusterListView.GridLines = true;
            clusterListView.FullRowSelect = true;

            clusterListView.Columns.Add("Roles", 100, HorizontalAlignment.Left);
            clusterListView.Columns.Add("Status", 50, HorizontalAlignment.Left);
            clusterListView.Columns.Add("Service Address", 300, HorizontalAlignment.Left);
            clusterListView.Columns.Add("DateStamp", 125, HorizontalAlignment.Left);
            clusterListView.Columns.Add("IsClusterLeader", 75, HorizontalAlignment.Left);
            clusterListView.Columns.Add("IsRoleLeader", 75, HorizontalAlignment.Left);

            unreachableListView.View = View.Details;
            unreachableListView.GridLines = true;
            unreachableListView.FullRowSelect = true;
            unreachableListView.Columns.Add("Roles", 100, HorizontalAlignment.Left);
            unreachableListView.Columns.Add("Status", 50, HorizontalAlignment.Left);
            unreachableListView.Columns.Add("Service Address", 300, HorizontalAlignment.Left);
            unreachableListView.Columns.Add("DateStamp", 125, HorizontalAlignment.Left);

            seenByListView.View = View.Details;
            seenByListView.GridLines = true;
            seenByListView.FullRowSelect = true;
            seenByListView.Columns.Add("Roles", 100, HorizontalAlignment.Left);
            seenByListView.Columns.Add("Status", 50, HorizontalAlignment.Left);
            seenByListView.Columns.Add("Service Address", 300, HorizontalAlignment.Left);
            seenByListView.Columns.Add("DateStamp", 125, HorizontalAlignment.Left);

            InitializeCluster();
        }

        public void InitializeCluster()
        {
            SystemActors.ClusterSystem = SystemHostFactory.Launch();
            loggerBox.Items.Insert(0, string.Format("{0}  {1}", DateTime.Now.ToString("MM-dd-yy hh:mm:ss.fff"), "Actor System Started"));
            InitializeActors();
        }

        private void InitializeActors()
        {
            SystemActors.ClusterManagerActor = SystemActors.ClusterSystem.ActorOf(Props.Create(() => new ClusterStatusActor(loggerBox, clusterListView, unreachableListView, seenByListView)), "monitor");
            SystemActors.Mediator = DistributedPubSub.Get(SystemActors.ClusterSystem).Mediator;
            loggerBox.Items.Insert(0, string.Format("{0}  {1}", DateTime.Now.ToString("MM-dd-yy hh:mm:ss.fff"), "Cluster Manager Actor Started"));
        }

        private void LeaveClusterButton_Click(object sender, EventArgs e)
        {
            var selectedItem = clusterListView.SelectedItems;
            if (selectedItem.Count > 0)
            {
                SystemActors.ClusterManagerActor.Tell(new Messages.Messages.MemberLeave(selectedItem[0].Name));
            }
            else
            {
                loggerBox.Items.Insert(0, string.Format("{0}  {1}", DateTime.Now.ToString("MM-dd-yy hh:mm:ss.fff"), "Select a member to have it leave the cluster."));
            }
        }

        private void DownClusterButton_Click(object sender, EventArgs e)
        {
            var selectedItem = clusterListView.SelectedItems;
            if (selectedItem.Count > 0)
            {
                SystemActors.ClusterManagerActor.Tell(new Messages.Messages.MemberDown(selectedItem[0].Name));
            }
            else
            {
                loggerBox.Items.Insert(0, string.Format("{0}  {1}", DateTime.Now.ToString("MM-dd-yy hh:mm:ss.fff"), "Select a member to have it be forced down."));
            }
        }

        
    }
}
