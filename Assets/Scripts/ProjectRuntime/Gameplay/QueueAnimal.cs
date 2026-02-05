using Cysharp.Threading.Tasks;
using DG.Tweening;
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

        // Accessible Variables
        public bool IsAbleToDrop { get; private set; }

        // Internal Variables
        private const string SPAWN_ANIM = "{0}_{1}_spawn"; // Direction then color
        private const string DROP_ANIM = "{0}_{1}_drop";   // Direction then color
        private QueueTileDirection _queueTileDirection;
        private TileColor _tileColor;

        public async UniTask Init(QueueTileDirection queueTileDirection, TileColor tileColor)
        {
            this._queueTileDirection = queueTileDirection;
            this._tileColor = tileColor;

            await this.DoSpawnAnimation();
            if (!this) return;

            this.IsAbleToDrop = true;
        }

        public async UniTask DropAnimal(BathSlideTile bathSlideTile)
        {
            // TODO: REMOVE
            await UniTask.CompletedTask;
            return;

            this.IsAbleToDrop = false;

            this.transform.parent = bathSlideTile.GetNearestDropTransform(this.transform.position);

            this.DoDropAnimation().Forget(); // Make sure drop animation is 0.1 seconds long?

            await this.transform.DOLocalMove(Vector3.zero, 0.1f);
            if (!this) return;

            await this.DoDropAnimation();
            if (!this) return;
        }

        private async UniTask DoSpawnAnimation()
        {
            // TODO: REMOVE
            await UniTask.CompletedTask;
            return;

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

        private async UniTask DoDropAnimation()
        {
            // TODO: REMOVE
            await UniTask.CompletedTask;
            return;

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
        }

        private string GetAnimationString(QueueAnimalAnimationEnum qaae)
        {
            return qaae switch
            {
                QueueAnimalAnimationEnum.Spawn => string.Format(SPAWN_ANIM, this._queueTileDirection.ToString().ToLowerInvariant(), this._tileColor.ToString().ToLowerInvariant()),
                QueueAnimalAnimationEnum.Drop => string.Format(DROP_ANIM, this._queueTileDirection.ToString().ToLowerInvariant(), this._tileColor.ToString().ToLowerInvariant()),
                _ => string.Empty,
            };
        }
    }
}