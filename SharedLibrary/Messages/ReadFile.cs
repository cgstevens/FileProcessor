using System.IO;
using Shared.Models;

namespace Shared.Messages
{
    public class ValidateFileWatcher
    {

    }
    
    public class LineComplete
    {
        public string UserName { get; }
        public LineComplete(string userName)
        {
            UserName = userName;
        }
    }

    

    public class RecordHasBeenProcessed
    {
        public bool Successful { get; }
        public string Message { get; }
        public RecordHasBeenProcessed(bool successful, string message)
        {
            Successful = successful;
            Message = message;
        }
    }

    

    public class ProcessLine
    {
        public string UserName { get; }
        public ProcessLine(string userName)
        {
            UserName = userName;
        }
    }

    public class ReadFile
    {
        public object Sender { get; }
        public FileSystemEventArgs Args { get; }

        public ReadFile(object sender, FileSystemEventArgs args)
        {
            Sender = sender;
            Args = args;
        }
    }

    public class WorkFile
    {
        public object Sender { get; }
        public FileSystemEventArgs Args { get; }

        public WorkFile(object sender, FileSystemEventArgs args)
        {
            Sender = sender;
            Args = args;
        }
    }

}
