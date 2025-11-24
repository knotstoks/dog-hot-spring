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

        // Tile Color should be set in level editor
        public async void Init()
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
            bathSlideTile.HandleAnimalDropped();

            GridManager.Instance.DeregisterAnimalDrop(this);

            await UniTask.WaitForSeconds(0.5f);
            if (!this) return;

            Destroy(this.gameObject);
        }
    }
}