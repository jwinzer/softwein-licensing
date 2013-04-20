// Copyright (c) 2011 Jochen Winzer, softwein. Alle Rechte vorbehalten.
using System;
using System.Security.Cryptography;
using System.Text;

namespace Softwein.Secure
{
    internal class TripleDESCrypto
    {
        private TripleDESCryptoServiceProvider tDESProvider;

        internal TripleDESCrypto(string key)
        {
            tDESProvider = new TripleDESCryptoServiceProvider();
            tDESProvider.Key = computeHash(key, tDESProvider.KeySize / 8);
            tDESProvider.IV = computeHash(string.Empty, tDESProvider.BlockSize / 8);
        }

        private byte[] computeHash(string key, int length)
        {
            MD5CryptoServiceProvider hashProvider = new MD5CryptoServiceProvider();
            byte[] hash = hashProvider.ComputeHash(Encoding.UTF8.GetBytes(key));

            Array.Resize<byte>(ref hash, length);
            return hash;
        }

        internal string EncryptData(string plainText)
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

            ICryptoTransform encryptor = tDESProvider.CreateEncryptor();
            byte[] results = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            return Convert.ToBase64String(results);
        }

        internal string DecryptData(string encryptedText)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);

            ICryptoTransform decryptor = tDESProvider.CreateDecryptor();
            byte[] results = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

            return Encoding.UTF8.GetString(results);
        }
    }
}
