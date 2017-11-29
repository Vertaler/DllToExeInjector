using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PeNet;

namespace DllToExeInjector
{
    class Program
    {
        static void Main(string[] args)
        {
            var injector = new DllToExeInjector();
            injector.InjectDll(@"E:\hiew32demo\lab.exe", "injected_dll.dll", "function_22");
        }
    }
}
