using System;
using System.Collections;
using System.Collections.Generic;

namespace GameWarriors.StorageDomain.Abstraction
{
    public interface IStorageItem
    {
        bool IsEncrypt { get; }
        string FileName { get; }
        Type DataType { get; }
        string GetDataString { get; }
        bool IsChanged { get; }
        bool IsInvalid { get; }
        void SetAsSaved();
        void Initialization();
        void ClearData();
    }
}