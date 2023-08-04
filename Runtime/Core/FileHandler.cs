using GameWarriors.StorageDomain.Abstraction;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GameWarriors.StorageDomain.Core
{
    /// <summary>
    /// This class provide file saving and loading utility, for serialization and encryption features need to handlers class 
    /// </summary>
    public class FileHandler : IPersistDataHandler
    {
        private readonly IStorageSerializationHandler _jsonHandler;
        private readonly ICryptographyHandler _cryptoHandler;

#if UNITY_2018_4_OR_NEWER
        [UnityEngine.Scripting.Preserve]
#endif
        public FileHandler(IStorageSerializationHandler jsonHandler, ICryptographyHandler cryptographyHandler)
        {
            _jsonHandler = jsonHandler;
            _cryptoHandler = cryptographyHandler != null ? cryptographyHandler : new OldDefaultCryptographyHandler();
        }

        //public (bool, T) LoadFromRegistry<T>(string key, byte[] hashKey)
        //{
        //    try
        //    {
        //        string dataString = PlayerPrefs.GetString(key);
        //        string hashString = PlayerPrefs.GetString(key + "hashData");
        //        if (!string.IsNullOrEmpty(dataString) && !string.IsNullOrEmpty(hashString))
        //        {
        //            string hashTmp = HashInputString(dataString, hashKey);
        //            if (string.Compare(hashTmp, hashString) == 0)
        //            {
        //                return (true, _jsonHandler.FromJson<T>(dataString));
        //            }
        //            return default;
        //        }
        //        else
        //            return default;
        //    }
        //    catch
        //    {
        //        return default;
        //    }
        //}

        //public bool SaveToRegister<T>(T input, string key, byte[] hashKey)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key))
        //            return false;

        //        string dataString = _jsonHandler.ToJson(input);
        //        string hashString = HashInputString(dataString, hashKey);
        //        PlayerPrefs.SetString(key, dataString);
        //        PlayerPrefs.SetString(key + "hashData", hashString);
        //        PlayerPrefs.Save();
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //    return true;
        //}

        public string LoadFile(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            if (!File.Exists(path)) return null;

            using (StreamReader reader = new StreamReader(path))
            {
                return reader.ReadToEnd();
            }
        }

        public async Task<string> LoadFileAsync(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            using (StreamReader reader = new StreamReader(path))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public (bool, object) LoadFile(string path, byte[] key, Type dataType)
        {
            if (string.IsNullOrEmpty(path))
                return (false, default);
            if (!File.Exists(path))
                return (false, default);
            BinaryReader reader = null;
            try
            {
                reader = new BinaryReader(File.Open(path, FileMode.Open));
                int hashLength = reader.ReadInt32();
                byte[] fileHash = new byte[hashLength];
                int readResult = reader.Read(fileHash, 0, hashLength);
                int stringLength = reader.ReadInt32();
                string tmp = reader.ReadString();
                byte[] dataHash = _cryptoHandler.HashInput(tmp, key);
                reader?.Dispose();
                reader = null;
                if (IsSameBytes(fileHash, dataHash))
                {
                    var data = _jsonHandler.Deserialize(tmp, dataType);
                    return (true, data);
                }
                else
                    return (false, default);
            }
            catch
            {
                reader?.Dispose();
                throw;
            }
        }

        public (bool, T) LoadData<T>(string path, byte[] key)
        {
            if (string.IsNullOrEmpty(path))
                return (false, default);
            if (!File.Exists(path))
                return (false, default);
            BinaryReader reader = null;

            try
            {
                reader = new BinaryReader(File.Open(path, FileMode.Open));
                int hashLength = reader.ReadInt32();
                byte[] fileHash = new byte[hashLength];
                int readResult = reader.Read(fileHash, 0, hashLength);
                int stringLength = reader.ReadInt32();
                string tmp = reader.ReadString();
                byte[] dataHash = _cryptoHandler.HashInput(tmp, key);
                reader?.Dispose();
                reader = null;
                if (IsSameBytes(fileHash, dataHash))
                {
                    var data = _jsonHandler.Deserialize<T>(tmp);
                    return (true, data);
                }
                else
                    return (false, default);
            }
            catch
            {
                reader?.Dispose();
                throw;
            }

        }

        public string LoadStringData(string path, byte[] key)
        {
            if (string.IsNullOrEmpty(path))
                return null;
            if (!File.Exists(path))
                return null;
            BinaryReader reader = null;
            {
                try
                {
                    reader = new BinaryReader(File.Open(path, FileMode.Open));
                    if (reader.BaseStream.Length < 1)
                        return null;
                    int hashLength = reader.ReadInt32();
                    byte[] fileHash = new byte[hashLength];
                    int readResult = reader.Read(fileHash, 0, hashLength);
                    int stringLength = reader.ReadInt32();
                    string tmp = reader.ReadString();
                    byte[] dataHash = _cryptoHandler.HashInput(tmp, key);
                    if (IsSameBytes(fileHash, dataHash))
                    {
                        return tmp;
                    }
                    else
                        return null;
                }
                catch
                {
                    throw;
                }
                finally { reader?.Dispose(); }
            }
        }

        public Task<string> LoadStringDataAsync(string path, byte[] key)
        {
            if (key != null)
                return Task.Factory.StartNew(() => LoadStringData(path, key));
            else
                return Task.Factory.StartNew(() => LoadDataStringFile(path));
        }

        public string LoadDataStringFile(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;
            if (!File.Exists(path))
                return null;
            BinaryReader reader = null;
            try
            {
                reader = new BinaryReader(File.Open(path, FileMode.Open));
                if (reader.BaseStream.Length < 1)
                    return null;
                return reader.ReadString();
            }
            catch
            {
                throw;
            }
            finally { reader?.Dispose(); }
        }

        public Task<(bool, object)> LoadDataAsync(string path, byte[] key, Type dataType)
        {
            return Task.Factory.StartNew(() => LoadFile(path, key, dataType));
        }

        public Task<(bool, T)> LoadDataAsync<T>(string path, byte[] key)
        {
            return Task.Factory.StartNew(() => LoadData<T>(path, key));
        }

        public async Task<(bool, T)> LoadDataAsync<T>(string path)
        {
            if (string.IsNullOrEmpty(path))
                return (false, default);
            if (!File.Exists(path))
                return (false, default);
            StreamReader reader = null;
            try
            {
                reader = new StreamReader(path);
                string tmp = await reader.ReadToEndAsync();
                T data = await Task.Factory.StartNew(() => _jsonHandler.Deserialize<T>(tmp));
                return (true, data);
            }
            catch
            {
                throw;
            }
            finally
            {
                reader?.Dispose();
            }

        }

        public bool SaveDataStringFile(string data, string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            StreamWriter file = null;
            try
            {
                file = new StreamWriter(File.Open(path, FileMode.OpenOrCreate));
                file.WriteLine(data);
                file.BaseStream.SetLength(data.Length);
                return true;
            }
            catch
            {
                throw;
            }
            finally
            {
                file?.Dispose();
            }
        }

        public bool SaveStringData(string data, string path, byte[] key)
        {
            if (string.IsNullOrEmpty(path))
                return false;
            BinaryWriter file = null;
            try
            {
                file = new BinaryWriter(File.Open(path, FileMode.OpenOrCreate));
                if (key != null)
                {
                    byte[] hash = _cryptoHandler.HashInput(data, key);
                    file.Write(hash.Length);
                    //long length = file.BaseStream.Length;
                    file.Write(hash);
                    file.Write(data.Length);
                }
                file.Write(data);
                return true;
            }
            catch
            {
                throw;
            }
            finally
            {
                file?.Dispose();
            }
        }

        public bool SaveData<T>(T data, string path, byte[] key)
        {
            if (string.IsNullOrEmpty(path))
                return false;
            BinaryWriter file = null;
            try
            {
                string tmp = _jsonHandler.Serialize(data);
                file = new BinaryWriter(File.Open(path, FileMode.OpenOrCreate));
                byte[] hash = _cryptoHandler.HashInput(tmp, key);
                file.Write(hash.Length);
                //long length = file.BaseStream.Length;
                file.Write(hash);
                file.Write(tmp.Length);
                file.Write(tmp);
                return true;
            }
            catch
            {
                throw;
            }
            finally
            {
                file?.Dispose();
            }

        }

        public bool SaveData<T>(T data, string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;
            try
            {
                string tmp = _jsonHandler.Serialize(data);
                return SaveDataStringFile(tmp, path);
            }
            catch
            {
                throw;
            }
        }

        public Task<bool> SaveDataAsync<T>(T data, string path, Action onDone = null)
        {
            return Task.Factory.StartNew(() => { bool result = SaveData<T>(data, path); onDone?.Invoke(); return result; });
        }

        public Task<bool> SaveDataAsync<T>(T data, string path, byte[] key, Action onDone = null)
        {
            return Task.Factory.StartNew(() => { bool result = SaveData<T>(data, path, key); onDone?.Invoke(); return result; });
        }

        //public void DeleteRegister(string key)
        //{
        //    PlayerPrefs.DeleteKey(key);
        //    PlayerPrefs.DeleteKey(key + "hashData");
        //}

        public void DeleteData(string path)
        {
            File.Delete(path);
        }



        public async Task<(bool, T)> LoadEncryptedDataAsync<T>(Encoding encoding, string path, byte[] key, byte[] iv)
        {
            if (!File.Exists(path))
                return (false, default);
            try
            {
                byte[] data = null;
                using (FileStream fileStream = File.OpenRead(path))
                {
                    data = await _cryptoHandler.DecryptDataAsync(fileStream, key, iv);
                }
                if (data == null)
                    return (false, default);
                T result = await Task.Factory.StartNew(() => { string stringData = encoding.GetString(data); return _jsonHandler.Deserialize<T>(stringData); });
                return (true, result);
            }
            catch
            {
                throw;
            }
        }

        public (bool, T) LoadEncryptedData<T>(Encoding encoding, string path, byte[] key, byte[] iv)
        {
            if (!File.Exists(path))
                return (false, default);
            try
            {
                byte[] data = null;
                using (FileStream fileStream = File.OpenRead(path))
                {
                    data = _cryptoHandler.DecryptData(fileStream, key, iv);
                }
                if (data == null)
                    return (false, default);
                string stringData = encoding.GetString(data);
                T result = _jsonHandler.Deserialize<T>(stringData);
                return (true, result);
            }
            catch
            {
                throw;
            }
        }

        public Task<(bool, object)> LoadEncrypteDataAsync(Encoding encoding, string path, byte[] key, byte[] iv, Type dataType)
        {
            return Task.Run(() => LoadEncryptedFile(encoding, path, key, iv, dataType));
        }

        public (bool, object) LoadEncryptedFile(Encoding encoding, string path, byte[] key, byte[] iv, Type dataType)
        {
            if (!File.Exists(path))
                return (false, null);
            try
            {
                byte[] data = null;
                using (FileStream fileStream = File.OpenRead(path))
                {
                    data = _cryptoHandler.DecryptData(fileStream, key, iv);
                }
                if (data == null)
                    return (false, default);
                string stringData = encoding.GetString(data);
                object result = _jsonHandler.Deserialize(stringData, dataType);
                return (true, result);
            }
            catch
            {
                throw;
            }
        }

        public bool SaveEncryptedData<T>(T source, Encoding encoding, string path, byte[] key, byte[] iv)
        {
            try
            {
                string stringData = _jsonHandler.Serialize(source);
                byte[] data = Encoding.UTF8.GetBytes(stringData);
                using (FileStream fileStream = File.OpenWrite(path))
                {
                    fileStream.SetLength(0);
                    _cryptoHandler.EncryptStream(data, key, iv, fileStream);
                }
                return true;
            }
            catch
            {
                throw;
            }
        }

        public bool SaveEncryptedStringData(string stringData, Encoding encoding, string path, byte[] key, byte[] iv)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(stringData);
                using (FileStream fileStream = File.OpenWrite(path))
                {
                    fileStream.SetLength(0);
                    _cryptoHandler.EncryptStream(data, key, iv, fileStream);
                }
                return true;
            }
            catch
            {
                throw;
            }
        }

        private bool IsSameBytes(byte[] T1, byte[] T2)
        {
            int length = T1.Length;
            if (T2.Length != length)
                return false;
            ParallelLoopResult result = Parallel.For(0, length, (index, state) =>
            {
                if (T1[index] != T2[index])
                    state.Break();
            });
            return !result.LowestBreakIteration.HasValue;
        }
    }
}
