using GameWarriors.StorageDomain.Abstraction;
using System;
using UnityEngine;

namespace GameWarriors.StorageDomain.Tests
{
    internal class FakeDataModel3 : IStorageItem
    {
        internal const string FILE_NAME = "fake3.bin";
        [SerializeField]
        private string _name;
        [SerializeField]
        private int _value;
        [SerializeField]
        private float _distance;
        [SerializeField]
        private double _maxDistance;

        public bool IsEncrypt => false;

        public string FileName => FILE_NAME;

        public Type DataType => typeof(FakeDataModel);

        public string GetDataString => JsonUtility.ToJson(this);

        public bool IsChanged => true;

        public bool IsInvalid => false;

        public void ClearData()
        {

        }

        public void Initialization()
        {
            _name = "mahdi";
            _value = 10;
            _maxDistance = 1015.13135151d;
            _distance = 99.9f;

        }

        public void SetAsSaved()
        {

        }
    }
}
