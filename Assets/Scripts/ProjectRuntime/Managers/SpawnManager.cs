using BroccoliBunnyStudios.Pools;
using Cysharp.Threading.Tasks;
using ProjectRuntime.Visuals;
using UnityEngine;

namespace ProjectRuntime.Managers
{
    public class SpawnManager : MonoBehaviour
    {
        public static SpawnManager Instance { get; private set; }

        private const string VFX_SPLASH = "vfx/vfx_splash.prefab";
        private const string VFX_ICESHATTER = "vfx/vfx_iceshatter.prefab";
        private const string VFX_FOGDISSOLVE = "vfx/vfx_iceshatter.prefab"; // TODO: "vfx/vfx_fogdissolve.prefab";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.Log("There are 2 or more SpawnManagers in the scene");
            }
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public async UniTask SpawnSplashVfx(Vector3 vfxPosition)
        {
            var splashVfx = await ResourcePool.FetchAsync<VisualFX>(VFX_SPLASH);
            splashVfx.transform.SetParent(BattleManager.Instance.VfxContainer);
            splashVfx.transform.position = vfxPosition;
            splashVfx.Init();
            splashVfx.gameObject.SetActive(true);
        }

        public async UniTask SpawnShatterVfx(Vector3 vfxPosition)
        {
            var shatterVfx = await ResourcePool.FetchAsync<VisualFX>(VFX_ICESHATTER);
            shatterVfx.transform.SetParent(BattleManager.Instance.VfxContainer);
            shatterVfx.transform.position = vfxPosition;
            shatterVfx.Init();
            shatterVfx.gameObject.SetActive(true);
        }

        public async UniTask SpawnFogDissolveVfx(Vector3 vfxPosition)
        {
            var fogDissolveVfx = await ResourcePool.FetchAsync<VisualFX>(VFX_FOGDISSOLVE);
            fogDissolveVfx.transform.SetParent(BattleManager.Instance.VfxContainer);
            fogDissolveVfx.transform.position = vfxPosition;
            fogDissolveVfx.Init();
            fogDissolveVfx.gameObject.SetActive(true);
        }
    }
}