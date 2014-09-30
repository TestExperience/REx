// Original from: http://www.codeproject.com/Articles/35773/Subverting-Vista-UAC-in-Both-and-bit-Archite

using System;
using System.Security;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace REx.LauncherService
{
    /// <summary>
    /// Class that allows running applications with full admin rights. In
    /// addition the application launched will bypass the Vista UAC prompt.
    /// </summary>
    public class ApplicationLoader
    {
        #region Structures

        [StructLayout(LayoutKind.Sequential)]
        public struct SecurityAttributes
        {
            public int Length;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Startupinfo
        {
            public int cb;
            public String lpReserved;
            public String lpDesktop;
            public String lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ProcessInformation
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        #endregion

        #region Enumerations

        // ReSharper disable UnusedMember.Local
        enum TokenType
        {
            TokenPrimary = 1,
            TokenImpersonation = 2
        }
        // ReSharper restore UnusedMember.Local

        // ReSharper disable UnusedMember.Local
        enum SecurityImpersonationLevel
        {

            SecurityAnonymous = 0,

            SecurityIdentification = 1,
            SecurityImpersonation = 2,
            SecurityDelegation = 3,
        }
        // ReSharper restore UnusedMember.Local

        #endregion

        #region Constants

        public const int TokenDuplicate = 0x0002;
        public const uint MaximumAllowed = 0x2000000;
        public const int CreateNewConsole = 0x00000010;

        public const int IdlePriorityClass = 0x40;
        public const int NormalPriorityClass = 0x20;
        public const int HighPriorityClass = 0x80;
        public const int RealtimePriorityClass = 0x100;

        #endregion

        #region Win32 API Imports

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hSnapshot);

        [DllImport("kernel32.dll")]
        static extern uint WTSGetActiveConsoleSessionId();

        [DllImport("advapi32.dll", EntryPoint = "CreateProcessAsUser", SetLastError = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public extern static bool CreateProcessAsUser(IntPtr hToken, String lpApplicationName, String lpCommandLine, ref SecurityAttributes lpProcessAttributes,
            ref SecurityAttributes lpThreadAttributes, bool bInheritHandle, int dwCreationFlags, IntPtr lpEnvironment,
            String lpCurrentDirectory, ref Startupinfo lpStartupInfo, out ProcessInformation lpProcessInformation);

        [DllImport("advapi32.dll", EntryPoint = "DuplicateTokenEx")]
        public extern static bool DuplicateTokenEx(IntPtr existingTokenHandle, uint dwDesiredAccess,
            ref SecurityAttributes lpThreadAttributes, int tokenType,
            int impersonationLevel, ref IntPtr duplicateTokenHandle);

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("advapi32", SetLastError = true), SuppressUnmanagedCodeSecurityAttribute]
        static extern bool OpenProcessToken(IntPtr processHandle, int desiredAccess, ref IntPtr tokenHandle);

        #endregion

        /// <summary>
        /// Launches the given application with full admin rights, and in addition bypasses the Vista UAC prompt
        /// </summary>
        /// <param name="applicationName">The name of the application to launch</param>
        /// <param name="procInfo">Process information regarding the launched application that gets returned to the caller</param>
        /// <returns></returns>
        public static bool StartProcessAndBypassUac(String applicationName, out ProcessInformation procInfo)
        {
            uint winlogonPid = 0;
            IntPtr hUserTokenDup = IntPtr.Zero, hPToken = IntPtr.Zero;            
            procInfo = new ProcessInformation();

            // obtain the currently active session id; every logged on user in the system has a unique session id
            uint dwSessionId = WTSGetActiveConsoleSessionId();

            // obtain the process id of the winlogon process that is running within the currently active session
            Process[] processes = Process.GetProcessesByName("winlogon");
            foreach (Process p in processes)
            {
                if ((uint)p.SessionId == dwSessionId)
                {
                    winlogonPid = (uint)p.Id;
                }
            }

            // obtain a handle to the winlogon process
            IntPtr hProcess = OpenProcess(MaximumAllowed, false, winlogonPid);

            // obtain a handle to the access token of the winlogon process
            if (!OpenProcessToken(hProcess, TokenDuplicate, ref hPToken))
            {
                CloseHandle(hProcess);
                return false;
            }

            // Security attibute structure used in DuplicateTokenEx and CreateProcessAsUser
            // I would prefer to not have to use a security attribute variable and to just 
            // simply pass null and inherit (by default) the security attributes
            // of the existing token. However, in C# structures are value types and therefore
            // cannot be assigned the null value.
            var sa = new SecurityAttributes();
            sa.Length = Marshal.SizeOf(sa);

            // copy the access token of the winlogon process; the newly created token will be a primary token
            if (!DuplicateTokenEx(hPToken, MaximumAllowed, ref sa, (int)SecurityImpersonationLevel.SecurityIdentification, (int)TokenType.TokenPrimary, ref hUserTokenDup))
            {
                CloseHandle(hProcess);
                CloseHandle(hPToken);
                return false;
            }

            // By default CreateProcessAsUser creates a process on a non-interactive window station, meaning
            // the window station has a desktop that is invisible and the process is incapable of receiving
            // user input. To remedy this we set the lpDesktop parameter to indicate we want to enable user 
            // interaction with the new process.
            var si = new Startupinfo();
            si.cb = Marshal.SizeOf(si);
            si.lpDesktop = @"winsta0\default"; // interactive window station parameter; basically this indicates that the process created can display a GUI on the desktop

            // flags that specify the priority and creation method of the process
            const int dwCreationFlags = NormalPriorityClass | CreateNewConsole;

            // create a new process in the current user's logon session
            bool result = CreateProcessAsUser(hUserTokenDup,        // client's access token
                                            null,                   // file to execute
                                            applicationName,        // command line
                                            ref sa,                 // pointer to process SECURITY_ATTRIBUTES
                                            ref sa,                 // pointer to thread SECURITY_ATTRIBUTES
                                            false,                  // handles are not inheritable
                                            dwCreationFlags,        // creation flags
                                            IntPtr.Zero,            // pointer to new environment block 
                                            null,                   // name of current directory 
                                            ref si,                 // pointer to STARTUPINFO structure
                                            out procInfo            // receives information about new process
                                            );

            // invalidate the handles
            CloseHandle(hProcess);
            CloseHandle(hPToken);
            CloseHandle(hUserTokenDup);

            return result; // return the result
        }

    }
}