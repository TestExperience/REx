using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        public void DeployService(string domain, string username, string password)
        {
            //TODO: Copy service to \\_hostname\admin$\system32\RExSvc\REXSvc.zip
            //TODO: Unzip service into \\_hostname\admin$\system32\RExSvc\

            ServiceHelper.CreateNewService(_hostname, @"C:\Windows\system32\RExSvc\REx.Service.exe",
                new NetworkCredential(username, password, domain));
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
