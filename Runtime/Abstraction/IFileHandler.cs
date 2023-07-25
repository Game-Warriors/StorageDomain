using System;
using System.Text;
using System.Threading.Tasks;

namespace GameWarriors.StorageDomain.Abstraction
{
    /// <summary>
    /// The base abstraction to provide some file operations like saving and loading.
    /// </summary>
    public interface IFileHandler
    {
        event Action<string> LogErrorListener;
        string LoadDataStringFile(string path, byte[] key);
        Task<string> LoadDataStringFileAsync(string path, byte[] key);
        bool SaveDataStringFile(string data, string path, byte[] key);
        bool SaveFile<T>(T data, string path);
        bool SaveFile<T>(T data, string path, byte[] key);
        Task<bool> SaveFileAsync<T>(T data, string path, Action onDone = null);
        Task<bool> SaveFileAsync<T>(T data, string path, byte[] key, Action onDone = null);
        (bool, T) LoadFile<T>(string path, byte[] key);
        Task<(bool, T)> LoadFileAsync<T>(string path);
        Task<(bool, object)> LoadFileAsync(string path, byte[] key, Type dataType);
        Task<(bool, T)> LoadFileAsync<T>(string path, byte[] key);
        void DeleteFile(string path);
        Task<(bool, T)> LoadEncryptedFileAsync<T>(Encoding encoding, string path, byte[] key, byte[] iv);
        Task<(bool, object)> LoadEncryptedFileAsync(Encoding encoding, string path, byte[] key, byte[] iv, Type dataType);
        (bool, T) LoadEncryptedFile<T>(Encoding encoding, string path, byte[] key, byte[] iv);
        bool SaveEncryptedFile<T>(T wordSource, Encoding encoding, string path, byte[] key, byte[] iv);
        bool SaveEncryptedStringFile(string data, Encoding encoding, string path, byte[] key, byte[] iv);
    }
}