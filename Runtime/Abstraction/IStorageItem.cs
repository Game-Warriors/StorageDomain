using System;

namespace GameWarriors.StorageDomain.Abstraction
{
    /// <summary>
    /// The base abstraction for storage item which identify as data models
    /// </summary>
    public interface IStorageItem
    {
        /// <summary>
        /// Indicate data saving by encrypted state in persist storage.
        /// </summary>
        bool IsEncrypt { get; }

        /// <summary>
        /// The unique name of data model which is identifier for storage system.
        /// </summary>
        string ModelName { get; }

        /// <summary>
        /// The actual type of the base class which implement interface
        /// </summary>
        Type DataType { get; }

        /// <summary>
        /// Is data has change and should be apply in persist storage
        /// </summary>
        bool IsChanged { get; }

        /// <summary>
        /// Is data is invalid and should re initialize by storage system
        /// </summary>
        bool IsInvalid { get; }

        /// <summary>
        /// Trigger by storage system when applied to persist storage
        /// </summary>
        void SetAsSaved();

        /// <summary>
        /// Trigger when the persist data for data model not exist and system initialized the data model for first time.
        /// </summary>
        void Initialization();

        /// <summary>
        /// Trigger when the system clear the data from persist storage.
        /// </summary>
        void ClearData();
    }
}