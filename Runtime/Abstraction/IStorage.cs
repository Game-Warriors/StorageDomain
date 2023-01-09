using System;
using System.Threading.Tasks;

namespace GameWarriors.StorageDomain.Abstraction
{
    public interface IStorage
    {
        string FileRoot { get; }
        Task<T> LoadingModelAsync<T>(string dataName, bool isEncrypt) where T : IStorageItem ,new();
        Task<U> LoadingDefaultModelAsync<T, U>(string dataName, bool isEncrypt) where T : IStorageDataItem, new() where U : IDefaultDataModel, new();
        T1 GetValue<T1>(string key, T1 defualtValue = default) where T1 : IConvertible;
        void SetValue<T1>(string key, T1 data) where T1 : IConvertible;
        void DeleteValue<T1>(string key);
    }
}
