using Cysharp.Threading.Tasks;
using DG.Tweening;
using ProjectRuntime.Managers;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

    public class QueueTile : MonoBehaviour
    {
        [field: SerializeField, Header("Scene References")]
        private Transform QueueAnimalParentTransform { get; set; }

        [field: SerializeField]
        private TextMeshProUGUI DropsLeftText { get; set; }

        [field: SerializeField]
        private GameObject RotationVisualParent { get; set; }

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
        private bool _isSetUp = false;
        private bool _isCurrentlyDropping = false;
        public Queue<TileColor> _tileQueueColours;
        private TileColor _currentTileColour;
        private QueueAnimal _currentQueueAnimal;
        private QueueAnimal _nextQueueAnimal;
        private QueueTileDirection _tileDirection;
        private int _dropsLeft = 0;

        // Tile Color should be set in level editor
        public async UniTask Init(QueueTileDirection tileDirection, Queue<TileColor> queueColors, float tileHeight, float tileWidth)
        {
            await UniTask.WaitUntil(() => GridManager.Instance != null);
            if (!this) return;

            var tileDetectionPosition = transform.position;
            this._tileDirection = tileDirection;
            this._tileQueueColours = queueColors;

            var temp = Vector3.zero;
            switch (this._tileDirection)
            {
                case QueueTileDirection.NONE:
                    Debug.LogError($"No tile direction for tile: {name}");
                    break;
                case QueueTileDirection.NORTH:
                    temp = this.NextQueueAnimalTransform.position;
                    temp.z = -0.01f;
                    this.NextQueueAnimalTransform.position = temp;

                    tileDetectionPosition.y += tileHeight;
                    break;
                case QueueTileDirection.SOUTH:
                    temp = this.CurrentQueueAnimalTransform.position;
                    temp.z = -0.01f;
                    this.CurrentQueueAnimalTransform.position = temp;

                    tileDetectionPosition.y -= tileHeight;
                    break;
                case QueueTileDirection.EAST:
                    temp = this.CurrentQueueAnimalTransform.position;
                    temp.z = -0.01f;
                    this.CurrentQueueAnimalTransform.position = temp;

                    tileDetectionPosition.x += tileWidth;
                    break;
                case QueueTileDirection.WEST:
                    temp = this.CurrentQueueAnimalTransform.position;
                    temp.z = -0.01f;
                    this.CurrentQueueAnimalTransform.position = temp;

                    tileDetectionPosition.x -= tileWidth;
                    break;
                default:
                    break;
            }

            this.TileDetectionPosition = tileDetectionPosition;

            this.UpdateFacingDirection(this._tileDirection);

            this.ReparentAnimalTransforms();

            GridManager.Instance.RegisterQueueTile(this);

            this._dropsLeft = this._tileQueueColours.Count;
            this.UpdateDropsLeftText(this._dropsLeft);

            this.InitColor().Forget();

            this._isSetUp = true;
        }

        private async UniTaskVoid InitColor()
        {
            var initQueueColor = this._tileQueueColours.Dequeue();
            this._currentTileColour = initQueueColor;

            var currentQueueAnimal = Instantiate(this.QueueAnimalPrefab, this.CurrentQueueAnimalTransform);
            currentQueueAnimal.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            currentQueueAnimal.Init(this._tileDirection, initQueueColor).Forget();

            this._currentQueueAnimal = currentQueueAnimal;

            if (this._tileQueueColours.Count > 0)
            {
                var nextTileColor = this._tileQueueColours.Dequeue();

                var nextQueueAnimal = Instantiate(this.QueueAnimalPrefab, this.NextQueueAnimalTransform);
                nextQueueAnimal.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                nextQueueAnimal.Init(this._tileDirection, nextTileColor).Forget();

                this._nextQueueAnimal = nextQueueAnimal;
            }
            else
            {
                this._nextQueueAnimal = null;
            }

            await UniTask.CompletedTask;
        }

        private void ReparentAnimalTransforms()
        {
            this.CurrentQueueAnimalTransform.SetParent(this.QueueAnimalParentTransform);
            this.CurrentQueueAnimalTransform.rotation = Quaternion.identity;
            this.NextQueueAnimalTransform.SetParent(this.QueueAnimalParentTransform);
            this.NextQueueAnimalTransform.rotation = Quaternion.identity;
        }

        /// <summary>
        /// This function moves the queue animals forward
        /// </summary>
        private async UniTask UpdateColour()
        {
            // Move next animal to current animal
            if (this._nextQueueAnimal != null)
            {
                this._currentQueueAnimal = this._nextQueueAnimal;
                this._nextQueueAnimal = null;

                this._currentTileColour = this._currentQueueAnimal.TileColor;

                this._currentQueueAnimal.transform.SetParent(this.CurrentQueueAnimalTransform);
                await this._currentQueueAnimal.transform.DOLocalMove(Vector3.zero, QueueAnimal.MoveNextTime);
                if (!this) return;
            }

            // Add new animal to next tile
            if (this._tileQueueColours.Count > 0)
            {
                var nextTileColor = this._tileQueueColours.Dequeue();

                var nextQueueAnimal = Instantiate(this.QueueAnimalPrefab, this.NextQueueAnimalTransform);
                nextQueueAnimal.transform.localPosition = Vector3.zero;
                await nextQueueAnimal.Init(this._tileDirection, nextTileColor);
                if (!this) return;

                this._nextQueueAnimal = nextQueueAnimal;
            }

            if (this._currentQueueAnimal == null && this._nextQueueAnimal == null
                && this._tileQueueColours.Count == 0)
            {
                this._currentTileColour = TileColor.NONE;
                GridManager.Instance.DeregisterQueueDrop(this);
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
                    this.RotationVisualParent.transform.Rotate(0, 0, 180f);
                    return;
                case QueueTileDirection.EAST:
                    this.RotationVisualParent.transform.Rotate(0, 0, -90f);
                    return;
                case QueueTileDirection.WEST:
                    this.RotationVisualParent.transform.Rotate(0, 0, 90f);
                    break;
                default:
                    break;
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!this._isSetUp)
            {
                return;
            }

            if (other.gameObject.layer == LayerMask.NameToLayer("Tiles"))
            {
                var bathSlideTile = other.gameObject.GetComponentInParent<BathSlideTile>();
                this.Drop(bathSlideTile).Forget();
            }
        }

        public async UniTaskVoid Drop(BathSlideTile bathSlideTile)
        {
            if (bathSlideTile == null)
            {
                return;
            }

            if (this._isCurrentlyDropping)
            {
                return;
            }
            this._isCurrentlyDropping = true;

			// Communicate with Tile that it has dropped instantly
			if (this._currentTileColour == bathSlideTile.TileColor && this._currentQueueAnimal != null)
            {
                this.UpdateDropsLeftText(this._dropsLeft - 1);
                this._dropsLeft--;

                bathSlideTile.HandleAnimalDropped();

                await this._currentQueueAnimal.DropAnimal(bathSlideTile);
                if (!this) return;

                this._currentQueueAnimal = null;

                await this.UpdateColour();
                if (!this) return;
            }

            this._isCurrentlyDropping = false;

            GridManager.Instance.DetectForVictory();
        }

        private void UpdateDropsLeftText(int count)
        {
            this.DropsLeftText.text = count.ToString();
        }
    }
}