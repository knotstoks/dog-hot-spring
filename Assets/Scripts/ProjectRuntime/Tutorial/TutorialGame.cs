using BroccoliBunnyStudios.Managers;
using ProjectRuntime.Tutorial;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

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
}
