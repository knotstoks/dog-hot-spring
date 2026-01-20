using ProjectRuntime.Gameplay;
using System.Collections.Generic;
using UnityEngine;

public class WallTile : MonoBehaviour
{
	[field: SerializeField]
	private SpriteRenderer TileSpriteRenderer { get; set; }

    public enum RuleTileType
    {
		NONE = 0,
        TOP = 1,
		RIGHT = 2,
		BOTTOM = 3,
		LEFT = 4,
		CORNER_TOP_LEFT = 5,
		CORNER_TOP_RIGHT = 6,
		CORNER_BOTTOM_RIGHT = 7,
		CORNER_BOTTOM_LEFT = 8,
		TOP_RIGHT = 9,
		BOTTOM_RIGHT = 10,
		BOTTOM_LEFT = 11,
		TOP_LEFT = 12,
		U_FACE_TOP = 13,
		U_FACE_RIGHT = 14,
		U_FACE_BOTTOM = 15,
		U_FACE_LEFT = 16,
		SURROUNDED = 17,
		BLANK = 18,
    }

	// It's 1am... I am sorry for this.
    private static readonly Dictionary<bool[,], RuleTileType> ParsedRuleTile = new()
    {
		{
			new bool[,]
			{
				{ true,     true,       true   },
				{ false,    false,      false  },
				{ false,    false,      false  },
			},
			RuleTileType.TOP
		},
		{
			new bool[,]
			{
				{ true,     true,       false },
				{ false,    false,      false },
				{ false,    false,      false },
			},
			RuleTileType.TOP
		},
		{
			new bool[,]
			{
				{ false,    true,       true  },
				{ false,    false,      false },
				{ false,    false,      false },
			},
			RuleTileType.TOP
		},
		{
			new bool[,]
			{
				{ false,     true,      false  },
				{ false,    false,      false },
				{ false,    false,      false },
			},
			RuleTileType.TOP
		},
		{
			new bool[,]
			{
				{ false,    false,      false },
				{ false,    false,      false },
				{ true,     true,       true  },
			},
			RuleTileType.BOTTOM
		},
		{
			new bool[,]
			{
				{ false,    false,      false },
				{ false,    false,      false },
				{ false,     true,      true  },
			},
			RuleTileType.BOTTOM
		},
		{
			new bool[,]
			{
				{ false,    false,      false },
				{ false,    false,      false },
				{ true,     true,       false },
			},
			RuleTileType.BOTTOM
		},
		{
			new bool[,]
			{
				{ false,    false,      false },
				{ false,    false,      false },
				{ false,     true,      false },
			},
			RuleTileType.BOTTOM
		},
		{
			new bool[,]
			{
				{ true,    false,     false },
				{ true,    false,     false },
				{ true,    false,     false },
			},
			RuleTileType.LEFT
		},
		{
			new bool[,]
			{
				{ false,   false,     false },
				{ true,    false,     false },
				{ true,    false,     false },
			},
			RuleTileType.LEFT
		},
		{
			new bool[,]
			{
				{ true,    false,     false },
				{ true,    false,     false },
				{ false,   false,     false },
			},
			RuleTileType.LEFT
		},
		{
			new bool[,]
			{
				{ false,    false,     false },
				{ true,    false,     false },
				{ false,    false,     false },
			},
			RuleTileType.LEFT
		},
		{
			new bool[,]
			{
				{ false,    false,     true },
				{ false,    false,     true },
				{ false,    false,     true },
			},
			RuleTileType.RIGHT
		},
		{
			new bool[,]
			{
				{ false,    false,     false },
				{ false,    false,     true  },
				{ false,    false,     true  },
			},
			RuleTileType.RIGHT
		},
		{
			new bool[,]
			{
				{ false,    false,     true  },
				{ false,    false,     true  },
				{ false,    false,     false },
			},
			RuleTileType.RIGHT
		},
		{
			new bool[,]
			{
				{ false,    false,     false  },
				{ false,    false,     true  },
				{ false,    false,     false },
			},
			RuleTileType.RIGHT
		},
		{
			new bool[,]
			{
				{ false,    false,      false },
				{ false,    false,      false },
				{ false,    false,      true  },
			}, RuleTileType.CORNER_BOTTOM_RIGHT
		},
		{
			new bool[,]
			{
				{ false,    false,      false },
				{ false,    false,      false },
				{ true,		false,		false },
			}, RuleTileType.CORNER_BOTTOM_LEFT
		},
		{
			new bool[,]
			{
				{ false,    false,      true  },
				{ false,    false,      false },
				{ false,    false,      false },
			}, RuleTileType.CORNER_TOP_RIGHT
		},
		{
			new bool[,]
			{
				{ true,     false,      false  },
				{ false,    false,      false },
				{ false,    false,      false },
			}, RuleTileType.CORNER_TOP_LEFT
		},
		{
			new bool[,]
			{
				{ false,    true,      true  },
				{ false,    false,     true  },
				{ false,    false,     false },
			}, RuleTileType.TOP_RIGHT
		},
		{
			new bool[,]
			{
				{ true,     true,      true  },
				{ false,    false,     true  },
				{ false,    false,     false },
			}, RuleTileType.TOP_RIGHT
		},
		{
			new bool[,]
			{
				{ false,    true,      true  },
				{ false,    false,     true  },
				{ false,    false,     true  },
			}, RuleTileType.TOP_RIGHT
		},
		{
			new bool[,]
			{
				{ true,     true,      true  },
				{ false,    false,     true  },
				{ false,    false,     true  },
			},
			RuleTileType.TOP_RIGHT
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
		{
			new bool[,]
			{
				{ false,    false,     false },
				{ false,    false,     true  },
				{ true,     true,      true  },
			}, RuleTileType.BOTTOM_RIGHT
		},
		{
			new bool[,]
			{
				{ false,    false,     true  },
				{ false,    false,     true  },
				{ true,     true,      true  },
			},
			RuleTileType.BOTTOM_RIGHT
		},
		{
			new bool[,]
			{
				{ false,    false,      false  },
				{ true,     false,      false  },
				{ true,     true,       false  },
			}, RuleTileType.BOTTOM_LEFT
		},
		{
			new bool[,]
			{
				{ true,     false,      false  },
				{ true,     false,      false  },
				{ true,     true,       false  },
			},
			RuleTileType.BOTTOM_LEFT
		},
		{
			new bool[,]
			{
				{ false,    false,      false  },
				{ true,     false,      false  },
				{ true,     true,       true   },
			}, RuleTileType.BOTTOM_LEFT
		},
		{
			new bool[,]
			{
				{ true,     false,      false  },
				{ true,     false,      false  },
				{ true,     true,       true   },
			}, RuleTileType.BOTTOM_LEFT
		},
		{
			new bool[,]
			{
				{ true,     true,      false  },
				{ true,     false,     false  },
				{ false,    false,     false  },
			},
			RuleTileType.TOP_LEFT
		},
		{
			new bool[,]
			{
				{ true,     true,      true   },
				{ true,     false,     false  },
				{ false,    false,     false  },
			},
			RuleTileType.TOP_LEFT
		},
		{
			new bool[,]
			{
				{ true,     true,      false  },
				{ true,     false,     false  },
				{ true,     false,     false },
			},
			RuleTileType.TOP_LEFT
		},
		{
			new bool[,]
			{
				{ true,     true,      true   },
				{ true,     false,     false  },
				{ true,     false,     false  },
			},
			RuleTileType.TOP_LEFT
		},
		{
			new bool[,]
			{
				{ true,     true,      true  },
				{ true,     false,     true  },
				{ false,    false,     false },
			},
			RuleTileType.U_FACE_TOP
		},
		{
			new bool[,]
			{
				{ true,     true,      true  },
				{ true,     false,     true  },
				{ true,     false,     false },
			},
			RuleTileType.U_FACE_TOP
		},
		{
			new bool[,]
			{
				{ true,     true,      true  },
				{ true,     false,     true  },
				{ false,    false,     true  },
			},
			RuleTileType.U_FACE_TOP
		},
		{
			new bool[,]
			{
				{ true,     true,      true  },
				{ true,     false,     true  },
				{ true,     false,     true  },
			},
			RuleTileType.U_FACE_TOP
		},
		{
			new bool[,]
			{
				{ false,     true,      true  },
				{ false,     false,     true  },
				{ false,     true,      true  },
			},
			RuleTileType.U_FACE_RIGHT
		},
		{
			new bool[,]
			{
				{ true,      true,      true  },
				{ false,     false,     true  },
				{ false,     true,      true  },
			},
			RuleTileType.U_FACE_RIGHT
		},
		{
			new bool[,]
			{
				{ false,     true,      true  },
				{ false,     false,     true  },
				{ true,      true,      true  },
			},
			RuleTileType.U_FACE_RIGHT
		},
		{
			new bool[,]
			{
				{ true,      true,      true  },
				{ false,     false,     true  },
				{ true,      true,      true  },
			},
			RuleTileType.U_FACE_RIGHT
		},
		{
			new bool[,]
			{
				{ false,     false,     false  },
				{ true,      false,     true   },
				{ true,      true,      true   },
			},
			RuleTileType.U_FACE_BOTTOM
		},
		{
			new bool[,]
			{
				{ true,      false,     false  },
				{ true,      false,     true   },
				{ true,      true,      true   },
			},
			RuleTileType.U_FACE_BOTTOM
		},
		{
			new bool[,]
			{
				{ false,     false,     true   },
				{ true,      false,     true   },
				{ true,      true,      true   },
			},
			RuleTileType.U_FACE_BOTTOM
		},
		{
			new bool[,]
			{
				{ true,      false,     true   },
				{ true,      false,     true   },
				{ true,      true,      true   },
			},
			RuleTileType.U_FACE_BOTTOM
		},
		{
			new bool[,]
			{
				{ true,      true,      false   },
				{ true,      false,     false   },
				{ true,      true,      false   },
			},
			RuleTileType.U_FACE_LEFT
		},
		{
			new bool[,]
			{
				{ true,      true,      true    },
				{ true,      false,     false   },
				{ true,      true,      false   },
			},
			RuleTileType.U_FACE_LEFT
		},
		{
			new bool[,]
			{
				{ true,      true,      false   },
				{ true,      false,     false   },
				{ true,      true,      true    },
			},
			RuleTileType.U_FACE_LEFT
		},
		{
			new bool[,]
			{
				{ true,      true,      true    },
				{ true,      false,     false   },
				{ true,      true,      true    },
			},
			RuleTileType.U_FACE_LEFT
		},
		{
			new bool[,]
			{
				{ true,    true,     true },
				{ true,    false,    true },
				{ true,    true,     true },
			}, RuleTileType.SURROUNDED
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
	
    private readonly bool[,] BlockedAreas =
    {
        { false, false, false },
		{ false, false, false },
		{ false, false, false },
	};

	[Sirenix.OdinInspector.ShowInInspector, Sirenix.OdinInspector.ReadOnly]
	public Vector2Int TileYXPosition { get; set; } = new(-1, -1);

	private RuleTileType _tileType;

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

		this._tileType = GetTileType(BlockedAreas);

		// TODO: Set the sprite for tile
	}

	private RuleTileType GetTileType(bool[,] blockedAreas)
	{
        foreach (var tileCombi in ParsedRuleTile.Keys)
        {
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					if (tileCombi[i, j] != blockedAreas[i, j]) break;
				}
			}

			return ParsedRuleTile[tileCombi];
        }

		Debug.LogError("No matching tile found!");
		return RuleTileType.NONE;
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
