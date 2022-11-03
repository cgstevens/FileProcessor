using System;
using System.Collections.ObjectModel;

namespace AvalonMonitor.ViewModels;

public interface IProcessLoggerItems
{
    void Process(string message);
}

public class LoggerViewModel : IProcessLoggerItems
{
    public LoggerViewModel()
    {
        LogItems = new ObservableCollection<string>();
    }
    
    public ObservableCollection<string> LogItems { get; }
    
    public void Process(string message)
    {
        var msg = $"{DateTime.Now:MM-dd-yy hh:mm:ss.fff} {message}";
        LogItems.Insert(0, msg);
    }
}