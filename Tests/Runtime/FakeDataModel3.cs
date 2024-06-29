using GameWarriors.StorageDomain.Abstraction;
using System;
using UnityEngine;

namespace GameWarriors.StorageDomain.Tests
{
    internal class FakeDataModel3 : IStorageItem
    {
        internal const string FILE_NAME = "fake3.bin";
        [SerializeField]
        private string _userId;
        [SerializeField]
        private string _forceVersion;
        [SerializeField]
        private string _newVersion;
        [SerializeField]
        private bool _newFlag;

        public bool IsEncrypt => false;

        public string ModelName => FILE_NAME;

        public Type DataType => typeof(FakeDataModel);


        public bool IsChanged => true;

        public bool IsInvalid => false;

        public void ClearData()
        {

        }

        public void Initialization()
        {
            _userId = Guid.NewGuid().ToString();
            _forceVersion = Application.version;
            _newVersion = Application.version;
            _newFlag = true;

        }

        public void SetAsSaved()
        {

        }
    }
}
