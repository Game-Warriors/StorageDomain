using System;

namespace GameWarriors.StorageDomain.Data
{
#if UNITY_2018_4_OR_NEWER
    using UnityEngine;

    [Serializable]
    public struct LongData
    {
        public const ulong VALUE_FILTER = ulong.MaxValue << 3;

        [SerializeField]
        private ulong data;

        public ulong DataValue
        {
            get { return (data & VALUE_FILTER) >> 3; }
            set { data = ((value << 3) & VALUE_FILTER) | (data & ~VALUE_FILTER); data |= (uint)UnityEngine.Random.Range(1, 8);  }
        }
    }
#endif
}