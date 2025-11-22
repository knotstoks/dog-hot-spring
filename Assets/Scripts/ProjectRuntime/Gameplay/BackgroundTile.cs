using BroccoliBunnyStudios.Utils;
using UnityEngine;

namespace ProjectRuntime.Gameplay
{
    public class BackgroundTile : MonoBehaviour
    {
        [field: SerializeField]
        private SpriteRenderer SpriteRenderer { get; set; }

        /// <summary>Tile position in grid, if in the main backpack area</summary>
        [Sirenix.OdinInspector.ShowInInspector, Sirenix.OdinInspector.ReadOnly]
        public Vector2Int TileYXPosition { get; set; } = new(-1, -1);

        public void HighlightTile(TileColor tileColor)
        {
            this.SpriteRenderer.color = CommonUtil.GetHighlightTintFromTileColor(tileColor);
        }

        public void UnhighlightTile()
        {
            this.SpriteRenderer.color = Color.white;
        }
    }

}