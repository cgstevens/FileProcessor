using System.IO;

namespace SharedLibrary.Messages
{
    public class FileCreated
    {
        public object Sender { get; }
        public FileSystemEventArgs Args { get; }

        public FileCreated(object sender, FileSystemEventArgs args)
        {
            Sender = sender;
            Args = args;
        }
    }

    public class FileDeleted
    {
        public object Sender { get; }
        public FileSystemEventArgs Args { get; }

        public FileDeleted(object sender, FileSystemEventArgs args)
        {
            Sender = sender;
            Args = args;
        }
    }

    public class FileChanged
    {
        public object Sender { get; }
        public FileSystemEventArgs Args { get; }
        public FileChanged(object sender, FileSystemEventArgs args)
        {
            Sender = sender;
            Args = args;
        }
    }
}
