using System.IO;
using System.Threading.Tasks;

namespace GameWarriors.StorageDomain.Abstraction
{
    /// <summary>
    /// The base abstraction to provide Cryptography features, like encryption, decryption and hashing.
    /// </summary>
    public interface ICryptographyHandler
    {
        byte[] DecryptData(Stream stream, byte[] key, byte[] iv);
        Task<byte[]> DecryptDataAsync(Stream stream, byte[] key, byte[] iv);
        void EncryptStream(byte[] inputData, byte[] key, byte[] iv, Stream outputStream);
        byte[] HashInput(string input, byte[] key);
        string HashInputString(string dataString, byte[] hashKey);
    }
}