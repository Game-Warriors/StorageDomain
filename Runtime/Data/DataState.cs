using System;

namespace GameWarriors.StorageDomain.Data
{
#if UNITY_2018_4_OR_NEWER
    using UnityEngine;
    [Serializable]
    public struct DataState
    {
        private const ulong PART1_FILTER = 4294967292;//(long)1073741823<<2‬;
        private const ulong PART2_FILTER = (ulong)4294967292 << 32;//(long)1073741823<<2‬;
        [SerializeField]
        private ulong data;

        public ulong LastValue
        {
            get { return (data & PART1_FILTER) >> 2; }
            set { data = ((value << 2) & PART1_FILTER) | (data & ~PART1_FILTER); }
        }

        public ulong NewValue
        {
            get { return (data & PART2_FILTER) >> 34; }
            set { data = ((value << 34) & PART2_FILTER) | (data & ~PART2_FILTER); }
        }

        public DataState(uint newValue, uint lastValue)
        {
            data = 0;
            NewValue = newValue;
            LastValue = lastValue;
        }
    }
#endif
}
