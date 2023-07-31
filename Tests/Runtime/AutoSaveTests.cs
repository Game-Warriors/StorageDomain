using GameWarriors.StorageDomain.Abstraction;
using GameWarriors.StorageDomain.Core;
using NUnit.Framework;
using System.Collections;
using System.Threading.Tasks;
#if UNITY_2018_4_OR_NEWER
using UnityEngine;
using UnityEngine.TestTools;

namespace GameWarriors.StorageDomain.Tests
{

    public class AutoSaveTests
    {
        private FakeDataModel _dataModel1;
        private FakeDataModel2 _dataModel2;
        private FakeDataModel3 _dataModel3;
        private StorageSystem InitSystem()
        {
            IStorageSerializationHandler jsonHandler = new DefaultJsonHandler();
            IPersistDataHandler fileHandler = new FileHandler(jsonHandler, null);
            IStorageConfig storageConfig = new FakeStorageConfig();
            StorageSystem storage = new StorageSystem(fileHandler, storageConfig);
            return storage;
        }


        [UnityTest]
        public IEnumerator SingleAutoSave()
        {
            StorageSystem storage = InitSystem();
            LoadingData(storage);
            Task task = storage.WaitForLoading();
            yield return new WaitUntil(() => task.IsCompleted);
            _dataModel1.ApplyChange();
            _dataModel2.ApplyChange();
            IStorageOperation storageOperations = storage as IStorageOperation;
            storageOperations.StorageUpdate(100);
            Assert.AreEqual(_dataModel1.IsChanged, false);
            Assert.AreEqual(_dataModel2.IsChanged, true);
        }

        [UnityTest]
        public IEnumerator MultiAutoSave()
        {
            StorageSystem storage = InitSystem();
            LoadingData(storage);
            Task task = storage.WaitForLoading();
            yield return new WaitUntil(() => task.IsCompleted);
            _dataModel1.ApplyChange();
            _dataModel2.ApplyChange();
            IStorageOperation storageOperations = storage as IStorageOperation;
            storageOperations.StorageUpdate(100);
            storageOperations.StorageUpdate(100);
            Assert.AreEqual(_dataModel1.IsChanged, false);
            Assert.AreEqual(_dataModel2.IsChanged, false);
        }

        private void LoadingData(IStorage storage)
        {
            storage.LoadingModelAsync<FakeDataModel>(FakeDataModel.FILE_NAME, false, (input) => _dataModel1 = input);
            storage.LoadingModelAsync<FakeDataModel2>(FakeDataModel2.FILE_NAME, false, (input) => _dataModel2 = input);
            storage.LoadingModelAsync<FakeDataModel3>(FakeDataModel3.FILE_NAME, false, (input) => _dataModel3 = input);
        }
    }
}
#endif
