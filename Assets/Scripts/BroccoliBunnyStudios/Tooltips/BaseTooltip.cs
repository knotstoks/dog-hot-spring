using System;
using BroccoliBunnyStudios.Extensions;
using BroccoliBunnyStudios.Utils;
using UnityEngine;

namespace BroccoliBunnyStudios.Tooltips
{
    public class BaseTooltip : MonoBehaviour
    {
        [field: SerializeField]
        private RectTransform TipRT { get; set; }

        protected TooltipAlignment _alignment;
        protected RectTransform _sourceRect;
        protected RectTransform _layer;
        protected const float TIP_INSET = 15f;
        protected const float TIP_CORNER_INSET = 28f;

        public event Action<BaseTooltip> OnTooltipHide;

        public void Show(RectTransform layer, RectTransform sourceRect, TooltipAlignment alignment)
        {
            this._layer = layer;
            this._sourceRect = sourceRect;
            this._alignment = alignment;

            this.transform.SetParent(this._layer);
            this.transform.ResetLocalTransform();

            CommonUtil.ForceRebuildLayoutImmediateRecursive(this.transform as RectTransform);

            this.PositionTooltip();
        }

        public virtual void Hide()
        {
            Destroy(this.gameObject);
            this.OnTooltipHide?.Invoke(this);
        }

        protected void PositionTooltip()
        {
            var selfRT = (RectTransform)this.transform;

            var sourceRectLocal = this._sourceRect.rect;
            var sourceCenter = sourceRectLocal.center;
            switch (this._alignment)
            {
                case TooltipAlignment.CenterOfScreen:
                    if (this.TipRT)
                    {
                        this.TipRT.gameObject.SetActive(false);
                    }
                    break;

                case TooltipAlignment.Center:
                    // Get bottom center pos
                    sourceCenter.y -= this._sourceRect.rect.height / 2f;
                    selfRT.transform.position = this._sourceRect.TransformPoint(sourceCenter);

                    if (this.TipRT)
                    {
                        this.TipRT.gameObject.SetActive(false);
                    }
                    break;

                case TooltipAlignment.TopCenter:
                    {
                        // Get top center pos
                        sourceCenter.y += this._sourceRect.rect.height / 2f;
                        selfRT.transform.position = this._sourceRect.TransformPoint(sourceCenter);

                        // Move ourselves up half height
                        var delta = new Vector3(0f, selfRT.rect.height / 2f, 0f);
                        //if (this.TipRT)
                        //{
                        //    delta.y += this.TipRT.rect.height - TIP_INSET;
                        //}
                        selfRT.transform.localPosition += delta;

                        // This section got commented out due to it messing up the dimensions
                        // Position tip
                        //if (this.TipRT)
                        //{
                        //    var tipPosition = new Vector3(0f, -selfRT.rect.height / 2f, 0f);
                        //    tipPosition.y -= this.TipRT.rect.height / 2f;
                        //    tipPosition.y += TIP_INSET;  // Put 15 pixels overlap of tip into tooltip
                        //    this.TipRT.localPosition = tipPosition;
                        //    this.TipRT.localScale = Vector3.one;
                        //    this.TipRT.gameObject.SetActive(true);
                        //}
                        break;
                    }

                case TooltipAlignment.BottomCenter:
                    {
                        // Get bottom center pos
                        sourceCenter.y -= this._sourceRect.rect.height / 2f;
                        selfRT.transform.position = this._sourceRect.TransformPoint(sourceCenter);

                        // Move ourselves down half height
                        var delta = new Vector3(0f, selfRT.rect.height / 2f, 0f);
                        if (this.TipRT)
                        {
                            delta.y += this.TipRT.rect.height - TIP_INSET;
                        }
                        selfRT.transform.localPosition -= delta;

                        // Position tip
                        if (this.TipRT)
                        {
                            var tipPosition = new Vector3(0f, selfRT.rect.height / 2f, 0f);
                            tipPosition.y += this.TipRT.rect.height / 2f;
                            tipPosition.y -= TIP_INSET;  // Put 15 pixels overlap of tip into tooltip
                            this.TipRT.localPosition = tipPosition;
                            this.TipRT.localScale = new Vector3(1f, -1f, 1f);
                            this.TipRT.gameObject.SetActive(true);
                        }
                        break;
                    }
            }

            // Shift tooltip to within screen bounds
            var canvasSize = this._layer.rect.size;
            var cw = canvasSize.x / 2 - 20f; // 10 pixels buffer on each side
            var ch = canvasSize.y / 2 - 20f; // 10 pixels buffer on each side
            var rect = selfRT.rect;
            var localPos = selfRT.localPosition;
            var xMin = rect.xMin + localPos.x;
            var xMax = rect.xMax + localPos.x;
            var yMin = rect.yMin + localPos.y;
            var yMax = rect.yMax + localPos.y;
            var dx = 0f;
            var dy = 0f;
            if (xMin < -cw)
            {
                dx = -cw - xMin;
            }
            if (xMax > cw)
            {
                dx = cw - xMax;
            }
            if (yMin < -ch)
            {
                dy = -ch - yMin;
            }
            if (yMax > ch)
            {
                dy = ch - yMax;
            }
            selfRT.localPosition = new Vector3(localPos.x + dx, localPos.y + dy, localPos.z);

            // Move tip of tooltip back to point at original rect
            if (this.TipRT)
            {
                switch (this._alignment)
                {
                    case TooltipAlignment.TopCenter:
                    case TooltipAlignment.BottomCenter:
                        // Calculate max x-offset the tip can move
                        var maxOffset = selfRT.rect.width / 2f - this.TipRT.rect.width / 2f - TIP_CORNER_INSET;
                        if (maxOffset < 0f)
                        {
                            maxOffset = 0f;
                        }

                        // Clamp dx between -maxOffset and maxOffset so it doesn't move outside the tooltip frame
                        if (dx < -maxOffset)
                        {
                            dx = -maxOffset;
                        }
                        else if (dx > maxOffset)
                        {
                            dx = maxOffset;
                        }

                        this.TipRT.localPosition += new Vector3(-dx, 0f, 0f);
                        break;
                }
            }
        }
    }
}