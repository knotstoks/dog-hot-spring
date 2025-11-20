using UnityEngine;

namespace BroccoliBunnyStudios.Pools
{
    public abstract class PooledGameObject : MonoBehaviour
    {
        public string Path { get; set; }
        public abstract void OnReturnToPool();
    }
}