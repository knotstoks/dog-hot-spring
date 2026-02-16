using BroccoliBunnyStudios.Managers;
using BroccoliBunnyStudios.Panel;
using BroccoliBunnyStudios.Utils;
using Cysharp.Threading.Tasks;
using ProjectRuntime.Gameplay;
using ProjectRuntime.Managers;
using ProjectRuntime.Tutorial;
using ProjectRuntime.UI;
using ProjectRuntime.UI.Panels;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectRuntime.Tutorial
{
    public class TutorialGame : TutorialController
    {
        [field: SerializeField, Header("Which world it triggers in")]
        public int WorldId { get; set; }

        [field: SerializeField, Header("Activate only if Tutorial Id is not completed")]
        public bool ActivateIfNotCompleted { get; private set; }

        [field: SerializeField, EnableIf("ActivateIfNotCompleted")]
        public List<string> TutorialIdsNotCompleted { get; private set; }

        [field: SerializeField, Header("And activate only if Tutorial Id is completed")]
        public bool ActivateIfCompleted { get; private set; }

        [field: SerializeField, EnableIf("ActivateIfCompleted")]
        public List<string> TutorialIdsCompleted { get; private set; }

        [field: SerializeField, Header("And activate only if World Id is not completed")]
        public bool ActivateIfWorldNotCompleted { get; private set; }

        [field: SerializeField, EnableIf("ActivateIfWorldNotCompleted")]
        public List<int> WorldIdsNotCompleted { get; private set; }

        [field: SerializeField, Header("And activate only if World Id is completed")]
        public bool ActivateIfWorldCompleted { get; private set; }

        [field: SerializeField, EnableIf("ActivateIfWorldCompleted")]
        public List<int> WorldIdsCompleted { get; private set; }

        [field: SerializeField, Header("Tutorial Steps")]
        private List<TutorialGameStepData> TutorialSteps { get; set; }

        protected override void Start()
        {
            base.Start();
        }

        public override bool CheckAndActivate()
        {
            var shouldActivate = this.WillActivate();
            if (shouldActivate)
            {
                this.Activate();
            }
            return shouldActivate;
        }

        private bool WillActivate()
        {
            var completedTutorials = SaveManager.Instance.CompletedTutorials;
            var shouldActivate = true;
            if (this.ActivateIfNotCompleted)
            {
                foreach (var tutorialId in this.TutorialIdsNotCompleted)
                {
                    if (completedTutorials.Contains(tutorialId))
                    {
                        // User already completed tutorialId, failed requirement
                        shouldActivate = false;
                        break;
                    }
                }
            }
            if (this.ActivateIfCompleted)
            {
                foreach (var tutorialId in this.TutorialIdsCompleted)
                {
                    if (!completedTutorials.Contains(tutorialId))
                    {
                        // User has not completed tutorialId, failed requirement
                        shouldActivate = false;
                        break;
                    }
                }
            }
            if (this.ActivateIfWorldNotCompleted)
            {
                foreach (var worldId in this.WorldIdsNotCompleted)
                {
                    if (UserSaveDataManager.Instance.IsWorldRequirementMet(worldId))
                    {
                        // User already completed worldId, failed requirement
                        shouldActivate = false;
                        break;
                    }
                }
            }
            if (this.ActivateIfWorldCompleted)
            {
                foreach (var worldId in this.WorldIdsCompleted)
                {
                    if (!UserSaveDataManager.Instance.IsWorldRequirementMet(worldId))
                    {
                        // User has not completed worldId, failed requirement
                        shouldActivate = false;
                        break;
                    }
                }
            }

            return shouldActivate;
        }

        protected override void PerformCurrentStep()
        {
            base.PerformCurrentStep();

            // Get the step data
            if (this._currentStep < this.TutorialSteps.Count)
            {
                var stepData = this.TutorialSteps[this._currentStep];
                //Debug.Log($"Performing step {this._currentStep}");

                // Perform step
                switch (stepData.StepType)
                {
                    case TutorialGameStepType.ShowImageAndTextOnly:
                        this.ShowTextAndImageOnly(stepData).Forget();
                        break;
                    case TutorialGameStepType.PauseGame:
                        this.PauseGame(stepData).Forget();
                        break;
                    case TutorialGameStepType.UnpauseGame:
                        this.UnpauseGame(stepData).Forget();
                        break;
                    case TutorialGameStepType.ShowPnlInfoPopup:
                        this.ShowPnlInfoPopup(stepData).Forget();
                        break;
                    default:
                        break;
                }
            }
            else
            {
                // Step not found, end tutorial
                if (this._pnlTutorial)
                {
                    this._pnlTutorial.Close();
                    this._pnlTutorial = null;
                }
                this.CompleteTutorial();
            }
        }

        private async UniTask ShowTextAndImageOnly(TutorialGameStepData stepData)
        {
            if (stepData.StepDurationType != TutorialGameStepDurationType.GoToNextStepImmediately)
            {
                if (this._pnlTutorial == null)
                {
                    // Show text and cutout panel
                    this._pnlTutorial = await PanelManager.Instance.ShowAsync<PnlTutorial>();
                    if (!this) return;  // Check for scene unload
                }

                this._pnlTutorial.Init(new PnlTutorialData(stepData));
            }

            await this.GoToNextStep(stepData, true);
        }

        private async UniTask PauseGame(TutorialGameStepData stepData)
        {
            BattleManager.Instance.PauseGame(BattleManager.PauseType.PnlTutorial);
            await this.GoToNextStep(stepData, false);
        }

        private async UniTask UnpauseGame(TutorialGameStepData stepData)
        {
            BattleManager.Instance.ResumeGame(BattleManager.PauseType.PnlTutorial);
            await this.GoToNextStep(stepData, false);
        }

        private async UniTask ShowPnlInfoPopup(TutorialGameStepData stepData)
        {
            PanelManager.Instance.ShowAsync<PnlInfoPopup>((pnl) =>
            {
                pnl.Init(stepData.HeaderText, stepData.PromptText, stepData.InfoPopupImagePath);
            }).Forget();
            await this.GoToNextStep(stepData, false);
        }

        private async UniTask GoToNextStep(TutorialGameStepData stepData, bool allowWaitForUserInput)
        {
            switch (stepData.StepDurationType)
            {
                case TutorialGameStepDurationType.WaitForUserInput: // 0
                    if (allowWaitForUserInput)
                    {
                        this._pnlTutorial.OnClickNextEvent += this.CompleteCurrentStep;
                    }
                    else
                    {
                        Debug.LogError($"Tutorial Step index {this._currentStep} of action {stepData.StepType} should not be waiting for user input");
                    }
                    break;

                case TutorialGameStepDurationType.GoToNextStepImmediately: // 1
                    this.CompleteCurrentStep();
                    break;

                case TutorialGameStepDurationType.GoToNextStepAfterSeconds: // 2
                    await UniTask.WaitForSeconds(stepData.NextStepAfterSeconds, stepData.IgnoreTimeScale);
                    if (!this) return;  // Check for scene unload
                    this.CompleteCurrentStep();
                    break;

                case TutorialGameStepDurationType.WaitForPnlInfoPopupClose: // 3
                    // Setup callbacks with closures
                    var flag = false;
                    void SetFlag() => flag = true;
                    bool CheckFlag() => flag;

                    PnlInfoPopup.OnOkay += SetFlag;

                    await UniTask.WaitUntil(CheckFlag);
                    if (!this)
                    {
                        PnlInfoPopup.OnOkay -= SetFlag;
                        return;
                    }

                    PnlInfoPopup.OnOkay -= SetFlag;

                    this.CompleteCurrentStep();

                    break;

                case TutorialGameStepDurationType.WaitForBathTileDragged: // 4
                    await UniTask.WaitUntil(() => BathSlideTile.CurrentDraggedTile != null);
                    if (!this) return;

                    this.CompleteCurrentStep();

                    break;

                default:
                    break;
            }
        }

        private void CompleteCurrentStep()
        {
            if (this._pnlTutorial)
            {
                this._pnlTutorial.OnClickNextEvent -= this.CompleteCurrentStep;

                var keepPnlTutorial = false;
                if (this._currentStep < this.TutorialSteps.Count)
                {
                    var stepData = this.TutorialSteps[this._currentStep];
                    keepPnlTutorial = stepData.KeepPnlTutorialForNextStep;
                }

                if (!keepPnlTutorial)
                {
                    this._pnlTutorial.Close();
                    this._pnlTutorial = null;
                }
            }

            // Do next step
            this._currentStep += 1;
            this.PerformCurrentStep();
        }
    }
}