using System.Collections.Generic;
using BroccoliBunnyStudios.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ProjectRuntime.Managers;
using TMPro;
using UnityEngine;
using static DropInterfaces;

namespace ProjectRuntime.Gameplay
{
    public enum QueueTileDirection
    {
        NONE = 0,
        NORTH = 1,
        SOUTH = 2,
        EAST = 3,
        WEST = 4,
    }

    public class QueueTile : MonoBehaviour, IDroppableTile
    {
        [field: SerializeField, Header("Scene References")]
        private BoxCollider2D AnimalCollider { get; set; }

        [field: SerializeField]
        private TextMeshProUGUI DropsLeftText { get; set; }

        [field: SerializeField]
        private GameObject RotationVisualParent { get; set; }

        [field: SerializeField]
        private Animator Animator { get; set; }

        [field: SerializeField]
        private Transform CurrentQueueAnimalTransform { get; set; }

        [field: SerializeField]
        private Transform NextQueueAnimalTransform { get; set; }

        [field: SerializeField, Header("Settings")]
        private float DropDelay { get; set; } = 0.5f;

        [field: SerializeField, Header("Prefabs")]
        private QueueAnimal QueueAnimalPrefab { get; set; }

        // Accessible Variables
        public Vector3 TileDetectionPosition { get; private set; }

        // Internal Variables
        private bool _isCurrentlyDeducting = true;
        public Queue<TileColor> _tileQueueColours;
        private TileColor _currentTileColour;
        private QueueAnimal _currentQueueAnimal;
        private QueueAnimal _nextQueueAnimal;
        private QueueTileDirection _tileDirection;

        // Tile Color should be set in level editor
        public async UniTask Init(QueueTileDirection tileDirection, Queue<TileColor> queueColors, float tileHeight, float tileWidth)
        {
            await UniTask.WaitUntil(() => GridManager.Instance != null);
            if (!this) return;

            var tileDetectionPosition = transform.position;
            this._tileDirection = tileDirection;
            this._tileQueueColours = queueColors;

            switch (_tileDirection)
            {
                case QueueTileDirection.NONE:
                    Debug.LogError($"No tile direction for tile: {name}");
                    break;
                case QueueTileDirection.NORTH:
                    tileDetectionPosition.y += tileHeight;
                    break;
                case QueueTileDirection.SOUTH:
                    tileDetectionPosition.y -= tileHeight;
                    break;
                case QueueTileDirection.EAST:
                    tileDetectionPosition.x += tileWidth;
                    break;
                case QueueTileDirection.WEST:
                    tileDetectionPosition.x -= tileWidth;
                    break;
                default:
                    break;
            }

            this.TileDetectionPosition = tileDetectionPosition;

            this.UpdateFacingDirection(this._tileDirection);

            GridManager.Instance.RegisterQueueTile(this);

            await this.UpdateColour();
            if (!this) return;
        }

        /// <summary>
        /// This function moves the queue animals forward
        /// </summary>
        private async UniTask UpdateColour()
        {
            if (_tileQueueColours.TryDequeue(out var dequeuedColour))
            {
				DropsLeftText.text = $"{this._tileQueueColours.Count + 1}";
				this._currentTileColour = dequeuedColour;

                // Move next animal to current animal
                if (this._nextQueueAnimal != null)
                {
                    this._currentQueueAnimal = this._nextQueueAnimal;
                    this._nextQueueAnimal = null;

                    this._currentQueueAnimal.transform.SetParent(this.CurrentQueueAnimalTransform);
                    await this._currentQueueAnimal.transform.DOLocalMove(Vector3.zero, 0.1f);
                    if (!this) return;
                }
                // Spawn in new current animal, should only happen in the init
                else
                {
                    var currentQueueAnimal = Instantiate(this.QueueAnimalPrefab, this.CurrentQueueAnimalTransform);
                    currentQueueAnimal.transform.localPosition = Vector3.zero;
                    await currentQueueAnimal.Init(this._tileDirection, this._currentTileColour);
                    if (!this) return;
                }
            }
            // No current animal
            else
            {
				DropsLeftText.text = $"{this._tileQueueColours.Count}";
				this._currentTileColour = TileColor.NONE;
				GridManager.Instance.DeregisterQueueDrop(this);
			}

            if (_tileQueueColours.TryPeek(out var nextColor))
            {
                var nextQueueAnimal = Instantiate(this.QueueAnimalPrefab, this.NextQueueAnimalTransform);
                nextQueueAnimal.transform.localPosition = Vector3.zero;
                await nextQueueAnimal.Init(this._tileDirection, nextColor);
                if (!this) return;
            }
            // No next animal
            else
            {
			    // Nothing for now
            }
        }

        public void UpdateFacingDirection(QueueTileDirection direction)
        {
            switch (direction)
            {
                case QueueTileDirection.NONE:
                    break;
                case QueueTileDirection.NORTH:
                    // Do nothing
                    break;
                case QueueTileDirection.SOUTH:
                    this.transform.Rotate(0, 0, 180f);
                    return;
                case QueueTileDirection.EAST:
                    this.transform.Rotate(0, 0, -90f);
                    return;
                case QueueTileDirection.WEST:
                    this.transform.Rotate(0, 0, 90f);
                    break;
                default:
                    break;
            }
        }

        public void ToggleTriggerCollider(bool isTrigger)
        {
            this.AnimalCollider.isTrigger = isTrigger;
        }

        public async UniTaskVoid Drop(BathSlideTile bathSlideTile)
        {
            if (bathSlideTile == null)
            {
                // Debug.LogError("Dropped animal when CurrentlyDraggedTile is null");
                return;
            }

			// Communicate with Tile that it has dropped instantly
			while (bathSlideTile.DropsLeft > 0 && _currentTileColour == bathSlideTile.TileColor)
            {
                // Fix for Null Issue when dragging rapidly
                if (bathSlideTile == null) return;
                this._isCurrentlyDeducting = true;

                await this._currentQueueAnimal.DropAnimal(bathSlideTile);
                if (!this) return;

				bathSlideTile.HandleAnimalDropped();

                await this.UpdateColour();
                if (!this) return;

                await UniTask.WaitForSeconds(this.DropDelay, true);
                if (!this) return;

                if (!_isCurrentlyDeducting)
                {
                    break;
                }
            }

            GridManager.Instance.DetectForVictory();
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
            _isCurrentlyDeducting = false;
        }
    }
}