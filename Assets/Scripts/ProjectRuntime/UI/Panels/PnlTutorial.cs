using BroccoliBunnyStudios.Managers;
using BroccoliBunnyStudios.Panel;
using DG.Tweening;
using ProjectRuntime.Tutorial;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectRuntime.UI
{
    /// <summary>
    /// Input data that PnlTutorial needs
    /// </summary>
    public struct PnlTutorialData
    {
        public bool ShowTutorialVideo;
        public string TutorialVideoPath;
        public string TutorialText;

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
            this.ShowTutorialVideo = d.ShowTutorialVideo;
            this.TutorialVideoPath = d.TutorialVideoPath;
            this.TutorialText = d.TutorialText;

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
        private RectTransform HandPointerRT { get; set; }

        // Internal variables
        private PnlTutorialData _stepData;
        private RectTransform _parent;
        private Sequence _handPointerSequence;

        private void Awake()
        {
            LocalizationManager.Instance.OnLocalizationChanged += this.OnLocalizationChanged;
        }

        private void OnDestroy()
        {
            LocalizationManager.Instance.OnLocalizationChanged -= this.OnLocalizationChanged;
        }

        private void OnLocalizationChanged()
        {

        }
    }
}