using GameWarriors.StorageDomain.Abstraction;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GameWarriors.StorageDomain.Core
{
    /// <summary>
    /// This class provider encryption service by Aes class and for hashing use HMACSHA256 class
    /// </summary>
    public class NewDefaultCryptographyHandler : ICryptographyHandler
    {
        public async Task<byte[]> DecryptDataAsync(Stream stream, byte[] key, byte[] iv)
        {
            using (Aes AES = Aes.Create())
            {
                AES.Key = key;
                AES.IV = iv;
                ICryptoTransform cryptoTransform = AES.CreateDecryptor(key, iv);
                using (CryptoStream cryptStream = new CryptoStream(stream, cryptoTransform, CryptoStreamMode.Read))
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        await cryptStream.CopyToAsync(memoryStream);
                        return memoryStream.GetBuffer();
                    }
                }
            }
        }

        byte[] ICryptographyHandler.DecryptData(Stream stream, byte[] key, byte[] iv)
        {
            using (Aes AES = Aes.Create())
            {
                AES.Key = key;
                AES.IV = iv;
                ICryptoTransform cryptoTransform = AES.CreateDecryptor(key, iv);
                using (CryptoStream cryptStream = new CryptoStream(stream, cryptoTransform, CryptoStreamMode.Read))
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        cryptStream.CopyTo(memoryStream);
                        return memoryStream.GetBuffer();
                    }
                }
            }
        }

        public byte[] HashInput(string input, byte[] key)
        {
            byte[] tmp = Encoding.Unicode.GetBytes(input);
            using (var sha = new HMACSHA256(key))
            {
                byte[] hash = sha.ComputeHash(tmp);
                return hash;
            }
        }

        public string HashInputString(string input, byte[] key)
        {
            byte[] tmp = Encoding.Unicode.GetBytes(input);
            using (var sha = new HMACSHA256(key))
            {
                var hash = sha.ComputeHash(tmp);
                return Convert.ToBase64String(hash);
            }
        }

        public void EncryptStream(byte[] inputData, byte[] key, byte[] iv, Stream outputStream)
        {
            using (Aes AES = Aes.Create())
            {
                AES.Key = key;
                AES.IV = iv;
                ICryptoTransform cryptoTransform = AES.CreateEncryptor(key, iv);
                using (MemoryStream memoryStream = new MemoryStream(inputData))
                {                  
                    using (CryptoStream cryptStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Read))
                    {
                        cryptStream.CopyTo(outputStream);
                    }
                }
            }
        }
    }
}
