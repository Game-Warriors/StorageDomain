using GameWarriors.StorageDomain.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

#if UNITY_2018_4_OR_NEWER
using UnityEngine;
using UnityEngine.Scripting;
namespace GameWarriors.StorageDomain.Core
{

    public class PlayPrefsStorageSystem : IStorage, IStorageOperation
    {
        private const string FILE_ROOT = "STSYS";

        private readonly IPersistDataHandler _persistStorage;
        private readonly IStorageConfig _storageConfig;
        private readonly IList<IStorageItem> _storageItemList;
        private readonly Dictionary<string, IStorageItem> _itemsTable;
        private float _timeTmp;

        public event Action<string> LogErrorListener;

        public string StorageRoot => string.Empty;

        [Preserve]
        public PlayPrefsStorageSystem(IPersistDataHandler persistStorage, IStorageConfig storageConfig)
        {
            _persistStorage = persistStorage;
            _storageConfig = storageConfig;
            _storageItemList = new List<IStorageItem>();
            _itemsTable = new Dictionary<string, IStorageItem>();
        }

        void IStorage.LoadingDefaultModelAsync<T, U>(string dataName, bool isEncrypt, Action<U> onLoad)
        {
            throw new NotSupportedException();
        }

        Task<U> IStorage.LoadingDefaultModelAsync<T, U>(string dataName, bool isEncrypt)
        {
            throw new NotSupportedException();
        }

        void IStorage.LoadingModelAsync<T>(string dataName, bool isEncrypt, Action<T> onLoad)
        {
            if (onLoad == null)
                throw new ArgumentNullException($"The onLoad call back is null for dataName {dataName}");

            if (string.IsNullOrEmpty(dataName))
                throw new ArgumentNullException($"The null or empty value input for dataName of type:{typeof(T)}");

            string path = FILE_ROOT + dataName;
            if (_itemsTable.TryGetValue(dataName, out var item))
            {
                onLoad((T)item);
            }
            else
            {
                (bool state, T model) = isEncrypt
                    ? ((bool state, T model))_persistStorage.LoadEncryptedData<T>(Encoding.UTF8, path, _storageConfig.Key, _storageConfig.IV)
                    : ((bool state, T model))_persistStorage.LoadData<T>(path, _storageConfig.Key);
                if (state)
                {
                    _itemsTable.Add(dataName, model);
                    _storageItemList.Add(model);
                    onLoad(model);
                }
                else
                {
                    T newFile = new();
                    _itemsTable.Add(dataName, newFile);
                    _storageItemList.Add(newFile);
                    newFile.Initialization();
                    onLoad(newFile);
                }
            }
        }

        Task<T> IStorage.LoadingModelAsync<T>(string dataName, bool isEncrypt)
        {
            throw new NotSupportedException();
        }

        void IStorage.SetValue<T1>(string key, T1 data)
        {
            if (data.GetTypeCode() is TypeCode.Int32 or TypeCode.Int16)
            {
                PlayerPrefs.SetInt(key, Convert.ToInt32(data));

            }
            else if (data.GetTypeCode() is TypeCode.Single)
            {
                PlayerPrefs.SetFloat(key, Convert.ToSingle(data));

            }
            else if (data.GetTypeCode() is TypeCode.String)
            {
                PlayerPrefs.SetString(key, Convert.ToString(data));
            }
        }

        public void DeleteValue<T1>(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }

        T1 IStorage.GetValue<T1>(string key, T1 defualtValue)
        {
            if (defualtValue.GetTypeCode() is TypeCode.Int32 or TypeCode.Int16)
            {
                IConvertible convertible = PlayerPrefs.GetInt(key, Convert.ToInt32(defualtValue));
                return (T1)convertible;
            }
            else if (defualtValue.GetTypeCode() is TypeCode.Single)
            {
                IConvertible convertible = PlayerPrefs.GetFloat(key, Convert.ToSingle(defualtValue));
                return (T1)convertible;
            }
            else if (defualtValue.GetTypeCode() is TypeCode.String)
            {
                IConvertible convertible = PlayerPrefs.GetString(key, Convert.ToString(defualtValue));
                return (T1)convertible;
            }
            return defualtValue;
        }


        void IStorageOperation.StorageUpdate(float deltaTime)
        {
            _timeTmp += deltaTime;
            if (_timeTmp > _storageConfig.SavingInterval)
            {
                bool isResetTime = true;
                try
                {
                    int count = _storageItemList?.Count ?? 0;
                    for (int i = 0; i < count; ++i)
                    {
                        IStorageItem item = _storageItemList[i];
                        if (item.IsChanged)
                        {
                            string path = FILE_ROOT + item.ModelName;
                            if (item.IsEncrypt)
                            {
                                _persistStorage.SaveEncryptedData(item, Encoding.UTF8, path, _storageConfig.Key, _storageConfig.IV);
                            }
                            else
                                _persistStorage.SaveData(item, path, _storageConfig.Key);
                            item.SetAsSaved();
                            if (!item.IsChanged)//prevent to permanent change file block save auto update
                            {
                                isResetTime = false;
                                break;
                            }
                        }
                    }
                }
                catch (Exception E)
                {
                    LogError(E.ToString());
                }
                if (isResetTime)
                    _timeTmp = 0;
            }
        }

        Task IStorageOperation.ReloadStorage(Action onDone)
        {
            onDone?.Invoke();
            return Task.CompletedTask;
        }

        void IStorageOperation.DeletePersistStorage(string storageName)
        {
            PlayerPrefs.DeleteAll();
        }

        bool IStorageOperation.ChangePersistStorage(bool isDeleteOldFiles)
        {
            PlayerPrefs.DeleteAll();
            return true;
        }

        void IStorageOperation.ForceSave()
        {
            try
            {
                SaveStorageItemFiles();
            }
            catch (Exception E)
            {
                LogError(E.ToString());
            }
        }

        private void SaveStorageItemFiles()
        {
            int count = _storageItemList?.Count ?? 0;
            for (int i = 0; i < count; ++i)
            {
                IStorageItem item = _storageItemList[i];
                if (item.IsChanged)
                {
                    string path = FILE_ROOT + item.ModelName;
                    if (item.IsEncrypt)
                    {
                        _persistStorage.SaveEncryptedData(item, Encoding.UTF8, path, _storageConfig.Key, _storageConfig.IV);
                    }
                    else
                        _persistStorage.SaveData(item, path, _storageConfig.Key);
                    item.SetAsSaved();
                }
            }
        }

        private void LogError(string message)
        {
            LogErrorListener?.Invoke(message);
        }
    }
}
#endif