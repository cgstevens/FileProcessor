using System;
using System.Runtime.InteropServices;
using SharedLibrary.Helpers;

namespace Lighthouse
{
    public partial class Program
    {
        #region Trap application termination
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;

        enum CtrlType
        {
            CtrlCEvent = 0,
            CtrlBreakEvent = 1,
            CtrlCloseEvent = 2,
            CtrlLogoffEvent = 5,
            CtrlShutdownEvent = 6
        }

        private static bool Handler(CtrlType sig)
        {
            Console.WriteLine("Exiting system due to external CTRL-C, or process kill, or shutdown");

            //do your cleanup here
            if (SystemActors.ClusterSystem != null)
            {
                var cluster = global::Akka.Cluster.Cluster.Get(SystemActors.ClusterSystem);
                cluster.Leave(cluster.SelfAddress);
                SystemActors.ClusterSystem?.Terminate();

                var waiter = new Waiter(TimeSpan.FromSeconds(2));
                waiter.Wait();
            }
            else
            {
                Console.WriteLine("ClusterSystem was null during shutdown.  Possible that cluster already shutdown itself.");
            }

            Console.WriteLine("ClusterSystem terminated.");

            //shutdown right away so there are no lingering threads
            Environment.Exit(-1);

            return true;
        }

        #endregion


        public static void Main(string[] args)
        {
            Console.Title = "Lighthouse";

            // Some biolerplate to react to close window event, CTRL-C, kill, etc
            if (Environment.UserInteractive)
            {
                _handler += new EventHandler(Handler);
                SetConsoleCtrlHandler(_handler, true);
            }

            var lighthouseService = new LighthouseService();
            lighthouseService.Start();
            Console.WriteLine("Press Control + C to terminate.");
            Console.CancelKeyPress += async (sender, eventArgs) =>
            {
                await lighthouseService.StopAsync();
            };
            lighthouseService.TerminationHandle.Wait(); 
        }
    }
}

