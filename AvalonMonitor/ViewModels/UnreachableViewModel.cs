using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Akka.Cluster;

namespace AvalonMonitor.ViewModels;

public interface IProcessUnreachableItems
{
    void Process(Member member);
    void RemoveByKey(string key);
    IEnumerable<string> Addresses { get; }
}

public class UnreachableViewModel : ViewModelBase, IProcessUnreachableItems
{
    public UnreachableViewModel()
    {
        Items = new ObservableCollection<MemberViewItem>();
    }
    
    public ObservableCollection<MemberViewItem> Items { get; }
    
    public void Process(Member member)
    {
        ProcessMember(Items, member);
    }

    public void RemoveByKey(string key)
    {
        var item = Items.FirstOrDefault(x => x.Address == key);
        if (item != null)
            Items.Remove(item);
    }

    public IEnumerable<string> Addresses => Items.Select(x => x.Address);
}