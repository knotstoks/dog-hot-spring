using BroccoliBunnyStudios.Utils;
using Cysharp.Threading.Tasks;
using ProjectRuntime.Managers;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ProjectRuntime.Gameplay
{
    public enum QueueTileDirection
    {
        NONE = 0,
        NORTH,
        SOUTH,
        EAST,
        WEST,
    }

    public class QueueTile : MonoBehaviour
    {
        [field: SerializeField]
        private float DropDelay { get; set; } = 0.5f;

        [field: SerializeField, Header("Scene References")]
        private BoxCollider2D AnimalCollider { get; set; }

        [field: SerializeField]
        private TextMeshProUGUI DropsLeftText { get; set; }

        [field: SerializeField]
        private GameObject RotationVisualParent { get; set; }

        [field: SerializeField]
        private SpriteRenderer CurrentColourIndicator { get; set; }

        [field: SerializeField]
        public TileColor TileColor { get; private set; }

        [field: SerializeField]
        public Vector3 TileDetectionPosition { get; private set; }

        [field: SerializeField]
        public QueueTileDirection TileDirection { get; private set; }

        [field: SerializeField]
        public Queue<TileColor> TileQueueColours { get; private set; }

        [field: SerializeField]
        private Animator Animator { get; set; }


        // Tile Color should be set in level editor
        public async void Init(int queueTileId, QueueTileDirection tileDirection, Queue<TileColor> queueColors, float tileHeight, float tileWidth)
        {
            await UniTask.WaitUntil(() => GridManager.Instance != null);
            if (!this) return;

            var tileDetectionPosition = transform.position;
            this.TileDirection = tileDirection;
            this.TileQueueColours = queueColors;
            

            switch (TileDirection)
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

            UpdateFacingDirection(this.TileDirection);

            GridManager.Instance.RegisterQueueTile(this);

            UpdateVisual();
        }

        public void UpdateVisual()
        {
            UpdateColour();
            UpdateDropsText();
        }

        public void UpdateColour()
        {
            if (TileQueueColours.TryPeek(out var currentColour))
            {
                CurrentColourIndicator.color = CommonUtil.GetHighlightTintFromTileColor(currentColour);
            }
        }

        public void UpdateDropsText()
        {
            DropsLeftText.text = TileQueueColours.Count.ToString();
        }

        public void UpdateFacingDirection(QueueTileDirection direction)
        {
            var transform = RotationVisualParent.transform;
            switch (direction)
            {
                case QueueTileDirection.NONE:
                    break;
                case QueueTileDirection.NORTH:
                    // Do nothing
                    break;
                case QueueTileDirection.SOUTH:
                    transform.Rotate(0, 0, 180f);
                    return;
                case QueueTileDirection.EAST:
                    transform.Rotate(0, 0, -90f);
                    return;
                case QueueTileDirection.WEST:
                    transform.Rotate(0, 0, 90f);
                    break;
                default:
                    break;
            }
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

            // Remove immediately to prevent leaving and entering square bug
            // GridManager.Instance.DeregisterAnimalDrop(this);

            // Communicate with Tile that it has dropped instantly

            while (TileQueueColours.TryPeek(out var tileColor))
            {
                print(tileColor);
                if (tileColor != bathSlideTile.TileColor)
                {
                    break;
                }
                bathSlideTile.HandleAnimalDropped();
                TileQueueColours.Dequeue();
                UpdateVisual();
                await UniTask.WaitForSeconds(DropDelay, true);
                if (!this) return;
            }

            if (TileQueueColours.Count == 0)
            {
                GridManager.Instance.DeregisterQueueDrop(this);
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
    }
}