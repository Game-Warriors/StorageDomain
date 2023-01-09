using System;
using System.Threading.Tasks;

namespace GameWarriors.StorageDomain.Abstraction
{
    public interface IStorageOperations
    {
        event Action<string> LogErrorListener;
        void DeleteDefaultDirectory(string defaultPerfix);
        Task ReloadFileDirectoy(Action onReloadDone);
        bool ChangeFileDirectionPrefix(bool isDeleteOldFiles);
        void StorageUpdate(float deltaTime);
        void ForceSave();
    }
}