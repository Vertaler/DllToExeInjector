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
            if(args.Length < 3)
            {
                Console.WriteLine("Usage: DllToExeInjector <some_exe.exe> <some_dll.dll>  <funtion_name>");
                return;
            }
            injector.InjectDll(args[0], args[1], args[2]);
            Console.WriteLine("Dll was successfuly injected!");

        }
    }
}
