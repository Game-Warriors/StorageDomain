using System;

namespace GameWarriors.StorageDomain.Abstraction
{
    public interface IStorageEventHandler
    {
        void LogErrorEvent(string message);
        void SetStorageUpdate(Action<float> update);
        void SetForceSaveEvent(Action forceSaveing);
    }
}