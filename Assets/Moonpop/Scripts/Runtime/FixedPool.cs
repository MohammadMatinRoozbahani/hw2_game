using System;
using System.Collections.Generic;
using UnityEngine;

namespace Moonpop
{
    public abstract class PoolObject : MonoBehaviour
    {
        internal Action<PoolObject> ReturnLease;
        internal bool Leased;
        public virtual void OnSpawn() { }
        public virtual void OnDespawn() { }
        public void Recycle() { if (Leased) ReturnLease(this); }
    }

                                                                                                 
    public sealed class FixedPool<T> where T : PoolObject
    {
        private readonly Queue<T> available;
        private readonly List<T> active;
        public List<T> Active => active;
        public int Capacity { get; }
        public int Peak { get; private set; }
        public int Misses { get; private set; }

        public FixedPool(int capacity, Func<int, T> factory)
        {
            Capacity = capacity;
            available = new Queue<T>(capacity);
            active = new List<T>(capacity);
            for (int i = 0; i < capacity; i++)
            {
                T item = factory(i);
                item.ReturnLease = Release;
                item.gameObject.SetActive(false);
                available.Enqueue(item);
            }
        }

        public T Rent(Vector3 position)
        {
            if (available.Count == 0) { Misses++; return null; }
            T item = available.Dequeue();
            item.transform.position = position;
            item.Leased = true;
            active.Add(item);
            Peak = Mathf.Max(Peak, active.Count);
            item.gameObject.SetActive(true);
            item.OnSpawn();
            return item;
        }

        private void Release(PoolObject lease)
        {
            T item = (T)lease;
            if (!item.Leased) return;
            item.Leased = false;
            item.OnDespawn();
            item.gameObject.SetActive(false);
            int index = active.IndexOf(item);
            if (index >= 0)
            {
                int last = active.Count - 1;
                active[index] = active[last];
                active.RemoveAt(last);
            }
            available.Enqueue(item);
        }

        public void ReturnAll()
        {
            while (active.Count > 0) active[active.Count - 1].Recycle();
        }
    }
}
