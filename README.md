# Storage System
![GitHub](https://img.shields.io/github/license/svermeulen/Extenject)

## Table Of Contents

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
<summory>

  - [Introduction](#introduction)
  - [Features](#features)
  - [Installation](#installation)
  - [Startup](#startup)
  - [How To Use](#how-to-use)
  - [Security Features](#security-features)
</summory>

## Introduction
This library provides the simple and lightweight Object Relation Mapper feature which provide persist object saving in file system. this package implemented by C# language and can use in all .NET standard environments, also this package does not use reflection library in its runtime. the package has some security feature like hashing or encryption.

Support platforms: 
* PC/Mac/Linux
* iOS
* Android
* UWP App

```text
* Note: The library may work on other platforms, the source code just used C# code and .net standard version 2.0.
        this library doesn't support WebGL because, it is using Threading.Task library.
```

* This library is design to be dependecy injection friendly, the recommended DI library is the [Dependency Injection](https://github.com/Game-Warriors/DependencyInjection-Unity3d) to be used.

```
```
This library used in the following games and apps:
</br>
[Street Cafe](https://play.google.com/store/apps/details?id=com.aredstudio.streetcafe.food.cooking.tycoon.restaurant.idle.game.simulation)
</br>
[Idle Family Adventure](https://play.google.com/store/apps/details?id=com.aredstudio.idle.merge.farm.game.idlefarmadventure)
</br>
[City Connect](https://play.google.com/store/apps/details?id=com.aredstudio.cityconnect.relax.story.connect.idle.simulation.management.country.state)
</br>
[CLC BA](https://play.google.com/store/apps/details?id=com.palsmobile.clc)

## Features
* Object relation maping using data models
* Specific Key/Value primitive value storage(string, float, int, bool)
* Dependency Injection structure and relations
* Security features like hashing and encryption
* Non blocking persistent data saving pipeline
* Loading data asynchronously
* Fully modular and configurable and ready to change and develop based on abstractions
* Auto-saving data feature
* Lightweight and simple to use

## Installation
This library can be added by unity package manager form git repository or could be downloaded.
for more information about how to install a package by unity package manager, please read the manual in this link:
[Install a package from a Git URL](https://docs.unity3d.com/Manual/upm-ui-giturl.html)

## Startup
After adding package to using the library features, following objects have to Initialize.

* StorageSystem: The library has main object “StorageSystem” which execute all storage logics like loading data to models from files, apply auto save, Key/Value storage and reloading data.

* FileHandler: The storage system depends on “IPersistDataHandler“ abstraction to provide some file operations like saving and loading.

* DefaultJsonHandler: The library needs serialization handler to act object serialization operation for file handler. this class implement “IStorageSerializationHandler“ abstraction to provide Unity3d Json serialization Utility for “FileHandler“ class.

* OldDefaultCryptographyHandler: The library has cryptography feature for storing data. for the file operation “FileHandler“ class using “ICryptographyHandler“ abstraction to fulfil the cryptography requirements. The “ICryptographyHandler“ is abstraction to provide Cryptography features, like encryption, decryption and hashing. this class provider encryption service by “RijndaelManaged” class and for hashing use “HMACSHA256” class.

* StorageConfiguration: the config class which provide some variables for “StorageSystem” operations.

    * Key: The array byte which use for hashing and encryption key, byte count should be multi of 8 and be more than 24.

    * IV: The array byte which is to input random data int start of encryption. the random value causes to be very hard to find plaintext from cypher. byte count should be multi of 8 and be more than 16.

    * StorageDataPath: The string value which system use as base directory path for saving data files. it's also could be connection string value to some storage system to use SQL Lite.

    * PersistStorageName: The name of root directory which hold all specific session data. for example, the name could be user id and contain specific data of one user. also, it could be database name of sort of SQL database.

    * SavingInterval: The time in second interval value for auto saving to apply pending changed data in persist storage.

```csharp
public class StorageConfiguration : IStorageConfig
{
    private readonly byte[] _iv;
    private readonly byte[] _key;
        
    string IStorageConfig.StorageDataPath => Application.persistentDataPath;
    byte[] IStorageConfig.Key => _key;
    byte[] IStorageConfig.IV => _iv;
    string IStorageConfig.PersistStorageName => "default";
    int IStorageConfig.SavingInterval => 15;
}
```
In order to initialize storage system, the “StorageSystem” should constructed, and other required instances should construct and passed to “StorageSystem” instance. after creating storage system, the WaitForLoading method should called. in the WaitForLoading all IO operation for loading and binding data will proceed and because this operation is blocking process, its return task object. like following example.

```csharp
private StorageSystem _storageSystem;

private void Awake()
{
    IStorageSerializationHandler jsonHandler = new DefaultJsonHandler();
    ICryptographyHandler cryptographyHandler = new OldDefaultCryptographyHandler();
    IPersistDataHandler fileHandler = new FileHandler(jsonHandler, cryptographyHandler);
    IStorageConfig storageConfig = new StorageConfiguration();
    _storageSystem = new StorageSystem(fileHandler, storageConfig);
    IStorage storageSystem = _storageSystem;
    
    IStorageOperation storageOperation = _storageSystem;
    storageOperation.LogErrorListener += LogError; 
    
    await _storageSystem.WaitForLoading();
}
```
The “StorageSystem” need some method call from program to fulfil its service.
* Runtime Update: The system has auto-save feature. the StorageUpdate method of system should call in each time tick of the program and receive delta time between each frame to calculate auto-save timer and applying changes.

* Force Save: The system will not automatically detect quitting or program process kill. by calling the system ForceSave method, all changes in data save instantly. this feature could be use in when program quitted or lose focus.

Based on dependency injection purpose the system has implemented IStorageOperation.
```csharp
private StorageSystem _storageSystem;

private void Update()
{
    IStorageOperation storageOperation = _storageSystem;
    storageOperation.StorageUpdate(Time.deltaTime);
}

private void OnApplicationFocus(bool focus)
{
    if (!focus)
    {
        IStorageOperation storageOperation = _storageSystem;
        storagOperation.ForceSave();
    }
}

private void OnApplicationQuit()
{
    IStorageOperation storageOperation = _storageSystem;
    storagOperation.ForceSave();
}
```
The IStorageOperation abstraction provide StorageUpdate, ForceSave and some more useful operation calls which describes in following.
```csharp
public interface IStorageOperation
{
    event Action<string> LogErrorListener;
    void DeletePersistStorage(string storageName);
    Task ReloadStorage(Action onReloadDone);
    bool ChangePersistStorage(bool isDeleteOldStorage);
    void StorageUpdate(float deltaTime);
    void ForceSave();
}
```
* LogErrorListener : The event will trigger when error raised in storage system.

* DeletePersistStorage: Instantly delete persist data storage, although data model still remains on memory.

* ReloadStorage: All data and models will reload from persist storage.

* ChangePersistStorage: This method should call when the persist storage name changed in storage config, all data models will transport to new persist storage.

## How To Use
After system setup, the data model should be defined to register in storage system for next step. the data model should be a class object and derived from “IStorageItem“ abstraction.

Next step, the data mode should register in system. so, system will detect data object and its type then bind data to registered data model. the “IStorage“ abstraction is created for accessing the storage system features which is registering data models.
```csharp
public sealed class AppService : IAppService
{
    private AppServiceDataModel _dataModel;
    
    public AppService(IStorage storage)
    {
        storage.LoadingModelAsync<AppServiceDataModel>(AppServiceDataModel.FILE_NAME, isEncrypt: false, 
        input => _dataModel = input);
    }
}
```
It’s better to register data models in storage abstraction before WaitForLoading call, because after that, the system assures all data models loaded and ready to use.

```text
* Note: It’s important to consider that all data queries should be call after loading storage done.
```

```csharp
private void Awake()
{
    //Storage Intitialize
    new AppService(_storageSystem);
    await _storageSystem.WaitForLoading();//wait to all data loaded
    //Use data and models whatever you want
    //Query data from storage
}
```
There is other overload for loading data model which return data by Task class and could be use by await operator. but it is not use in construer because it could bypass storage system wait for loading pipeline and cause malfunction of system.


<h3>Working With Data-Model</h3>

The data models are major part of the system which system interact with those and read and bind data in corresponding. the data model should have fields or properties which the system serializes those and put them in persist storage. each data model must have unique name for identification.

The code routine process may change the data model data values and after that the IsChanged property value should set “True”. this property stays true until storage system applies changes on persist storage and making the property value false. the IsChanged is changing on main-thread and free to changes.
```csharp
public class AppServiceDataModel : IStorageItem
{
    public const string FILE_NAME = "AppDataModel.bin";
    [SerializeField] private string _serialNumber;
    [SerializeField] private string _connectionIP;
    [SerializeField] private string _connectionPort;
    
    public string SerialNumber { get => _serialNumber; set { _serialNumber = value; IsChanged = true; } }
    public string ConnectionIP { get => _connectionIP; set { _connectionIP = value; IsChanged = true; } }
    public string ConnectionPort { get => _connectionPort; set { _connectionPort = value; IsChanged = true; } }
        
    public bool IsEncrypt => false;
    public string ModelName => FILE_NAME;
    public Type DataType => typeof(AppServiceDataModel);
    public bool IsChanged { get; set; }
    public bool IsInvalid => false;
}
```

```text
* Note: The serialization attribute and other details is depending on the data serializer which is using in project,
        the default data serializer in storage system is unity3D Json serializer.
```

<h3>Working With Key/Values</h3>
The storage system has feature to persist save the 4 primitive value type: string, float, int, bool. this feature may be alternative to Unity “PlayerPrefs“ class. the Key/Value data will save on file in default storage system. the key/value doesn't have any security protection and it could be expose by user cheat.

```csharp
private void ChangeState(IStorage storage)
{
    int state = storage.GetValue<int>("appState");
    if (state == 1)
    {
       storage.SetValue<int>("appState", 9);
    }
}
```

## Security Features
The storage system utilize encryption and hashing feature to protect the data from modification outside system.

* Hashing: the system uses the hashing data base on the model data content to validate the consistency on change out of system process. system use [HMACSHA256](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.hmacsha256?view=netframework-4.7.1) is a type of keyed hash algorithm. the Hash Key passed to system by Key property in “IStorageConfig“ abstraction. the key length should be at least 24 bytes.

* Encryption: the encryption is using in system to prevent file contents from being access, monitoring and to be exposed. the default encryption is applying to data in “FileHandler” class, and it is using symmetric algorithm, the default symmetric in system is AES which use the Key and [IV](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.symmetricalgorithm.iv?view=netframework-4.7.1#system-security-cryptography-symmetricalgorithm-iv).