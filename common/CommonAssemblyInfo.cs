using System;
using System.Reflection;
using System.Runtime.InteropServices;

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyProduct("REx - Remote Executor")]
[assembly: AssemblyCopyright("Copyright 2014 AbraxasCSharp")]

[assembly: ComVisible(false)]
[assembly: CLSCompliant(false)]