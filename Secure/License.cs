// Copyright (c) 2011-2014 Jochen Winzer, softwein. Alle Rechte vorbehalten.
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;

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
            Value = value;
            
            // Get MAC address of the primary network adapter.            
            string mac = GetNetworkAdapterAddress();
            if (!string.IsNullOrEmpty(mac))
                SerialNumber = productID.ToString("X2") + mac.Substring(mac.Length - 8, 8);
        }

        /// <summary>
        /// Creates an encrypted license key.
        /// </summary>
        /// <param name="serialNumber">Serial number of the license.</param>
        /// <param name="expirationDate">Expiration date of the license.</param>
        /// <param name="value">Generic license value.</param>
        /// <returns>A license key.</returns>
        internal static string CreateKey(string serialNumber, DateTime expirationDate, uint value)
        {
            // Encode and encrypt license content.
            string plainCode = Encode(serialNumber, expirationDate, value);
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
                string plainCode = Encode(SerialNumber, ExpirationDate, Value);
                TripleDESCrypto crypto = new TripleDESCrypto(key);
                return crypto.EncryptData(plainCode);
            }
            return string.Empty;
        }

        /// <summary>
        /// Serializes the license content into a fixed length string.
        /// </summary>
        private static string Encode(string serialNumber, DateTime expiration, uint count)
        {
            // Fixed string length = 10 + 16 + 8 = 34.
            return serialNumber + expiration.Ticks.ToString("X16") + count.ToString("X8");
        }

        /// <summary>
        /// Decodes a plain license code.
        /// </summary>
        private bool Decode(string licenseCode)
        {
            // Deserialize the plain fixed length license string.
            if (licenseCode.Length == 34)
            {
                // Extract and validate serial number.
                string sn = licenseCode.Substring(0, 10);
                if (sn == SerialNumber)
                {
                    // Extract expirationDate date and convert from base 16.
                    long expirationTicks = Convert.ToInt64(licenseCode.Substring(10, 16), 16);

                    // Extract value and convert from base 16.
                    uint count = Convert.ToUInt32(licenseCode.Substring(26), 16);

                    // Update licence.
                    ExpirationDate = new DateTime(expirationTicks);
                    Value = count;
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
    }
}
