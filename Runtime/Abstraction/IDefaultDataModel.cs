using System;
using System.Collections.Generic;

namespace GameWarriors.StorageDomain.Abstraction
{
    public interface IDefaultDataModel : IStorageItem
    {
        int DataCount { get; }
        void SetFileName(string fileName);

        //void UpdateItem(T item, bool isAdd = true);
        //bool RemoveItem(T item);
        //bool IsItemExist(T item);
        //T FindItem(Predicate<T> predicate);
        //public IEnumerable<T> GetAllItems();
        //IEnumerable<IStorageDataItem> GetAllItems();
    }
}
