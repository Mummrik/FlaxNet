using System;
using System.Collections.Generic;
using FlaxEngine;

namespace Game
{
    public class ObjectPool<T> where T : class
    {
        protected Queue<T> pool = new Queue<T>();

        protected T Get() => pool.Count > 0 ? pool.Dequeue() : null;
        protected T Rent() => Get();
        protected void Return(T obj) => pool.Enqueue(obj);
    }
}
