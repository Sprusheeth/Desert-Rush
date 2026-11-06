using System.Collections.Generic;
using UnityEngine;

namespace DesertRush
{
    public sealed class PrefabPool
    {
        private readonly Dictionary<GameObject, Stack<GameObject>> free = new();
        private readonly Transform root;

        public PrefabPool(Transform root) => this.root = root;

        public GameObject Rent(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
        {
            if (!free.TryGetValue(prefab, out var stack))
                free.Add(prefab, stack = new Stack<GameObject>());
            var instance = stack.Count > 0 ? stack.Pop() : Object.Instantiate(prefab, root);
            instance.transform.SetParent(parent, false);
            instance.transform.SetLocalPositionAndRotation(position, rotation);
            instance.transform.localScale = prefab.transform.localScale;
            instance.SetActive(true);
            return instance;
        }

        public void Return(GameObject prefab, GameObject instance)
        {
            instance.SetActive(false);
            instance.transform.SetParent(root, false);
            free[prefab].Push(instance);
        }
    }
}
