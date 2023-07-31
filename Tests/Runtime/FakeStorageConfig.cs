using GameWarriors.StorageDomain.Abstraction;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GameWarriors.StorageDomain.Tests
{
    public class FakeStorageConfig : IStorageConfig
    {
        private readonly byte[] _iv;
        private readonly byte[] _key;
        public string StorageDataPath => Application.persistentDataPath;
        byte[] IStorageConfig.Key => _key;

        byte[] IStorageConfig.IV => _iv;

        string IStorageConfig.PersistStorageName => "0";
        int IStorageConfig.SavingInterval => 15;

        public FakeStorageConfig()
        {
            _key = new byte[] { 4, 5, 3, 18, 65, 10, 15, 55, 63, 12, 25, 94, 116, 83, 17, 57, 50, 36, 45, 75, 14, 28, 13, 119 };
            _iv = Encoding.ASCII.GetBytes(SystemInfo.deviceUniqueIdentifier.Substring(0, 16));
        }
    }
}