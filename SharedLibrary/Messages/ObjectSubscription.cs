namespace Shared.Messages
{
    public class ObjectSubscription
    {
        public string Name { get; set; }
        public string ObjectPath { get; set; }

        public ObjectSubscription(string name, string objectPath)
        {
            Name = name;
            ObjectPath = objectPath;
        }

    }
}
