using System;
using System.Collections.Generic;
using UnityEngine;

namespace ClimbGames
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [Serializable]
        public struct Pair
        {
            public TKey Key;
            public TValue Value;

            public Pair(TKey key, TValue value)
            {
                Key = key;
                Value = value;
            }
        }

        [SerializeField] private List<Pair> list = new List<Pair>();
        private bool _isDirty = false;

        public new TValue this[TKey key]
        {
            get => base[key];
            set
            {
                base[key] = value;
                _isDirty = true;
            }
        }

        public new void Add(TKey key, TValue value)
        {
            base.Add(key, value);
            _isDirty = true;
        }

        public new bool Remove(TKey key)
        {
            if (base.Remove(key))
            {
                _isDirty = true;
                return true;
            }
            return false;
        }

        public void OnBeforeSerialize()
        {
            if (_isDirty == false)
                return;

            list.Clear();
            foreach (var pair in this)
                list.Add(new Pair(pair.Key, pair.Value));

            _isDirty = false;
        }

        public void OnAfterDeserialize()
        {
            Clear();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Key != null)
                {
                    // 인스펙터에서 중복 키가 있을 경우 에러 방지를 위해 덮어쓰기 허용
                    this[list[i].Key] = list[i].Value;
                }
            }
            _isDirty = false;
        }
    }
}