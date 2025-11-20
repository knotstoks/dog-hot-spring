using UnityEngine;

namespace BroccoliBunnyStudios.Pools
{
    /// <summary>
    /// Helper class to auto unload resource on component destroy.
    /// </summary>
    public class ResourceUnloader : MonoBehaviour
    {
        private Object _resource;

        public void SetResource(Object resource)
        {
            this.UnloadResource();
            this._resource = resource;
        }

        public void OnDestroy()
        {
            this.UnloadResource();
        }

        private void UnloadResource()
        {
            if (this._resource == null)
            {
                return;
            }

            ResourceLoader.Unload(this._resource);
            this._resource = null;
        }
    }
}