// Copyright (c) 2011-2015 Jochen Winzer, softwein. Alle Rechte vorbehalten.
using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace Softwein.Secure
{
    /// <summary>
    /// Provides a copy protection software license.
    /// An Ethernet adapter is required to generate a unique serial number.
    /// </summary>
    public class License
    {
        // Encryption key to generate license key.
        // old key: Bianchi Infinito C2C
        private static readonly string key = "I love 2 develop great products.";
        
        /// <summary>
        /// Serial number of the license instance.
        /// </summary>
        public string SerialNumber { get; private set; }

        /// <summary>
        /// Expiration date of the license instance.
        /// </summary>
        public DateTime ExpirationDate { get; private set; }

        /// <summary>
        /// Dongle value of the license instance. Empty if no dongle required.
        /// </summary>
        public string Dongle { get; private set; }

        /// <summary>
        /// Generic value of the license instance.
        /// </summary>
        public uint Value { get; private set; }

        /// <summary>
        /// Indicates if the license is valid.
        /// </summary>
        public bool IsValid { get { return !string.IsNullOrEmpty(SerialNumber); } }

        /// <summary>
        /// Constructs a <see cref="License"/> object.
        /// </summary>
        /// <param name="productID">Unique product identifier.</param>
        /// <param name="expirationDate">Expiration date of the license.</param>
        /// <param name="value">Generic license value.</param>
        public License(uint productID, DateTime expirationDate, uint value)
        {
            SerialNumber = string.Empty;
            ExpirationDate = expirationDate;
            Dongle = string.Empty;
            Value = value;

            // Create an 8 digit hardware fingerprint from motherboard ID, BIOS ID and Disk ID.
            int len = 8;
            string fp = GetFingerPrint(GetBaseBoardId(), len);
            if (fp.Length < len)
            {
                fp += GetFingerPrint(GetBiosId(), len - fp.Length);
                if (fp.Length < len)
                {
                    fp += GetFingerPrint(GetDiskId(), len - fp.Length);
                    if (fp.Length < len)
                        return; // failed to generate fingerprint
                }
            }
            
            // Generate 10 digit serial number.            
            SerialNumber = productID.ToString("X2") + fp;
        }

        /// <summary>
        /// Creates an encrypted license key.
        /// </summary>
        /// <param name="serialNumber">Serial number of the license.</param>
        /// <param name="dongle">Dongle value of the license. May be empty.</param>
        /// <param name="expirationDate">Expiration date of the license.</param>
        /// <param name="value">Generic license value.</param>
        /// <returns>A license key.</returns>
        internal static string CreateKey(string serialNumber, string dongle, DateTime expirationDate, uint value)
        {
            // NOTE: THIS METHOD MUST NOT BE PUBLIC TO PREVENT FRAUD!
            // Encode and encrypt license content.
            string plainCode = Encode(serialNumber, dongle, expirationDate, value);
            TripleDESCrypto crypto = new TripleDESCrypto(key);
            return crypto.EncryptData(plainCode);
        }
        
        /// <summary>
        /// Update the license by applying a license key.
        /// </summary>
        /// <param name="licenseKey">The license key.</param>
        /// <returns>True if license key was accepted.</returns>
        public bool WriteKey(string licenseKey)
        {
            if (!IsValid || string.IsNullOrEmpty(licenseKey)) return false;
            
            // Decrypt and decode.
            try
            {
                TripleDESCrypto crypto = new TripleDESCrypto(key);
                string plainCode = crypto.DecryptData(licenseKey);
                return Decode(plainCode);
            }
            catch (Exception e)
            {
                Trace.WriteLine("Failed to apply license code.\n" + e.Message);
                return false;
            }
        }

        /// <summary>
        /// Reads the license key identifying the license instance.
        /// </summary>
        /// <returns>A license key or an empty string if license instance not valid.</returns>
        public string ReadKey()
        {
            if (IsValid)
            {
                // Encode and encrypt license content.
                string plainCode = Encode(SerialNumber, Dongle, ExpirationDate, Value);
                TripleDESCrypto crypto = new TripleDESCrypto(key);
                return crypto.EncryptData(plainCode);
            }
            return string.Empty;
        }

        /// <summary>
        /// Serializes the license content into a fixed length string.
        /// </summary>
        private static string Encode(string serialNumber, string dongle, DateTime expiration, uint count)
        {
            string s = (!string.IsNullOrEmpty(dongle)) ?
                serialNumber.Substring(0, 2) + "##" + dongle + "#" : serialNumber;
            return s + expiration.Ticks.ToString("X16") + count.ToString("X8");
        }

        /// <summary>
        /// Decodes a plain license code.
        /// </summary>
        private bool Decode(string licenseCode)
        {
            // Deserialize a dongle license.
            //  2 + 2 + X  + 1 +    16   +   8   >= 30       
            // [ID]##[dongle]#[expiration][value]
            if (licenseCode.Substring(2, 2) == "##" && licenseCode.Length >= 30)
            {
                string productID = licenseCode.Substring(0, 2);

                // Product ID must match.
                if (productID == SerialNumber.Substring(0, 2))
                {
                    // Extract dongle.
                    Dongle = licenseCode.Substring(4, licenseCode.Length - 29);

                    // Extract expiration date and convert from base 16.
                    long expirationTicks = Convert.ToInt64(licenseCode.Substring(licenseCode.Length - 24, 16), 16);
                    ExpirationDate = new DateTime(expirationTicks);

                    // Extract value and convert from base 16.
                    Value = Convert.ToUInt32(licenseCode.Substring(licenseCode.Length - 8), 16);
                    return true;
                }
            }            
            
            // Deserialize the plain fixed length license string.
            //  10 +    16    +   8  = 34
            // [sn][expiration][value]
            else if (licenseCode.Length == 34)
            {
                // Extract and validate serial number.
                string sn = licenseCode.Substring(0, 10);

                // Serial number must match.
                if (sn == SerialNumber)
                {
                    Dongle = string.Empty;

                    // Extract expiration date and convert from base 16.
                    long expirationTicks = Convert.ToInt64(licenseCode.Substring(10, 16), 16);
                    ExpirationDate = new DateTime(expirationTicks);

                    // Extract value and convert from base 16.
                    Value = Convert.ToUInt32(licenseCode.Substring(26), 16);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the MAC address of the first Ethernet LAN network adapter
        /// uniquely identifying a computer.
        /// </summary>
        /// <returns>Network adapter address or null if no adapter found.</returns>
        private static string GetNetworkAdapterAddress()
        {
            // Get first Ethernet network adapter of collection ordered by Id that
            // contains "LAN" or "local area" within its name.
            NetworkInterface adapter =
                NetworkInterface.GetAllNetworkInterfaces().OrderBy(nif => nif.Id).FirstOrDefault(
                nif => nif.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
                (nif.Name.Contains("LAN") || nif.Name.ToLower().Contains("local area")));
            if (adapter == null) return null;
            
            // Get adapter's MAC address.
            return adapter.GetPhysicalAddress().ToString();
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

        private static string GetFingerPrint(string value, int length)
        {
            if (value != null)
            {
                value = RemoveSpecialCharacters(value);
                length = Math.Min(length, value.Length);
                return value.Substring(value.Length - length, length);
            }
            return string.Empty;
        }

        private static string RemoveSpecialCharacters(string value)
        {
            return Regex.Replace(value, "[^a-zA-Z0-9]+", string.Empty, RegexOptions.Compiled);
        }
    }
}
