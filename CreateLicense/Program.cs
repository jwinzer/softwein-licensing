using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Softwein.Secure;

namespace CreateLicense
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.Write("Serial-No.     : ");
                string sn = Console.ReadLine();

                Console.Write("Expiration Date: ");
                DateTime expiration = Convert.ToDateTime(Console.ReadLine());

                Console.Write("License Count  : ");
                uint count = Convert.ToUInt32(Console.ReadLine());

                string code = License.CreateKey(sn, expiration, count);

                Console.WriteLine();
                Console.WriteLine(code);

                Console.Write("Press any key to exit...");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
