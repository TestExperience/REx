using System;
using System.Net;

namespace REx.Client
{
    public class RemoteExeutor : IDisposable
    {
        public delegate void ProcessExitedHandler(int exitcode);
        private readonly string _hostname;

        public RemoteExeutor(string hostname)
        {
            _hostname = hostname;
        }

        public void DeployService()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deploys and starts the service on the remote node
        /// </summary>
        public void DeployService(NetworkCredential credential)
        {
            //TODO: Copy service to \\_hostname\admin$\system32\RExSvc\REXSvc.zip
            //TODO: Unzip service into \\_hostname\admin$\system32\RExSvc\

            ServiceHelper.CreateNewService(_hostname, @"C:\Windows\system32\RExSvc\REx.Service.exe",
                credential);
        }

        /// <summary>
        /// Stops and removes deployed service on the remote node
        /// </summary>
        public void CleanUpRemoteNode()
        {

        }

        /// <summary>
        /// Starts a process on the remote node
        /// </summary>
        /// <param name="application">application to execute on remotenode</param>
        /// <param name="arguments">arguments for application</param>
        /// <param name="callback">callback executed after process is executed, parameter is exitcode</param>
        public void StartProcessAsync(string application, string arguments, Action<int> callback)
        {
            // Send exitcode to callback
            callback(0);
        }


        /// <summary>
        /// Cleanup after service is done with his work
        /// </summary>
        public void Dispose()
        {
            CleanUpRemoteNode();
        }
    }
}
