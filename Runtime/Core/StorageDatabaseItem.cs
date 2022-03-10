using System;
using System.Collections;
using System.Collections.Generic;

namespace GameWarriors.StorageDomain.Core
{
    public class StorageDatabaseItem : Dictionary<string, IConvertible>
    {
        private const string DATABASE_FILE_NAME = "DatabaseFile.bin";

        //private bool _isDataChange;
        //private string _databaseFilePath;

        //public bool IsDataChanged => _isDataChange;
        //public string DatabaseFilePath => _databaseFilePath;

        //public void SetDatabaseFilePath(string fileRoot, Type dataBaseType)
        //{
        //    _databaseFilePath = $"{fileRoot}{dataBaseType}{DATABASE_FILE_NAME}";
        //}

        //public void SetAsSaved()
        //{
        //    _isDataChange = false;
        //}

        //public void SetAsChanged()
        //{
        //    _isDataChange = true;
        //}
    }
}
