using GameWarriors.StorageDomain.Abstraction;
using GameWarriors.StorageDomain.Core;
using GameWarriors.StorageDomain.Data;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace GameWarriors.StorageDomain.Tests
{
    public class LoadingPlayPrefsDataTests
    {
        private DefaultDataModel<FakeStorageDataItem> _dataModel1;

        [UnityTest]
        public IEnumerator SingleDataLoading()
        {
            PlayPrefsStorageSystem storage = InitSystem();
            LoadingData1(storage);
            yield return null;
            Assert.AreNotEqual(_dataModel1, null);
        }

        private void LoadingData1(IStorage storage)
        {
            storage.LoadingDefaultModelAsync<FakeStorageDataItem, DefaultDataModel<FakeStorageDataItem>>("fakeDefault", false,
                (input) => { _dataModel1 = input; _dataModel1.SetFileName("fakeDefault"); });
        }

        //private void LoadingData2(IStorage storage)
        //{
        //    storage.LoadingModelAsync<FakeDataModel>(FakeDataModel.FILE_NAME, false, (input) => _dataModel1 = input);
        //    storage.LoadingModelAsync<FakeDataModel2>(FakeDataModel2.FILE_NAME, false, (input) => _dataModel2 = input);
        //}

        private PlayPrefsStorageSystem InitSystem()
        {
            IStorageSerializationHandler jsonHandler = new DefaultJsonHandler();
            ICryptographyHandler cryptographyHandler = new NewDefaultCryptographyHandler();
            IPersistDataHandler dataHandler = new PlayPrefsPersistStorage(jsonHandler, cryptographyHandler);
            IStorageConfig storageConfig = new FakeStorageConfig();
            PlayPrefsStorageSystem storage = new PlayPrefsStorageSystem(dataHandler, storageConfig);
            return storage;
        }
    }
}