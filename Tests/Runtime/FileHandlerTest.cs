using GameWarriors.StorageDomain.Abstraction;
using GameWarriors.StorageDomain.Core;
using GameWarriors.StorageDomain.Tests;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class FileHandlerTest
{
    private readonly byte[] _key = new byte[] { 4, 5, 3, 18, 65, 10, 15, 55, 63, 12, 25, 94, 116, 83, 17, 57, 50, 36, 45, 75, 14, 28, 13, 119 };
    private readonly byte[] _iv = Encoding.ASCII.GetBytes(SystemInfo.deviceUniqueIdentifier.Substring(0, 16));

    [Test]
    public void SaveEncryptedFileTest()
    {
        ICryptographyHandler cryptographyHandler = new OldDefaultCryptographyHandler();
        IStorageSerializationHandler jsonHandler = new DefaultJsonHandler();
        IPersistDataHandler persistDataHandler = new FileHandler(jsonHandler, cryptographyHandler);
        FakeDataModel3 fakeDataModel3 = new FakeDataModel3();
        persistDataHandler.SaveEncryptedData<FakeDataModel3>(fakeDataModel3, Encoding.UTF8, fakeDataModel3.ModelName, _key, _iv);
    }

    [Test]
    public void NewCryptographyTest()
    {

    }
}
