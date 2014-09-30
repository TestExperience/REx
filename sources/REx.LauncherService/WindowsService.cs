using System.Configuration;
using System.ServiceProcess;
using System.Diagnostics;
using System.Threading;

namespace REx.LauncherService
{
    public sealed class WindowsService : ServiceBase
    {
        ApplicationLoader.ProcessInformation _procInfo;
        private Process _proc;

        // The name of the application to launch;
        // to launch an application using the full command path simply escape
        // the path with quotes, for example to launch firefox.exe:
        const string ApplicationName = "REx.Service.exe";

        /// <summary>
        /// Public Constructor for WindowsService.
        /// - Put all of your Initialization code here.
        /// </summary>
        public WindowsService()
        {
            ServiceName = "REx Launcher Service";
            EventLog.Log = "Application";

            // These Flags set whether or not to handle that specific
            //  type of event. Set to true if you need it, false otherwise.
            CanHandlePowerEvent = true;
            CanHandleSessionChangeEvent = true;
            CanPauseAndContinue = false;
            CanShutdown = true;
            CanStop = true;
        }

        /// <summary>
        /// The Main Thread: This is where your Service is Run.
        /// </summary>
        static void Main()
        {
            Run(new WindowsService());
        }

        /// <summary>
        /// OnStart(): Put startup code here
        ///  - Start threads, get inital data, etc.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            StartApplication(false);

            base.OnStart(args);
        }

        private void StartApplication(bool restart)
        {
            if (restart)
            {
                if (ConfigurationManager.AppSettings["AutoRestartServer"].ToLower() != "true")
                    return;
                Thread.Sleep(2000); // Wait until start of application
            }

            // Launch the application   
            ApplicationLoader.StartProcessAndBypassUac(ApplicationName, out _procInfo);
            _proc = Process.GetProcessById((int) _procInfo.dwProcessId);
            _proc.Exited += (sender, e) => StartApplication(true);
        }

        /// <summary>
        /// OnStop(): Put your stop code here
        /// - Stop threads, set final data, etc.
        /// </summary>
        protected override void OnStop()
        {
            var proc = Process.GetProcessById((int)_procInfo.dwProcessId);
            proc.Kill();

            base.OnStop();
        }
    }
}
