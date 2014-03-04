using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;

namespace GetNetworkInterfaces
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Network Interfaces\n");
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                Console.WriteLine("Name   : " + ni.Name);
                Console.WriteLine("Descr. : " + ni.Description);
                Console.WriteLine("Type   : " + ni.NetworkInterfaceType.ToString());
                Console.WriteLine("Address: " + ni.GetPhysicalAddress());
                Console.WriteLine("Status : " + ni.OperationalStatus);
                Console.WriteLine();
            }
            Console.Write("Press key...");
            Console.ReadKey();
        }
    }
}
