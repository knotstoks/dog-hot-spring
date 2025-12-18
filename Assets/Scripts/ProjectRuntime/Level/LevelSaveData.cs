using System.Collections.Generic;
using ProjectRuntime.Gameplay;
using UnityEngine;

namespace ProjectRuntime.Level
{
    public struct LevelSaveData
    {
        public int GridHeight { get; set; }
        public int GridWidth { get; set; }
        public List<Vector2Int> LockedTiles { get; set; }
        public List<TileSaveData> TileSaveDatas { get; set; }
        public List<AnimalSaveData> AnimalSaveDatas { get; set; }
        public List<QueueSaveData> QueueTileSaveDatas { get; set; }
        public List<IceTileSaveData> IceTileSaveDatas { get; set; }
        public List<EmptyTileSaveData> EmptyTileSaveDatas { get; set; }

        public LevelSaveData(int gridHeight, int gridWidth, List<Vector2Int> lockedTiles, List<TileSaveData> tileSaveDatas, List<AnimalSaveData> animalSaveDatas,
            List<QueueSaveData> queueSaveDatas, List<IceTileSaveData> iceSaveDatas, List<EmptyTileSaveData> emptyTileSaveDatas)
        {
            this.GridHeight = gridHeight;
            this.GridWidth = gridWidth;
            this.LockedTiles = lockedTiles;
            this.TileSaveDatas = tileSaveDatas;
            this.AnimalSaveDatas = animalSaveDatas;
            this.QueueTileSaveDatas = queueSaveDatas;
            this.IceTileSaveDatas = iceSaveDatas;
            this.EmptyTileSaveDatas = emptyTileSaveDatas;
        }
    }

    public struct TileSaveData
    {
        public int TileId { get; set; }
        public TileColor TileColor { get; set; }
        public Vector2Int TileYX { get; set; }
        public int DropsLeft { get; set; }

        public TileSaveData(int tileId, TileColor tileColor, Vector2Int tileYX, int dropsLeft)
        {
            this.TileId = tileId;
            this.TileColor = tileColor;
            this.TileYX = tileYX;
            this.DropsLeft = dropsLeft;
        }
    }

    public struct AnimalSaveData
    {
        public TileColor AnimalColor { get; set; }
        public Vector2Int TileYX { get; set; }

        public AnimalSaveData(TileColor tileColor, Vector2Int tileYX)
        {
            this.AnimalColor = tileColor;
            this.TileYX = tileYX;
        }
    }

    public struct QueueSaveData
    {
        public Vector2Int TileYX { get; set; }
        public QueueTileDirection FacingDirection { get; set; }
        public Queue<TileColor> QueueColours { get; set; }

        public QueueSaveData(Vector2Int tileYX, QueueTileDirection facingDirection, Queue<TileColor> queueColours)
        {
            this.TileYX = tileYX;
            this.FacingDirection = facingDirection;
            this.QueueColours = queueColours;
        }
    }

    public struct IceTileSaveData
    {
        public int TileId { get; set; }
        public TileColor TileColor { get; set; }
        public Vector2Int TileYX { get; set; }
        public int DropsLeft { get; set; }
        public int IceCracksLeft { get; set; }

        public IceTileSaveData(int tileId, TileColor tileColor, Vector2Int tileYX, int dropsLeft, int iceCracksLeft)
        {
            this.TileId = tileId;
            this.TileColor = tileColor;
            this.TileYX = tileYX;
            this.DropsLeft = dropsLeft;
            this.IceCracksLeft = iceCracksLeft;
        }
    }

    public struct EmptyTileSaveData
    {
        public int TileId { get; set; }
        public Vector2Int TileYX { get; set; }

        public EmptyTileSaveData(int tileId, Vector2Int tileYX)
        {
            this.TileId = tileId;
            this.TileYX = tileYX;
        }
    }
}