using REx.Common.ServiceContracts;
using System.Threading;

namespace REx.Client
{
    public delegate void ProcessExitedHandler(int exitcode);
    public delegate void ProcessWriteToHandler(string value);

    public class RemoteExecutorCallback : IRemoteExecutorCallback
    {
        public event ProcessExitedHandler ProcessExited;
        public event ProcessWriteToHandler ProcessWriteToStdOut;
        public event ProcessWriteToHandler ProcessWriteToStdErr;

        public void OnProcessExited(int exitcode)
        {
            if (ProcessExited == null)
                return;

            // Create a thread for event processing since this call is blocking for the server.
            ThreadPool.QueueUserWorkItem(x => ProcessExited(exitcode));
        }

        public void OnProcessWriteToStdOut(string value)
        {
            if (ProcessWriteToStdOut == null)
                return;

            // Create a thread for event processing since this call is blocking for the server.
            ThreadPool.QueueUserWorkItem(x => ProcessWriteToStdOut(value));
        }

        public void OnProcessWriteToStdErr(string value)
        {
            if (ProcessWriteToStdErr == null)
                return;

            // Create a thread for event processing since this call is blocking for the server.
            ThreadPool.QueueUserWorkItem(x => ProcessWriteToStdErr(value));
        }
    }
}
