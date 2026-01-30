using BroccoliBunnyStudios.Utils;
using UnityEngine;

namespace ProjectRuntime.Gameplay
{
    public class BackgroundTile : MonoBehaviour
    {
        public bool IsBathOnTile { get; private set; } = false;

        [field: SerializeField]
        private SpriteRenderer FrontSpriteRenderer { get; set; }

        /// <summary>Tile position in grid, if in the main backpack area</summary>
        [Sirenix.OdinInspector.ShowInInspector, Sirenix.OdinInspector.ReadOnly]
        public Vector2Int TileYXPosition { get; set; } = new(-1, -1);

        private const float SHRINK_SCALE = 0.8f;
        private Color ORIGINAL_TILE_COLOR = new(0.8f, 0.7f, 0.61f, 1f);
        private const float SHRINK_SPEED = 3f;
        private Color _currentColor;
        private bool _isBathTileOnTop;
        private float _shrinkProgress; // 0 means full sized, 1 means fully shrunk
        private float _lastCachedShrinkProgress;

        private void Update()
        {
            if (this._isBathTileOnTop)
            {
                this._shrinkProgress += Time.deltaTime * SHRINK_SPEED;
            }
            else
            {
                this._shrinkProgress -= Time.deltaTime * SHRINK_SPEED;
            }

            this._shrinkProgress = Mathf.Clamp01(this._shrinkProgress);
            if (this._lastCachedShrinkProgress == this._shrinkProgress)
            {
                // Already at final correct value so early return
                return;
            }

            this._lastCachedShrinkProgress = this._shrinkProgress;
            var scale = Mathf.Lerp(1f, SHRINK_SCALE, this._shrinkProgress);
            this.FrontSpriteRenderer.gameObject.transform.localScale = Vector3.one * scale;
        }

        public void HighlightTile(TileColor tileColor)
        {
            this._isBathTileOnTop = true;
            this._currentColor = CommonUtil.GetHighlightTintFromTileColor(tileColor);
        }

        public void UnhighlightTile()
        {
            this._isBathTileOnTop = false;
            this._currentColor = ORIGINAL_TILE_COLOR;
        }
    }

}