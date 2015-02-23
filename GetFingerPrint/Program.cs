using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;

namespace GetFingerPrint
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("BIOS : " + GetBiosId());
            Console.WriteLine("Board: " + GetBaseBoardId());
            Console.WriteLine("Disk : " + GetDiskId());            
            
            //ShowID("Win32_Processor", "UniqueID");
            //ShowID("Win32_Processor", "ProcessorId");
            //ShowID("Win32_BIOS", "SerialNumber");
            //ShowID("Win32_BIOS", "IdentificationCode");
            //ShowID("Win32_DiskDrive", "Signature");
            //ShowID("Win32_BaseBoard", "SerialNumber");
        }

        private static void ShowID(string wmiClass, string wmiProperty)
        {
            ManagementClass mc = new System.Management.ManagementClass(wmiClass);
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (System.Management.ManagementObject mo in moc)
            {
                string result = "";
                try
                {
                    result = mo[wmiProperty].ToString();
                }
                catch
                {
                }
                Console.WriteLine("{0}.{1}: {2}", wmiClass, wmiProperty, result);
            }
        }

        /// <summary>
        /// Gets the serial number of the BIOS.
        /// </summary>
        private static string GetBiosId()
        {
            string id = null;
            try
            {
                ManagementObjectSearcher mos = new ManagementObjectSearcher("Select * From Win32_BIOS");
                foreach (ManagementObject mo in mos.Get())
                {
                    // Return null if multiple BIOS numbers have been found.
                    if (id != null) return null;

                    id = mo["SerialNumber"].ToString();
                }
            }
            catch (Exception) { }
            return id;
        }

        /// <summary>
        /// Gets the serial number of the motherboard.
        /// Could be identical with the BIOS ID.
        /// </summary>
        private static string GetBaseBoardId()
        {
            string id = null;
            try
            {
                ManagementObjectSearcher mos = new ManagementObjectSearcher("Select * From Win32_BaseBoard");
                foreach (ManagementObject mo in mos.Get())
                {
                    // Return null if multiple BIOS numbers have been found.
                    if (id != null) return null;

                    id = mo["SerialNumber"].ToString();
                }
            }
            catch (Exception) { }
            return id;
        }

        /// <summary>
        /// Gets the volume serial number of the logical system drive.
        /// The S/N is generated when the disk is formatted. It can easily be changed.
        /// Identical computers might have the same S/N. So it's not really unique.
        /// </summary>
        private static string GetDiskId()
        {
            string id = null;
            string systemDeviceId = Environment.GetFolderPath(Environment.SpecialFolder.System).Substring(0, 2);
            try
            {
                ManagementObject dsk = new ManagementObject("Win32_LogicalDisk.DeviceId='" + systemDeviceId + "'");
                dsk.Get();
                id = dsk["VolumeSerialNumber"].ToString();
            }
            catch (Exception) { }
            return id;
        }
    }
}
