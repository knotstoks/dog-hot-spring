using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectRuntime.Tutorial
{
    [Serializable]
    public enum TutorialGameStepType
    {
        ShowImageAndTextOnly = 0,
        PauseGame = 1,
        UnpauseGame = 2,
    }

    [Serializable]
    public enum TutorialGameStepDurationType
    {
        WaitForUserInput = 0,
        GoToNextStepImmediately = 1,
        GoToNextStepAfterSeconds = 2,
    }

    [Serializable]
    public enum HandPointerType
    {
        Tap = 0,
        Drag = 1,
        Infinity = 2,
    }

    [Serializable]
    public enum PositionType
    {
        SceneReference = 0,
        ScenePath = 1,
    }

    [Serializable]
    public struct CutoutData
    {
        public Transform CutoutTarget;
        public Vector2 AdditionalSize;
    }

    [Serializable]
    public struct CutoutPathData
    {
        public string CutoutPath;
        public Vector2 AdditionalSize;
    }

    [Serializable]
    public struct TutorialGameStepData
    {
        [Header("Tutorial Step Data")]
        [TextArea]
        public string DesignComment;

        [Header("Tutorial Step Type")]
        public TutorialGameStepType StepType;
        [HideIf("HideIfIgnoreDurationType")]
        public TutorialGameStepDurationType StepDurationType;

        [ShowIf("@StepDurationType", TutorialGameStepDurationType.GoToNextStepAfterSeconds)]
        public float NextStepAfterSeconds;
        [ShowIf("@StepDurationType", TutorialGameStepDurationType.GoToNextStepAfterSeconds)]
        public bool IgnoreTimeScale;

        public bool ShowGrayBackground;
        public bool ShowTutorialTextbox;

        [Header("Tutorial Image"), ShowIf("ShowTutorialImage")]
        public bool ShowTutorialImage;
        [ShowIf("ShowIfInteractionAndShowTutorialImage")]
        public string TutorialImagePath;
        [ShowIf("ShowIfTutorialTextBox")]
        public Transform TutorialTextPosition;
        [ShowIf("ShowIfTutorialTextBox"), TextArea]
        public string TutorialText;
        [ShowIf("ShowIfInteractionAndShowTutorialImage")]
        public bool ShowNpc;
        public bool KeepPnlTutorialForNextStep;

        [Header("Tutorial hand pointer"), ShowIf("ShowIfInteraction")]
        public bool ShowHandPointer;
        [ShowIf("ShowIfInteractionAndHandPointer")]
        public float RotateHandDegrees;
        [ShowIf("ShowIfInteractionAndHandPointer")]
        public HandPointerType HandPointerType;
        [ShowIf("ShowIfInteractionAndHandPointer")]
        public PositionType PointerPositionType;
        [ShowIf("ShowIfInteractionAndHandPointerAndSceneReference")]
        public Transform PointerPosition;
        [ShowIf("ShowIfInteractionAndHandPointerAndSceneReference"), EnableIf("@HandPointerType", HandPointerType.Drag)]
        public Transform PointerPosition2;
        [ShowIf("ShowIfInteractionAndHandPointerAndScenePath")]
        public string PointerPositionPath;
        [ShowIf("ShowIfInteractionAndHandPointerAndScenePath"), EnableIf("@HandPointerType", HandPointerType.Drag)]
        public string PointerPositionPath2;
        [ShowIf("ShowIfInteractionAndHandPointerNeedTimingControl")]
        public float PointerStartStationaryTime;
        [ShowIf("ShowIfInteractionAndHandPointerNeedTimingControl")]
        public float PointerMoveTime;
        [ShowIf("ShowIfInteractionAndHandPointerNeedTimingControl")]
        public float PointerEndStationaryTime;

        [Header("Whether to block touches"), ShowIf("ShowIfInteraction")]
        public bool IsClickThrough;

        private bool HideIfIgnoreDurationType()
        {
            // Tutorial steps that ignore the StepDurationType
            return false;
        }

        private bool ShowIfInteraction()
        {
            // Interaction type tutorial steps
            return this.StepType == TutorialGameStepType.ShowImageAndTextOnly;
        }

        private bool ShowIfInteractionAndShowTutorialImage()
        {
            return this.ShowIfInteraction() && this.ShowTutorialImage;
        }

        private bool ShowIfTutorialTextBox()
        {
            return this.ShowTutorialTextbox;
        }

        private bool ShowIfInteractionAndHandPointer()
        {
            return this.ShowIfInteraction() && this.ShowHandPointer;
        }

        private bool ShowIfInteractionAndHandPointerAndSceneReference()
        {
            return this.ShowIfInteractionAndHandPointer() && this.PointerPositionType == PositionType.SceneReference;
        }

        private bool ShowIfInteractionAndHandPointerAndScenePath()
        {
            return this.ShowIfInteractionAndHandPointer() && this.PointerPositionType == PositionType.ScenePath;
        }

        private bool ShowIfInteractionAndHandPointerNeedTimingControl()
        {
            return this.ShowIfInteractionAndHandPointer() && (this.HandPointerType == HandPointerType.Tap || this.HandPointerType == HandPointerType.Drag);
        }
    }
}