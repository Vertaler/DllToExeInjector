using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PeNet;
using Trinet.Core.IO.Ntfs;

namespace DllToExeInjector
{
    class Program
    {
        static void Main(string[] args)
        {
            var injector = new DllToExeInjector();
            injector.InjectDll(@"E:\hiew32demo\lab.exe", @"E:\hiew32demo\injected_dll.dll", "function_22");
        }
    }
}
