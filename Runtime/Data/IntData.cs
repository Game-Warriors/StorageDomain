using System;

namespace GameWarriors.StorageDomain.Data
{
#if UNITY_2018_4_OR_NEWER
    using UnityEngine;
    using Random = System.Random;

    [Serializable]
    public struct IntData
    {
        private static readonly Random RANDOM = new System.Random();
        //private const uint VALUE_FILTER = 4294967292;//1073741823‬ << 2;
        public const uint VALUE_FILTER = uint.MaxValue << 3;

        [SerializeField]
        private uint data;

        public uint DataValue
        {
            get { return (data & VALUE_FILTER) >> 3; }
            set { data = ((value << 3) & VALUE_FILTER) | (data & ~VALUE_FILTER); data |= (uint) RANDOM.Next(1,8); }
        }
    }
#endif
}