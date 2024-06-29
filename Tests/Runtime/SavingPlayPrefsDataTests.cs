using GameWarriors.StorageDomain.Abstraction;
using GameWarriors.StorageDomain.Core;
using GameWarriors.StorageDomain.Data;
using System.Collections;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace GameWarriors.StorageDomain.Tests
{
    public class SavingPlayPrefsDataTests
    {
        private DefaultDataModel<FakeStorageDataItem> _dataModel1;
        private DefaultDataModel<FakeStorageDataItem> _dataModel2;

        [UnityTest]
        public IEnumerator SingleDataLoading()
        {
            PlayPrefsStorageSystem storage = InitSystem();
            LoadingData1(storage);
            _dataModel1.UpdateItem(new FakeStorageDataItem(1), true);
            yield return null;
            (storage as IStorageOperation).ForceSave();

            PlayPrefsStorageSystem storage2 = InitSystem();
            LoadingData2(storage2);
            Assert.AreNotEqual(_dataModel2, null);
            FakeStorageDataItem item = _dataModel2.FindItem(input => input.Id == 1);
            Assert.AreNotEqual(item, null);
        }


        private void LoadingData1(IStorage storage)
        {
            storage.LoadingDefaultModelAsync<FakeStorageDataItem, DefaultDataModel<FakeStorageDataItem>>("fakeDefault", false,
                (input) => { _dataModel1 = input; _dataModel1.SetFileName("fakeDefault"); });
        }

        private void LoadingData2(IStorage storage)
        {
            storage.LoadingDefaultModelAsync<FakeStorageDataItem, DefaultDataModel<FakeStorageDataItem>>("fakeDefault", false,
                (input) => { _dataModel2 = input; _dataModel2.SetFileName("fakeDefault"); });
        }

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
