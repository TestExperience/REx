using System;
using System.Reflection;
using System.ServiceModel;
using System.Windows.Forms;
using NLog;
using REx.ServiceLibrary;

namespace REx.Server
{
    public partial class MainForm : Form
    {
        private ServiceHost _rexService;
        private static Logger Log;
        private bool _allowClosing;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (Log == null)
                Log = LogManager.GetCurrentClassLogger();

            Log.Info("REx is waking up ...");
            _rexService = RemoteExecutorService.CreateHost();
            Log.Info("REx is waiting for his pray @ net.tcp://{0}:9000/RExServer", Environment.MachineName.ToLower());

            serviceIcon.ShowBalloonTip(3000,
                "REx - Remote Executor",
                "You are running V" + Assembly.GetExecutingAssembly().GetName().Version,
                ToolTipIcon.Info);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_allowClosing)
            {
                _rexService.Close();
                return;
            }

            switch (e.CloseReason)
            {
                case CloseReason.FormOwnerClosing:
                case CloseReason.MdiFormClosing:
                case CloseReason.UserClosing:
                    e.Cancel = true;
                    WindowState = FormWindowState.Minimized;
                    Visible = false;
                    break;

                case CloseReason.ApplicationExitCall:
                case CloseReason.TaskManagerClosing:
                case CloseReason.WindowsShutDown:
                    _rexService.Close();
                    break;
            }
        }

        private void showLoggingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Visible = true;
            WindowState = FormWindowState.Normal;
            Focus();
        }

        private void shutdownServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _allowClosing = true;
            Close();
        }
    }
}
