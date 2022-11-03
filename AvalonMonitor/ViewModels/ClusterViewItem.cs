using System;
using ReactiveUI;

namespace AvalonMonitor.ViewModels;

public class MemberViewItem : ReactiveObject
{
    string _roles;
    public string Roles
    {
        get => _roles;
        set => this.RaiseAndSetIfChanged(ref _roles, value);
    }

    string _status;
    public string Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value); 
    }

    string _address;
    public string Address
    {
        get => _address;
        set => this.RaiseAndSetIfChanged(ref _address, value);
    }

    DateTime _timeStamp;
    public DateTime TimeStamp
    {
        get => _timeStamp;
        set => this.RaiseAndSetIfChanged(ref _timeStamp, value);
    }
}

public class ClusterViewItem : MemberViewItem
{
    bool _isClusterLeader;
    public bool IsClusterLeader
    {
        get => _isClusterLeader;
        set => this.RaiseAndSetIfChanged(ref _isClusterLeader, value);
    }

    bool _isRoleLeader;
    public bool IsRoleLeader
    {
        get => _isRoleLeader;
        set => this.RaiseAndSetIfChanged(ref _isRoleLeader, value);
    }
}