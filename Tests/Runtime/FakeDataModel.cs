using GameWarriors.StorageDomain.Abstraction;
using System;
using UnityEngine;

namespace GameWarriors.StorageDomain.Tests
{
    internal class FakeDataModel : IStorageItem
    {
        internal const string FILE_NAME = "fake.bin";
        [SerializeField]
        private string _name;
        [SerializeField]
        private int _value;
        [SerializeField]
        private float _distance;
        [SerializeField]
        private double _maxDistance;

        public bool IsEncrypt => false;

        public string ModelName => FILE_NAME;

        public Type DataType => typeof(FakeDataModel);


        public bool IsChanged { get; private set; }

        public bool IsInvalid => false;

        public void ClearData()
        {

        }

        public void Initialization()
        {
            Debug.Log("alooooooo");
            _name = "mahdi";
            _value = 10;
            _maxDistance = 1015.13135151d;
            _distance = 99.9f;
            IsChanged = true;
        }

        public void SetAsSaved()
        {
            IsChanged = false;
        }

        public void ApplyChange()
        {
            IsChanged = true;
        }
    }
}
