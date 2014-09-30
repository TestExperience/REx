using System;
using System.ServiceModel;
using System.Threading;
using NLog;
using REx.Common.ServiceContracts;

namespace REx.Client
{
    public delegate void ConnectionStateChangedHandler(bool isConnected);

    /// <summary>
    /// A Executor Client which is used to communicate with a single Executor on another or the same machine.
    /// For each Executor/Machine an Executor Client is needed.
    /// </summary>
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public class RemoteExecutorClient : IRemoteExecutorMethods
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Raised as soon as the connection state of the executor proxy channel changes.
        /// The isConnected argument of the ConnectionStateChangedHandler will be set to true if
        /// the client establised a connection to the executor proxy successfully.
        /// </summary>
        public event ConnectionStateChangedHandler ConnectionStateChanged;

        /// <summary>
        /// Raised after the process is stopped, returns the exitcode
        /// </summary>
        public event ProcessExitedHandler ProcessExited
        {
            add { _executorServiceCallback.ProcessExited += value; }
            remove { _executorServiceCallback.ProcessExited -= value; }
        }

        /// <summary>
        /// Raised if the process writes to STDOUT
        /// </summary>
        public event ProcessWriteToHandler ProcessWriteToStdOut
        {
            add { _executorServiceCallback.ProcessWriteToStdOut += value; }
            remove { _executorServiceCallback.ProcessWriteToStdOut -= value; }
        }

        /// <summary>
        /// Raised if the process writes to STDERR
        /// </summary>
        public event ProcessWriteToHandler ProcessWriteToStdErr
        {
            add { _executorServiceCallback.ProcessWriteToStdErr += value; }
            remove { _executorServiceCallback.ProcessWriteToStdErr -= value; }
        }

        private bool _isConnected;
        private volatile IRemoteExecutor _executorProxy;
        private readonly RemoteExecutorCallback _executorServiceCallback;

        private bool _autoReconnectEnabled;

        private readonly DuplexChannelFactory<IRemoteExecutor> _channelFactory;

        private readonly System.Timers.Timer _reconnectTimer;
        private const Int32 ReconnectTimeout = 5000;

        public RemoteExecutorClient()
            : this("localhost")
        { }

        public RemoteExecutorClient(string hostname)
        {
            _executorServiceCallback = new RemoteExecutorCallback();
            var context = new InstanceContext(_executorServiceCallback);

            // TODO: Process more than this single Endpoint named "NetTcpBinding_IExecutorService".
            // What about InstanceContext. Do we need another one if more than one Endpoint is parameterized.
            // Will it be usefull to offer more than one endpoint?
            // -> It will be usefull. What about offering chooseable predefined Endpoints? Eg.: NamedPipe or TCP.
            // .) NamedPipe will be used for local Executors or for the "Send a message to the Executor" Service.
            // .) TCP will be used for Executors on other machines.
            var uri = new Uri("net.tcp://" + hostname + ":9000/RExServer");
            EndpointIdentity identity = EndpointIdentity.CreateSpnIdentity("RExTheMighty");
            _channelFactory = new DuplexChannelFactory<IRemoteExecutor>(
                context, new NetTcpBinding(), new EndpointAddress(uri, identity));

            _reconnectTimer = new System.Timers.Timer(ReconnectTimeout) {AutoReset = false}; // Wait after channel failure before reconnect.
            _reconnectTimer.Elapsed += (s, e) => 
            {
                try
                {
                    if (!_isConnected)
                    {
                        Connect();
                    }
                }
                catch (Exception ex)
                {
                    Log.Trace("Failed to reconnect: "+ ex.Message);
                }
            };

            _isConnected = false;
        }

        /// <summary>
        /// Connect to the Executor Service and subscribe for state change events.
        /// Register also event handlers for the communication state changes.
        /// </summary>
        public void Connect()
        {
            _executorProxy = _channelFactory.CreateChannel();

            ClientChannel.Opened += clientCommunication_Opened;
            ClientChannel.Faulted += clientCommunication_Faulted;
            ClientChannel.Closed += clientCommunication_Closed;

            _executorProxy.ConnectClient();
        }

        void clientCommunication_Faulted(object sender, EventArgs e)
        {
            if (_isConnected)
            {
                _isConnected = false;
                ThreadPool.QueueUserWorkItem(state => OnConnectionStateChanged(_isConnected));
            }
            // IClientChannel clientChannel = sender as IClientChannel;
            // System.Diagnostics.Trace.WriteLine("Client communication faulted." + clientChannel.SessionId.ToString());
            AbortProxy();

            if (AutoReconnectEnabled)
                _reconnectTimer.Start();
        }

        void clientCommunication_Opened(object sender, EventArgs e)
        {
            _isConnected = true;
            // Is called before the ConnectClient function returns.
            // Due to the current configuration this is an synchronus call and is blocked by the 
            // ConnectClient call. Decouple both by calling the event clients asynchronouosly.
            ThreadPool.QueueUserWorkItem(state => OnConnectionStateChanged(_isConnected));
        }

        void clientCommunication_Closed(object sender, EventArgs e)
        {
            if (_isConnected)
            {
                _isConnected = false;
                ThreadPool.QueueUserWorkItem(state => OnConnectionStateChanged(_isConnected));
            }
        }

        /// <summary>
        /// Disconnect from the executor service.
        /// Will set AutoReconnectEnabled to false.
        /// </summary>
        public void Disconnect()
        {
            try
            {
                AutoReconnectEnabled = false;
                _executorProxy.DisconnectClient();
            }
            catch (Exception ex)
            {
                Log.Trace("Failed to disconnect: " + ex.Message);
            }
            finally
            {
                AbortProxy();
            }
        }

        private void AbortProxy()
        {
            if (_executorProxy != null)
            {
                ClientChannel.Abort();
                ClientChannel.Close();
                _executorProxy = null;
            }
        }

        private void OnConnectionStateChanged(bool connectionState)
        {
            if (ConnectionStateChanged != null)
                ConnectionStateChanged(connectionState);
        }

        /// <summary>
        /// The executor service proxy casted to IClientChannel.
        /// </summary>
        private IClientChannel ClientChannel
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            get { return _executorProxy as IClientChannel; }
        }

        /// <summary>
        /// If set to true the auto reconnect procedure is started.
        /// </summary>
        public bool AutoReconnectEnabled
        {
            get
            {
                return _autoReconnectEnabled;
            }

            set
            {
                // Make sure that the reconnect timer is started
                // as soon as the reconnect flag is set.
                if (!_autoReconnectEnabled)
                {
                    if (value)
                        _reconnectTimer.Start();
                }
                _autoReconnectEnabled = value;
            }
        }

        public void StartProcessAsync(string application, string arguments)
        {
            if (_executorProxy == null)
                throw new Exception("Proxy seems to be null, an error appeared within client");
            _executorProxy.StartProcessAsync(application, arguments);
        }

        public void KillProcess()
        {
            if (_executorProxy == null)
                throw new Exception("Proxy seems to be null, an error appeared within client");
            _executorProxy.KillProcess();
        }

        public void WriteToProcess(string value)
        {
            if (_executorProxy == null)
                throw new Exception("Proxy seems to be null, an error appeared within client");
            _executorProxy.WriteToProcess(value);
        }

        public void WriteLineToProcess(string value)
        {
            if (_executorProxy == null)
                throw new Exception("Proxy seems to be null, an error appeared within client");
            
            _executorProxy.WriteLineToProcess(value);
        }
    }
}
