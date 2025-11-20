using System;
using System.Collections.Generic;
using BroccoliBunnyStudios.Extensions;
using BroccoliBunnyStudios.Panel;
using BroccoliBunnyStudios.Utils;
using UnityEngine;

namespace BroccoliBunnyStudios.Tooltips
{
    public enum TooltipAlignment
    {
        CenterOfScreen = 0,
        Center = 1,
        TopCenter = 2,
        BottomCenter = 3,
    }

    public class TooltipManager
    {
        private static readonly Lazy<TooltipManager> s_lazy = new(() => new TooltipManager());
        public static TooltipManager Instance => s_lazy.Value;

        // Internal variables
        private RectTransform _tooltipCanvas;

        private readonly List<BaseTooltip> _activeTooltips = new();

        private TooltipManager()
        {
            this.InitializeTooltipLayer();
        }

        private void InitializeTooltipLayer()
        {
            if (this._tooltipCanvas)
            {
                return;
            }

            this._tooltipCanvas = PanelManager.Instance.TooltipCanvas.transform as RectTransform;
            this._tooltipCanvas.FGetComp<TooltipCanvas>();
        }

        private void ShowTooltip(BaseTooltip tooltip, RectTransform sourceRect, TooltipAlignment alignment)
        {
            this._activeTooltips.Add(tooltip);
            this._tooltipCanvas.SetActive(true);
            tooltip.Show(this._tooltipCanvas, sourceRect, alignment);

            tooltip.OnTooltipHide += this.OnTooltipHide;
        }

        private void OnTooltipHide(BaseTooltip toolTip)
        {
            toolTip.OnTooltipHide -= this.OnTooltipHide;

            this._activeTooltips.Remove(toolTip);
        }

        public void HideAllToolTips()
        {
            for (var i = this._activeTooltips.Count - 1; i >= 0; i--)
            {
                this._activeTooltips[i].Hide();
            }
            this._activeTooltips.Clear();

            this.CheckToHideTooltipCanvas();
        }

        //public void HideAllToolTipsExceptRewardListTooltip()
        //{
        //    this.RemoveNullTooltips();

        //    // Backwards loop because tooltip.Hide() will call OnTooltipHide() which removes from this._activeTooltips
        //    for (var i = this._activeTooltips.Count - 1; i >= 0; i--)
        //    {
        //        var tooltip = this._activeTooltips[i];
        //        if (tooltip.GetType() != typeof(RewardListTooltip))
        //        {
        //            tooltip.Hide();
        //        }
        //    }
        //}

        private void RemoveNullTooltips()
        {
            for (var i = this._activeTooltips.Count - 1; i >= 0; i--)
            {
                if (this._activeTooltips[i] == null)
                {
                    this._activeTooltips.RemoveAt(i);
                }
            }
        }

        public T GetToolTipInStack<T>() where T : BaseTooltip
        {
            for (var i = this._activeTooltips.Count - 1; i >= 0; i--)
            {
                var tooltip = this._activeTooltips[i];
                if (tooltip != null && tooltip.GetType() == typeof(T))
                {
                    return tooltip as T;
                }
            }
            return null;
        }

        public void CheckToHideTooltipCanvas()
        {
            if (this._activeTooltips.Count == 0)
            {
                this._tooltipCanvas.SetActive(false);
            }
        }
    }
}