using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BroccoliBunnyStudios.Pools;
using BroccoliBunnyStudios.Utils;
using Cysharp.Threading.Tasks;
using ProjectRuntime.Gameplay;
using ProjectRuntime.Level;
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

        public int GridWidth { get; private set; }
        public int GridHeight { get; private set; }

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
        public WorldData CurrentWorldData => this._dWorld;

        // Internal variables
        private WorldData _dWorld;

        private Vector3 _bottomLeftOffset;                      // Local offset from pivot of BackpackMainArea for bottomleft-most tile
        private int _finalGridWidth;
        private int _finalGridHeight;
        private bool _alreadyPlayingVictory;

        // Animal Drop Tracking
        private Dictionary<TileColor, List<AnimalDrop>> _animalDropDict;
        private Dictionary<Vector2Int, AnimalDrop> _animalDropPositionDict;

        private Dictionary<Vector2Int, QueueTile> _queueDropPositionDict;

        // Events
        public event Action OnBathTileCompleted;

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
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public async UniTask Init(int worldId)
        {
            this._dWorld = DWorld.GetDataById(worldId).Value;

            var levelData = this.ParseLevelSaveData(this._dWorld.ParsedLevel);

            await UniTask.WaitUntil(() => CameraManager.Instance != null);
            CameraManager.Instance.SetCameraScale(levelData.GridHeight);

            this.GridHeight = levelData.GridHeight;
            this.GridWidth = levelData.GridWidth;
            this._alreadyPlayingVictory = false;
            this._animalDropDict = new();
            this._animalDropPositionDict = new();
            this._queueDropPositionDict = new();

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
                        || rowY == 0 || rowY == this._finalGridHeight - 1
                        || this.IsLockedTile(levelData.LockedTiles, new Vector2Int(rowY, colX)))
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

            // Create Bath Slide Tiles
            foreach (var bathSlideTile in levelData.TileSaveDatas)
            {
                var tilePrefabPath = string.Format("prefabs/bath_tiles/tile_{0}.prefab", bathSlideTile.TileId.ToString());
                var tileObject = await ResourceLoader.InstantiateAsync(tilePrefabPath, this.TileContainer);
                if (!this) return;
                var tile = tileObject.GetComponent<BathSlideTile>();

                // Set position
                var tilePos = this.GetTilePosition(bathSlideTile.TileYX.x, bathSlideTile.TileYX.y);
                var offset = this.TileContainer.InverseTransformVector(tileObject.transform.position - tile.BottomLeftTransform.position);
                tileObject.transform.localPosition = tilePos + offset;

                tile.Init(bathSlideTile.TileId, bathSlideTile.TileColor, bathSlideTile.DropsLeft);
            }

            // Create Animals
            foreach (var animalTile in levelData.AnimalSaveDatas)
            {
                var animalPrefabPath = string.Format("prefabs/animals/animal_{0}.prefab", animalTile.AnimalColor.ToString().ToLowerInvariant());
                var animalObject = await ResourceLoader.InstantiateAsync(animalPrefabPath, this.TileContainer);
                if (!this) return;

                var animalTilePos = this.GetTilePosition(animalTile.TileYX.x, animalTile.TileYX.y);
                animalTilePos.z = 0.01f;
                animalObject.transform.position = animalTilePos;

                var animal = animalObject.GetComponent<AnimalDrop>();
                animal.Init();
            }

            // Create Queue Tiles
            foreach (var queueTile in levelData.QueueTileSaveDatas)
            {
                var queuePrefabPath = "prefabs/queue_tiles/queue_tile.prefab";
                var queueObject = await ResourceLoader.InstantiateAsync(queuePrefabPath, this.TileContainer);
                if (!this) return;

                var queueTilePos = this.GetTilePosition(queueTile.TileYX.x, queueTile.TileYX.y);
                queueTilePos.z = 0.01f;
                queueObject.transform.position = queueTilePos;

                var queue = queueObject.GetComponent<QueueTile>();
                queue.Init(queueTile.FacingDirection, queueTile.QueueColours, TileHeight, TileWidth);
            }

            // Create Ice Tiles
            foreach (var iceTile in levelData.IceTileSaveDatas)
            {
                var iceTilePrefabPath = string.Format("prefabs/bath_tiles/tile_{0}.prefab", iceTile.TileId.ToString());
                var tileObject = await ResourceLoader.InstantiateAsync(iceTilePrefabPath, this.TileContainer);
                if (!this) return;
                var tile = tileObject.GetComponent<BathSlideTile>();

                // Set position
                var tilePos = this.GetTilePosition(iceTile.TileYX.x, iceTile.TileYX.y);
                var offset = this.TileContainer.InverseTransformVector(tileObject.transform.position - tile.BottomLeftTransform.position);
                tileObject.transform.localPosition = tilePos + offset;

                tile.Init(iceTile.TileId, iceTile.TileColor, iceTile.DropsLeft, iceTile.IceCracksLeft);
            }
        }

        private bool IsLockedTile(List<Vector2Int> lockedTiles, Vector2Int tileYX)
        {
            foreach (var lockedTile in lockedTiles)
            {
                if (lockedTile.x == tileYX.x && lockedTile.y == tileYX.y)
                {
                    return true;
                }
            }

            return false;
        }

        private LevelSaveData ParseLevelSaveData(string s)
        {
            var stringSplit = s.Split(',');
            var gridHeight = CommonUtil.ConvertToInt32(stringSplit[0]);
            var gridWidth = CommonUtil.ConvertToInt32(stringSplit[1]);

            var lockedTilesSplit = Regex.Matches(stringSplit[2], @"\((.*?)\)")
                       .Select(m => m.Groups[1].Value)
                       .ToList();
            var lockedTiles = new List<Vector2Int>();
            foreach (var lockedTile in lockedTilesSplit)
            {
                var lockedTileSplit = lockedTile.Split(':', StringSplitOptions.RemoveEmptyEntries);
                lockedTiles.Add(new Vector2Int(CommonUtil.ConvertToInt32(lockedTileSplit[0]), CommonUtil.ConvertToInt32(lockedTileSplit[1])));
            }

            var slideTileLocations = Regex.Matches(stringSplit[3], @"\((.*?)\)")
                       .Select(m => m.Groups[1].Value)
                       .ToList();
            var slideTiles = new List<TileSaveData>();
            foreach (var slideTile in slideTileLocations)
            {
                var slideTileSplit = slideTile.Split(':', StringSplitOptions.RemoveEmptyEntries);
                slideTiles.Add(new TileSaveData(CommonUtil.ConvertToInt32(slideTileSplit[0]),
                    Enum.TryParse(slideTileSplit[1], out TileColor slideTileColor) ? slideTileColor : TileColor.NONE,
                    new Vector2Int(CommonUtil.ConvertToInt32(slideTileSplit[2]), CommonUtil.ConvertToInt32(slideTileSplit[3])),
                    CommonUtil.ConvertToInt32(slideTileSplit[4])));
            }

            var animalLocations = Regex.Matches(stringSplit[4], @"\((.*?)\)")
                       .Select(m => m.Groups[1].Value)
                       .ToList();
            var animals = new List<AnimalSaveData>();
            foreach (var animal in animalLocations)
            {
                var animalSplit = animal.Split(':', StringSplitOptions.RemoveEmptyEntries);
                animals.Add(new AnimalSaveData(Enum.TryParse(animalSplit[0], out TileColor animalTileColor) ? animalTileColor : TileColor.NONE,
                    new Vector2Int(CommonUtil.ConvertToInt32(animalSplit[1]), CommonUtil.ConvertToInt32(animalSplit[2]))));
            }

            var queueTileLocations = Regex.Matches(stringSplit[5], @"\((.*?)\)")
                .Select(m => m.Groups[1].Value)
                .ToList();
            var queueTiles = new List<QueueSaveData>();
            foreach (var queue in queueTileLocations)
            {
                var queueSplit = queue.Split(':', StringSplitOptions.RemoveEmptyEntries);

                var rowY = CommonUtil.ConvertToInt32(queueSplit[0]);
                var colX = CommonUtil.ConvertToInt32(queueSplit[1]);
                var direction = ParseQueueTileDirectionString(queueSplit[2]);

                var queueColorsList = new Queue<TileColor>();

                for (var i = 3; i < queueSplit.Length; i += 2)
                {
                    var dropColour = Enum.TryParse<TileColor>(queueSplit[i], true, out var resultColour) ? resultColour : TileColor.NONE;
                    var dropsLeft = CommonUtil.ConvertToInt32(queueSplit[i + 1]);

                    for (var j = 0; j < dropsLeft; j++)
                    {
                        queueColorsList.Enqueue(dropColour);
                    }
                }
                queueTiles.Add(new QueueSaveData(new Vector2Int(rowY, colX), direction, queueColorsList));
            }

            var iceTileLocations = Regex.Matches(stringSplit[6], @"\((.*?)\)")
                .Select(m => m.Groups[1].Value)
                .ToList();
            var iceTiles = new List<IceTileSaveData>();
            foreach (var iceTile in iceTileLocations)
            {
                var iceTileSplit = iceTile.Split(':', StringSplitOptions.RemoveEmptyEntries);
                iceTiles.Add(new IceTileSaveData(CommonUtil.ConvertToInt32(iceTileSplit[0]),
                    Enum.TryParse(iceTileSplit[1], out TileColor slideTileColor) ? slideTileColor : TileColor.NONE,
                    new Vector2Int(CommonUtil.ConvertToInt32(iceTileSplit[2]), CommonUtil.ConvertToInt32(iceTileSplit[3])),
                    CommonUtil.ConvertToInt32(iceTileSplit[4]), CommonUtil.ConvertToInt32(iceTileSplit[5])));
            }

            return new LevelSaveData(gridHeight, gridWidth, lockedTiles, slideTiles, animals, queueTiles, iceTiles);
        }

        public QueueTileDirection ParseQueueTileDirectionString(string s)
        {
            switch (s)
            {
                case "N":
                    return QueueTileDirection.NORTH;
                case "S":
                    return QueueTileDirection.SOUTH;
                case "E":
                    return QueueTileDirection.EAST;
                case "W":
                    return QueueTileDirection.WEST;


                default:
                    break;
            }

            return QueueTileDirection.NONE;
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
            if (BathSlideTile.CurrentDraggedTile == null)
            {
                return;
            }

            // Remove highlight for everything
            for (var rowY = 1; rowY < this._finalGridHeight - 1; rowY++)
            {
                for (var colX = 1; colX < this._finalGridWidth - 1; colX++)
                {
                    if (this.Tiles[rowY, colX])
                    {
                        this.Tiles[rowY, colX].UnhighlightTile();
                    }
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
                            animalDrop.Drop(BathSlideTile.CurrentDraggedTile);
                        }

                        if (this._queueDropPositionDict.TryGetValue(new Vector2Int(tileYX.x + colX, tileYX.y + rowY), out var queueDrop))
                        {
                            queueDrop.Drop(BathSlideTile.CurrentDraggedTile);
                        }
                    }
                }
            }
        }

        public void DetectForVictory()
        {
            if (this._animalDropPositionDict.Count != 0) return;
            if (this._queueDropPositionDict.Count != 0) return;
            if (this._alreadyPlayingVictory) return;

            this._alreadyPlayingVictory = true;
            BattleManager.Instance.ShowVictoryPanel();
        }

        public void ResetHighlightsForAllTiles()
        {
            // Remove highlight for everything
            for (var rowY = 1; rowY < this._finalGridHeight - 1; rowY++)
            {
                for (var colX = 1; colX < this._finalGridWidth - 1; colX++)
                {
                    if (this.Tiles[rowY, colX])
                    {
                        this.Tiles[rowY, colX].UnhighlightTile();
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

        public void RegisterQueueTile(QueueTile queueTile)
        {
            this._queueDropPositionDict[this.GetNearestTileYX(queueTile.TileDetectionPosition)] = queueTile;
        }

        public void DeregisterAnimalDrop(AnimalDrop animalDrop)
        {
            if (this._animalDropDict[animalDrop.TileColor].Contains(animalDrop))
            {
                this._animalDropDict[animalDrop.TileColor].Remove(animalDrop);
            }

            this._animalDropPositionDict.Remove(this.GetNearestTileYX(animalDrop.transform.position));
        }

        public void DeregisterQueueDrop(QueueTile queueTile)
        {
            this._queueDropPositionDict.Remove(this.GetNearestTileYX(queueTile.TileDetectionPosition));
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

        public void OnBathTileComplete()
        {
            this.OnBathTileCompleted?.Invoke();
        }
    }
}