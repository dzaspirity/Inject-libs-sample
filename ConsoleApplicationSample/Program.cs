using System;
using System.Linq;
using ClassLibrarySample;

namespace ConsoleApplicationSample
{
    class Program
    {
        static void Main(string[] args)
        {
            const string newtonsoftJsonDll = "Newtonsoft.Json.dll";
            const string zerobasedCoreDll = "Zerobased.Core.dll";

            //DLL from EXE file
            EmbeddedAssembly.Load($"ConsoleApplicationSample.{newtonsoftJsonDll}", newtonsoftJsonDll);

            //DLL from other DLL
            EmbeddedAssembly.Load($"ClassLibrarySample.{zerobasedCoreDll}", zerobasedCoreDll, typeof(EmptyClass).Assembly);

            //WARNING:
            //Current class should not use classes from loading DLLs. It will cause an exception.


            Example.Run(args.FirstOrDefault());
            Console.WriteLine("press Enter");
            Console.ReadLine();
        }
    }
}
