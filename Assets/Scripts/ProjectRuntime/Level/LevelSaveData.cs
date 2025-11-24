using System;
using System.Collections.Generic;
using ProjectRuntime.Gameplay;
using UnityEngine;

namespace ProjectRuntime.Level
{
    public class LevelSaveData
    {
        [field: SerializeField]
        public int GridHeight { get; set; }

        [field: SerializeField]
        public int GridWidth { get; set; }

        [field: SerializeField]
        public List<Vector2Int> UnlockedTiles { get; set; }

        [field: SerializeField]
        public List<TileSaveData> TileSaveDatas { get; set; }

        [field: SerializeField]
        public List<AnimalSaveData> AnimalSaveDatas { get; set; }
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

        public TileSaveData(int tileId, TileColor tileColor, Vector2Int tileYX)
        {
            this.TileId = tileId;
            this.TileColor = tileColor;
            this.TileYX = tileYX;
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
}