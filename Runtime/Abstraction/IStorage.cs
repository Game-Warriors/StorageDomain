using System;
using System.Threading.Tasks;

namespace GameWarriors.StorageDomain.Abstraction
{
    /// <summary>
    /// The abstraction is created for accessing the storage system features like loading data models in memory and working with Key/Value storage
    /// </summary>
    public interface IStorage
    {
        /// <summary>
        /// The root full path name where storage data where saving
        /// </summary>
        string StorageRoot { get; }

        /// <summary>
        /// Register data model for loading by storage system, switch context and loading data on other thread, trigger callback and pass the loaded data model after data model fully loaded.
        /// </summary>
        /// <typeparam name="T">The type of data model</typeparam>
        /// <param name="dataName">Unique name of data model</param>
        /// <param name="isEncrypt">Indicating the data should persist save encrypted</param>
        /// <param name="onLoad">callback for receiving data model after load completed</param>
        void LoadingModelAsync<T>(string dataName, bool isEncrypt, Action<T> onLoad) where T : IStorageItem, new();

        /// <summary>
        /// Register data model for loading by storage system, switch context and loading data on other thread.
        /// </summary>
        /// <typeparam name="T">The type of data model</typeparam>
        /// <param name="dataName">Unique name of data model</param>
        /// <param name="isEncrypt">Indicating the data should persist save encrypted</param>
        /// <returns>return loaded data model after data model fully loaded</returns>
        Task<T> LoadingModelAsync<T>(string dataName, bool isEncrypt) where T : IStorageItem, new();

        void LoadingDefaultModelAsync<T, U>(string dataName, bool isEncrypt, Action<U> onLoad) where T : IStorageDataItem, new() where U : IDefaultDataModel, new();
        Task<U> LoadingDefaultModelAsync<T, U>(string dataName, bool isEncrypt) where T : IStorageDataItem, new() where U : IDefaultDataModel, new();

        /// <summary>
        /// retrive value from storage by key if exist, create data and set default value where not exist.
        /// </summary>
        /// <typeparam name="T1">Type of data. just int, float, bool, string are support</typeparam>
        /// <param name="key"></param>
        /// <param name="defualtValue"></param>
        /// <returns></returns>
        T1 GetValue<T1>(string key, T1 defualtValue = default) where T1 : IConvertible;

        /// <summary>
        /// Set new value or update exist value.
        /// </summary>
        /// <typeparam name="T1">Type of data. just int, float, bool, string are support</typeparam>
        /// <param name="key"></param>
        /// <param name="data"></param>
        void SetValue<T1>(string key, T1 data) where T1 : IConvertible;

        /// <summary>
        /// remove data key and value from storage if exist.
        /// </summary>
        /// <typeparam name="T1">Type of data. just int, float, bool, string are support</typeparam>
        /// <param name="key"></param>
        void DeleteValue<T1>(string key);
    }
}