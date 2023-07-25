namespace GameWarriors.StorageDomain.Abstraction
{
    /// <summary>
    /// The config class which provide some usefull data values for Storage System.
    /// </summary>
    public interface IStorageConfig
    {
        /// <summary>
        /// The time in second interval value for auto saving to apply pending changed data in persist storage.
        /// default value could be 15 seconds.
        /// </summary>
        int SavingInterval { get; }
        /// <summary>
        /// The array byte which use for hashing and encryption key. byte count should be multi of 8 and be more than 24.
        /// </summary>
        byte[] Key { get; }
        /// <summary>
        /// The array byte which is to input random data int start of encryption. byte count should be multi of 8 and be more than 16.
        /// </summary>
        byte[] IV { get; }// atleast 16 byte
        /// <summary>
        /// The name of root place which hold spcific related data, like for data of one user, it could be user id
        /// defaul value could be "0"
        /// </summary>
        string PersistStorageName { get; }
        /// <summary>
        /// The string value which system use as base path for saving data, like Application.persistentDataPath
        /// </summary>
        string StorageDataPath { get; }
    }
}