using System.Reflection;
using UnityEngine;

namespace BroccoliBunnyStudios.Extensions
{
    public static class ExtensionsGameObject
    {
        public static T FGetComp<T>(this GameObject go)
            where T : Component
        {
            if (!go)
            {
                Debug.LogError("[ERROR] go is null");

                return null;
            }

            var t = go.GetComponent<T>();

            return t ? t : go.AddComponent<T>();
        }

        public static GameObject FindChildGameObjectByName(this GameObject rootGo, string name, bool recursive = false)
        {
            var trans = recursive ? rootGo.transform.FindRecursive(name) : rootGo.transform.Find(name);
            return trans ? trans.gameObject : null;
        }

        public static void SetLayerRecursively(this GameObject g, int newLayer)
        {
            g.layer = newLayer;
            foreach (Transform child in g.transform)
            {
                child.gameObject.SetLayerRecursively(newLayer);
            }
        }

        public static void ForceRebuildLayoutImmediateRecursive(this GameObject behaviour)
        {
            foreach (var rectTransform in behaviour.GetComponentsInChildren<RectTransform>())
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            }
        }

        public static void InitTransform(this GameObject go, bool isScaleOne)
        {
            go.transform.InitTransform(isScaleOne);
        }

        public static void SetParent(this GameObject go, Transform parentTrans)
        {
            go.transform.SetParent(parentTrans);
        }

        public static void SetParent(this GameObject go, Component parentComp)
        {
            go.transform.SetParent(parentComp ? parentComp.transform : null);
        }

        public static void SetParent(this GameObject go, GameObject parentGo)
        {
            go.transform.SetParent(parentGo ? parentGo.transform : null);
        }

        public static T CopyComponent<T>(this GameObject go, T toAdd) where T : Component
        {
            return go.AddComponent<T>().GetCopyOf(toAdd);
        }

        private static T GetCopyOf<T>(this Component comp, T other) where T : Component
        {
            var type = comp.GetType();
            if (type != other.GetType()) return null; // type mis-match
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
            var propertyInfo = type.GetProperties(flags);
            foreach (var info in propertyInfo)
            {
                if (info.CanWrite)
                {
                    info.SetValue(comp, info.GetValue(other, null), null);
                }
            }

            var fieldInfo = type.GetFields(flags);
            foreach (var info in fieldInfo)
            {
                info.SetValue(comp, info.GetValue(other));
            }

            return comp as T;
        }
    }
}