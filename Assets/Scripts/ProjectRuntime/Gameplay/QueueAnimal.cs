using BroccoliBunnyStudios.Sound;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ProjectRuntime.Managers;
using UnityEngine;

namespace ProjectRuntime.Gameplay
{
    /// <summary>
    /// This class is for the animals in the queue tiles, purely visual and does not have any colliders
    /// </summary>

    public enum QueueAnimalAnimationEnum
    {
        Spawn = 1,
        Idle = 2,
        Transition = 3,
        Drop = 4,
    }

    public class QueueAnimal : MonoBehaviour
    {
        public const float MoveNextTime = 0.1f;

        [field: SerializeField, Header("Scene References")]
        private Animator AnimalAnimator { get; set; }

        [field: SerializeField, Header("Sfxes")]
        private AudioPlaybackInfo SplashSfx { get; set; }

        [field: SerializeField]
        private AudioPlaybackInfo RandomAnimalDropSfx { get; set; }

        // Accessible Variables
        public TileColor TileColor { get; set; }

        // Internal Variables
        private const string SPAWN_ANIM = "{0}_{1}_spawn";           // Direction then color
        private const string IDLE_ANIM = "{0}_{1}_idle";             // Direction then color
        private const string TRANSITION_ANIM = "{0}_{1}_transition"; // Direction then color
        private const string DROP_ANIM = "{0}_{1}_drop";             // Direction then color
        private QueueTileDirection _queueTileDirection;

        public async UniTask Init(QueueTileDirection queueTileDirection, TileColor tileColor)
        {
            this._queueTileDirection = queueTileDirection;
            this.TileColor = tileColor;

            await this.DoSpawnAnimation();
            if (!this) return;

            this.AnimalAnimator.Play(this.GetAnimationString(QueueAnimalAnimationEnum.Idle), 0, 0f);
        }

        public async UniTask DropAnimal(BathSlideTile bathSlideTile, QueueTile queueTile)
        {
            // Play transition animation
            this.AnimalAnimator.Play(string.Format(TRANSITION_ANIM, this._queueTileDirection.ToString().ToLowerInvariant(), this.TileColor.ToString().ToLowerInvariant()), 0, 0f);

            // Move it to the location
            this.transform.parent = bathSlideTile.GetNearestDropTransform(queueTile.DropAnimalDetectionTransform.position);
            await this.transform.DOLocalMove(Vector3.zero, AnimalDrop.MOVE_DELAY);
            if (!this) return;

            // TODO: Uncomment
            //SoundManager.Instance.PlayAudioPlaybackInfoAsync(this.RandomAnimalDropSfx, false, Vector3.zero).Forget();

            await this.PlayDropAnimation();
            if (!this) return;

            await SpawnManager.Instance.SpawnSplashVfx(this.transform.position);
            if (!this) return;

            SoundManager.Instance.PlayAudioPlaybackInfoAsync(this.SplashSfx, false, Vector3.zero).Forget();

            Destroy(this.gameObject);
        }

        private async UniTask DoSpawnAnimation()
        {
            var spawnAnim = this.GetAnimationString(QueueAnimalAnimationEnum.Spawn);
            var stateInfo = this.AnimalAnimator.GetCurrentAnimatorStateInfo(0);
            this.AnimalAnimator.Play(spawnAnim, 0, 0f);

            while (!stateInfo.IsName(spawnAnim))
            {
                await UniTask.Yield();
                if (!this) return;

                stateInfo = this.AnimalAnimator.GetCurrentAnimatorStateInfo(0);
            }
            stateInfo = this.AnimalAnimator.GetCurrentAnimatorStateInfo(0);
            while (stateInfo.IsName(spawnAnim) && stateInfo.normalizedTime < 1f)
            {
                await UniTask.Yield();
                if (!this) return;

                stateInfo = this.AnimalAnimator.GetCurrentAnimatorStateInfo(0);
            }
        }

        private async UniTask PlayDropAnimation()
        {
            var dropAnim = this.GetAnimationString(QueueAnimalAnimationEnum.Drop);
            var stateInfo = this.AnimalAnimator.GetCurrentAnimatorStateInfo(0);
            this.AnimalAnimator.Play(dropAnim, 0, 0f);

            while (!stateInfo.IsName(dropAnim))
            {
                await UniTask.Yield();
                if (!this) return;

                stateInfo = this.AnimalAnimator.GetCurrentAnimatorStateInfo(0);
            }
            stateInfo = this.AnimalAnimator.GetCurrentAnimatorStateInfo(0);
            while (stateInfo.IsName(dropAnim) && stateInfo.normalizedTime < 1f)
            {
                await UniTask.Yield();
                if (!this) return;

                stateInfo = this.AnimalAnimator.GetCurrentAnimatorStateInfo(0);
            }
        }

        private string GetAnimationString(QueueAnimalAnimationEnum qaae)
        {
            return qaae switch
            {
                QueueAnimalAnimationEnum.Spawn => string.Format(SPAWN_ANIM, this._queueTileDirection.ToString().ToLowerInvariant(), this.TileColor.ToString().ToLowerInvariant()),
                QueueAnimalAnimationEnum.Idle => string.Format(IDLE_ANIM, this._queueTileDirection.ToString().ToLowerInvariant(), this.TileColor.ToString().ToLowerInvariant()),
                QueueAnimalAnimationEnum.Transition => string.Format(TRANSITION_ANIM, this._queueTileDirection.ToString().ToLowerInvariant(), this.TileColor.ToString().ToLowerInvariant()),
                QueueAnimalAnimationEnum.Drop => string.Format(DROP_ANIM, this._queueTileDirection.ToString().ToLowerInvariant(), this.TileColor.ToString().ToLowerInvariant()),
                _ => string.Empty,
            };
        }
    }
}