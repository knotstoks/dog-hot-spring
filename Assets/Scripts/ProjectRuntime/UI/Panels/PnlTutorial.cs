using BroccoliBunnyStudios.Extensions;
using BroccoliBunnyStudios.Managers;
using BroccoliBunnyStudios.Panel;
using BroccoliBunnyStudios.Util;
using BroccoliBunnyStudios.Utils;
using DG.Tweening;
using ProjectRuntime.Tutorial;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectRuntime.UI
{
    /// <summary>
    /// Input data that PnlTutorial needs
    /// </summary>
    public struct PnlTutorialData
    {
        public bool ShowTutorialImage;
        public bool ShowGrayBackground;
        public bool ShowTutorialTextbox;
        public Transform TutorialTextPosition;
        public string TutorialText;
        public string TutorialImagePath;

        public bool ShowHandPointer;
        public float RotateHandDegrees;
        public HandPointerType HandPointerType;
        public PositionType PointerPositionType;
        public Transform PointerPosition;
        public Transform PointerPosition2;
        public string PointerPositionPath;
        public string PointerPositionPath2;
        public float PointerStartStationaryTime;
        public float PointerMoveTime;
        public float PointerEndStationaryTime;

        public bool IsClickThrough;

        public PnlTutorialData(TutorialGameStepData d)
        {
            this.ShowTutorialImage = d.ShowTutorialImage;
            this.ShowGrayBackground = d.ShowGrayBackground;

            this.ShowTutorialTextbox = d.ShowTutorialTextbox;
            this.TutorialTextPosition = d.TutorialTextPosition;
            this.TutorialText = d.TutorialText;
            this.TutorialImagePath = d.TutorialImagePath;

            this.ShowHandPointer = d.ShowHandPointer;
            this.RotateHandDegrees = d.RotateHandDegrees;
            this.HandPointerType = d.HandPointerType;
            this.PointerPositionType = d.PointerPositionType;
            this.PointerPosition = d.PointerPosition;
            this.PointerPosition2 = d.PointerPosition2;
            this.PointerPositionPath = d.PointerPositionPath;
            this.PointerPositionPath2 = d.PointerPositionPath2;
            this.PointerStartStationaryTime = d.PointerStartStationaryTime;
            this.PointerMoveTime = d.PointerMoveTime;
            this.PointerEndStationaryTime = d.PointerEndStationaryTime;

            this.IsClickThrough = d.IsClickThrough;
        }
    }

    public class PnlTutorial : BasePanel
    {
        [field: SerializeField]
        private Image GrayBackground { get; set; }

        [field: SerializeField]
        private RectTransform TutorialTextBackingRT { get; set; }

        [field: SerializeField]
        private TextMeshProUGUI TutorialTMP { get; set; }

        [field: SerializeField]
        private Button FullscreenButton { get; set; }

        [field: SerializeField]
        private RectTransform HandPointerRT { get; set; }

        [field: SerializeField]
        private RectTransform HandPointerInfinity { get; set; }

        // Internal variables
        private PnlTutorialData _stepData;
        private RectTransform _parent;
        private Sequence _handPointerSequence;

        // Callbacks
        public event Action OnClickNextEvent;

        private void Awake()
        {
            LocalizationManager.Instance.OnLocalizationChanged += this.OnLocalizationChanged;

            this.FullscreenButton.OnClick(this.OnFullscreenButtonClick);
        }

        private void OnDestroy()
        {
            LocalizationManager.Instance.OnLocalizationChanged -= this.OnLocalizationChanged;
        }

        public void Init(PnlTutorialData stepData)
        {
            this._stepData = stepData;
            this._parent = (RectTransform)this.transform.parent;

            // Show gray background if needed
            this.GrayBackground.gameObject.SetActive(stepData.ShowGrayBackground);

            if (stepData.ShowTutorialTextbox)
            {
                this.TutorialTextBackingRT.gameObject.SetActive(true);
                this.SetRectPosition(this.TutorialTextBackingRT, stepData.TutorialTextPosition);
                this.TutorialTMP.text = LocalizationManager.Instance[stepData.TutorialText];
            }
            else
            {
                this.TutorialTextBackingRT.gameObject.SetActive(false);
            }

            if (stepData.ShowHandPointer)
            {
                switch (stepData.HandPointerType)
                {
                    case HandPointerType.Tap:
                    case HandPointerType.Drag:
                        {
                            this.HandPointerRT.gameObject.SetActive(true);
                            this.HandPointerInfinity.gameObject.SetActive(false);

                            this.HandPointerRT.transform.localRotation = Quaternion.Euler(0f, 0f, stepData.RotateHandDegrees);
                            if (stepData.PointerPositionType == PositionType.SceneReference)
                            {
                                this.AnimateHandPointer(
                                    stepData.HandPointerType,
                                    stepData.PointerPosition,
                                    stepData.PointerPosition2,
                                    stepData.PointerStartStationaryTime,
                                    stepData.PointerMoveTime,
                                    stepData.PointerEndStationaryTime
                                );
                            }
                            else if (stepData.PointerPositionType == PositionType.ScenePath)
                            {
                                var go1 = GameObject.Find(stepData.PointerPositionPath);
                                var go2 = GameObject.Find(stepData.PointerPositionPath2);
                                //if (!string.IsNullOrEmpty(stepData.PointerPositionPath) && go1 == null)
                                //{
                                //    Debug.LogError($"Cannot find path {stepData.PointerPositionPath} for tutorial hand pointer");
                                //}
                                //if (!string.IsNullOrEmpty(stepData.PointerPositionPath2) && go2 == null)
                                //{
                                //    Debug.LogError($"Cannot find path {stepData.PointerPositionPath2} for tutorial hand pointer");
                                //}
                                this.AnimateHandPointer(
                                    stepData.HandPointerType,
                                    go1 ? go1.transform : null,
                                    go2 ? go2.transform : null,
                                    stepData.PointerStartStationaryTime,
                                    stepData.PointerMoveTime,
                                    stepData.PointerEndStationaryTime
                                );
                            }
                        }
                        break;

                    case HandPointerType.Infinity:
                        {
                            this.HandPointerRT.gameObject.SetActive(false);
                            this.HandPointerInfinity.gameObject.SetActive(true);

                            this.HandPointerInfinity.transform.localRotation = Quaternion.Euler(0f, 0f, stepData.RotateHandDegrees);

                            if (stepData.PointerPositionType == PositionType.SceneReference)
                            {
                                this.SetRectPosition(this.HandPointerRT, stepData.PointerPosition);
                            }
                            else if (stepData.PointerPositionType == PositionType.ScenePath)
                            {
                                var go1 = GameObject.Find(stepData.PointerPositionPath);
                                //if (!string.IsNullOrEmpty(stepData.PointerPositionPath) && go1 == null)
                                //{
                                //    Debug.LogError($"Cannot find path {stepData.PointerPositionPath} for tutorial hand pointer");
                                //}

                                this.SetRectPosition(this.HandPointerRT, go1 ? go1.transform : null);
                            }
                        }
                        break;

                    default:
                        break;
                }
            }
            else
            {
                this.HandPointerRT.gameObject.SetActive(false);
                this.HandPointerInfinity.gameObject.SetActive(false);
            }

            this.OnLocalizationChanged();
        }

        private void SetRectPosition(RectTransform rectToSetPosition, Transform target)
        {
            if (!target)
            {
                return;
            }

            if (target is RectTransform rt)
            {
                var canvas = rt.GetComponentInParent<Canvas>();
                if (!canvas || canvas.renderMode == RenderMode.WorldSpace)
                {
                    rectToSetPosition.localPosition = this.GetUiPosition(rt.position);
                }
                else
                {
                    rectToSetPosition.position = rt.position;
                }
            }
            else
            {
                rectToSetPosition.localPosition = this.GetUiPosition(target.position);
            }
        }

        private Vector3 ConvertToLocalPosition(Transform target)
        {
            if (target is RectTransform rt)
            {
                var canvas = rt.GetComponentInParent<Canvas>();
                if (!canvas || canvas.renderMode == RenderMode.WorldSpace)
                {
                    return this.GetUiPosition(rt.position);
                }
                else
                {
                    var max = rt.TransformPoint(rt.rect.max);
                    var min = rt.TransformPoint(rt.rect.min);
                    var pos = (max + min) / 2f;
                    return this.transform.InverseTransformPoint(pos);
                }
            }
            else
            {
                return this.GetUiPosition(target.position);
            }
        }

        public void AnimateHandPointer(HandPointerType handPointerType, Transform pointerPosition, Transform pointerPosition2, float pointerStartStationaryTime, float pointerMoveTime, float pointerEndStationaryTime)
        {
            // Remove any previous sequence
            if (this._handPointerSequence != null)
            {
                this._handPointerSequence.Kill();
                this._handPointerSequence = null;
            }

            // Reset scale caused by any previous running sequence (which still runs even when disabled)
            this.HandPointerRT.transform.localScale = Vector3.one;

            switch (handPointerType)
            {
                case HandPointerType.Tap:
                    {
                        if (!pointerPosition)
                        {
                            this.HandPointerRT.gameObject.SetActive(false);
                            return;
                        }

                        this.SetRectPosition(this.HandPointerRT, pointerPosition);

                        var seq = DOTween.Sequence();
                        seq.AppendInterval(pointerStartStationaryTime);
                        seq.Append(this.HandPointerRT.transform.DOScale(1.2f, 0.25f).SetEase(Ease.InOutQuad));
                        seq.Append(this.HandPointerRT.transform.DOScale(1f, 0.25f).SetEase(Ease.InOutQuad));
                        seq.AppendInterval(pointerEndStationaryTime);
                        seq.SetLoops(-1, LoopType.Restart);
                        seq.SetUpdate(true);
                        seq.Play();
                        this._handPointerSequence = seq;
                    }
                    break;

                case HandPointerType.Drag:
                    {
                        if (!pointerPosition || !pointerPosition2)
                        {
                            this.HandPointerRT.gameObject.SetActive(false);
                            return;
                        }

                        this.SetRectPosition(this.HandPointerRT, pointerPosition);
                        var pos1 = this.HandPointerRT.localPosition;
                        var pos2 = this.ConvertToLocalPosition(pointerPosition2);
                        var offset = pos2 - pos1;

                        var seq = DOTween.Sequence();
                        seq.AppendInterval(pointerStartStationaryTime);
                        seq.Append(this.HandPointerRT.DOLocalMove(offset, pointerMoveTime).SetRelative(true));
                        seq.AppendInterval(pointerEndStationaryTime);
                        seq.Append(this.HandPointerRT.DOLocalMove(-offset, 0f).SetRelative(true));
                        seq.SetLoops(-1, LoopType.Restart);
                        seq.SetUpdate(true);
                        seq.Play();
                        this._handPointerSequence = seq;
                    }
                    break;

                default:
                    break;
            }
        }

        private Vector2 GetUiPosition(Vector3 worldPosition, Camera lookingCamera = null)
        {
            var cam = lookingCamera == null ? Camera.main : lookingCamera;
            var pos = CameraHelper.GetUiPosition(cam,
                this._parent.sizeDelta,
                worldPosition);
            return pos;
        }

        private void OnLocalizationChanged()
        {
            if (this._stepData.ShowTutorialTextbox)
            {
                var lm = LocalizationManager.Instance;
                this.TutorialTMP.text = lm[this._stepData.TutorialText];

                CommonUtil.ForceRebuildLayoutImmediateRecursive(this.transform as RectTransform);
                this.TutorialTMP.ForceMeshUpdate(true);
                //this.TutorialTMP.horizontalAlignment = this._stepData.ShowNpc && this.TutorialText.textInfo.lineCount > 1
                //    ? HorizontalAlignmentOptions.Left
                //    : HorizontalAlignmentOptions.Center;
            }
        }

        private void OnFullscreenButtonClick()
        {
            this.OnClickNextEvent?.Invoke();
        }
    }
}