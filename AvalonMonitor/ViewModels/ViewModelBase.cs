using System;
using System.Collections.ObjectModel;
using System.Linq;
using Akka.Cluster;
using Akka.Util.Internal;
using ReactiveUI;

namespace AvalonMonitor.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        public void ProcessMember(ObservableCollection<MemberViewItem> items, Member member)
        {
            var created = false;
            var key = member.Address.ToString();
            var item = items.FirstOrDefault(x => x.Address == key);
            if (item == null)
            {
                item = new MemberViewItem();
                created = true;
            }
            item.Roles = member.Roles.Join(",");
            item.Status = member.Status.ToString();
            item.Address = key;
            item.TimeStamp = DateTime.Now;
            if(created)
                items.Add(item);
        }
    }
}
