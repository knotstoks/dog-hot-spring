using System.Collections.Generic;
using BroccoliBunnyStudios.Sound;
using BroccoliBunnyStudios.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ProjectRuntime.Data;
using ProjectRuntime.Managers;
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

    public enum AxisAlignEnum
    {
        NONE = 0,
        BOTH = 1,
        HORIZONTAL = 2,
        VERTICAL = 3,
    }

    public class BathSlideTile : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [field: SerializeField, Header("Scene References")]
        private Animator TileAnimator { get; set; }

        [field: SerializeField]
        public Transform BottomLeftTransform { get; private set; }

        [field: SerializeField]
        private TextMeshProUGUI DropsLeftTMP { get; set; }

        [field: SerializeField]
        private List<Transform> DropTransforms { get; set; }

        [field: SerializeField]
        private SpriteRenderer SpriteRenderer { get; set; }

        [field: SerializeField]
        private SpriteRenderer OverlaidSpriteRenderer { get; set; }

        [field: SerializeField, Header("Ice Block Logic")]
        private TextMeshProUGUI IceCracksLeftTMP { get; set; }

        [field: SerializeField, Header("Axis Align Logic")]
        private SpriteRenderer HoritzontalAxisAlignSpriteRenderer { get; set; }

        [field: SerializeField]
        private SpriteRenderer VerticalAxisAlignSpriteRenderer { get; set; }

        [field: SerializeField, Header("Sfxes")]
        private AudioPlaybackInfo DragSfx { get; set; }

        [field: SerializeField]
        private AudioPlaybackInfo ReleaseSfx { get; set; }

        [field: SerializeField]
        private AudioPlaybackInfo IceShatterSfx { get; set; }

        [field: SerializeField]
        private AudioPlaybackInfo FogDissolveSfx { get; set; }

        public TileShape TileShape { get; private set; }
        public TileColor TileColor { get; private set; }
        public bool CanDropIn => this._dropsLeft > 0 && this._iceCracksLeft == 0 && this._fogDropsLeft == 0;

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
        private readonly float _dragSpeed = 0.3f;
        private static Vector2Int s_lastDragTileYX; // Previous frame's tile position

        private List<BoxCollider2D> _myColliders;
        private int _dropsLeft;

        private bool CanMove => this._isEmptyTile || (this._iceCracksLeft == 0 && this._dropsLeft > 0);

        // Ice Logic
        private int _iceCracksLeft;

        // Empty Tile Logic
        private bool _isEmptyTile;

        // Axis Align Logic
        private AxisAlignEnum _axisAlignEnum;

        // Fog Logic
        private int _fogDropsLeft;

        // Juice Logic
        private bool _isPunching = false;

        // Animation states
        private const string ANIM_SHRINK = "tile_shrink";

        private void Awake()
        {
            this._myColliders = new(this.GetComponentsInChildren<BoxCollider2D>(true));

            this._contactFilter = new ContactFilter2D();
            this._contactFilter.SetLayerMask(LayerMask.GetMask("Tiles", "Blocking"));
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

        public void Init(int tileId, TileColor tileColor, int dropsLeft, int iceCracksLeft, bool isEmptyTile, AxisAlignEnum axisAlignEnum, int fogDropsLeft)
        {
            this.TileShape = DTileShape.GetDataById(tileId).Value.Shape;
            this.TileColor = tileColor;
            this._dropsLeft = dropsLeft;
            this._isEmptyTile = isEmptyTile;
            this._axisAlignEnum = axisAlignEnum;

            // Update the tile sprite
            if (!isEmptyTile)
            {
                CommonUtil.UpdateSprite(this.SpriteRenderer, string.Format("images/tiles/tile_{0}_{1}.png", tileId.ToString(), tileColor.ToString().ToLowerInvariant()));
            }

            // Update the axis align tile sprites (if any)
            this.HoritzontalAxisAlignSpriteRenderer.gameObject.SetActive(this._axisAlignEnum == AxisAlignEnum.HORIZONTAL);
            this.VerticalAxisAlignSpriteRenderer.gameObject.SetActive(this._axisAlignEnum == AxisAlignEnum.VERTICAL);

            // Logic for ice and fog blocks
            // Fog blocks cannot contain ice blocks!!
            this._iceCracksLeft = iceCracksLeft;
            this._fogDropsLeft = fogDropsLeft;
            if (isEmptyTile)
            {
                this.OverlaidSpriteRenderer.gameObject.SetActive(true);
                this.IceCracksLeftTMP.gameObject.SetActive(false);
                this.DropsLeftTMP.gameObject.SetActive(false);
                CommonUtil.UpdateSprite(this.OverlaidSpriteRenderer, string.Format("images/tiles/tile_{0}_black.png", tileId.ToString())); // TODO: images/empty_tiles/empty_tile_{0}.png
            }
            else if (iceCracksLeft > 0)
            {
                this.OverlaidSpriteRenderer.gameObject.SetActive(true);
                this.IceCracksLeftTMP.gameObject.SetActive(true);
                this.RefreshIceCracksLeftText();
                this.DropsLeftTMP.gameObject.SetActive(false);
                CommonUtil.UpdateSprite(this.OverlaidSpriteRenderer, string.Format("images/ice_tiles/ice_tile_{0}.png", tileId.ToString()));

                GridManager.Instance.OnBathTileCompleted += this.OnBathTileCompleted;
            }
            else if (fogDropsLeft > 0)
            {
                this.OverlaidSpriteRenderer.gameObject.SetActive(true);
                this.IceCracksLeftTMP.gameObject.SetActive(true);
                this.RefreshFogDropsLeftText();
                this.DropsLeftTMP.gameObject.SetActive(false);
                CommonUtil.UpdateSprite(this.OverlaidSpriteRenderer, string.Format("images/tiles/tile_{0}_black.png", tileId.ToString())); // TODO: images/ice_tiles/fog_tile_{0}.png

                GridManager.Instance.OnBathTileCompleted += this.OnBathTileCompleted;
            }
            else
            {
                this.IceCracksLeftTMP.gameObject.SetActive(false);
                this.OverlaidSpriteRenderer.gameObject.SetActive(false);
            }

            this.RefreshDropsLeftText();
        }

        private void RefreshIceCracksLeftText()
        {
            this.IceCracksLeftTMP.text = this._iceCracksLeft.ToString();
        }

        private void RefreshFogDropsLeftText()
        {
            this.IceCracksLeftTMP.text = this._fogDropsLeft.ToString();
        }

        private void RefreshDropsLeftText()
        {
            this.DropsLeftTMP.text = this._dropsLeft.ToString();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!this.CanMove)
            {
                return;
            }

            if (s_currentPointerId == InvalidPointerId)
            {
                SoundManager.Instance.PlayAudioPlaybackInfoAsync(this.DragSfx, false, Vector3.zero).Forget();

                this.TryPunchTile().Forget();

                s_currentPointerId = eventData.pointerId;
                CurrentDraggedTile = this;

                if (this._fogDropsLeft == 0)
                {
                    GridManager.Instance.ToggleDropColor(this.TileColor, true);
                }
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!this.CanMove)
            {
                return;
            }

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
            if (this.TileColor != TileColor.NONE && (!this.CanMove || this._dropsLeft == 0))
            {
                return;
            }

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
                if (this._axisAlignEnum == AxisAlignEnum.HORIZONTAL)
                {
                    castDirection.y = 0f;
                    castDirection.z = 0f;
                }
                else if (this._axisAlignEnum == AxisAlignEnum.VERTICAL)
                {
                    castDirection.x = 0f;
                    castDirection.z = 0f;
                }

                this.transform.position += castDirection;
            }
            else
            {
                var dist = Mathf.Max(hit.distance - this._slowDownAmount, 0f);
                var dragVector = castDirection.normalized * dist;

                if (this._axisAlignEnum == AxisAlignEnum.HORIZONTAL)
                {
                    dragVector.y = 0f;
                    dragVector.z = 0f;
                }
                else if (this._axisAlignEnum == AxisAlignEnum.VERTICAL)
                {
                    dragVector.x = 0f;
                    dragVector.z = 0f;
                }

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
                    if (this._axisAlignEnum == AxisAlignEnum.HORIZONTAL)
                    {
                        castDirection.y = 0f;
                        castDirection.z = 0f;
                    }
                    else if (this._axisAlignEnum == AxisAlignEnum.VERTICAL)
                    {
                        castDirection.x = 0f;
                        castDirection.z = 0f;
                    }

                    this.transform.position += castDirection;
                }
                else
                {
                    dist = Mathf.Max(hit.distance - this._slowDownAmount, 0f);
                    dragVector = castDirection.normalized * dist;
                    if (this._axisAlignEnum == AxisAlignEnum.HORIZONTAL)
                    {
                        dragVector.y = 0f;
                        dragVector.z = 0f;
                    }
                    else if (this._axisAlignEnum == AxisAlignEnum.VERTICAL)
                    {
                        dragVector.x = 0f;
                        dragVector.z = 0f;
                    }

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
            if (!this.CanMove)
            {
                return;
            }

            if (CurrentDraggedTile != null && s_currentPointerId == eventData.pointerId)
            {
                SoundManager.Instance.PlayAudioPlaybackInfoAsync(this.ReleaseSfx, false, Vector3.zero).Forget();

                this.TryPunchTile().Forget();

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

        //public void ToggleDrag(bool toggle)
        //{
        //    foreach (var myCollider in this._myColliders)
        //    {
        //        myCollider.enabled = toggle;
        //    }
        //}

        public void HandleAnimalDropped()
        {
            this._dropsLeft--;

            if (this._dropsLeft < 0)
            {
                return;
            }

            this.TryPunchDropsLeftText().Forget();

            this.RefreshDropsLeftText();

            if (this._dropsLeft == 0)
            {
                s_currentPointerId = InvalidPointerId;
                this.ForceSnapToGrid();
                GridManager.Instance.ToggleDropColor(this.TileColor, false);
                GridManager.Instance.OnBathTileComplete();

                this.HandleDestroyTile().Forget();
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

        public void OnBathTileCompleted()
        {
            if (this._iceCracksLeft == 0 && this._fogDropsLeft == 0)
            {
                return;
            }
            else if (this._iceCracksLeft > 0)
            {
                this._iceCracksLeft--;
                this.RefreshIceCracksLeftText();

                SpawnManager.Instance.SpawnShatterVfx(this.transform.position).Forget();
                SoundManager.Instance.PlayAudioPlaybackInfoAsync(this.IceShatterSfx, false, Vector3.zero).Forget();

                if (this._iceCracksLeft == 0)
                {
                    GridManager.Instance.OnBathTileCompleted -= this.OnBathTileCompleted;
                    this.OverlaidSpriteRenderer.gameObject.SetActive(false);
                    this.IceCracksLeftTMP.gameObject.SetActive(false);
                    this.RefreshDropsLeftText();
                    this.DropsLeftTMP.gameObject.SetActive(true);
                }
            }
            else if (this._fogDropsLeft > 0)
            {
                this._fogDropsLeft--;
                this.RefreshFogDropsLeftText();

                SpawnManager.Instance.SpawnShatterVfx(this.transform.position).Forget();
                SoundManager.Instance.PlayAudioPlaybackInfoAsync(this.FogDissolveSfx, false, Vector3.zero).Forget();

                if (this._fogDropsLeft == 0)
                {
                    GridManager.Instance.OnBathTileCompleted -= this.OnBathTileCompleted;
                    this.OverlaidSpriteRenderer.gameObject.SetActive(false);
                    this.IceCracksLeftTMP.gameObject.SetActive(false);
                    this.RefreshDropsLeftText();
                    this.DropsLeftTMP.gameObject.SetActive(true);
                }
            }

            
        }

        private async UniTaskVoid HandleDestroyTile()
        {
            GridManager.Instance.ResetHighlightsForAllTiles();

            await UniTask.WaitForSeconds(1.5f); // Delay to let the animal drop and splash vfx to play
            if (!this)
            {
                Destroy(this.gameObject);
                return;
            }

            // Play shrink animation and then destroy
            GridManager.Instance.ResetHighlightsForAllTiles();
            this.TileAnimator.Play(ANIM_SHRINK);
            var stateInfo = this.TileAnimator.GetCurrentAnimatorStateInfo(0);
            while (!stateInfo.IsName(ANIM_SHRINK))
            {
                await UniTask.Yield();
                if (!this)
                {
                    Destroy(this.gameObject);
                    return;
                }

                stateInfo = this.TileAnimator.GetCurrentAnimatorStateInfo(0);
            }

            while (stateInfo.IsName(ANIM_SHRINK) && stateInfo.normalizedTime < 1f)
            {
                await UniTask.Yield();
                if (!this)
                {
                    Destroy(this.gameObject);
                    return;
                }

                stateInfo = this.TileAnimator.GetCurrentAnimatorStateInfo(0);
            }

            Destroy(this.gameObject);
        }

        private void ForceStopDrag()
        {
            if (CurrentDraggedTile == this)
            {
                CurrentDraggedTile = null;
                s_currentPointerId = InvalidPointerId;
            }
        }

        private void OnApplicationFocus(bool focus)
        {
            if (!focus)
            {
                this.ForceStopDrag();
                this.ForceSnapToGrid();
            }
        }

        private async UniTask TryPunchTile()
        {
            if (this._isPunching)
            {
                return;
            }

            if (this._dropsLeft > 0 && this.CanMove)
            {
                this._isPunching = true;

                // Punch animation on move
                await this.transform.DOPunchScale(Vector3.one * 0.05f, 0.1f);
                if (!this) return;
                this.transform.localScale = Vector3.one;

                this._isPunching = false;
            }
        }

        private async UniTask TryPunchDropsLeftText()
        {
            if (this._dropsLeft > 0 && this.CanMove)
            {
                // Punch animation on move
                await this.DropsLeftTMP.transform.DOPunchScale(Vector3.one * 0.5f, 0.1f);
                if (!this) return;
                this.DropsLeftTMP.transform.localScale = Vector3.one;
            }
        }
    }
}