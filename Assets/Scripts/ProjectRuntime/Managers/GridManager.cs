using Cysharp.Threading.Tasks;
using ProjectRuntime.Gameplay;
using System.Collections.Generic;
using UnityEngine;

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
        public GameObject WallTilePrefab { get; private set; }

        [field: SerializeField]
        public float TileWidth { get; private set; } = 1f;

        [field: SerializeField]
        public float TileHeight { get; private set; } = 1f;

        [field: SerializeField]
        public float TileGap { get; private set; } = 0.1f;

        [field: SerializeField, Header("References")]
        public Transform TileContainer { get; private set; }

        // Accessible variables
        public BackgroundTile[,] Tiles { get; private set; }      // 2D Array of Tiles in the grid

        // Internal variables
        private Vector3 _bottomLeftOffset;                      // Local offset from pivot of BackpackMainArea for bottomleft-most tile
        private int _finalGridWidth;
        private int _finalGridHeight;

        // Animal Drop Tracking
        private Dictionary<TileColor, List<AnimalDrop>> _animalDropDict;
        private Dictionary<Vector2Int, AnimalDrop> _animalDropPositionDict;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogError("There are 2 or more GridManagers in the scene");
            }

            this._animalDropDict = new();
            this._animalDropPositionDict = new();

            this._finalGridWidth = this.GridWidth + 2;
            this._finalGridHeight = this.GridHeight + 2;

            var totalWidth = this._finalGridWidth * this.TileWidth + (this._finalGridWidth - 1) * this.TileGap;
            var totalHeight = this._finalGridHeight * this.TileHeight + (this._finalGridHeight - 1) * this.TileGap;
            this._bottomLeftOffset = new Vector3(-totalWidth / 2, -totalHeight / 2, 0);
            this._bottomLeftOffset += new Vector3(this.TileWidth / 2, this.TileHeight / 2, 0); // Offset as tile is anchored at center

            this.Tiles = new BackgroundTile[this._finalGridHeight, this._finalGridWidth];

            // Create the grid of tiles
            for (var rowY = 0; rowY < _finalGridHeight; rowY++)
            {
                for (var colX = 0; colX < this._finalGridWidth; colX++)
                {
                    if (colX == 0 || colX == this._finalGridWidth - 1
                        || rowY == 0 || rowY == this._finalGridHeight - 1)
                    {
                        // Spawn a wall tile
                        var wallTile = Instantiate(this.WallTilePrefab, this.TileContainer);
                        wallTile.transform.localPosition = this.GetTilePosition(rowY, colX);
                    }
                    else
                    {
                        // Instantiate and initialize
                        var tile = Instantiate(this.BackgroundTilePrefab, this.TileContainer);
                        tile.transform.localPosition = this.GetTilePosition(rowY, colX);
                        tile.UnhighlightTile();
                        tile.TileYXPosition = new(colX, rowY);

                        // Store in array
                        this.Tiles[rowY, colX] = tile;
                    }
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
            return this._bottomLeftOffset + new Vector3(colX, rowY, 0) * (this.TileWidth + this.TileGap);
        }

        /// <summary>
        /// The reverse of GetTilePosition(), returns the nearest tile (rowY, colX) given a position in TileContainer local space.
        /// </summary>
        public Vector2Int GetNearestTileYX(Vector3 pos)
        {
            var p = (pos - this._bottomLeftOffset) / (this.TileWidth + this.TileGap);
            return new Vector2Int(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y));
        }

        public void SnapToGrid(BathSlideTile tile, Vector2Int tileYX)
        {
            // Set position
            var tilePos = this.GetTilePosition(tileYX.y, tileYX.x);
            var offset = this.TileContainer.InverseTransformVector(tile.transform.position - tile.BottomLeftTransform.position);
            tile.transform.localPosition = tilePos + offset;
        }

        public void HighlightBackgroundTilesForTile(BathSlideTile tile, Vector2Int tileYX)
        {
            // Remove highlight for everything
            for (var rowY = 1; rowY < this._finalGridHeight - 1; rowY++)
            {
                for (var colX = 1; colX < this._finalGridWidth - 1; colX++)
                {
                    this.Tiles[rowY, colX].UnhighlightTile();
                }
            }

            var tileShape = tile.TileShape;
            // Highlight the positions the Tile would occupy
            for (var rowY = 0; rowY < tileShape.Height; rowY++)
            {
                for (var colX = 0; colX < tileShape.Width; colX++)
                {
                    // Shape occupies this tile
                    if (tileShape[rowY][colX])
                    {
                        // Highlight the unlocked tiles
                        this.Tiles[tileYX.y + rowY, tileYX.x + colX].HighlightTile(tile.TileColor);
                        if (this._animalDropPositionDict.TryGetValue(new Vector2Int(tileYX.x + colX, tileYX.y + rowY), out var animalDrop))
                        {
                            animalDrop.Drop();
                        }
                    }
                }
            }
            
        }

        public void RegisterAnimalDrop(AnimalDrop animalDrop)
        {
            if (!this._animalDropDict.ContainsKey(animalDrop.TileColor))
            {
                this._animalDropDict[animalDrop.TileColor] = new();
            }

            this._animalDropDict[animalDrop.TileColor].Add(animalDrop);
            this._animalDropPositionDict[this.GetNearestTileYX(animalDrop.transform.position)] = animalDrop;
        }

        public void DeregisterAnimalDrop(AnimalDrop animalDrop)
        {
            if (this._animalDropDict[animalDrop.TileColor].Contains(animalDrop))
            {
                this._animalDropDict[animalDrop.TileColor].Remove(animalDrop);
            }

            this._animalDropPositionDict.Remove(this.GetNearestTileYX(animalDrop.transform.position));
        }

        public void ToggleDropColor(TileColor tileColor, bool isDroppable)
        {
            if (this._animalDropDict.TryGetValue(tileColor, out var animalDropList))
            {
                foreach (var animalDrop in animalDropList)
                {
                    animalDrop.ToggleTriggerCollider(isDroppable);
                }
            }
        }
    }
}