using System;

namespace GameWarriors.StorageDomain.Abstraction
{
    public interface IStorageJsonHandler
    {
        T FromJson<T>(string dataString);
        string ToJson(object input);
        object FromJson(string tmp, Type dataType);
    }
}
