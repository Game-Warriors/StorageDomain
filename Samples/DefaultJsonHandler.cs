using GameWarriors.StorageDomain.Abstraction;
using System;

namespace Management.Handlers.Json
{
#if UNITY_2018_4_OR_NEWER
    using UnityEngine;
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
#endif
}