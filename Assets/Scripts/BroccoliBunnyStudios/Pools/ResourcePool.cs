using System.Collections.Generic;
using System.Linq;
using BroccoliBunnyStudios.Extensions;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace BroccoliBunnyStudios.Pools
{
    public static class ResourcePool
    {
        private static readonly Dictionary<string, GameObject> s_parents = new Dictionary<string, GameObject>();
        private static readonly Dictionary<string, GameObject> s_objects = new Dictionary<string, GameObject>();
        private static readonly Dictionary<string, Stack<GameObject>> s_pool = new Dictionary<string, Stack<GameObject>>();
        private static readonly HashSet<string> s_creating = new HashSet<string>();

#if UNITY_EDITOR
        private static Transform s_globalParent;
#endif

#if UNITY_EDITOR
        /// <summary>
        /// Used for debugging, trying to get all the objects of certain type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> GetAllObjectsOfType<T>()
            where T : MonoBehaviour
        {
            return s_globalParent.GetComponentsInChildren<T>(true).ToList();
        }
#endif

        public static async UniTask<T> FetchAsync<T>(string path, int initialPoolSize = 1)
            where T : PooledGameObject
        {
            if (!s_objects.ContainsKey(path))
            {
                await CreatePoolAsync<T>(path, initialPoolSize);
            }

            GameObject res = null;
            if (s_pool.TryGetValue(path, out var stack) && stack.Count > 0)
            {
                res = stack.Pop();
            }

            if (res == null)
            {
                if (!s_objects.ContainsKey(path))
                {
                    // Someone called ClearAllPools() during the "await CreatePoolAsync<T>(path)"
                    // and here (probably because of a scene change)
                    return null;
                }
                res = Instantiate<T>(s_objects[path]);
                res.SetActive(false);
            }

            return res.FGetComp<T>();
        }

        public static void ReturnToPool<T>(this T comp)
            where T : PooledGameObject
        {
            if (comp && comp.gameObject)
            {
                var path = comp.Path;
                if (!string.IsNullOrWhiteSpace(path) && s_pool.TryGetValue(path, out var stack))
                {
                    Assert.IsFalse(stack.Contains(comp.gameObject), "Trying to return object that is already in pool.");

                    comp.OnReturnToPool();
                    stack.Push(comp.gameObject);
                    if (s_parents.TryGetValue(path, out var parent))
                    {
                        if (parent)
                        {
                            comp.gameObject.SetParent(parent.transform);
                        }
                        else
                        {
                            Object.Destroy(comp.gameObject);
                        }
                    }
                }
                else
                {
                    Object.Destroy(comp.gameObject);
                }
            }
        }

        public static void ReturnToPool(this GameObject instance)
        {
            if (instance)
            {
                var comp = instance.GetComponent<PooledGameObject>();
                if (comp)
                {
                    ReturnToPool(comp);
                }
                else
                {
                    Object.Destroy(instance);
                }
            }
        }

        public static void ReturnAll(params IEnumerable<PooledGameObject>[] enumerables)
        {
            if (enumerables != null)
            {
                foreach (var enumerable in enumerables)
                {
                    if (enumerable != null)
                    {
                        foreach (var pgo in enumerable)
                        {
                            if (pgo != null)
                            {
                                pgo.ReturnToPool();
                            }
                        }
                    }
                }
            }
        }

        public static void ClearPool(string path)
        {
            if (s_objects.TryGetValue(path, out var obj))
            {
                s_objects.Remove(path);
                ResourceLoader.Unload(obj);
            }

            if (s_pool.TryGetValue(path, out var stack))
            {
                foreach (var instance in stack)
                {
                    Object.Destroy(instance);
                }

                s_pool.Remove(path);
            }
        }

        public static void ClearAllPools()
        {
            var keys = s_objects.Keys.ToList();
            foreach (var path in keys)
            {
                ClearPool(path);
            }
        }

        public static bool IsInResourcePool<T>(this T comp)
            where T : PooledGameObject
        {
            if (comp && comp.gameObject)
            {
                var path = comp.Path;
                if (!string.IsNullOrWhiteSpace(path) && s_pool.TryGetValue(path, out var stack))
                {
                    return stack.Contains(comp.gameObject);
                }
            }
            return false;
        }

        private static GameObject Instantiate<T>(GameObject prefab, Transform parent = null)
            where T : PooledGameObject
        {
            var instance = Object.Instantiate(prefab, parent);
            instance.FGetComp<T>().Path = prefab.FGetComp<T>().Path;
            return instance;
        }

        private static async UniTask CreatePoolAsync<T>(string path, int initialPoolSize = 0)
            where T : PooledGameObject
        {
            // Return if object cache already has object of path loaded
            if (s_objects.ContainsKey(path))
            {
                return;
            }

            // Creation of object cache already started, wait for it to complete before returning
            if (s_creating.Contains(path))
            {
                while (s_creating.Contains(path))
                {
                    await UniTask.Yield();
                }

                return;
            }

            // Load object into object cache
            s_creating.Add(path);
            var obj = await ResourceLoader.LoadAsync<GameObject>(path);
            s_objects.Add(path, obj);
            var comp = obj.FGetComp<T>();
            comp.Path = path;
            s_creating.Remove(path);

            // Create parent object for organization
            if (!s_parents.TryGetValue(path, out var poolParent))
            {
                poolParent = new GameObject(path);
                s_parents.Add(path, poolParent);
                poolParent.SetActive(false);
                poolParent.transform.position = new Vector3(-10000, -10000, -10000);
                if (UnityEngine.Application.isPlaying)
                {
                    Object.DontDestroyOnLoad(poolParent);
                }
#if UNITY_EDITOR
                if (!s_globalParent)
                {
                    s_globalParent = new GameObject("Pools").transform;
                    if (UnityEngine.Application.isPlaying)
                    {
                        Object.DontDestroyOnLoad(s_globalParent);
                    }
                }

                poolParent.SetParent(s_globalParent);
#endif
            }

            // Get/initialize object pool
            if (!s_pool.TryGetValue(path, out var pool))
            {
                pool = new Stack<GameObject>();
                s_pool.Add(path, pool);
            }

            // Populate pool
            for (var i = 0; i < initialPoolSize; i++)
            {
                pool.Push(Instantiate<T>(obj, poolParent.transform));
            }
        }
    }
}