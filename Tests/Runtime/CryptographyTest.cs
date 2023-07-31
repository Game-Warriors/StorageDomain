using GameWarriors.StorageDomain.Abstraction;
using GameWarriors.StorageDomain.Core;
using NUnit.Framework;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.TestTools;

namespace GameWarriors.StorageDomain.Tests
{
    public class CryptographyTest
    {
        private readonly byte[] _key = new byte[] { 4, 5, 3, 18, 65, 10, 15, 55, 63, 12, 25, 94, 116, 83, 17, 57, 50, 36, 45, 75, 14, 28, 13, 119 };
        private readonly byte[] _iv = Encoding.ASCII.GetBytes(SystemInfo.deviceUniqueIdentifier.Substring(0, 16));

        [Test]
        public void OldCryptographyTest()
        {
            ICryptographyHandler cryptographyHandler = new OldDefaultCryptographyHandler();

            string plainText = "Hi everyone";
            byte[] data = Encoding.UTF8.GetBytes(plainText);

            using MemoryStream memoryStream = new MemoryStream();
            cryptographyHandler.EncryptStream(data, _key, _iv, memoryStream);
            Assert.AreNotEqual(memoryStream.Length, 0);

            memoryStream.Seek(0, SeekOrigin.Begin);
            byte[] outputData = cryptographyHandler.DecryptData(memoryStream, _key, _iv);

            string finalData = Encoding.UTF8.GetString(outputData);
            finalData = finalData.TrimEnd('\0');
            Assert.AreEqual(plainText, finalData);
        }

        [Test]
        public void NewCryptographyTest()
        {
            ICryptographyHandler cryptographyHandler = new NewDefaultCryptographyHandler();

            string plainText = "Hi everyone";
            byte[] data = Encoding.UTF8.GetBytes(plainText);

            using MemoryStream memoryStream = new MemoryStream();
            cryptographyHandler.EncryptStream(data, _key, _iv, memoryStream);
            Assert.AreNotEqual(memoryStream.Length, 0);

            memoryStream.Seek(0, SeekOrigin.Begin);
            byte[] outputData = cryptographyHandler.DecryptData(memoryStream, _key, _iv);

            string finalData = Encoding.UTF8.GetString(outputData);
            finalData = finalData.TrimEnd('\0');
            Assert.AreEqual(plainText, finalData);
        }
    }
}