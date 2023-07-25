using System;

namespace GameWarriors.StorageDomain.Abstraction
{
    /// <summary>
    /// The base abstraction to provide serialization and deserialization features.
    /// </summary>
    public interface IStorageSerializationHandler
    {
        T Deserialize<T>(string dataString);
        object Deserialize(string tmp, Type dataType);
        string Serialize(object input);
    }
}
