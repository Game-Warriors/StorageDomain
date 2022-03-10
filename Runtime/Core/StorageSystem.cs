using GameWarriors.StorageDomain.Abstraction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GameWarriors.StorageDomain.Core
{
    public class StorageSystem : IStorage
    {
        private enum EDatabaseType { None, Int, Float, String, Bool }
        private const string TYPE_INDICATOR_KEY = "@DataType!";
        private const string DATABASE_FILE_NAME = "DatabaseFile.bin";
        private const string DATABASE_DIRECTORY_NAME = "{0}_Storage";

        private readonly IStorageEventHandler _storageEvent;
        private readonly IFileHandler _fileHandler;
        private readonly IStorageConfig _storageConfig;
        private readonly Dictionary<Type, IStorageItem> _fileTable;
        private Dictionary<Type, StorageDatabaseItem> _databaseTable;
        //private List<Task> _loadDataTaskList;

        private bool _isDataChange;
        private string _fileRoot;
        private string _currentDirectoryPrefix;
        private string _databaseFilePath;
        private float _timeTmp;

        public string FileRoot => _fileRoot;
        public byte[] Key => _storageConfig.Key;

#if UNITY_2018_4_OR_NEWER
        [UnityEngine.Scripting.Preserve]
#endif
        public StorageSystem(IFileHandler fileHandler, IStorageConfig storageConfig, IStorageEventHandler storageEvent)
        {
            _fileHandler = fileHandler;
            _storageConfig = storageConfig;
            _currentDirectoryPrefix = storageConfig.DirectoryPrefix;
            GenerateDatabaseFilePath();
            if (!Directory.Exists(_fileRoot))
                Directory.CreateDirectory(_fileRoot);
            storageEvent.SetForceSaveEvent(ForceSaveing);
            storageEvent.SetStorageUpdate(StorageUpdate);
            _storageEvent = storageEvent;
            _fileTable = new Dictionary<Type, IStorageItem>();
            //_loadDataTaskList = new List<Task>();
        }

#if UNITY_2018_4_OR_NEWER
        [UnityEngine.Scripting.Preserve]
#endif
        public Task WaitForLoading()
        {
            return Task.Run(ReadDatabaseFiles);
        }

        public void DeleteDefaultDirectory(string defaultPerfix)
        {
            string fileRoot;
#if !UNITY_EDITOR
            fileRoot = _storageConfig.StorageDataPath + "/" + string.Format(DATABASE_DIRECTORY_NAME, defaultPerfix) + "/";
#else
            fileRoot = string.Format(DATABASE_DIRECTORY_NAME, defaultPerfix) + "/";
#endif

            if (Directory.Exists(fileRoot))
                Directory.Delete(fileRoot, true);
        }

        public async Task ReloadFileDirectoy(Action onDone)
        {
            await Task.Run(ReadDatabaseFiles);
            onDone?.Invoke();            
        }

        public bool ChangeFileDirectionPrefix(bool isDeleteOldFiles)
        {
            if (string.Compare(_currentDirectoryPrefix, _storageConfig.DirectoryPrefix) == 0)
                return false;

            _currentDirectoryPrefix = _storageConfig.DirectoryPrefix;
            string oldFileRoot = _fileRoot;
            GenerateDatabaseFilePath();
            bool isNewPathExist = Directory.Exists(_fileRoot);
            if (isNewPathExist)
            {
                ReloadStorageItemFiles();
                ReadDatabaseFiles();
                return true;
            }
            if (isDeleteOldFiles)
                Directory.Delete(oldFileRoot, true);
            if (!Directory.Exists(_fileRoot))
                Directory.CreateDirectory(_fileRoot);
            ForceSaveing();
            return false;
        }

        private void ForceSaveing()
        {
            try
            {
                WriteOnFile();
                SaveStorageItemFiles();
            }
            catch (Exception E)
            {
                _storageEvent.LogErrorEvent(E.ToString());
            }
        }

        public T1 GetValue<T1>(string key, T1 defualtValue = default) where T1 : IConvertible
        {
            T1 data = default;
            StorageDatabaseItem dataTable = GetValueTable(data);
            if (dataTable.ContainsKey(key))
            {
                return (T1)dataTable[key];
            }
            else
            {
                dataTable.Add(key, defualtValue);
                _isDataChange = true;
                return defualtValue;
            }
        }

        public void SetValue<T1>(string key, T1 data) where T1 : IConvertible
        {
            var dataTable = GetValueTable(data);
            if (dataTable.ContainsKey(key))
            {
                dataTable[key] = data;
            }
            else
            {
                dataTable.Add(key, data);
            }
            _isDataChange = true;
        }

        public void DeleteValue<T1>(string key)
        {
            T1 data = default;
            StorageDatabaseItem dataTable = GetValueTable(data);

            bool isRemove = dataTable.Remove(key);
            if (isRemove)
                _isDataChange = true;

        }

        public Task<T> RegisterLoadingModel<T>(string dataName, bool isEncrypt) where T : IStorageItem, new()
        {
            string path = _fileRoot + dataName;
            Task<T> task;
            if (isEncrypt)
            {
                task = _fileHandler.LoadEncryptedFileAsync<T>(Encoding.UTF8, path, _storageConfig.Key, _storageConfig.IV).ContinueWith(FetchLoadingData<T>);
            }
            else
                task = _fileHandler.LoadFileAsync<T>(path, _storageConfig.Key).ContinueWith(FetchLoadingData<T>);

            //_loadDataTaskList.Add(task);
            return task;
        }

        public Task<U> RegisterLoadingDefaultModel<T, U>(string dataName, bool isEncrypt) where T : IStorageDataItem, new() where U : IDefaultDataModel, new()
        {
            string path = _fileRoot + dataName;
            Task<U> task;
            if (isEncrypt)
            {
                task = _fileHandler.LoadEncryptedFileAsync<U>(Encoding.UTF8, path, _storageConfig.Key, _storageConfig.IV).ContinueWith(FetchLoadingData<U>);
            }
            else
                task = _fileHandler.LoadFileAsync<U>(path, _storageConfig.Key).ContinueWith(FetchLoadingData<U>);

            //_loadDataTaskList.Add(task);
            return task;
        }

        private void StorageUpdate(float deltaTime)
        {
            _timeTmp += deltaTime;
            if (_timeTmp > _storageConfig.SaveingInterval)
            {
                try
                {
                    WriteOnFile();
                    foreach (IStorageItem item in _fileTable?.Values)
                    {
                        if (item.IsChanged)
                        {
                            string data = item.GetDataString;
                            string path = _fileRoot + item.FileName;
                            if (item.IsEncrypt)
                            {
                                _fileHandler.SaveEncryptedStringFile(data, Encoding.UTF8, path, _storageConfig.Key, _storageConfig.IV);
                            }
                            else
                                _fileHandler.SaveDataStringFile(data, path, _storageConfig.Key);
                            item.SetAsSaved();

                            if (!item.IsChanged)
                                break;
                        }
                    }
                }
                catch (Exception E)
                {
                    _storageEvent.LogErrorEvent(E.ToString());
                }
                _timeTmp = 0;
            }
        }

        private StorageDatabaseItem GetValueTable<U>(U data)
        {
            Type uType = typeof(U);
            if (_databaseTable.TryGetValue(uType, out var item))
            {
                return item;
            }
            return null;
        }

        private void WriteOnFile()
        {
            if (_isDataChange)
            {
                using (StreamWriter writer = new StreamWriter(File.Open(_databaseFilePath, FileMode.OpenOrCreate)))
                {
                    foreach (var database in _databaseTable)
                    {
                        writer.WriteLine("@DataType!");
                        writer.WriteLine(database.Key);
                        foreach (KeyValuePair<string, IConvertible> item in database.Value)
                        {
                            writer.WriteLine(item.Key);
                            writer.WriteLine(item.Value);
                        }
                    }
                }
                _isDataChange = false;
            }
        }

        private void ReadDatabaseFiles()
        {
            Dictionary<Type, StorageDatabaseItem> dataTable = new Dictionary<Type, StorageDatabaseItem>();

            dataTable.Add(typeof(string), new StorageDatabaseItem());
            dataTable.Add(typeof(int), new StorageDatabaseItem());
            dataTable.Add(typeof(float), new StorageDatabaseItem());
            dataTable.Add(typeof(bool), new StorageDatabaseItem());

            using (StreamReader reader = new StreamReader(File.Open(_databaseFilePath, FileMode.OpenOrCreate)))
            {
                StorageDatabaseItem currentDatabase = null;
                EDatabaseType databaseType = EDatabaseType.None;
                while (!reader.EndOfStream)
                {
                    string key = reader.ReadLine();
                    string value = reader.ReadLine();
                    if (string.Compare(key, TYPE_INDICATOR_KEY) == 0)
                    {
                        Type currentDataType = Type.GetType(value);

                        if (dataTable.TryGetValue(currentDataType, out var item))
                        {
                            currentDatabase = item;
                            if (currentDataType == typeof(int))
                                databaseType = EDatabaseType.Int;
                            else if (currentDataType == typeof(float))
                                databaseType = EDatabaseType.Float;
                            else if (currentDataType == typeof(string))
                                databaseType = EDatabaseType.String;
                            else if (currentDataType == typeof(bool))
                                databaseType = EDatabaseType.Bool;
                        }
                        else
                        {
                            databaseType = EDatabaseType.None;
                            currentDataType = null;
                        }
                    }
                    else if (!string.IsNullOrEmpty(key) && currentDatabase != null)
                    {
                        switch (databaseType)
                        {
                            case EDatabaseType.Int:
                                if (int.TryParse(value, out var intData))
                                    currentDatabase.Add(key, intData);
                                break;
                            case EDatabaseType.Float:
                                if (float.TryParse(value, out var floatData))
                                    currentDatabase.Add(key, floatData);
                                break;
                            case EDatabaseType.String:
                                currentDatabase.Add(key, value);
                                break;
                            case EDatabaseType.Bool:
                                if (bool.TryParse(value, out var boolData))
                                    currentDatabase.Add(key, boolData);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            _databaseTable = dataTable;
        }

        private void ReloadStorageItemFiles()
        {
            try
            {
                int length = _fileTable.Count;
                Task<(bool, object)>[] loadTask = new Task<(bool, object)>[length];
                int counter = 0;
                foreach (var item in _fileTable)
                {
                    string path = _fileRoot + item.Value.FileName;
                    if (item.Value.IsEncrypt)
                    {
                        loadTask[counter] = _fileHandler.LoadEncryptedFileAsync(Encoding.UTF8, path, _storageConfig.Key, _storageConfig.IV, item.Key);
                    }
                    else
                        loadTask[counter] = _fileHandler.LoadFileAsync(path, _storageConfig.Key, item.Key);

                    _fileTable[item.Key] = null;
                    ++counter;
                }

                Task.WaitAll(loadTask);
                for (int i = 0; i < length; ++i)
                {
                    (bool, object) result = loadTask[i].Result;
                    if (result.Item1)
                    {
                        IStorageItem item = (IStorageItem)result.Item2;
                        _fileTable.Add(item.DataType, item);
                    }
                }
            }
            catch (Exception E)
            {
                _storageEvent.LogErrorEvent(E.ToString());
            }
        }

        private void SaveStorageItemFiles()
        {
            foreach (IStorageItem item in _fileTable?.Values)
            {
                if (item.IsChanged)
                {
                    string data = item.GetDataString;
                    string path = _fileRoot + item.FileName;
                    if (item.IsEncrypt)
                    {
                        _fileHandler.SaveEncryptedStringFile(data, Encoding.UTF8, path, _storageConfig.Key, _storageConfig.IV);
                    }
                    else
                        _fileHandler.SaveDataStringFile(data, path, _storageConfig.Key);
                    item.SetAsSaved();
                }
            }
        }

        private void GenerateDatabaseFilePath()
        {
#if !UNITY_EDITOR
            _fileRoot = _storageConfig.StorageDataPath + "/" + string.Format(DATABASE_DIRECTORY_NAME, _storageConfig.DirectoryPrefix) + "/";
#else
            _fileRoot = string.Format(DATABASE_DIRECTORY_NAME, _storageConfig.DirectoryPrefix) + "/";
#endif
            _databaseFilePath = _fileRoot + DATABASE_FILE_NAME;
            //foreach (var item in _databaseTable)
            //{
            //    item.Value.SetDatabaseFilePath(_fileRoot, item.Key);
            //}
        }

        private T FetchLoadingData<T>(Task<(bool, T)> loadTask) where T : IStorageItem, new()
        {
            (bool isSuccess, T data) = loadTask.Result;
            if (isSuccess)
            {
                lock (_fileTable)
                {
                    _fileTable.Add(typeof(T), data);
                }
            }
            //_loadDataTaskList = null;

            Type dataType = typeof(T);
            if (_fileTable.TryGetValue(dataType, out var file))
            {
                if (file.IsInvalid)
                {
                    file.Initialization();
                }
                return (T)file;
            }
            else
            {
                T newFile = new T();
                newFile.Initialization();
                _fileTable.Add(dataType, newFile);
                return newFile;
            }
        }

        //private IEnumerable<Task> WaitForAll()
        //{
        //    Task task1 = Task.Run(ReadDatabaseFiles);
        //    yield return task1;
        //    foreach (var item in _loadDataTaskList)
        //    {
        //        yield return item;
        //    }         
        //}
    }
}