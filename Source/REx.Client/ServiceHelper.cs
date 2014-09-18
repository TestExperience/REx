using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;

namespace REx.Client
{
    /// <summary>
    /// A wrapper for the command line tool "sc"
    /// </summary>
    public static class ServiceHelper
    {
        public static void CreateNewService(string hostname, string servicePath, NetworkCredential credential)
        {
            var argumentArray = new[]
            {
                @"\\" + hostname,
                "create",
                "binPath= " + servicePath,
                "start= auto",
                "obj= " + credential.Domain + @"\" + credential.UserName,
                "password= " + credential.Password
            };

            StartProcess(argumentArray);
        }

        public static void DeleteService(string hostname, string serviceName)
        {
            var argumentArray = new[]
            {
                @"\\" + hostname,
                "delete",
                serviceName
            };

            StartProcess(argumentArray);
        }

        public static void StartService(string hostname, string serviceName)
        {
            var argumentArray = new[]
            {
                @"\\" + hostname,
                "start",
                serviceName
            };

            StartProcess(argumentArray);
        }

        public static void StopService(string hostname, string serviceName)
        {
            var argumentArray = new[]
            {
                @"\\" + hostname,
                "stop",
                serviceName
            };

            StartProcess(argumentArray);
        }

        private static void StartProcess(string[] argumentArray)
        {
            var proc = CreateProcess(argumentArray);

            proc.Start();

            // Wait for process to return with an 10 second timeout
            proc.WaitForExit(10000);
            if (!proc.HasExited)
            {
                proc.Kill();
                throw new Exception("CreateNewService failed, timeout was reached.");
            }

            if (proc.ExitCode < 0)
            {
                throw new Exception(string.Format("CreateNewService returned an negative result[{0}].", proc.ExitCode));
            }
        }

        private static Process CreateProcess(string[] argumentArray)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "sc",
                    Arguments = string.Join(" ", argumentArray),
                    CreateNoWindow = true
                }
            };
            return proc;
        }
    }
}
