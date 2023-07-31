using System;
using System.Text;
using System.Threading.Tasks;

namespace GameWarriors.StorageDomain.Abstraction
{
    /// <summary>
    /// The base abstraction to provide some presiste data operations like saving and loading.
    /// </summary>
    public interface IPersistDataHandler
    {
        string LoadStringData(string path, byte[] key);
        Task<string> LoadStringDataAsync(string path, byte[] key);
        bool SaveStringData(string data, string path, byte[] key);
        bool SaveData<T>(T data, string path);
        bool SaveData<T>(T data, string path, byte[] key);
        Task<bool> SaveDataAsync<T>(T data, string path, Action onDone = null);
        Task<bool> SaveDataAsync<T>(T data, string path, byte[] key, Action onDone = null);
        (bool, T) LoadData<T>(string path, byte[] key);
        Task<(bool, T)> LoadDataAsync<T>(string path);
        Task<(bool, object)> LoadDataAsync(string path, byte[] key, Type dataType);
        Task<(bool, T)> LoadDataAsync<T>(string path, byte[] key);
        void DeleteData(string path);
        Task<(bool, T)> LoadEncryptedDataAsync<T>(Encoding encoding, string path, byte[] key, byte[] iv);
        Task<(bool, object)> LoadEncrypteDataAsync(Encoding encoding, string path, byte[] key, byte[] iv, Type dataType);
        (bool, T) LoadEncryptedData<T>(Encoding encoding, string path, byte[] key, byte[] iv);
        bool SaveEncryptedData<T>(T wordSource, Encoding encoding, string path, byte[] key, byte[] iv);
        bool SaveEncryptedStringData(string data, Encoding encoding, string path, byte[] key, byte[] iv);
    }
}