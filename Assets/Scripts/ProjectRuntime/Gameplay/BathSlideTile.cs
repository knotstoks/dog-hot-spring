using BroccoliBunnyStudios.Utils;
using Cysharp.Threading.Tasks;
using ProjectRuntime.Data;
using ProjectRuntime.Managers;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectRuntime.Gameplay
{
    public enum TileColor
    {
        NONE = 0,
        RED = 1,
        BLUE = 2,
        GREEN = 3,
        YELLOW = 4,
        ORANGE = 5,
        WHITE = 6,
        BLACK = 7,
        PURPLE = 8,
        PINK = 9,
    }

    public class BathSlideTile : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [field: SerializeField, Header("Scene References")]
        public Transform BottomLeftTransform { get; private set; }

        [field: SerializeField]
        private TextMeshProUGUI DropsLeftTMP { get; set; }

        [field: SerializeField]
        private List<Transform> DropTransforms { get; set; }

        [field: SerializeField]
        private SpriteRenderer SpriteRenderer { get; set; }

        public TileShape TileShape { get; private set; }

        public TileColor TileColor { get; private set; }

        // Currently dragged tile, can be null
        public static BathSlideTile CurrentDraggedTile { get; private set; } = null;

        // Internal variables for drag handling
        private const int InvalidPointerId = -99;
        private static int s_currentPointerId = InvalidPointerId;
        private Vector3 _dragOffset;
        private ContactFilter2D _contactFilter;
        private bool _hasStartedDragging = false;
        private PointerEventData _currentEventData = null;
        private readonly float _slowDownAmount = 0.1f;
        private readonly float _dragSpeed = 0.2f;
        private static Vector2Int s_lastDragTileYX; // Previous frame's tile position

        private List<BoxCollider2D> _myColliders;
        private int _dropsLeft;

        private void Awake()
        {
            this._myColliders = new(this.GetComponentsInChildren<BoxCollider2D>(true));

            this._contactFilter = new ContactFilter2D();
            this._contactFilter.SetLayerMask(LayerMask.GetMask("Tiles"));
            this._contactFilter.useLayerMask = true;
            this._contactFilter.useTriggers = false;
        }

        private void Update()
        {
            if (this._hasStartedDragging && this._currentEventData != null)
            {
                this.OnStartAndUpdateDrag();
            }
        }

        public void Init(int tileId, TileColor tileColor, int dropsLeft)
        {
            this.TileShape = DTileShape.GetDataById(tileId).Value.Shape;
            this.TileColor = tileColor;
            this._dropsLeft = dropsLeft;

            // Update the tile sprite
            // TODO
            //CommonUtil.UpdateSprite(this.SpriteRenderer, string.Format("images/tiles/tile_{0}_{1}.png", tileId.ToString(), tileColor.ToString().ToLowerInvariant()));
            CommonUtil.UpdateSprite(this.SpriteRenderer, string.Format("images/tiles/tile_{0}_{1}.png", "1", tileColor.ToString().ToLowerInvariant()));

            this.RefreshDropsLeftText();
        }

        private void RefreshDropsLeftText()
        {
            this.DropsLeftTMP.text = this._dropsLeft.ToString();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (s_currentPointerId == InvalidPointerId)
            {
                s_currentPointerId = eventData.pointerId;
                CurrentDraggedTile = this;

                GridManager.Instance.ToggleDropColor(this.TileColor, true);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (CurrentDraggedTile != null && s_currentPointerId == eventData.pointerId)
            {
                if (!this._hasStartedDragging)
                {
                    this._hasStartedDragging = true;
                    this._dragOffset = this.transform.position - this.GetWorldPosition(eventData);
                    this._dragOffset.z = 0;
                }

                this._currentEventData = eventData;
            }
        }

        private void OnStartAndUpdateDrag()
        {
            // Do overlap tests
            var castDirection = this.GetWorldPosition(this._currentEventData) - this.transform.position + this._dragOffset;
            if (castDirection.magnitude == 0)
            {
                return;
            }
            if (castDirection.magnitude > this._dragSpeed)
            {
                castDirection = castDirection.normalized * this._dragSpeed;
            }
            RaycastHit2D hit = new();
            hit.distance = float.MaxValue;
            foreach (var myCollider in this._myColliders)
            {
                var collidersHit = new RaycastHit2D[10];
                var numHit = myCollider.Cast(castDirection, this._contactFilter, collidersHit, castDirection.magnitude);

                for (var i = 0; i < numHit; i++)
                {
                    if (this._myColliders.Contains(collidersHit[i].collider as BoxCollider2D))
                    {
                        continue;
                    }
                    else
                    {
                        if (collidersHit[i].distance < hit.distance)
                        {
                            hit = collidersHit[i];
                        }
                        break;
                    }
                }
            }

            if (hit.collider == null)
            {
                this.transform.position += castDirection;
            }
            else
            {
                var dist = Mathf.Max(hit.distance - this._slowDownAmount, 0f);
                var dragVector = castDirection.normalized * dist;

                this.transform.position += dragVector;
                Physics2D.SyncTransforms();

                var remainderDistance = castDirection.magnitude - dist;
                castDirection = castDirection.normalized * remainderDistance;
                var perpendicular = Vector3.Cross(hit.normal, Vector3.back);
                castDirection = Vector3.Project(castDirection, perpendicular);

                hit = default;
                hit.distance = float.MaxValue;

                foreach (var myCollider in this._myColliders)
                {
                    var collidersHit = new RaycastHit2D[10];
                    var numHit = myCollider.Cast(castDirection, this._contactFilter, collidersHit, castDirection.magnitude);

                    for (var i = 0; i < numHit; i++)
                    {
                        if (this._myColliders.Contains(collidersHit[i].collider as BoxCollider2D))
                        {
                            continue;
                        }
                        else
                        {
                            if (collidersHit[i].distance < hit.distance)
                            {
                                hit = collidersHit[i];
                            }
                            break;
                        }
                    }
                }

                if (hit.collider == null)
                {
                    this.transform.position += castDirection;
                }
                else
                {
                    dist = Mathf.Max(hit.distance - this._slowDownAmount, 0f);
                    dragVector = castDirection.normalized * dist;
                    this.transform.position += dragVector;
                }
            }

            var gm = GridManager.Instance;
            var dragPos = gm.TileContainer.InverseTransformPoint(this.BottomLeftTransform.position);
            var tileYX = gm.GetNearestTileYX(dragPos);

            // Figure out if we are hovering over the grid and highlight tiles
            if (tileYX != s_lastDragTileYX)
            {
                gm.HighlightBackgroundTilesForTile(this, tileYX);
                s_lastDragTileYX = tileYX;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (CurrentDraggedTile != null && s_currentPointerId == eventData.pointerId)
            {
                CurrentDraggedTile = null;
                GridManager.Instance.ToggleDropColor(this.TileColor, false);
                s_currentPointerId = InvalidPointerId;
                this._hasStartedDragging = false;
                this._currentEventData = null;

                var gm = GridManager.Instance;
                var dragPos = gm.TileContainer.InverseTransformPoint(this.BottomLeftTransform.position);
                var tileYX = gm.GetNearestTileYX(dragPos);

                gm.ResetHighlightsForAllTiles();

                gm.SnapToGrid(this, tileYX);
            }
        }

        // Converts a drag position to where a BathSlideTile would be in the world
        private Vector3 GetWorldPosition(PointerEventData eventData)
        {
            var mainCam = Camera.main;
            var plane = new Plane(Vector3.back, this.transform.position);
            var ray = mainCam.ScreenPointToRay(eventData.position);
            plane.Raycast(ray, out var dist);
            return ray.GetPoint(dist);
        }

        public void ForceSnapToGrid()
        {
            CurrentDraggedTile = null;
            s_currentPointerId = InvalidPointerId;
            this._hasStartedDragging = false;
            this._currentEventData = null; 

            var gm = GridManager.Instance;
            var dragPos = gm.TileContainer.InverseTransformPoint(this.BottomLeftTransform.position);
            var tileYX = gm.GetNearestTileYX(dragPos);

            gm.SnapToGrid(this, tileYX);
        }

        public void ToggleDrag(bool toggle)
        {
            foreach (var myCollider in this._myColliders)
            {
                myCollider.enabled = toggle;
            }
        }

        public async void HandleAnimalDropped()
        {
            this._dropsLeft--;
            this.RefreshDropsLeftText();

            if (this._dropsLeft == 0)
            {
                this.ToggleDrag(false);
                this.ForceSnapToGrid();

                await UniTask.WaitForSeconds(0.5f);
                if (!this)
                {
                    GridManager.Instance.ResetHighlightsForAllTiles();
                    Destroy(this.gameObject);
                    return;
                }

                GridManager.Instance.ResetHighlightsForAllTiles();
                Destroy(this.gameObject);
            }
        }

        public Transform GetNearestDropTransform(Vector3 position)
        {
            var minDistance = float.MaxValue;
            var nearestTransform = this.DropTransforms[0]; // Will always have at least 1 drop transform
            foreach (var t in this.DropTransforms)
            {
                var dist = Vector3.Distance(position, t.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearestTransform = t;
                }
            }

            return nearestTransform;
        }
    }
}