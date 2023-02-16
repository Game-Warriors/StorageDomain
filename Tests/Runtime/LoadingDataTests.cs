using GameWarriors.StorageDomain.Abstraction;
using GameWarriors.StorageDomain.Core;
using NUnit.Framework;
using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;

namespace GameWarriors.StorageDomain.Tests
{
    public class DefaultJsonHandler : IStorageJsonHandler
    {
        public T FromJson<T>(string dataString)
        {
            return JsonUtility.FromJson<T>(dataString);
        }

        public object FromJson(string tmp, Type dataType)
        {
            return JsonUtility.FromJson(tmp, dataType);
        }

        public string ToJson(object input)
        {
            return JsonUtility.ToJson(input);
        }
    }



    public class LoadingDataTests
    {
        private FakeDataModel _dataModel1;
        private FakeDataModel2 _dataModel2;

        [UnityTest]
        public IEnumerator SingleDataLoading()
        {
            StorageSystem storage = InitSystem();
            LoadingData1(storage);
            Task task = storage.WaitForLoading();
            yield return new WaitUntil(() => task.IsCompleted);
            Assert.AreNotEqual(_dataModel1, null);
        }

        [UnityTest]
        public IEnumerator DoubleDataLoading()
        {
            StorageSystem storage = InitSystem();
            LoadingData2(storage);
            Task task = storage.WaitForLoading();
            yield return new WaitUntil(() => task.IsCompleted);
            Assert.AreNotEqual(_dataModel1, null);
            Assert.AreNotEqual(_dataModel2, null);
            storage.ForceSave();
        }

        private void LoadingData1(IStorage storage)
        {
            storage.LoadingModelAsync<FakeDataModel>(FakeDataModel.FILE_NAME, false, (input) => _dataModel1 = input);
        }

        private void LoadingData2(IStorage storage)
        {
            storage.LoadingModelAsync<FakeDataModel>(FakeDataModel.FILE_NAME, false, (input) => _dataModel1 = input);
            storage.LoadingModelAsync<FakeDataModel2>(FakeDataModel2.FILE_NAME, false, (input) => _dataModel2 = input);
        }

        private StorageSystem InitSystem()
        {
            IStorageJsonHandler jsonHandler = new DefaultJsonHandler();
            IFileHandler fileHandler = new FileHandler(jsonHandler);
            IStorageConfig storageConfig = new FakeStorageConfig();
            StorageSystem storage = new StorageSystem(fileHandler, storageConfig);
            return storage;
        }
    }
}
