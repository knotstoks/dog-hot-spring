using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BroccoliBunnyStudios.Pools
{
    public static class ResourceLoader
    {
        private static readonly List<AsyncOperationHandle> s_loadedHandles = new List<AsyncOperationHandle>();

        public static async UniTask InitializeAsync()
        {
            await Addressables.InitializeAsync().Task;
        }

        /// <summary>
        /// Loads asset into memory.
        /// </summary>
        /// <param name="name">The addressable name of the asset.</param>
        /// <param name="unloadDuringSceneChange">Set to true to unload asset automatically during scene change.
        /// Default is false, which means you have to call ResourceLoader.Unload after you are done with the asset.
        /// It's recommend to unload manually to avoid too many assets being loaded in scene and facing memory pressure
        /// in the future.</param>
        /// <typeparam name="T">Type of asset to be loaded.</typeparam>
        /// <returns>Loaded asset.</returns>
        public static async UniTask<T> LoadAsync<T>(string name, bool unloadDuringSceneChange = false)
        {
            var op = Addressables.LoadAssetAsync<T>(name);

            if (unloadDuringSceneChange)
            {
                s_loadedHandles.Add(op);
            }

            var obj = await op.Task;

            return obj;
        }

        /// <summary>
        /// Loads asset into memory.
        /// </summary>
        /// <param name="name">The addressable name of the asset.</param>
        /// <param name="unloadDuringSceneChange">Set to true to unload asset automatically during scene change.
        /// Default is false, which means you have to call ResourceLoader.Unload after you are done with the asset.
        /// It's recommend to unload manually to avoid too many assets being loaded in scene and facing memory pressure
        /// in the future.</param>
        /// <typeparam name="T">Type of asset to be loaded.</typeparam>
        /// <returns>Loaded asset.</returns>
        public static T Load<T>(string name, bool unloadDuringSceneChange = false)
        {
            var op = Addressables.LoadAssetAsync<T>(name);

            if (unloadDuringSceneChange)
            {
                s_loadedHandles.Add(op);
            }

            return op.WaitForCompletion();
        }

        /// <summary>
        /// Unload asset loaded using ResourceLoader.LoadAsync.
        /// </summary>
        /// <param name="o">Loaded asset to be unloaded.</param>
        public static void Unload(object o)
        {
            Addressables.Release(o);
        }

        /// <summary>
        /// Unload all loaded assets marked with unloadDuringSceneChange at ResourceLoader.LoadAsync.
        /// </summary>
        public static void UnloadForSceneChange()
        {
            foreach (var op in s_loadedHandles)
            {
                Addressables.Release(op);
            }

            s_loadedHandles.Clear();
        }

        public static async UniTask<GameObject> InstantiateAsync(string key, Transform parent = null)
        {
            var obj = await LoadAsync<GameObject>(key);
            if (obj != null)
            {
                var instance = Object.Instantiate(obj, parent);
                instance.AddComponent<ResourceUnloader>().SetResource(obj);
                return instance;
            }

            return null;
        }

        public static async UniTask<GameObject> InstantiateAsync(
            string key,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null)
        {
            var obj = await LoadAsync<GameObject>(key);
            if (obj != null)
            {
                var instance = Object.Instantiate(obj, position, rotation, parent);
                instance.AddComponent<ResourceUnloader>().SetResource(obj);
                return instance;
            }

            return null;
        }

        public static void Destroy(GameObject go)
        {
            if (go)
            {
                Object.Destroy(go);
            }
        }
    }
}