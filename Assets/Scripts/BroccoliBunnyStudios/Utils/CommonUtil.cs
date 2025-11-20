using System.Collections.Generic;
using BroccoliBunnyStudios.Pools;
using UnityEngine;
using UnityEngine.UI;

namespace BroccoliBunnyStudios.Utils
{
    public static class CommonUtil
    {
#if UNITY_EDITOR
        /// <summary>
        /// Save AssetDatabase
        /// </summary>
        /// <param name="obj"></param>
        public static void SaveScriptableObject(Object obj)
        {
            // Mark the ScriptableObject as dirty
            UnityEditor.EditorUtility.SetDirty(obj);

            // Save the changes to the asset file
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            Debug.Log("ScriptableObject updated and saved!");
        }
#endif

        /// <summary>
        /// Convert string to int
        /// </summary>
        /// <param name="text"></param>
        /// <returns>int</returns>
        public static int ConvertToInt32(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            if (int.TryParse(text, out int result))
            {
                return result;
            }

            return 0;
        }

        /// <summary>
        /// Convert string to float
        /// </summary>
        /// <param name="text"></param>
        /// <returns>float</returns>
        public static float ConvertToSingle(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            if (float.TryParse(text, out float result))
            {
                return result;
            }

            return 0;
        }

        /// <summary>
        /// Convert string to double
        /// </summary>
        /// <param name="text"></param>
        /// <returns>double</returns>
        public static double ConvertToDouble(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            if (double.TryParse(text, out double result))
            {
                return result;
            }

            return 0;
        }

        /// <summary>
        /// Convert string to decimal
        /// </summary>
        /// <param name="text"></param>
        /// <returns>decimal</returns>
        public static decimal ConvertToDecimal(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            if (decimal.TryParse(text, out decimal result))
            {
                return result;
            }

            return 0;
        }

        /// <summary>
        /// Loads a sprite into a Image component, making sure to unload correctly
        /// </summary>
        public static void UpdateSprite(Image image, string path)
        {
            // Update sprite
            Sprite sprite = null;
            if (!string.IsNullOrEmpty(path))
            {
                sprite = ResourceLoader.Load<Sprite>(path, false);
            }
            image.sprite = sprite;

            // Make sure we unload the sprite
            var unloader = image.GetComponent<ResourceUnloader>();
            if (unloader)
            {
                unloader.SetResource(sprite);
            }
            else
            {
                unloader = image.gameObject.AddComponent<ResourceUnloader>();
                unloader.SetResource(sprite);
            }
        }

        /// <summary>
        /// Loads a sprite into a SpriteRenderer component, making sure to unload correctly
        /// </summary>
        public static void UpdateSprite(SpriteRenderer spriteRenderer, string path)
        {
            // Update sprite
            Sprite sprite = null;
            if (!string.IsNullOrEmpty(path))
            {
                sprite = ResourceLoader.Load<Sprite>(path, false);
            }
            spriteRenderer.sprite = sprite;

            // Make sure we unload the sprite
            var unloader = spriteRenderer.GetComponent<ResourceUnloader>();
            if (unloader)
            {
                unloader.SetResource(sprite);
            }
            else
            {
                unloader = spriteRenderer.gameObject.AddComponent<ResourceUnloader>();
                unloader.SetResource(sprite);
            }
        }

        /// <summary>
        /// Given a list of weights, return one of them at random treating the data as weights.
        /// Return value of -1 means invalid input (nothing to random from or no positive weights).
        /// </summary>
        public static int GetRandomIndexByWeight(List<int> weights)
        {
            if (weights == null || weights.Count == 0)
            {
                return -1;
            }

            // Calculate total weight, ignore values <= 0
            var totalWeight = 0;
            foreach (var weight in weights)
            {
                if (weight > 0)
                {
                    totalWeight += weight;
                }
            }

            if (totalWeight <= 0)
            {
                // Nothing with positive chance to random from
                return -1;
            }

            // Roll a random number from [1 to totalWeight] inclusive
            var randomValue = Random.Range(1, totalWeight + 1);

            // Select based on the weight, ignore values <= 0
            var cumulativeWeight = 0;
            for (var i = 0; i < weights.Count; i++)
            {
                var weight = weights[i];
                if (weight > 0)
                {
                    cumulativeWeight += weight;
                    if (randomValue <= cumulativeWeight)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Given a list of weights, return one of them at random treating the data as weights.
        /// Return value of -1 means invalid input (nothing to random from or no positive weights).
        /// </summary>
        public static int GetRandomIndexByWeight(List<float> weights)
        {
            if (weights == null || weights.Count == 0)
            {
                return -1;
            }

            // Calculate total weight, ignore values <= 0
            var totalWeight = 0f;
            foreach (var weight in weights)
            {
                if (weight > 0f)
                {
                    totalWeight += weight;
                }
            }

            if (totalWeight <= 0f)
            {
                // Nothing with positive chance to random from
                return -1;
            }

            // Roll a random number from [1 to totalWeight] inclusive
            var randomValue = Random.Range(0f, totalWeight);

            // Select based on the weight, ignore values <= 0
            var cumulativeWeight = 0f;
            for (var i = 0; i < weights.Count; i++)
            {
                var weight = weights[i];
                if (weight > 0)
                {
                    cumulativeWeight += weight;
                    if (randomValue <= cumulativeWeight)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Instantiates a prefab with proper refcount addressables unloading (unloadDuringSceneChange = false)
        /// </summary>
        public static GameObject InstantiatePrefab(string path, Transform parent)
        {
            var prefab = ResourceLoader.Load<GameObject>(path, false);
            var instance = Object.Instantiate(prefab, parent);
            instance.AddComponent<ResourceUnloader>().SetResource(prefab);
            return instance;
        }

        public static void ForceRebuildLayoutImmediateRecursive(RectTransform rt)
        {
            // Depth-first search to go rebuild children LayoutGroup and ContentSizeFitter first
            foreach (Transform child in rt)
            {
                if (child.gameObject.activeSelf && child is RectTransform childRt)
                {
                    ForceRebuildLayoutImmediateRecursive(childRt);
                }
            }

            // After handling all children, update layout of ourselves
            var lg = rt.GetComponent<LayoutGroup>();
            var csf = rt.GetComponent<ContentSizeFitter>();
            if ((lg != null && lg.enabled) || (csf != null && csf.enabled))
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            }
        }
    }
}