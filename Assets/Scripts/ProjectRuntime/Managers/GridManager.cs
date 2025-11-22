using ProjectRuntime.Gameplay;
using ProjectRuntime.Managers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectRuntime.Managers
{
    /*
     * Terminology:
     * Backpack space
     *   In backpack space, coordinates are counted in tiles.
     *   (y,x) means the tile at (rowY, colX).
     *   In a 5 column, 7 row backpack:
     *     (0,0) is the bottom left corner tile.
     *     (0,4) is the bottom right corner tile.
     *     (6,0) is the top left corner tile.
     *     (6,4) is the top right corner tile.
     */
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }

        [field: SerializeField, Header("Dimensions of grid")]
        public int GridWidth { get; private set; } = 5;

        [field: SerializeField]
        public int GridHeight { get; private set; } = 7;

        [field: SerializeField, Header("Dimensions of a single tile in the grid")]
        public BackgroundTile BackgroundTilePrefab { get; private set; }

        [field: SerializeField]
        public float TileWidth { get; private set; } = 1f;

        [field: SerializeField]
        public float TileHeight { get; private set; } = 1f;

        [field: SerializeField]
        public float TileGap { get; private set; } = 0.1f;

        [field: SerializeField]
        public Vector2Int[] InitialUnlockedPositions { get; private set; }

        [field: SerializeField, Header("References")]
        public Transform TileContainer { get; private set; }

        // Accessible variables
        public BackgroundTile[,] Tiles { get; private set; }      // 2D Array of Tiles in the grid

        // Internal variables
        private Vector3 _bottomLeftOffset;                      // Local offset from pivot of BackpackMainArea for bottomleft-most tile

        private void Awake()
        {
            Instance = this;

            var totalWidth = this.GridWidth * this.TileWidth + (this.GridWidth - 1) * this.TileGap;
            var totalHeight = this.GridHeight * this.TileHeight + (this.GridHeight - 1) * this.TileGap;
            this._bottomLeftOffset = new Vector3(-totalWidth / 2, 0, -totalHeight / 2);
            this._bottomLeftOffset += new Vector3(this.TileWidth / 2, 0, this.TileHeight / 2); // Offset as tile is anchored at center

            this.Tiles = new BackgroundTile[this.GridHeight, this.GridWidth];

            // Create the grid of tiles
            for (var rowY = 0; rowY < this.GridHeight; rowY++)
            {
                for (var colX = 0; colX < this.GridWidth; colX++)
                {
                    // Instantiate and initialize
                    var tile = Instantiate(this.BackgroundTilePrefab, this.TileContainer);
                    tile.transform.localPosition = this.GetTilePosition(rowY, colX);
                    tile.SetHighlight(false, false);
                    tile.TileYXPosition = new(colX, rowY);

                    // Store in array
                    this.Tiles[rowY, colX] = tile;
                }
            }
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        /// <summary>
        /// Gets the position of a tile (in TileContainer local space).
        /// </summary>
        public Vector3 GetTilePosition(int rowY, int colX)
        {
            return this._bottomLeftOffset + new Vector3(colX, 0, rowY) * (this.TileWidth + this.TileGap);
        }

        /// <summary>
        /// The reverse of GetTilePosition(), returns the nearest tile (rowY, colX) given a position in TileContainer local space.
        /// </summary>
        public Vector2Int GetNearestTileYX(Vector3 pos)
        {
            var p = (pos - this._bottomLeftOffset) / (this.TileWidth + this.TileGap);
            return new Vector2Int(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.z));
        }

        private Vector3 GetWorldPosition(PointerEventData eventData)
        {
            var mainCam = Camera.main;
            var plane = new Plane(Vector3.up, this.transform.position);
            var ray = mainCam.ScreenPointToRay(eventData.position);
            plane.Raycast(ray, out var dist);
            return ray.GetPoint(dist);
        }
    }
}