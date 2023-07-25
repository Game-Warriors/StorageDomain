using GameWarriors.StorageDomain.Abstraction;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GameWarriors.StorageDomain.Core
{
    public class FileHandler : IFileHandler
    {
        private readonly IStorageSerializationHandler _jsonHandler;
        public event Action<string> LogErrorListener;

#if UNITY_2018_4_OR_NEWER
        [UnityEngine.Scripting.Preserve]
#endif
        public FileHandler(IStorageSerializationHandler jsonHandler)
        {
            _jsonHandler = jsonHandler;
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
                byte[] dataHash = HashInput(tmp, key);
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
            catch (Exception E)
            {
                LogError($"file:{path} , " + E.ToString());
                reader?.Dispose();
            }
            return (false, default);
        }

        public (bool, T) LoadFile<T>(string path, byte[] key)
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
                byte[] dataHash = HashInput(tmp, key);
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
            catch (Exception E)
            {
                LogError(E.ToString());
                reader?.Dispose();
            }
            return (false, default);
        }

        public string LoadDataStringFile(string path, byte[] key)
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
                    byte[] dataHash = HashInput(tmp, key);
                    if (IsSameBytes(fileHash, dataHash))
                    {
                        return tmp;
                    }
                    else
                        return null;
                }
                catch (Exception E)
                {
                    LogError(E.ToString());
                }
                finally { reader?.Dispose(); }
                return null;
            }
        }

        public Task<string> LoadDataStringFileAsync(string path, byte[] key)
        {
            if (key != null)
                return Task.Factory.StartNew(() => LoadDataStringFile(path, key));
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
            catch (Exception E)
            {
                LogError(E.ToString());
            }
            finally { reader?.Dispose(); }
            return null;
        }

        public Task<(bool, object)> LoadFileAsync(string path, byte[] key, Type dataType)
        {
            return Task.Factory.StartNew(() => LoadFile(path, key, dataType));
        }

        public Task<(bool, T)> LoadFileAsync<T>(string path, byte[] key)
        {
            return Task.Factory.StartNew(() => LoadFile<T>(path, key));
        }

        public async Task<(bool, T)> LoadFileAsync<T>(string path)
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
                var data = await Task.Factory.StartNew(() => _jsonHandler.Deserialize<T>(tmp));
                return (true, data);
            }
            catch (Exception E)
            {
                LogError(E.ToString());
                return (false, default);
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
            catch (Exception E)
            {
                LogError(E.ToString());
            }
            finally
            {
                file?.Dispose();
            }
            return false;
        }

        public bool SaveDataStringFile(string data, string path, byte[] key)
        {
            if (string.IsNullOrEmpty(path))
                return false;
            BinaryWriter file = null;
            try
            {
                file = new BinaryWriter(File.Open(path, FileMode.OpenOrCreate));
                if (key != null)
                {
                    var hash = HashInput(data, key);
                    file.Write(hash.Length);
                    //long length = file.BaseStream.Length;
                    file.Write(hash);
                    file.Write(data.Length);
                }
                file.Write(data);
                return true;
            }
            catch (Exception E)
            {
                LogError(E.ToString());
                return false;
            }
            finally
            {
                file?.Dispose();
            }
        }

        public bool SaveFile<T>(T data, string path, byte[] key)
        {
            if (string.IsNullOrEmpty(path))
                return false;
            BinaryWriter file = null;
            try
            {
                string tmp = _jsonHandler.Serialize(data);
                file = new BinaryWriter(File.Open(path, FileMode.OpenOrCreate));
                var hash = HashInput(tmp, key);
                file.Write(hash.Length);
                //long length = file.BaseStream.Length;
                file.Write(hash);
                file.Write(tmp.Length);
                file.Write(tmp);
                return true;
            }
            catch (Exception E)
            {
                LogError(E.ToString());
                return false;
            }
            finally
            {
                file?.Dispose();
            }

        }

        public bool SaveFile<T>(T data, string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;
            try
            {
                string tmp = _jsonHandler.Serialize(data);
                return SaveDataStringFile(tmp, path);
            }
            catch (Exception E)
            {
                LogError(E.ToString());
                return false;
            }
        }

        public Task<bool> SaveFileAsync<T>(T data, string path, Action onDone = null)
        {
            return Task.Factory.StartNew(() => { bool result = SaveFile<T>(data, path); onDone?.Invoke(); return result; });
        }

        public Task<bool> SaveFileAsync<T>(T data, string path, byte[] key, Action onDone = null)
        {
            return Task.Factory.StartNew(() => { bool result = SaveFile<T>(data, path, key); onDone?.Invoke(); return result; });
        }

        //public void DeleteRegister(string key)
        //{
        //    PlayerPrefs.DeleteKey(key);
        //    PlayerPrefs.DeleteKey(key + "hashData");
        //}

        public void DeleteFile(string path)
        {
            File.Delete(path);
        }

        private byte[] HashInput(string input, byte[] key)
        {
            byte[] tmp = System.Text.Encoding.Unicode.GetBytes(input);
            using (var sha = new HMACSHA256(key))
            {
                var hash = sha.ComputeHash(tmp);
                return hash;
            }
        }

        private string HashInputString(string input, byte[] key)
        {
            byte[] tmp = System.Text.Encoding.Unicode.GetBytes(input);
            using (var sha = new HMACSHA256(key))
            {
                var hash = sha.ComputeHash(tmp);
                return Convert.ToBase64String(hash);
            }
        }

        private bool IsSameBytes(byte[] T1, byte[] T2)
        {
            int length = T1.Length;
            if (T2.Length != length)
                return false;
            var result = Parallel.For(0, length, (index, state) =>
              {
                  if (T1[index] != T2[index])
                      state.Break();
              });
            return !result.LowestBreakIteration.HasValue;
        }

        public async Task<(bool, T)> LoadEncryptedFileAsync<T>(Encoding encoding, string path, byte[] key, byte[] iv)
        {
            try
            {
                byte[] data = null;
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    ICryptoTransform cryptoTransform = AES.CreateDecryptor(key, iv);
                    using (FileStream fileStream = File.OpenRead(path))
                    {
                        using (CryptoStream cryptStream = new CryptoStream(fileStream, cryptoTransform, CryptoStreamMode.Read))
                        {
                            using (MemoryStream memoryStream = new MemoryStream())
                            {
                                await cryptStream.CopyToAsync(memoryStream);
                                data = memoryStream.GetBuffer();
                            }
                        }
                    }
                    T result = await Task.Factory.StartNew(() => { string stringData = encoding.GetString(data); return _jsonHandler.Deserialize<T>(stringData); });
                    return (true, result);
                }
            }
            catch (Exception E)
            {
                LogError(E.ToString());
                return (false, default);
            }
        }

        public (bool, T) LoadEncryptedFile<T>(Encoding encoding, string path, byte[] key, byte[] iv)
        {
            try
            {
                byte[] data = null;
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    ICryptoTransform cryptoTransform = AES.CreateDecryptor(key, iv);
                    using (FileStream fileStream = File.OpenRead(path))
                    {
                        using (CryptoStream cryptStream = new CryptoStream(fileStream, cryptoTransform, CryptoStreamMode.Read))
                        {
                            using (MemoryStream memoryStream = new MemoryStream())
                            {
                                cryptStream.CopyTo(memoryStream);
                                data = memoryStream.GetBuffer();
                            }
                        }
                    }
                }
                string stringData = encoding.GetString(data);
                T result = _jsonHandler.Deserialize<T>(stringData);
                return (true, result);
            }
            catch (Exception E)
            {
                LogError(E.ToString());
                return (false, default);
            }
        }

        public bool SaveEncryptedFile<T>(T source, Encoding encoding, string path, byte[] key, byte[] iv)
        {
            try
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    string stringData = _jsonHandler.Serialize(source);
                    byte[] data = Convert.FromBase64String(stringData);
                    //byte[] data = encoding.GetBytes(stringData);
                    using (MemoryStream memoryStream = new MemoryStream(data))
                    {
                        ICryptoTransform cryptoTransform = AES.CreateEncryptor(key, iv);
                        using (CryptoStream cryptStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Read))
                        {
                            using (FileStream fileStream = File.OpenWrite(path))
                                cryptStream.CopyTo(fileStream);
                        }
                    }
                }
                return true;
            }
            catch (Exception E)
            {
                LogError(E.ToString());
                return false;
            }
        }

        public Task<(bool, object)> LoadEncryptedFileAsync(Encoding encoding, string path, byte[] key, byte[] iv, Type dataType)
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
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    ICryptoTransform cryptoTransform = AES.CreateDecryptor(key, iv);
                    using (FileStream fileStream = File.OpenRead(path))
                    {
                        using (CryptoStream cryptStream = new CryptoStream(fileStream, cryptoTransform, CryptoStreamMode.Read))
                        {
                            using (MemoryStream memoryStream = new MemoryStream())
                            {
                                cryptStream.CopyTo(memoryStream);
                                data = memoryStream.GetBuffer();
                            }
                        }
                    }
                }
                string stringData = encoding.GetString(data);
                //Debug.Log(stringData);
                //string stringData = Convert.ToBase64String(data);
                //Convert.for
                object result = _jsonHandler.Deserialize(stringData, dataType);
                return (true, result);
            }
            catch (Exception E)
            {
                LogError($"file{dataType} " + E.ToString());
                return (false, null);
            }
        }

        public bool SaveEncryptedStringFile(string stringData, Encoding encoding, string path, byte[] key, byte[] iv)
        {
            try
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    byte[] data = Encoding.UTF8.GetBytes(stringData);
                    using (MemoryStream memoryStream = new MemoryStream(data))
                    {
                        ICryptoTransform cryptoTransform = AES.CreateEncryptor(key, iv);
                        using (CryptoStream cryptStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Read))
                        {
                            using (FileStream fileStream = File.OpenWrite(path))
                            {
                                fileStream.SetLength(0);
                                cryptStream.CopyTo(fileStream);
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception E)
            {
                LogError(E.ToString());
                return false;
            }
        }

        private void LogError(string message)
        {
            LogErrorListener?.Invoke(message);
        }
    }
}
