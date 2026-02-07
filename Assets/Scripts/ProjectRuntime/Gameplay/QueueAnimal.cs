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
        Drop = 2,
    }

    public class QueueAnimal : MonoBehaviour
	{
        [field: SerializeField, Header("Scene References")]
        private Animator AnimalAnimator { get; set; }

        [field: SerializeField, Header("Sfxes")]
        private AudioPlaybackInfo SplashSfx { get; set; }

        [field: SerializeField]
        private AudioPlaybackInfo RandomAnimalDropSfx { get; set; }

        // Accessible Variables
        public TileColor TileColor { get; set; }

        // Internal Variables
        private const string SPAWN_ANIM = "{0}_{1}_spawn"; // Direction then color
        private const string DROP_ANIM = "{0}_{1}_drop";   // Direction then color
        private QueueTileDirection _queueTileDirection;

        public async UniTask Init(QueueTileDirection queueTileDirection, TileColor tileColor)
        {
            this._queueTileDirection = queueTileDirection;
            this.TileColor = tileColor;

            await this.DoSpawnAnimation();
            if (!this) return;
        }

        public async UniTask DropAnimal(BathSlideTile bathSlideTile)
        {
            // Move it to the location
            this.transform.parent = bathSlideTile.GetNearestDropTransform(this.transform.position);
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
            var spawnAnim = this.GetAnimationString(QueueAnimalAnimationEnum.Drop);
            var stateInfo = this.AnimalAnimator.GetCurrentAnimatorStateInfo(0);
            this.AnimalAnimator.Play(spawnAnim);

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
            Debug.Log($"{this.name} starts drop animation");

            var dropAnim = this.GetAnimationString(QueueAnimalAnimationEnum.Drop);
            var stateInfo = this.AnimalAnimator.GetCurrentAnimatorStateInfo(0);
            this.AnimalAnimator.Play(dropAnim);

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

            Debug.Log($"{this.name} ends drop animation");
        }

        private string GetAnimationString(QueueAnimalAnimationEnum qaae)
        {
            return qaae switch
            {
                QueueAnimalAnimationEnum.Spawn => string.Format(SPAWN_ANIM, this._queueTileDirection.ToString().ToLowerInvariant(), this.TileColor.ToString().ToLowerInvariant()),
                QueueAnimalAnimationEnum.Drop => string.Format(DROP_ANIM, this._queueTileDirection.ToString().ToLowerInvariant(), this.TileColor.ToString().ToLowerInvariant()),
                _ => string.Empty,
            };
        }
    }
}