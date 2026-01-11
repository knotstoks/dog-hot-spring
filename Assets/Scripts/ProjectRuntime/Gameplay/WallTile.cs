using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using ProjectRuntime.Gameplay;
using UnityEngine;

public class WallTile : MonoBehaviour
{
    public enum RuleTileType
    {
		NONE,
        TOP_LEFT,
        TOP_RIGHT,
        BOTTOM_LEFT,
        BOTTOM_RIGHT,
        HORIZONTAL_UPPER,
        HORIZONTAL_LOWER,
        VERTICAL_LEFT,
        VERTICAL_RIGHT,
        SURRONDED,
		BLANK,
		ERROR
    }

	// It's 1am... I am sorry for this.
    private readonly Dictionary<bool[,], RuleTileType> ParsedRuleTile = new()
    {
# region CORNERS
		{
			new bool[,]
			{
				{ false,    false,      false },
				{ false,    false,      false },
				{ false,    false,      true  },
			}, RuleTileType.TOP_LEFT
		},
		{
			new bool[,]
			{
				{ false,    false,      false },
				{ false,    false,      false },
				{ true,		false,		false },
			}, RuleTileType.TOP_RIGHT
		},
		{
			new bool[,]
			{
				{ false,    false,      true  },
				{ false,    false,      false },
				{ false,    false,      false },
			}, RuleTileType.BOTTOM_LEFT
		},
		{
			new bool[,]
			{
				{ false,    false,      false  },
				{ true,     false,      false  },
				{ true,     true,		false  },
			}, RuleTileType.BOTTOM_LEFT
		},
		{
			new bool[,]
			{
				{ true,    false,      false  },
				{ true,     false,      false  },
				{ true,     true,       false  },
			}, RuleTileType.BOTTOM_LEFT
		},
		{ 
            new bool[,] 
            { 
                { true,     false,		false },
                { false,    false,      false },
                { false,    false,      false },
            }, RuleTileType.BOTTOM_RIGHT
		},
		{
			new bool[,]
			{
				{ false,    false,      false },
				{ false,    false,      true  },
				{ false,    true,       true  },
			}, RuleTileType.BOTTOM_RIGHT
		},
		{
			new bool[,]
			{
				{ false,    false,      true },
				{ false,    false,      true  },
				{ false,    true,       true  },
			}, RuleTileType.BOTTOM_RIGHT
		},
#endregion
		{
			new bool[,]
			{
				{ true,		true,		true   },
				{ false,    false,      false  },
				{ false,    false,      false  },
			}, RuleTileType.HORIZONTAL_LOWER
		},
		{
			new bool[,]
			{
				{ true,     true,       false },
				{ false,    false,      false },
				{ false,    false,      false },
			}, RuleTileType.HORIZONTAL_LOWER
		},
		{
			new bool[,]
			{
				{ false,     true,       true  },
				{ false,    false,      false },
				{ false,    false,       false },
			}, RuleTileType.HORIZONTAL_LOWER
		},
		{
			new bool[,]
			{
				{ false,     true,       false  },
				{ false,    false,       false },
				{ false,    false,       false },
			}, RuleTileType.HORIZONTAL_LOWER
		},
		{
			new bool[,]
			{
				{ false,    false,      false },
				{ false,    false,		false },
				{ true,		true,		true  },
			}, RuleTileType.HORIZONTAL_UPPER
		},
		{
			new bool[,]
			{
				{ false,    false,      false },
				{ false,    false,      false },
				{ false,     true,      true  },
			}, RuleTileType.HORIZONTAL_UPPER
		},
		{
			new bool[,]
			{
				{ false,    false,      false },
				{ false,    false,      false },
				{ true,     true,       false },
			}, RuleTileType.HORIZONTAL_UPPER
		},
		{
			new bool[,]
			{
				{ false,    false,      false },
				{ false,    false,      false },
				{ false,     true,       false },
			}, RuleTileType.HORIZONTAL_UPPER
		},
		{
			new bool[,]
			{
				{ true,    false,     false },
				{ true,    false,     false },
				{ true,    false,     false },
			}, RuleTileType.VERTICAL_RIGHT
		},
		{
			new bool[,]
			{
				{ false,    false,     false },
				{ true,    false,     false },
				{ true,    false,     false },
			}, RuleTileType.VERTICAL_RIGHT
		},
		{
			new bool[,]
			{
				{ true,    false,     false },
				{ true,    false,     false },
				{ false,    false,     false },
			}, RuleTileType.VERTICAL_RIGHT
		},
		{
			new bool[,]
			{
				{ false,    false,     false },
				{ true,    false,     false },
				{ false,    false,     false },
			}, RuleTileType.VERTICAL_RIGHT
		},
		{
			new bool[,]
			{
				{ false,    false,     true },
				{ false,    false,     true },
				{ false,    false,     true },
			}, RuleTileType.VERTICAL_LEFT
		},
		{
			new bool[,]
			{
				{ false,    false,     false },
				{ false,    false,     true	 },
				{ false,    false,     true  },
			}, RuleTileType.VERTICAL_LEFT
		},
		{
			new bool[,]
			{
				{ false,    false,     true  },
				{ false,    false,     true  },
				{ false,    false,     false },
			}, RuleTileType.VERTICAL_LEFT
		},
		{
			new bool[,]
			{
				{ false,    false,     false  },
				{ false,    false,     true  },
				{ false,    false,     false },
			}, RuleTileType.VERTICAL_LEFT
		},
		{
			new bool[,]
			{
				{ true,    true,     true },
				{ true,    false,    true },
				{ true,    true,     true },
			}, RuleTileType.SURRONDED
		},
		{
			new bool[,]
			{
				{ false,    false,    false },
				{ false,    false,    false },
				{ false,    false,    false },
			}, RuleTileType.BLANK
		},
	};

	
    private bool[,] BlockedAreas = 
    {
        { false, false, false },
		{ false, false, false },
		{ false, false, false },
	};

	[Sirenix.OdinInspector.ShowInInspector, Sirenix.OdinInspector.ReadOnly]
	public Vector2Int TileYXPosition { get; set; } = new(-1, -1);

	public RuleTileType TileType;

	public void Init(BackgroundTile[,] tiles)
    {
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                var rowY = TileYXPosition.y - i;
                var colX = TileYXPosition.x + j;
                BlockedAreas[i+1, j+1] = QueryTile(tiles, rowY, colX);
			}
        }
		TileType = GetTileType(BlockedAreas);
	}

	private RuleTileType GetTileType(bool[,] blockedAreas)
	{
		var isTileFound = true;		

        foreach (var tileCombi in ParsedRuleTile.Keys)
        {
			isTileFound = true;
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					if (tileCombi[i, j] != blockedAreas[i, j]) isTileFound = false;

					if (!isTileFound) break;
				}

				if (!isTileFound) break;
			}

			if (isTileFound)
			{
				var combi = ParsedRuleTile[tileCombi];
				return combi;
			}
        }

		Debug.LogError("No matching tile found!");
		return RuleTileType.ERROR;
    }

    private bool QueryTile(BackgroundTile[,] tiles, int rowY, int colX)
    {
        var colX_len = tiles.GetLength(1);
        var rowY_len = tiles.GetLength(0);

        if (colX < 0 || colX + 1 > colX_len) return false;
		if (rowY < 0 || rowY + 1 > rowY_len) return false;

		// print($"{colX}, {rowY}");
        var tile = tiles[rowY, colX];

        if (tile == null) return false;

		return true;
    }
}
