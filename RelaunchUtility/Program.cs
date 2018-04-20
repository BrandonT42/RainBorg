using System;
using System.Diagnostics;
using System.Threading;

namespace RelaunchUtility
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Sleeping 5 seconds.");
            Thread.Sleep(5000);
            Console.WriteLine("Relaunching application");
            Process.Start(args[0]);
        }
    }
}
