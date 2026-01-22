using ProjectRuntime.Gameplay;
using System.Collections.Generic;
using UnityEngine;

public class WallTile : MonoBehaviour
{
	[field: SerializeField]
	private SpriteRenderer TileSpriteRenderer { get; set; }

	[field: SerializeField, Header("Wall Tile Sprties")]
	private Sprite TopSprite { get; set; }

	[field: SerializeField]
	private Sprite RightSprite { get; set; }

	[field: SerializeField]
	private Sprite BottomSprite { get; set; }

	[field: SerializeField]
	private Sprite LeftSprite { get; set; }

	[field: SerializeField]
	private Sprite CornerTopLeftSprite { get; set; }

	[field: SerializeField]
	private Sprite CornerTopRightSprite { get; set; }

	[field: SerializeField]
	private Sprite CornerBottomRightSprite { get; set; }

	[field: SerializeField]
	private Sprite CornerBottomLeftSprite { get; set; }

	[field: SerializeField]
	private Sprite TopRightSprite { get; set; }

	[field: SerializeField]
	private Sprite BottomRightSprite { get; set; }

	[field: SerializeField]
	private Sprite BottomLeftSprite { get; set; }

	[field: SerializeField]
	private Sprite TopLeftSprite { get; set; }

	[field: SerializeField]
	private Sprite UFaceTopSprite { get; set; }

	[field: SerializeField]
	private Sprite UFaceRightSprite { get; set; }

	[field: SerializeField]
	private Sprite UFaceBottomSprite { get; set; }

	[field: SerializeField]
	private Sprite UFaceLeftSprite { get; set; }

	[field: SerializeField]
	private Sprite TwoCornersTopSprite { get; set; }

	[field: SerializeField]
	private Sprite TwoCornersRightSprite { get; set; }

	[field: SerializeField]
	private Sprite TwoCornersBottomSprite { get; set; }

	[field: SerializeField]
	private Sprite TwoCornersLeftSprite { get; set; }

	[field: SerializeField]
	private Sprite TwoCornersDiagLeftSprite { get; set; }

	[field: SerializeField]
	private Sprite TwoCornersDiagRightSprite { get; set; }

	[field: SerializeField]
	private Sprite ThreeCornersTopRightSprite { get; set; }

	[field: SerializeField]
	private Sprite ThreeCornersBottomRightSprite { get; set; }

	[field: SerializeField]
	private Sprite ThreeCornersBottomLeftSprite { get; set; }

	[field: SerializeField]
	private Sprite ThreeCornersTopLeftSprite { get; set; }

	[field: SerializeField]
	private Sprite AllCornersSprite { get; set; }

