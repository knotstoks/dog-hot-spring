using System;
using System.Collections.Generic;
using ProjectRuntime.Gameplay;
using UnityEngine;

namespace ProjectRuntime.Level
{
    public struct LevelSaveData
    {
        [field: SerializeField]
        public int GridHeight { get; set; }

        [field: SerializeField]
        public int GridWidth { get; set; }

        [field: SerializeField]
        public List<Vector2Int> LockedTiles { get; set; }

        [field: SerializeField]
        public List<TileSaveData> TileSaveDatas { get; set; }

        [field: SerializeField]
        public List<AnimalSaveData> AnimalSaveDatas { get; set; }

        [field: SerializeField]
        public List<QueueSaveData> QueueTileSaveDatas { get; set; }

        public LevelSaveData(int gridHeight, int gridWidth, List<Vector2Int> lockedTiles, List<TileSaveData> tileSaveDatas, List<AnimalSaveData> animalSaveDatas, List<QueueSaveData> queueSaveDatas)
        {
            this.GridHeight = gridHeight;
            this.GridWidth = gridWidth;
            this.LockedTiles = lockedTiles;
            this.TileSaveDatas = tileSaveDatas;
            this.AnimalSaveDatas = animalSaveDatas;
            this.QueueTileSaveDatas = queueSaveDatas;
        }
    }
    

    [Serializable]
    public struct TileSaveData
    {
        [field: SerializeField]
        public int TileId { get; set; }

        [field: SerializeField]
        public TileColor TileColor { get; set; }

        [field: SerializeField]
        public Vector2Int TileYX { get; set; }

        [field: SerializeField]
        public int DropsLeft { get; set; }

        public TileSaveData(int tileId, TileColor tileColor, Vector2Int tileYX, int dropsLeft)
        {
            this.TileId = tileId;
            this.TileColor = tileColor;
            this.TileYX = tileYX;
            this.DropsLeft = dropsLeft;
        }
    }

    [Serializable]
    public struct AnimalSaveData
    {
        [field: SerializeField]
        public TileColor AnimalColor { get; set; }

        [field: SerializeField]
        public Vector2Int TileYX { get; set; }

        public AnimalSaveData(TileColor tileColor, Vector2Int tileYX)
        {
            this.AnimalColor = tileColor;
            this.TileYX = tileYX;
        }
    }

    [Serializable]
    public struct QueueSaveData
    {
        public QueueSaveData(int queueTileId, Vector2Int tileYX, QueueTileDirection facingDirection, Queue<TileColor> queueColours)
        {
            this.QueueTileId = queueTileId;
            this.TileYX = tileYX;
            this.FacingDirection = facingDirection;
            this.QueueColours = queueColours;
        }

        [field: SerializeField]
        public int QueueTileId { get; set; }

        [field: SerializeField]
        public Vector2Int TileYX { get; set; }
        [field: SerializeField]
        public QueueTileDirection FacingDirection { get; set; }

        [field: SerializeField]
        public Queue<TileColor> QueueColours { get; set; }


       
    }
}