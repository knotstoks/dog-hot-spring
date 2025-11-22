using BroccoliBunnyStudios.Panel;
using BroccoliBunnyStudios.Sound;
using ProjectRuntime.Data;
using ProjectRuntime.Managers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectRuntime.Gameplay
{
    public enum TileColor
    {
        RED = 1,
        BLUE = 2,
        GREEN = 3,
        YELLOW = 4,
        ORANGE = 5,
        WHITE = 6,
        BLACK = 7,
    }

    public class BathSlideTile : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [field: SerializeField, Header("Scene References")]
        private float DragOffset { get; set; }

        [field: SerializeField]
        public Transform BottomLeftTransform { get; private set; }

        [field: SerializeField]
        private int TileShapeId { get; set; }

        [field: SerializeField]
        private TileColor EditorTileColor { get; set; }

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

        private void Awake()
        {
            this._myColliders = new(this.GetComponentsInChildren<BoxCollider2D>(true));

            this._contactFilter = new ContactFilter2D();
            this._contactFilter.SetLayerMask(LayerMask.GetMask("Tiles"));
            this._contactFilter.useLayerMask = true;
            this._contactFilter.useTriggers = false;

            this.Init(this.TileShapeId, TileColor.GREEN);
        }

        private void Update()
        {
            if (this._hasStartedDragging && this._currentEventData != null)
            {
                this.OnStartAndUpdateDrag();
            }
        }

        public void Init(int tileId, TileColor tileColor)
        {
            this.TileShape = DTileShape.GetDataById(tileId).Value.Shape;
            this.TileColor = this.EditorTileColor; // TODO: TEMP
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
    }
}