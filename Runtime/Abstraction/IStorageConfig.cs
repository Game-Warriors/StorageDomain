namespace GameWarriors.StorageDomain.Abstraction
{
    public interface IStorageConfig
    {
        int SaveingInterval { get; } //15 sec
        byte[] Key { get; }// atleast 24 byte
        byte[] IV { get; }// atleast 16 byte
        string DirectoryPrefix { get; } // default "0"
        string StorageDataPath { get; } //Application.persistentDataPath
    }
}