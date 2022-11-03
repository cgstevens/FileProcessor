using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Akka.Cluster;

namespace AvalonMonitor.ViewModels;

public interface IProcessSeenByItems
{
    void Process(Member member);
    IEnumerable<string> Addresses { get; }
    void RemoveByKey(string key);
}


public class SeenByViewModel : ViewModelBase, IProcessSeenByItems
{
    public SeenByViewModel()
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