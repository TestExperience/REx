using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using NLog;
using REx.Common.ServiceContracts;

namespace REx.ServiceLibrary
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class RemoteExecutorService : IRemoteExecutor
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        // Holds the channels to all currently connected callbacks (=clients)
        protected volatile IDictionary<string, IRemoteExecutorCallback> ConnectedClients;

        // Lock-object to synchronize between multiple concurrent threads
        protected object SyncRoot;

        // Holds the process started by the service
        private Process _process;

        #region Primitive Ctor Singleton

        private static RemoteExecutorService _service;

        public static RemoteExecutorService Instance
        {
            get { return _service ?? (_service = new RemoteExecutorService()); }
        }

        private RemoteExecutorService()
        {
            SyncRoot = new object();
            ConnectedClients = new Dictionary<string, IRemoteExecutorCallback>();
        }

        #endregion

        #region Self-hosting Service

        private static ServiceHost _serviceHost;

        public static ServiceHost CreateHost()
        {
            if (_serviceHost != null)
                return _serviceHost;
            _serviceHost = new ServiceHost(Instance);
            _serviceHost.Open();
            return _serviceHost;
        }

        #endregion

        public void ConnectClient()
        {
            // get the callback-channel to the client that called this method
            var callbackChannel = OperationContext.Current.GetCallbackChannel<IRemoteExecutorCallback>();
            string sessionId = OperationContext.Current.Channel.SessionId;

            // store it for later use (to push messages)
            lock (SyncRoot)
            {
                Log.Info("Connect client.");
                // make sure, that we don't add the same session twice (e.g. the client tries to subscribe multiple times)
                if (!ConnectedClients.ContainsKey(sessionId))
                {
                    //Console.WriteLine("Connect Client {0}", sessionId);
                    
                    ConnectedClients.Add(sessionId, callbackChannel);
                    // Register for error-events of the current channel to disconnect the client on channel close or failure.
                    OperationContext.Current.Channel.Closing += Channel_Closing;
                    OperationContext.Current.Channel.Faulted += Channel_Faulted;
                }
            }
        }

        /// <summary>
        /// Called by the client to unregister from event notification.
        /// </summary>
        public void DisconnectClient()
        {
            RemoveClient(OperationContext.Current.Channel.SessionId);
        }

        /// <summary>
        /// If a communication channel faulted we remove the according client.
        /// </summary>
        /// <param name="sender">The channel which faulted.</param>
        /// <param name="e">Empty</param>
        void Channel_Faulted(object sender, EventArgs e)
        {
            Log.Info("Client faulted");
            var channel = sender as IContextChannel;
            if (channel != null)
                RemoveClient(channel.SessionId);
        }

        /// <summary>
        /// If a channel is closed by the client we remove it from the list of
        /// connected clients.
        /// </summary>
        /// <param name="sender">The channel of the client which was closed.</param>
        /// <param name="e">Empty</param>
        void Channel_Closing(object sender, EventArgs e)
        {
            Log.Info("Client closing");
            var channel = sender as IContextChannel;
            if (channel != null)
                RemoveClient(channel.SessionId);
        }

        /// <summary>
        /// Just remove the client with the given sessionId from the 
        /// list of connected clients in a thread save way.
        /// </summary>
        /// <param name="sessionId">The id of the client to remove.</param>
        private void RemoveClient(string sessionId)
        {
            // remove the session
            lock (SyncRoot)
            {
                if (ConnectedClients.ContainsKey(sessionId))
                {
                    Log.Info("Remove client.");
                    ConnectedClients.Remove(sessionId);
                }
            }
        }

        /// <summary>
        /// Inform the connected clients about a finished process and report the exitcode.
        /// This is done by invoking all callback channels of the clients asynchronuously.
        /// </summary>
        /// <param name="exitcode">The exitcode of the finished process</param>
        private void PublishProcessExitCode(int exitcode)
        {
            lock (SyncRoot)
            {
                Log.Debug("Publish process exitcode. ExitCode: " + exitcode);
                // Inform each register client about the exitcode.
                foreach (var callbackChannel in ConnectedClients.Values)
                {
                    // actually the client has to schedule a new thread for processing the event
                    callbackChannel.OnProcessExited(exitcode);
                }
            }
        }

        /// <summary>
        /// Inform the connected clients that a process has written text to stdout.
        /// This is done by invoking all callback channels of the clients asynchronuously.
        /// </summary>
        /// <param name="sender">Process which sent the data</param>
        /// <param name="e">Received StdOut data</param>
        private void PublishProcessWriteToStdOut(object sender, DataReceivedEventArgs e)
        {
            lock (SyncRoot)
            {
                Log.Debug("Publish process stdout: " + e.Data);
                // Inform each register client about the exitcode.
                foreach (var callbackChannel in ConnectedClients.Values)
                {
                    // actually the client has to schedule a new thread for processing the event
                    callbackChannel.OnProcessWriteToStdOut(e.Data);
                }
            }
        }

        /// <summary>
        /// Inform the connected clients that a process has written text to stderr.
        /// This is done by invoking all callback channels of the clients asynchronuously.
        /// </summary>
        /// <param name="sender">Process which sent the data</param>
        /// <param name="e">Received StdErr data</param>
        private void PublishProcessWriteToStdErr(object sender, DataReceivedEventArgs e)
        {
            lock (SyncRoot)
            {
                Log.Debug("Publish process stderr line: " + e.Data);
                // Inform each register client about the exitcode.
                foreach (var callbackChannel in ConnectedClients.Values)
                {
                    // actually the client has to schedule a new thread for processing the event
                    callbackChannel.OnProcessWriteToStdErr(e.Data);
                }
            }
        }

        public void StartProcessAsync(string application, string arguments)
        {
            lock (SyncRoot)
            {
                if (_process != null && !_process.HasExited)
                {
                    throw new Exception(
                        string.Format("Process can't be started, because recently started process[{0}] is currently running",
                        _process.ProcessName));
                }

                var processStartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    ErrorDialog = false,
                    UseShellExecute = false,
                    Arguments = arguments,
                    FileName = application
                };

                _process = new Process
                {
                    StartInfo = processStartInfo,
                    // Enable raising events because Process does not raise events by default
                    EnableRaisingEvents = true
                };

                // Attach the event handler for OutputDataReceived before starting the process
                _process.OutputDataReceived += PublishProcessWriteToStdOut;

                // Attach the event handler for ErrorDataReceived before starting the process
                _process.ErrorDataReceived += PublishProcessWriteToStdErr;

                // Attach the event handler for Exited before starting the process
                _process.Exited += (sender, e) =>
                {
                    _process.OutputDataReceived -= PublishProcessWriteToStdOut;
                    _process.ErrorDataReceived -= PublishProcessWriteToStdErr;

                    _process.CancelOutputRead();
                    _process.CancelErrorRead();
                    PublishProcessExitCode(_process.ExitCode);
                };

                // Start the process
                // then begin asynchronously reading the output
                // then wait for the process to exit
                // then cancel asynchronously reading the output
                Log.Debug("Starting Application[" + application + "] with Arguments[" + arguments + "]");
                _process.Start();

                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();
            }
        }

        public void KillProcess()
        {
            lock (SyncRoot)
            {
                if (_process == null || _process.HasExited) return;

                Log.Info("Killing current process[{0}]", _process.StartInfo.FileName);
                _process.Kill();
            }
        }

        public void WriteToProcess(string value)
        {
            lock (SyncRoot)
            {
                if (_process == null || _process.HasExited) return;

                Log.Trace("StandardInput to process: " + value);
                _process.StandardInput.Write(value);
            }
        }

        public void WriteLineToProcess(string value)
        {
            lock (SyncRoot)
            {
                if (_process == null || _process.HasExited) return;

                Log.Trace("StandardInput to process: " + value);
                _process.StandardInput.WriteLine(value);
            }
        }
    }
}
