using GameWarriors.StorageDomain.Abstraction;
using UnityEngine;

namespace GameWarriors.StorageDomain.Tests
{
    [System.Serializable]
    internal class FakeStorageDataItem : IStorageDataItem
    {
        [SerializeField]
        private int id;

        public int Id => id;

        public FakeStorageDataItem()
        {
            id = 0;
        }

        public FakeStorageDataItem(int id)
        {
            this.id = id;
        }

        public bool Equals(IStorageDataItem other)
        {
            FakeStorageDataItem dataItem = other as FakeStorageDataItem;
            return id == dataItem.id;
        }
    }
}
