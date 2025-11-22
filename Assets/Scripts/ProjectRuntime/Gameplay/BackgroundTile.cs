using BroccoliBunnyStudios.Utils;
using UnityEngine;

namespace ProjectRuntime.Gameplay
{
    public class BackgroundTile : MonoBehaviour
    {
        [field: SerializeField]
        private SpriteRenderer SpriteRenderer { get; set; }

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