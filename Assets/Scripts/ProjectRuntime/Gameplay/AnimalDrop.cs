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
        public static float MOVE_DELAY { get; private set; } = 0.3f; // This controls how fast the animals move to the tile

        [field: SerializeField, Header("Scene References")]
        private BoxCollider2D AnimalCollider { get; set; }

        [field: SerializeField]
        public TileColor TileColor { get; private set; }

        [field: SerializeField]
        private Animator Animator { get; set; }

        [field: SerializeField, Header("Sfxes")]
        private AudioPlaybackInfo SplashSfx { get; set; }

        [field: SerializeField]
        private AudioPlaybackInfo RandomAnimalDropSfx { get; set; }

        private bool _isDropping = false;

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

        private void OnTriggerStay2D(Collider2D other)
        {
            if (this._isDropping)
            {
                return;
            }

            if (other.gameObject.CompareTag("Tiles"))
            {
                this._isDropping = true;
                var bathSlideTile = other.GetComponentInParent<BathSlideTile>();
                this.Drop(bathSlideTile).Forget();
            }
        }

        public async UniTaskVoid Drop(BathSlideTile bathSlideTile)
        {
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

            // Move it to the location
            this.transform.parent = bathSlideTile.GetNearestDropTransform(this.transform.position);
            await this.transform.DOLocalMove(Vector3.zero, MOVE_DELAY);
            if (!this) return;

            // Communicate with Tile that it has dropped instantly
            bathSlideTile.HandleAnimalDropped();

            // Animate it falling!
            await this.PlayDropAnimation();
            if (!this) return;


            await SpawnManager.Instance.SpawnSplashVfx(this.transform.position);
            if (!this) return;

            SoundManager.Instance.PlayAudioPlaybackInfoAsync(this.SplashSfx, false, Vector3.zero).Forget();

            //SoundManager.Instance.PlayAudioPlaybackInfoAsync(this.RandomAnimalDropSfx, false, Vector3.zero).Forget();

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