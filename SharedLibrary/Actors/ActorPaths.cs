using Akka.Actor;

namespace SharedLibrary.Actors
{
    /// <summary>
    /// Static helper class used to define paths to fixed-name actors
    /// (helps eliminate errors when using <see cref="ActorSelection"/>)
    /// </summary>
    public static class ActorPaths
    {
        public static readonly string ActorSystem = "mysystem";
        public static readonly ActorMetaData JobManagerActor = new ActorMetaData("JobManager", "/user/JobManager");
        public static readonly ActorMetaData SingletonManagerActor = new ActorMetaData("singletonmanager", "/user/singletonmanager");
        public static readonly ActorMetaData SingletonManagerProxy = new ActorMetaData("singletonmanagerproxy", "/user/singletonmanagerproxy");
    }

    /// <summary>
    /// Meta-data class
    /// </summary>
    public class ActorMetaData
    {
        public ActorMetaData(string name, string path)
        {
            Name = name;
            Path = path;
        }

        public string Name { get; private set; }
        public string Path { get; private set; }
    }
}
