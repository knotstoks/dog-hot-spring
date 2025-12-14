using BroccoliBunnyStudios.Managers;
using ProjectRuntime.UI;
using System;
using UnityEngine;

namespace ProjectRuntime.Tutorial
{
    public abstract class TutorialController : MonoBehaviour
    {
        public static int TutorialsInProgress { get; protected set; } = 0;

        [field: SerializeField]
        public string TutorialId { get; set; }

        // Accessible variables
        public static event Action<string> OnTutorialStart;
        public static event Action<string> OnTutorialEnd;
        protected int _currentStep = -1;

        // Internal variables
        protected PnlTutorial _pnlTutorial;

        // Subclasses can override this to add conditions to activate
        public virtual bool CheckAndActivate()
        {
            this.Activate();
            return true;
        }

        protected void Activate()
        {
            this._currentStep = 0;
            this.gameObject.SetActive(true);

            TutorialsInProgress++;

            // Callback
            OnTutorialStart?.Invoke(this.TutorialId);
        }

        protected virtual void Start()
        {
            this.PerformCurrentStep();
        }

        protected virtual void PerformCurrentStep()
        {
            // Subclasses override this
        }

        protected void CompleteTutorial()
        {
            var completedTutorials = SaveManager.Instance.CompletedTutorials;
            if (!completedTutorials.Contains(this.TutorialId))
            {
                completedTutorials.Add(this.TutorialId);
                SaveManager.Instance.CompletedTutorials = completedTutorials;
            }
            this.gameObject.SetActive(false);
            TutorialsInProgress--;

            // Callback
            OnTutorialEnd?.Invoke(this.TutorialId);
        }
    }
}