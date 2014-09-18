using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace REx.Client
{
    public class RemoteExeutor : IDisposable
    {
        private readonly string _hostname;

        public RemoteExeutor(string hostname)
        {
            _hostname = hostname;
        }

        /// <summary>
        /// Deploys and starts the service on the remote node
        /// </summary>
        public void DeployService(string username, string password)
        {

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
        /// <param name="application"></param>
        /// <param name="arguments"></param>
        public void StartProcessAsync(string application, string arguments)
        { }


        /// <summary>
        /// Cleanup after service is done with his work
        /// </summary>
        public void Dispose()
        {
            CleanUpRemoteNode();
        }
    }
}
