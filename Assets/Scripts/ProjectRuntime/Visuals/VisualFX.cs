using BroccoliBunnyStudios.Pools;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ProjectRuntime.Visuals
{
    public class VisualFX : PooledGameObject
    {
        [field: SerializeField]
        private float DelayUntilPooled { get; set; }

        public async void Init()
        {
            await UniTask.WaitForSeconds(this.DelayUntilPooled);
            if (!this) return;

            this.ReturnToPool();
        }

        public override void OnReturnToPool()
        {
            // Nothing
        }
    }
}