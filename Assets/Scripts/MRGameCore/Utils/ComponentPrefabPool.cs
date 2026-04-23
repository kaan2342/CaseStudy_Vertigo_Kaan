using System.Collections.Generic;
using UnityEngine;

namespace MRGameCore.Utils
{
    public interface IPoolCallbacks
    {
        void OnTakenFromPool();
        void OnReturnedToPool();
    }

    public sealed class ComponentPrefabPool<T> where T : Component
    {
        private readonly Queue<T> availableInstances = new Queue<T>();
        private readonly HashSet<T> rentedInstances = new HashSet<T>();
        private readonly T prefab;
        private readonly Transform storageRoot;

        public ComponentPrefabPool(T prefab, Transform storageRoot, int preloadCount = 0)
        {
            if (prefab == null)
            {
                throw new System.ArgumentNullException(nameof(prefab));
            }

            this.prefab = prefab;
            this.storageRoot = storageRoot;
            if (preloadCount > 0)
            {
                Prewarm(preloadCount);
            }
        }

        public T Prefab => prefab;

        public int AvailableCount => availableInstances.Count;

        public int RentedCount => rentedInstances.Count;

        public T Get(Transform parent = null, bool worldPositionStays = false)
        {
            var instance = availableInstances.Count > 0 ? availableInstances.Dequeue() : CreateInstance();
            rentedInstances.Add(instance);

            if (parent != null && instance.transform.parent != parent)
            {
                instance.transform.SetParent(parent, worldPositionStays);
            }

            instance.gameObject.SetActive(true);
            NotifyTaken(instance);
            return instance;
        }

        public void Release(T instance, bool worldPositionStays = false)
        {
            if (instance == null || !rentedInstances.Remove(instance))
            {
                return;
            }

            NotifyReturned(instance);

            if (storageRoot != null && instance.transform.parent != storageRoot)
            {
                instance.transform.SetParent(storageRoot, worldPositionStays);
            }

            instance.gameObject.SetActive(false);
            availableInstances.Enqueue(instance);
        }

        public void ReleaseAll(IReadOnlyList<T> instances, bool worldPositionStays = false)
        {
            if (instances == null)
            {
                return;
            }

            for (var i = 0; i < instances.Count; i++)
            {
                Release(instances[i], worldPositionStays);
            }
        }

        public void Prewarm(int count)
        {
            for (var i = availableInstances.Count; i < count; i++)
            {
                availableInstances.Enqueue(CreateInstance());
            }
        }

        private T CreateInstance()
        {
            var instance = Object.Instantiate(prefab, storageRoot);
            instance.name = prefab.name;
            NotifyReturned(instance);
            instance.gameObject.SetActive(false);
            return instance;
        }

        private static void NotifyTaken(T instance)
        {
            if (instance is IPoolCallbacks callbacks)
            {
                callbacks.OnTakenFromPool();
            }
        }

        private static void NotifyReturned(T instance)
        {
            if (instance is IPoolCallbacks callbacks)
            {
                callbacks.OnReturnedToPool();
            }
        }
    }
}
