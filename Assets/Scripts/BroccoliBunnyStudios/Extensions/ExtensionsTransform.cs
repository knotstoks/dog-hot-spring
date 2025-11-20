using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace BroccoliBunnyStudios.Extensions
{
    public static class ExtensionsTransform
    {
        public static void FindChildrenByName(this Transform t, ref List<Transform> childrenList, string name)
        {
            if (t.name.ToLower().Contains(name.ToLower()))
            {
                childrenList.Add(t);
            }

            for (var i = 0; i < t.childCount; i++)
            {
                t.GetChild(i).FindChildrenByName(ref childrenList, name);
            }
        }

        public static void InitTransform(this Transform transform, bool isScaleOne)
        {
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            if (isScaleOne)
            {
                transform.localScale = Vector3.one;
            }
        }

        public static void ResetLocalPosition(this Transform trans)
        {
            trans.localPosition = Vector3.zero;
        }

        public static void ResetLocalRotation(this Transform trans)
        {
            trans.localRotation = Quaternion.identity;
        }

        public static void ResetLocalScale(this Transform trans)
        {
            trans.localScale = Vector3.one;
        }

        public static void ResetLocalTransform(this Transform trans)
        {
            trans.ResetLocalPosition();
            trans.ResetLocalRotation();
            trans.ResetLocalScale();
        }

        public static void Clear(this Transform transform)
        {
            foreach (var child in transform)
            {
                Object.Destroy(((Transform)child).gameObject);
            }
        }

        /// <summary>
        ///     Find a game object searching recursively through all children and grand-children, etc.
        /// </summary>
        /// <param name="parent">Transform.</param>
        /// <param name="name">Name of transform to find.</param>
        /// <returns>Transform or null if not found.</returns>
        public static Transform FindRecursive(this Transform parent, string name)
        {
            var result = parent.Find(name);
            if (result != null)
            {
                return result;
            }

            foreach (Transform child in parent)
            {
                result = child.FindRecursive(name);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public static float GetHeight(this Transform self)
        {
            Assert.IsNotNull(self, "self != null");
            var rect = (RectTransform)self;
            return rect.GetHeight();
        }

        public static float GetHeight(this RectTransform self)
        {
            Assert.IsNotNull(self, "self != null");
            return self.sizeDelta.y;
        }

        public static GameObject FindChildByNamePartial(this Transform t, string name)
        {
            if (t.name.ToLower().Contains(name.ToLower()))
            {
                return t.gameObject;
            }

            for (var i = 0; i < t.childCount; i++)
            {
                var obj = FindChildByNamePartial(t.GetChild(i), name);
                if (obj != null)
                {
                    return obj;
                }
            }

            return null;
        }

        public static List<GameObject> FindChildrenByNamePartial(this Transform t, string name)
        {
            var childrenWithName = new List<GameObject>();

            for (var i = 0; i < t.childCount; i++)
            {
                var obj = FindChildByNamePartial(t.GetChild(i), name);
                if (obj != null)
                {
                    childrenWithName.Add(obj);
                }
            }

            return childrenWithName;
        }

        public static void FindChildrenByNamePartialNoAlloc(this Transform t, ref List<Transform> childrenList, string name)
        {
            if (t.name.ToLower().Contains(name.ToLower()))
            {
                childrenList.Add(t);
            }

            for (var i = 0; i < t.childCount; i++)
            {
                t.GetChild(i).FindChildrenByNamePartialNoAlloc(ref childrenList, name);
            }
        }
    }
}