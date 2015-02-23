using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Softwein.Secure;

namespace TestLicensing
{
    class Program
    {
        static void Main(string[] args)
        {
            License license = new License(1, DateTime.Now, 20);

            Console.WriteLine(license.SerialNumber);
            Console.WriteLine(license.ExpirationDate.ToString());
            Console.WriteLine(license.Dongle);
            Console.WriteLine(license.Value.ToString());

            string code = license.ReadKey();
            Console.WriteLine(code);

            code = License.CreateKey(license.SerialNumber, "33FA52", DateTime.Now.AddDays(5), 100);
            Console.WriteLine(code);
            Console.WriteLine(license.WriteKey(code).ToString());
            Console.WriteLine(license.ReadKey());

            Console.WriteLine(license.SerialNumber);
            Console.WriteLine(license.ExpirationDate.ToString());
            Console.WriteLine(license.Dongle);
            Console.WriteLine(license.Value.ToString());

            Console.ReadKey();
        }
    }
}
