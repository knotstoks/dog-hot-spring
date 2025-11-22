using Cysharp.Threading.Tasks;
using ProjectRuntime.Managers;
using UnityEngine;

namespace ProjectRuntime.Gameplay
{
    public class AnimalDrop : MonoBehaviour
    {
        [field: SerializeField, Header("Scene References")]
        private BoxCollider2D AnimalCollider { get; set; }

        [field: SerializeField]
        public TileColor TileColor { get; private set; }

        private void Awake()
        {
            this.Init();
        }

        // TODO: Init with tile color in level editor
        private async void Init()
        {
            await UniTask.WaitUntil(() => GridManager.Instance != null);
            if (!this) return;

            GridManager.Instance.RegisterAnimalDrop(this);
        }

        public void ToggleTriggerCollider(bool isTrigger)
        {
            this.AnimalCollider.isTrigger = isTrigger;
        }

        public async void Drop(BathSlideTile bathSlideTile)
        {
            if (bathSlideTile == null)
            {
                Debug.LogError("Dropped animal when CurrentlyDraggedTile is null");
                return;
            }

            // TODO: Communicate with Tile that it has dropped instantly
            GridManager.Instance.DeregisterAnimalDrop(this);

            await UniTask.WaitForSeconds(1f);
            if (!this) return;

            Destroy(this.gameObject);
        }
    }
}