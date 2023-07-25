using System;
using System.Threading.Tasks;

namespace GameWarriors.StorageDomain.Abstraction
{
    /// <summary>
    /// The basic abstraction which present high level storage operations for system, like force save and system routine update.
    /// </summary>
    public interface IStorageOperation
    {
        /// <summary>
        /// The event will trigger when error raised in storage system.
        /// </summary>
        event Action<string> LogErrorListener;
        /// <summary>
        /// Instantly delete persist data storage, although data model still remains on memory.
        /// </summary>
        /// <param name="storageName">persist storage name</param>
        void DeletePersistStorage(string storageName);
        /// <summary>
        /// All data and models will reload from persist storage
        /// </summary>
        /// <param name="onReloadDone"></param>
        /// <returns></returns>
        Task ReloadStorage(Action onReloadDone);
        /// <summary>
        /// This method should call when the persist storage name changed in storage config, all data models will transport to new persist storage.
        /// </summary>
        /// <param name="isDeleteOldStorage">indicate the old persist storage deleted after changes or not</param>
        /// <returns></returns>
        bool ChangePersistStorage(bool isDeleteOldStorage);
        /// <summary>
        /// The method should call each time tick of the program to process auto-save feature
        /// </summary>
        /// <param name="deltaTime">delta time between each frame to calculate auto-save timer</param>
        void StorageUpdate(float deltaTime);
        /// <summary>
        /// The all changes instantly apply to persist storage.
        /// </summary>
        void ForceSave();
    }
}