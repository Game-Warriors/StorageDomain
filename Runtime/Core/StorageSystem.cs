﻿using GameWarriors.StorageDomain.Abstraction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GameWarriors.StorageDomain.Core
{
    /// <summary>
    /// The main implementation of system which execute all storage logics like loading data to models from files, apply auto save, Key/Value storage and reloading data.
    /// </summary>
    public class StorageSystem : IStorage, IStorageOperation
    {
        private enum EDatabaseType { None, Int, Float, String, Bool }

        private const string TYPE_INDICATOR_KEY = "@DataType!";
        private const string DATABASE_FILE_NAME = "DatabaseFile.bin";
        private const string DATABASE_DIRECTORY_NAME = "{0}_Storage";

        private readonly IPersistDataHandler _fileHandler;
        private readonly IStorageConfig _storageConfig;
        private readonly IList<IStorageItem> _filesList;
        private Dictionary<Type, StorageDatabaseItem> _databaseTable;
        private Dictionary<string, Task> _loadingTable;

        private bool _isDataChange;
        private string _fileRoot;
        private string _currentDirectoryPrefix;
        private string _databaseFilePath;
        private float _timeTmp;

        public event Action<string> LogErrorListener;
        public string StorageRoot => _fileRoot;

#if UNITY_2018_4_OR_NEWER
        [UnityEngine.Scripting.Preserve]
#endif
        public StorageSystem(IPersistDataHandler fileHandler, IStorageConfig storageConfig)
        {
            _fileHandler = fileHandler;
            _storageConfig = storageConfig;
            _currentDirectoryPrefix = storageConfig.PersistStorageName;
            GenerateDatabaseFilePath();
            if (!Directory.Exists(_fileRoot))
                Directory.CreateDirectory(_fileRoot);
            _filesList = new List<IStorageItem>();
            _loadingTable = new Dictionary<string, Task>
            {
                { DATABASE_FILE_NAME, Task.Run(ReadDatabaseFiles) }
            };
        }

#if UNITY_2018_4_OR_NEWER
        [UnityEngine.Scripting.Preserve]
#endif
        public Task WaitForLoading()
        {
            return Task.WhenAll(_loadingTable.Values);
        }

        T1 IStorage.GetValue<T1>(string key, T1 defaultValue)
        {
            T1 data = default;
            StorageDatabaseItem dataTable = GetValueTable(data);
            if (dataTable.ContainsKey(key))
            {
                return (T1)dataTable[key];
            }
            else
            {
                dataTable.Add(key, defaultValue);
                _isDataChange = true;
                return defaultValue;
            }
        }

        void IStorage.SetValue<T1>(string key, T1 data)
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

        void IStorage.DeleteValue<T1>(string key)
        {
            T1 data = default;
            StorageDatabaseItem dataTable = GetValueTable(data);

            bool isRemove = dataTable.Remove(key);
            if (isRemove)
                _isDataChange = true;

        }

        Task<T> IStorage.LoadingModelAsync<T>(string dataName, bool isEncrypt)
        {
            string path = _fileRoot + dataName;
            if (_loadingTable.TryGetValue(dataName, out var oldTask))
            {
                return oldTask as Task<T>;
            }
            else
            {
                Task<T> task;
                if (isEncrypt)
                {
                    task = _fileHandler.LoadEncryptedDataAsync<T>(Encoding.UTF8, path, _storageConfig.Key, _storageConfig.IV).ContinueWith(FetchLoadingData<T>);
                }
                else
                    task = _fileHandler.LoadDataAsync<T>(path, _storageConfig.Key).ContinueWith(FetchLoadingData<T>);

                _loadingTable.Add(dataName, task);
                return task;
            }
        }

        void IStorage.LoadingModelAsync<T>(string dataName, bool isEncrypt, Action<T> onLoad)
        {
            if (onLoad == null)
                throw new ArgumentNullException($"The onLoad call back is null for dataName {dataName}");

            string path = _fileRoot + dataName;
            if (_loadingTable.TryGetValue(dataName, out var oldTask))
            {
                HandleOldLoading(onLoad, oldTask);
            }
            else
            {
                Task originTask = HandleNewLoading(isEncrypt, onLoad, path);
                _loadingTable.Add(dataName, originTask);
            }
        }

        Task<U> IStorage.LoadingDefaultModelAsync<T, U>(string dataName, bool isEncrypt)
        {
            string path = _fileRoot + dataName;
            if (_loadingTable.TryGetValue(dataName, out var oldTask))
            {
                return oldTask as Task<U>;
            }
            else
            {
                Task<U> task;
                if (isEncrypt)
                {
                    task = _fileHandler.LoadEncryptedDataAsync<U>(Encoding.UTF8, path, _storageConfig.Key, _storageConfig.IV).ContinueWith(FetchLoadingData<U>);
                }
                else
                    task = _fileHandler.LoadDataAsync<U>(path, _storageConfig.Key).ContinueWith(FetchLoadingData<U>);

                _loadingTable.Add(dataName, task);
                return task;
            }
        }

        void IStorage.LoadingDefaultModelAsync<T, U>(string dataName, bool isEncrypt, Action<U> onLoad)
        {
            if (onLoad == null)
                throw new ArgumentNullException($"The onLoad call back is null for dataName {dataName}");

            string path = _fileRoot + dataName;
            if (_loadingTable.TryGetValue(dataName, out var oldTask))
            {
                HandleOldLoading(onLoad, oldTask);
            }
            else
            {
                Task originTask = HandleNewLoading(isEncrypt, onLoad, path);
                _loadingTable.Add(dataName, originTask);
            }
        }

        void IStorageOperation.StorageUpdate(float deltaTime)
        {
            _timeTmp += deltaTime;
            if (_timeTmp > _storageConfig.SavingInterval)
            {
                bool isResetTime = true;
                try
                {
                    Task writeDatabaseTask = WriteOnFileAsync();
                    int count = _filesList?.Count ?? 0;
                    for (int i = 0; i < count; ++i)
                    {
                        IStorageItem item = _filesList[i];
                        if (item.IsChanged)
                        {
                            string path = _fileRoot + item.ModelName;
                            if (item.IsEncrypt)
                            {
                                _fileHandler.SaveEncryptedData(item, Encoding.UTF8, path, _storageConfig.Key, _storageConfig.IV);
                            }
                            else
                                _fileHandler.SaveData(item, path, _storageConfig.Key);
                            item.SetAsSaved();

                            if (!item.IsChanged)//prevent to permanent change file block save auto update
                            {
                                isResetTime = false;
                                break;
                            }
                        }
                    }
                    writeDatabaseTask.Wait();
                }
                catch (Exception E)
                {
                    LogError(E.ToString());
                }
                if (isResetTime)
                    _timeTmp = 0;
            }
        }

        async Task IStorageOperation.ReloadStorage(Action onDone)
        {
            await Task.Run(ReadDatabaseFiles);
            onDone?.Invoke();
        }

        void IStorageOperation.DeletePersistStorage(string storageName)
        {
            string fileRoot;
#if !UNITY_EDITOR
            fileRoot = _storageConfig.StorageDataPath + "/" + string.Format(DATABASE_DIRECTORY_NAME, storageName) + "/";
#else
            fileRoot = string.Format(DATABASE_DIRECTORY_NAME, storageName) + "/";
#endif

            if (Directory.Exists(fileRoot))
                Directory.Delete(fileRoot, true);
        }

        bool IStorageOperation.ChangePersistStorage(bool isDeleteOldFiles)
        {
            if (string.Compare(_currentDirectoryPrefix, _storageConfig.PersistStorageName) == 0)
                return false;

            _currentDirectoryPrefix = _storageConfig.PersistStorageName;
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
            ForceSave();
            return false;
        }

        public void ForceSave()
        {
            try
            {
                WriteOnFile();
                SaveStorageItemFiles();
            }
            catch (Exception E)
            {
                LogError(E.ToString());
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
                _isDataChange = false;
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
            }
        }

        private Task WriteOnFileAsync()
        {
            if (_isDataChange)
            {
                return Task.Run(WriteOnFile);
            }
            return Task.CompletedTask;
        }

        private void ReadDatabaseFiles()
        {
            Dictionary<Type, StorageDatabaseItem> dataTable = new Dictionary<Type, StorageDatabaseItem>
            {
                { typeof(string), new StorageDatabaseItem() },
                { typeof(int), new StorageDatabaseItem() },
                { typeof(float), new StorageDatabaseItem() },
                { typeof(bool), new StorageDatabaseItem() }
            };

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
                foreach (var item in _filesList)
                {
                    string path = _fileRoot + item.ModelName;
                    if (item.IsEncrypt)
                    {
                        _loadingTable[item.ModelName] = _fileHandler.LoadEncrypteDataAsync(Encoding.UTF8, path, _storageConfig.Key, _storageConfig.IV, item.DataType);
                    }
                    else
                        _loadingTable[item.ModelName] = _fileHandler.LoadDataAsync(path, _storageConfig.Key, item.DataType);
                }
                _filesList.Clear();
                Task wait = Task.WhenAll(_loadingTable.Values);
                wait.Wait();
                foreach (var item in _loadingTable.Values)
                {
                    _filesList.Add((IStorageItem)item);
                }
            }
            catch (Exception E)
            {
                LogError(E.ToString());
            }
        }

        private void SaveStorageItemFiles()
        {
            int count = _filesList?.Count ?? 0;
            for (int i = 0; i < count; ++i)
            {
                IStorageItem item = _filesList[i];
                if (item.IsChanged)
                {
                    string path = _fileRoot + item.ModelName;
                    if (item.IsEncrypt)
                    {
                        _fileHandler.SaveEncryptedData(item, Encoding.UTF8, path, _storageConfig.Key, _storageConfig.IV);
                    }
                    else
                        _fileHandler.SaveData(item, path, _storageConfig.Key);
                    item.SetAsSaved();
                }
            }
        }

        private void GenerateDatabaseFilePath()
        {
#if !UNITY_EDITOR
            _fileRoot = _storageConfig.StorageDataPath + "/" + string.Format(DATABASE_DIRECTORY_NAME, _storageConfig.PersistStorageName) + "/";
#else
            _fileRoot = string.Format(DATABASE_DIRECTORY_NAME, _storageConfig.PersistStorageName) + "/";
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
                if (data.IsInvalid)
                {
                    data.Initialization();
                }
                lock (_filesList)
                {
                    _filesList.Add(data);
                }
                return (T)data;
            }
            else
            {
                //LogError($"failed to loading the dataType:{dataType}");
                T newFile = new T();
                newFile.Initialization();
                lock (_filesList)
                {
                    _filesList.Add(newFile);
                }
                return newFile;
            }
        }

        private void LogError(string message)
        {
            LogErrorListener?.Invoke(message);
        }

        private static void HandleOldLoading<T>(Action<T> onLoad, Task oldTask) where T : IStorageItem, new()
        {
            Task<T> originTask = oldTask as Task<T>;
            if (originTask.IsCompletedSuccessfully)
            {
                onLoad(originTask.Result);
            }
            else
                originTask.ContinueWith(task => onLoad(task.Result));
        }

        private Task HandleNewLoading<T>(bool isEncrypt, Action<T> onLoad, string path) where T : IStorageItem, new()
        {
            Task originTask;
            if (isEncrypt)
            {
                originTask = _fileHandler.LoadEncryptedDataAsync<T>(Encoding.UTF8, path, _storageConfig.Key, _storageConfig.IV)
                    .ContinueWith(FetchLoadingData<T>).ContinueWith(task => onLoad(task.Result));
            }
            else
                originTask = _fileHandler.LoadDataAsync<T>(path, _storageConfig.Key)
                    .ContinueWith(FetchLoadingData<T>).ContinueWith(task => onLoad(task.Result));
            return originTask;
        }
    }
}