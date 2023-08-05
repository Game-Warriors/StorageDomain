using GameWarriors.StorageDomain.Abstraction;
using System;
using System.Text;
using System.Threading.Tasks;

#if UNITY_2018_4_OR_NEWER
using UnityEngine;
using UnityEngine.Scripting;

namespace GameWarriors.StorageDomain.Core
{
    public class PlayPrefsPersistStorage : IPersistDataHandler
    {
        private readonly IStorageSerializationHandler _jsonHandler;
        private readonly ICryptographyHandler _cryptoHandler;

        [Preserve]
        public PlayPrefsPersistStorage(IStorageSerializationHandler jsonHandler, ICryptographyHandler cryptographyHandler)
        {
            _jsonHandler = jsonHandler;
            _cryptoHandler = cryptographyHandler ?? new OldDefaultCryptographyHandler();
        }

        public void DeleteData(string path)
        {
            PlayerPrefs.DeleteKey(path);
        }

        public (bool, T) LoadData<T>(string key, byte[] hashKey)
        {
            try
            {
                string dataString = PlayerPrefs.GetString(key);
                string hashString = PlayerPrefs.GetString(key + "hashData");
                if (!string.IsNullOrEmpty(dataString) && !string.IsNullOrEmpty(hashString))
                {
                    string hashTmp = _cryptoHandler.HashInputString(dataString, hashKey);
                    if (string.Compare(hashTmp, hashString) == 0)
                    {
                        return (true, _jsonHandler.Deserialize<T>(dataString));
                    }
                    return default;
                }
                else
                    return default;
            }
            catch
            {
                throw;
            }
        }

        public (bool, T) LoadEncryptedData<T>(Encoding encoding, string path, byte[] key, byte[] iv)
        {
            throw new NotSupportedException();
        }

        public bool SaveData<T>(T data, string key, byte[] hashKey)
        {
            try
            {
                if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key))
                    return false;

                string dataString = _jsonHandler.Serialize(data);
                string hashString = _cryptoHandler.HashInputString(dataString, hashKey);
                PlayerPrefs.SetString(key, dataString);
                PlayerPrefs.SetString(key + "hashData", hashString);
                PlayerPrefs.Save();
                return true;
            }
            catch
            {
                throw;
            }
        }

        public bool SaveEncryptedData<T>(T wordSource, Encoding encoding, string path, byte[] key, byte[] iv)
        {
            throw new NotSupportedException();
        }

        public Task<(bool, T)> LoadDataAsync<T>(string path) => throw new NotSupportedException();
        public Task<(bool, object)> LoadDataAsync(string path, byte[] key, Type dataType) => throw new NotSupportedException();
        public Task<(bool, T)> LoadDataAsync<T>(string path, byte[] key) => throw new NotSupportedException();
        public Task<(bool, object)> LoadEncrypteDataAsync(Encoding encoding, string path, byte[] key, byte[] iv, Type dataType) => throw new NotSupportedException();
        public Task<(bool, T)> LoadEncryptedDataAsync<T>(Encoding encoding, string path, byte[] key, byte[] iv) => throw new NotSupportedException();
        public string LoadStringData(string path, byte[] key) => throw new NotSupportedException();
        public Task<string> LoadStringDataAsync(string path, byte[] key) => throw new NotSupportedException();
        public bool SaveData<T>(T data, string path) => throw new NotSupportedException();
        public Task<bool> SaveDataAsync<T>(T data, string path, Action onDone = null) => throw new NotSupportedException();
        public Task<bool> SaveDataAsync<T>(T data, string path, byte[] key, Action onDone = null) => throw new NotSupportedException();
        public bool SaveStringData(string data, string path, byte[] key) => throw new NotSupportedException();
        public bool SaveEncryptedStringData(string data, Encoding encoding, string path, byte[] key, byte[] iv) => throw new NotSupportedException();
    }
}
#endif