	[field: SerializeField]
	private Sprite SurroundedSprite { get; set; }

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
		TWO_CORNERS_TOP = 17,
		TWO_CORNERS_RIGHT = 18,
		TWO_CORNERS_BOTTOM = 19,
		TWO_CORNERS_LEFT = 20,
		TWO_CORNERS_DIAG_LEFT = 21,
		TWO_CORNERS_DIAG_RIGHT = 22,
		THREE_CORNERS_TOP_RIGHT = 23,
		THREE_CORNERS_BOTTOM_RIGHT = 24,
		THREE_CORNERS_BOTTOM_LEFT = 25,
		THREE_CORNERS_TOP_LEFT = 26,
		ALL_CORNERS = 27,
		SURROUNDED = 28,
		BLANK = 29,
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
				{ false,    true,       false },
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
				{ true,      false,     true    },
				{ false,     false,     false   },
				{ false,     false,     false   },
			},
			RuleTileType.TWO_CORNERS_TOP
		},
		{
			new bool[,]
			{
				{ false,     false,     true    },
				{ false,     false,     false   },
				{ false,     false,     true    },
			},
			RuleTileType.TWO_CORNERS_RIGHT
		},
		{
			new bool[,]
			{
				{ false,     false,     false   },
				{ false,     false,     false   },
				{ true,      false,     true    },
			},
			RuleTileType.TWO_CORNERS_BOTTOM
		},
		{
			new bool[,]
			{
				{ true,      false,     false   },
				{ false,     false,     false   },
				{ true,      false,     false   },
			},
			RuleTileType.TWO_CORNERS_LEFT
		},
		{
			new bool[,]
			{
				{ true,      false,     false   },
				{ false,     false,     false   },
				{ false,     false,     true    },
			},
			RuleTileType.TWO_CORNERS_DIAG_LEFT
		},
		{
			new bool[,]
			{
				{ false,     false,     true    },
				{ false,     false,     false   },
				{ true,      false,     false   },
			},
			RuleTileType.TWO_CORNERS_DIAG_RIGHT
		},
		{
			new bool[,]
			{
				{ true,      false,     true   },
				{ false,     false,     false  },
				{ false,     false,     true   },
			},
			RuleTileType.THREE_CORNERS_TOP_RIGHT
		},
		{
			new bool[,]
			{
				{ false,     false,     true   },
				{ false,     false,     false  },
				{ true,      false,     true   },
			},
			RuleTileType.THREE_CORNERS_BOTTOM_RIGHT
		},
		{
			new bool[,]
			{
				{ true,      false,     false   },
				{ false,     false,     false   },
				{ true,      false,     true    },
			},
			RuleTileType.THREE_CORNERS_BOTTOM_LEFT
		},
		{
			new bool[,]
			{
				{ true,      false,     true    },
				{ false,     false,     false   },
				{ true,      false,     false   },
			},
			RuleTileType.THREE_CORNERS_TOP_LEFT
		},
		{
			new bool[,]
			{
				{ true,     true,     true },
				{ true,     false,    true },
				{ true,     true,     true },
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
	
    private readonly bool[,] _blockedAreas =
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
                var rowY = TileYXPosition.y + i;
                var colX = TileYXPosition.x + j;
                this._blockedAreas[i+1, j+1] = QueryTile(tiles, rowY, colX);
			}
        }

		/* -1, -1
		 * 0, -1
		 * 1, -1
		 * 
		 * 
		 */

		this._tileType = GetTileType(this._blockedAreas);

        // TODO: Set the sprite for tile
        switch (this._tileType)
        {
            case RuleTileType.NONE:
                Debug.LogError("Tile type not found!");
                break;
            case RuleTileType.TOP:
                this.TileSpriteRenderer.sprite = this.TopSprite;
                break;
            case RuleTileType.RIGHT:
                this.TileSpriteRenderer.sprite = this.RightSprite;
                break;
            case RuleTileType.BOTTOM:
                this.TileSpriteRenderer.sprite = this.BottomSprite;
                break;
            case RuleTileType.LEFT:
                this.TileSpriteRenderer.sprite = this.LeftSprite;
                break;
            case RuleTileType.CORNER_TOP_LEFT:
                this.TileSpriteRenderer.sprite = this.CornerTopLeftSprite;
                break;
            case RuleTileType.CORNER_TOP_RIGHT:
                this.TileSpriteRenderer.sprite = this.CornerTopRightSprite;
                break;
            case RuleTileType.CORNER_BOTTOM_RIGHT:
                this.TileSpriteRenderer.sprite = this.CornerBottomRightSprite;
                break;
            case RuleTileType.CORNER_BOTTOM_LEFT:
                this.TileSpriteRenderer.sprite = this.CornerBottomLeftSprite;
                break;
            case RuleTileType.TOP_RIGHT:
                this.TileSpriteRenderer.sprite = this.TopRightSprite;
                break;
            case RuleTileType.BOTTOM_RIGHT:
                this.TileSpriteRenderer.sprite = this.BottomRightSprite;
                break;
            case RuleTileType.BOTTOM_LEFT:
                this.TileSpriteRenderer.sprite = this.BottomLeftSprite;
                break;
            case RuleTileType.TOP_LEFT:
                this.TileSpriteRenderer.sprite = this.TopLeftSprite;
                break;
            case RuleTileType.U_FACE_TOP:
                this.TileSpriteRenderer.sprite = this.UFaceTopSprite;
                break;
            case RuleTileType.U_FACE_RIGHT:
                this.TileSpriteRenderer.sprite = this.UFaceRightSprite;
                break;
            case RuleTileType.U_FACE_BOTTOM:
                this.TileSpriteRenderer.sprite = this.UFaceBottomSprite;
                break;
            case RuleTileType.U_FACE_LEFT:
                this.TileSpriteRenderer.sprite = this.UFaceLeftSprite;
                break;
            case RuleTileType.TWO_CORNERS_TOP:
				this.TileSpriteRenderer.sprite = this.TwoCornersTopSprite;
				break;
            case RuleTileType.TWO_CORNERS_RIGHT:
				this.TileSpriteRenderer.sprite = this.TwoCornersRightSprite;
				break;
            case RuleTileType.TWO_CORNERS_BOTTOM:
				this.TileSpriteRenderer.sprite = this.TwoCornersBottomSprite;
				break;
            case RuleTileType.TWO_CORNERS_LEFT:
				this.TileSpriteRenderer.sprite = this.TwoCornersLeftSprite;
				break;
            case RuleTileType.TWO_CORNERS_DIAG_LEFT:
				this.TileSpriteRenderer.sprite = this.TwoCornersDiagLeftSprite;
				break;
            case RuleTileType.TWO_CORNERS_DIAG_RIGHT:
				this.TileSpriteRenderer.sprite = this.TwoCornersDiagRightSprite;
				break;
            case RuleTileType.THREE_CORNERS_TOP_RIGHT:
				this.TileSpriteRenderer.sprite = this.ThreeCornersTopRightSprite;
				break;
            case RuleTileType.THREE_CORNERS_BOTTOM_RIGHT:
				this.TileSpriteRenderer.sprite = this.ThreeCornersBottomRightSprite;
				break;
            case RuleTileType.THREE_CORNERS_BOTTOM_LEFT:
				this.TileSpriteRenderer.sprite = this.ThreeCornersBottomLeftSprite;
				break;
            case RuleTileType.THREE_CORNERS_TOP_LEFT:
				this.TileSpriteRenderer.sprite = this.ThreeCornersTopLeftSprite;
				break;
            case RuleTileType.ALL_CORNERS:
				this.TileSpriteRenderer.sprite = this.AllCornersSprite;
				break;
            case RuleTileType.SURROUNDED:
                this.TileSpriteRenderer.sprite = this.SurroundedSprite;
                break;
            case RuleTileType.BLANK:
                this.TileSpriteRenderer.sprite = null;
                break;
            default:
                Debug.LogError("Tile type not found!");
                break;
        }
    }

    private RuleTileType GetTileType(bool[,] blockedAreas)
	{
        foreach (var (tileCombi, tileEnum) in ParsedRuleTile)
        {
			var transposedTileCombi = new bool[3, 3]
			{
				{ tileCombi[2,0], tileCombi[2,1], tileCombi[2,2] },
				{ tileCombi[1,0], tileCombi[1,1], tileCombi[1,2] },
				{ tileCombi[0,0], tileCombi[0,1], tileCombi[0,2] },
			};

			var stillContinue = true;
			for (var i = 0; i < 3; i++)
            {
				for (var j = 0; j < 3; j++)
                {
					if (transposedTileCombi[i,j] != blockedAreas[i,j])
                    {
						stillContinue = false;
						break;
					}
                }

				if (!stillContinue)
                {
					break;
                }
            }

			if (!stillContinue)
            {
				continue;
            }

			return tileEnum;
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

		//print($"{colX}, {rowY}");
        var tile = tiles[rowY, colX];

        if (tile == null) return false;

		return true;
    }
}
