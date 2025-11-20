using UnityEngine;
using UnityEngine.Assertions;

namespace BroccoliBunnyStudios.Extensions
{
    public static class ExtensionsComponent
    {
        public static T FGetComp<T>(this Component component)
            where T : Component
        {
            return component.gameObject.FGetComp<T>();
        }

        public static GameObject FindChildGameObjectByName(this Component component, string name)
        {
            return component.gameObject.FindChildGameObjectByName(name);
        }

        public static bool ActiveSelf(this Component comp)
        {
            if (!comp || !comp.gameObject)
            {
                return false;
            }

            return comp.gameObject.activeSelf;
        }

        public static void SetActive(this Component comp, bool isActive)
        {
            if (!comp || !comp.gameObject)
            {
                return;
            }

            comp.gameObject.SetActive(isActive);
        }

        public static void ForceRebuildLayoutImmediateRecursive(this Component component)
        {
            foreach (var rectTransform in component.GetComponentsInChildren<RectTransform>())
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            }
        }

        public static void InitTransform(this Component component, bool isScaleOne = false)
        {
            component.transform.InitTransform(isScaleOne);
        }

        public static void SetParent(this Component comp, Transform parentTrans)
        {
            comp.transform.SetParent(parentTrans);
        }

        public static void SetParent(this Component comp, Component parentComp)
        {
            comp.transform.SetParent(parentComp ? parentComp.transform : null);
        }

        public static void SetParent(this Component comp, GameObject parentGo)
        {
            comp.transform.SetParent(parentGo ? parentGo.transform : null);
        }
    }
}