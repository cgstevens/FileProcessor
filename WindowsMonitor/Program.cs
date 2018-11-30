using System;
using System.Windows.Forms;
using Akka.Actor;

namespace WinForms
{
    static class Program
    {

        public static ActorSystem MyActorSystem;
        public static IActorRef ClusterManagerActor;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }
    }
}
