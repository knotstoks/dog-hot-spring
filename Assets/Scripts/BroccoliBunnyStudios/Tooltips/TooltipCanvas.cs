using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BroccoliBunnyStudios.Tooltips
{
    public class TooltipCanvas : MonoBehaviour
    {
        private int _lastFrameTouchCount = 0;
        private bool _skipCheckForClosingTooltips;

        private void OnEnable()
        {
            this._lastFrameTouchCount = Input.touchCount;
        }

        private void Update()
        {
            if (!this._skipCheckForClosingTooltips && (Input.touchCount > this._lastFrameTouchCount || Input.anyKeyDown))
            {
                //if (this.CheckIfAnyTouchIsOverTooltip<MapLocationTooltip>())
                //{
                //    // Empty
                //}
                //if (this.CheckIfAnyTouchIsOverTooltip<RewardListTooltip>())
                //{
                //    TooltipManager.Instance.HideAllToolTipsExceptRewardListTooltip();
                //}
                //else if (this.CheckIfAnyTouchIsOverTooltip<TalentUpgradeTooltip>())
                //{
                //    // Empty intentionally
                //}
                //else
                TooltipManager.Instance.HideAllToolTips();
            }

            this._lastFrameTouchCount = Input.touchCount;
        }

        public void SkipCheckForClosingTooltips(bool skipCheck)
        {
            this._skipCheckForClosingTooltips = skipCheck;
        }

        private bool CheckIfAnyTouchIsOverTooltip<T>() where T : BaseTooltip
        {
            var raycastResults = new List<RaycastResult>();
            var eventData = new PointerEventData(EventSystem.current);

            // For each touch
            foreach (var touch in Input.touches)
            {
                eventData.position = touch.position;
                raycastResults.Clear();
                EventSystem.current.RaycastAll(eventData, raycastResults);

                foreach (var result in raycastResults)
                {
                    var tooltip = result.gameObject.GetComponent<T>();
                    if (tooltip)
                    {
                        return true;
                    }
                }
            }

            // Check mouse
            eventData.position = Input.mousePosition;
            raycastResults.Clear();
            EventSystem.current.RaycastAll(eventData, raycastResults);
            foreach (var result in raycastResults)
            {
                var tooltip = result.gameObject.GetComponent<T>();
                if (tooltip)
                {
                    return true;
                }
            }

            return false;
        }
    }
}