using System;
using System.Runtime.InteropServices;
using ProcessorCentral.Actors;
using SharedLibrary.Helpers;

namespace ProcessorCentral
{
    public partial class Program
    {
        #region Trap application termination
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler();
        static EventHandler _handler;

        private static bool Handler()
        {
            Console.WriteLine("Exiting system due to external CTRL-C, or process kill, or shutdown");

            Console.WriteLine("System terminated.");

            //shutdown right away so there are no lingering threads
            Environment.Exit(-1);

            return true;
        }

        #endregion
        
        public static void Main(string[] args)
        {
            Console.Title = StaticMethods.GetSystemUniqueName();

            // Some biolerplate to react to close window event, CTRL-C, kill, etc
            if (Environment.UserInteractive)
            {
                _handler += new EventHandler(Handler);
                SetConsoleCtrlHandler(_handler, true);
            }


            var myService = new MyService();
            myService.Start();
            Console.WriteLine("Press Control + C to terminate.");
            Console.CancelKeyPress += async (sender, eventArgs) =>
            {
                await myService.StopAsync();
            };
            myService.TerminationHandle.Wait();
        }
    }
}

