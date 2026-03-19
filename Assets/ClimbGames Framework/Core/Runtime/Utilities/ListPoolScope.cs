using System;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace ClimbGames.Core
{
    public struct ListPoolScope<T> : IDisposable
    {
        private List<T> value;

        public ListPoolScope(out List<T> pool)
        {
            value = ListPool<T>.Get();
            pool = value;
        }

        public ListPoolScope(out List<T> pool, IEnumerable<T> initializeData, bool dataReverse = false)
        {
            value = ListPool<T>.Get();
            pool = value;

            pool.AddRange(initializeData);

            if (dataReverse)
                pool.Reverse();
        }

        public void Dispose()
        {
            ListPool<T>.Release(value);
        }
    }
}