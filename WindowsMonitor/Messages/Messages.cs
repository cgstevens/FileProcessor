namespace WinForms
{
    public class Messages
    {
        public class GetCurrentClusterState
        {
        }

        public class MemberLeave
        {
            public MemberLeave(string address)
            {
                Address = address;
            }
            public string Address { get; private set; }
        }
        public class MemberDown
        {
            public MemberDown(string address)
            {
                Address = address;
            }
            public string Address { get; private set; }
        }
        
        public class SubscribeMessage
        {
            public SubscribeMessage(string message)
            {
                Message = message;
            }

            public string Message { get; private set; }
        }

        internal class GetClusterState
        {
            public GetClusterState()
            {
            }
        }
    }
}
