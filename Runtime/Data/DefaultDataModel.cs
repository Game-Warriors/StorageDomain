using GameWarriors.StorageDomain.Abstraction;
using System;
using System.Collections.Generic;

namespace GameWarriors.StorageDomain.Data
{
#if UNITY_2018_4_OR_NEWER
    using UnityEngine;

    [Serializable]
    public class DefaultDataModel<T> : IDefaultDataModel where T : IStorageDataItem
    {
        [SerializeField]
        private int _itemCounter;
        [SerializeField]
        private T[] _items;

        private Type _classType;

        public bool IsEncrypt { get; private set; }

        public string ModelName { get; private set; }

        public Type DataType
        {
            get
            {
                if (_classType == null)
                    _classType = typeof(DefaultDataModel<T>);
                return _classType;
            }
        }


        public bool IsChanged { get; private set; }

        public bool IsInvalid => _items == null;

        public int DataCount => _itemCounter;

        public event Action<Type, T> OnItemAdd;
        public event Action<Type, T> OnItemUpdate;
        public event Action<Type, T> OnItemDelete;


        public void SetAsSaved()
        {
            IsChanged = true;
        }

        public void SetAsChange()
        {
            IsChanged = true;
        }

        public void Initialization()
        {
            _items = new T[10];
            _itemCounter = 0;
        }

        public void SetFileName(string fileName)
        {
            ModelName = fileName;
        }

        public void UpdateItem(T item, bool isAdd = true)
        {
            if (_itemCounter >= _items.Length)
            {
                Array.Resize(ref _items, _itemCounter + 10);
            }

            int index = FindItem(item);
            if (index == -1)
            {
                if (isAdd)
                {
                    _items[_itemCounter] = item;
                    ++_itemCounter;
                    SetAsChange();
                    OnItemAdd?.Invoke(DataType, item);
                }
            }
            else
            {
                _items[index] = item;
                SetAsChange();
                OnItemUpdate?.Invoke(DataType, item);
            }
        }

        public bool RemoveItem(T item)
        {
            if (_itemCounter == 0)
                return false;

            int index = FindItem(item);
            if (index > -1)
            {
                var targetItem = _items[index];
                --_itemCounter;
                if (_itemCounter != index)
                {
                    _items[index] = _items[_itemCounter];
                }
                else
                {
                    _items[index] = default;
                }
                SetAsChange();
                OnItemDelete?.Invoke(DataType, targetItem);
                return true;
            }
            return false;
        }

        public bool IsItemExist(T item)
        {
            return FindItem(item) > -1;
        }

        public IEnumerable<T> GetAllItems()
        {
            int length = _itemCounter;
            for (int i = 0; i < length; ++i)
            {
                yield return _items[i];
            }
        }

        private int FindItem(T item)
        {
            int length = _itemCounter;
            for (int i = 0; i < length; ++i)
            {

                if (_items[i].Equals(item))
                {
                    return i;
                }
            }
            return -1;
        }

        public T FindItem(Predicate<T> predicate)
        {
            int length = _itemCounter;
            for (int i = 0; i < length; ++i)
            {
                if (predicate?.Invoke(_items[i]) ?? false)
                {
                    return _items[i];
                }
            }
            return default;
        }

        public void ClearData()
        {
            foreach (T item in _items)
            {
                OnItemDelete?.Invoke(DataType, item);
            }

            SetAsChange();
        }
    }
#endif
}