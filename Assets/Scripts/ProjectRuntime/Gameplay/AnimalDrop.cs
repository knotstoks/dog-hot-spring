using BroccoliBunnyStudios.Sound;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ProjectRuntime.Managers;
using UnityEngine;
using static DropInterfaces;

namespace ProjectRuntime.Gameplay
{
    public class AnimalDrop : MonoBehaviour, IDroppableTile
	{
        [field: SerializeField, Header("Scene References")]
        private BoxCollider2D AnimalCollider { get; set; }

        [field: SerializeField]
        public TileColor TileColor { get; private set; }

        [field: SerializeField]
        private Animator Animator { get; set; }

        [field: SerializeField, Header("Sfx")]
        private AudioPlaybackInfo DropSfx { get; set; }

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
            // Removing this null check error for now
            //if (bathSlideTile == null)
            //{
            //    Debug.LogError("Dropped animal when CurrentlyDraggedTile is null");
            //    return;
            //}

            if (bathSlideTile == null)
            {
                return;
            }

            if (bathSlideTile.DropsLeft == 0)
            {
                return;
            }

            // Remove immediately to prevent leaving and entering square bug
            GridManager.Instance.DeregisterAnimalDrop(this);

            // Communicate with Tile that it has dropped instantly
            bathSlideTile.HandleAnimalDropped();
            this.transform.parent = bathSlideTile.GetNearestDropTransform(this.transform.position);
            await this.transform.DOLocalMove(Vector3.zero, 0.1f);
            if (!this) return;

            // Animate it falling!
            await this.PlayDropAnimation();
            if (!this) return;

            await SpawnManager.Instance.SpawnSplashVfx(this.transform.position);
            if (!this) return;

            GridManager.Instance.DetectForVictory();

            Destroy(this.gameObject);
        }

        private async UniTask PlayDropAnimation()
        {
            this.Animator.Play("drop");
            var stateInfo = this.Animator.GetCurrentAnimatorStateInfo(0);
            while (!stateInfo.IsName("drop"))
            {
                await UniTask.Yield();
                if (!this) return;

                stateInfo = this.Animator.GetCurrentAnimatorStateInfo(0);
            }

            while (stateInfo.normalizedTime < 1f)
            {
                await UniTask.Yield();
                if (!this) return;

                stateInfo = this.Animator.GetCurrentAnimatorStateInfo(0);
            }
        }

        public void CancelDrop()
        {
            
        }
    }
}