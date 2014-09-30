using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using REx.Client;

namespace REx.Console
{
    class Program
    {
        private static string _lastLineWrittenToConsole = string.Empty;
        private static string _lastOutputReceivedFromConsole = string.Empty;
        private readonly BackgroundWorker _inputReader = new BackgroundWorker();
        private int? _processExitCode;
        private RemoteExecutorClient _client;

        public Program()
        {
            _inputReader.DoWork += InputReader_DoWork;
            _inputReader.WorkerSupportsCancellation = true;
        }
        
        void InputReader_DoWork(object sender, DoWorkEventArgs e)
        {
            var builder = new StringBuilder();
            while (!_processExitCode.HasValue)
            {
                string value = char.ConvertFromUtf32(System.Console.Read());
                builder.Append(value);


                if (builder.ToString().EndsWith("\r\n"))
                {
                    // Clear the builder if an line is finished and safe the last line
                    _lastLineWrittenToConsole = builder.ToString();
                    builder.Clear();
                }

                _client.WriteToProcess(value); // WriteToStdInputOfProcess
            }
        }

        /// <summary>
        /// Application starting point
        /// </summary>
        /// <param name="args">Commandline arguments</param>
        private static int Main(string[] args)
        {
            return new Program().ExecuteAsync(args);
        }

        /// <summary>
        /// Executes application logic
        /// </summary>
        /// <param name="args">Commandline arguments</param>
        public int ExecuteAsync(string[] args)
        {
            string hostname = (args.Any()) ? args.First() : "localhost";

            _client = new RemoteExecutorClient(hostname);
            _client.ProcessWriteToStdOut += OnOutputDataReceived;
            _client.ProcessWriteToStdErr += OnErrorDataReceived;
            _client.ProcessExited += delegate(int exitcode)
            {
                _client.ProcessWriteToStdOut -= OnOutputDataReceived;
                _client.ProcessWriteToStdErr -= OnErrorDataReceived;
                _processExitCode = exitcode;
            };

            _client.Connect();
            _client.AutoReconnectEnabled = true;

            _client.StartProcessAsync(@"cmd", "");

            // Start backgroundworker which reads the STDIN from console
            // In this way the program is not blocked by Console.Read() method
            // and is closed after hosted process has exited
            _inputReader.RunWorkerAsync();

            // Wait for exit of process
            while (!_processExitCode.HasValue) { }
            _client.Disconnect();

            // ReSharper disable once PossibleInvalidOperationException
            return (_processExitCode.HasValue) ? _processExitCode.Value : -1000;
        }

        private void OnOutputDataReceived(string value)
        {
            if (value == null)
                return; // If no data is send, write nothing (yes this can happen)

            // append the new data to the data already read-in
            if (value.EndsWith(">"))
            {
                _lastOutputReceivedFromConsole = value;
                System.Console.Write(value);
            }
            else if (!string.IsNullOrEmpty(_lastLineWrittenToConsole) &&
                     value.StartsWith(_lastOutputReceivedFromConsole + _lastLineWrittenToConsole.Trim()))
            {
                // Do nothing
            }
            else { System.Console.WriteLine(value); }
        }

        private void OnErrorDataReceived(string value)
        {
            // append the new data to the data already read-in
            System.Console.Write(value);
        }
    }
}
