using UnityEngine;

namespace BroccoliBunnyStudios.Util
{
    public static class CameraHelper
    {
        public static Vector3 GetUiPosition(Camera camera, Vector2 canvasSize, Vector3 worldPosition)
        {
            // ref: https://answers.unity.com/questions/799616/unity-46-beta-19-how-to-convert-from-world-space-t.html
            var viewportPosition = camera.WorldToViewportPoint(worldPosition);
            var worldObjectScreenPosition = new Vector2(
                (viewportPosition.x * canvasSize.x) - (canvasSize.x * 0.5f),
                (viewportPosition.y * canvasSize.y) - (canvasSize.y * 0.5f));

            return worldObjectScreenPosition;
        }

        public static (Vector3 position, Quaternion rotation, bool onScreen) GetProjectedUiPositionOnScreen(
            Camera camera,
            Vector2 canvasSize,
            Rect indicatorArea,
            Vector3 referencePosition,
            Vector3 worldPosition,
            Vector2 uiComponentSize)
        {
            // ref: https://answers.unity.com/questions/799616/unity-46-beta-19-how-to-convert-from-world-space-t.html
            var viewportReferencePosition = (Vector2)camera.WorldToViewportPoint(referencePosition);
            var viewportPosition = (Vector2)camera.WorldToViewportPoint(worldPosition);

            var halfWidth = canvasSize.x / 2;
            var halfHeight = canvasSize.y / 2;

            var x = uiComponentSize.x / canvasSize.x / 2;
            var y = uiComponentSize.y / canvasSize.y / 2;

            var direction = (viewportPosition - viewportReferencePosition).normalized;
            var angleInRadians = Mathf.Atan2(direction.y, direction.x) - (Mathf.Deg2Rad * 90);
            var rotation = Quaternion.AngleAxis(angleInRadians * Mathf.Rad2Deg, Vector3.forward);

            if (!PointWithinBounds(viewportPosition, new Vector2(x, y)))
            {
                var uiPosition = new Vector2(
                    (viewportPosition.x * canvasSize.x) - halfWidth,
                    (viewportPosition.y * canvasSize.y) - halfHeight);

                return (uiPosition, rotation, true);
            }

            // calculate indicator position using directional vector and angles
            var cos = Mathf.Cos(angleInRadians);
            var sin = -Mathf.Sin(angleInRadians);
            var m = cos / sin;

            var leftWidth = viewportReferencePosition.x * canvasSize.x;
            var rightWith = (1 - viewportReferencePosition.x) * canvasSize.x;
            var bottomHeight = viewportReferencePosition.y * canvasSize.y;
            var topHeight = (1 - viewportReferencePosition.y) * canvasSize.y;

            var indicatorPosition = cos > 0
                ? new Vector2(topHeight / m, topHeight)
                : new Vector2(-bottomHeight / m, -bottomHeight);

            if (indicatorPosition.x > rightWith)
            {
                indicatorPosition = new Vector2(rightWith, rightWith * m);
            }
            else if (indicatorPosition.x < -leftWidth)
            {
                indicatorPosition = new Vector2(-leftWidth, -leftWidth * m);
            }

            var uiReferencePosition = new Vector2(
                (viewportReferencePosition.x * canvasSize.x) - halfWidth,
                (viewportReferencePosition.y * canvasSize.y) - halfHeight);

            // offset indicator to reference position on screen
            var offset = uiReferencePosition - indicatorArea.center;
            indicatorPosition += offset;

            // limit indicator to indicator area
            var left = indicatorArea.xMin + uiComponentSize.x / 2;
            var right = indicatorArea.xMax - uiComponentSize.x / 2;
            var bottom = indicatorArea.yMin + uiComponentSize.y / 2;
            var top = indicatorArea.yMax - uiComponentSize.y / 2;

            if (indicatorPosition.x < left) indicatorPosition.x = left;
            if (indicatorPosition.x > right) indicatorPosition.x = right;
            if (indicatorPosition.y < bottom) indicatorPosition.y = bottom;
            if (indicatorPosition.y > top) indicatorPosition.y = top;

            return (indicatorPosition, rotation, false);
        }

        public static float GetOrthographicSizeForScreenSize(float referenceSize, Vector2 referenceResolution)
        {
            var referenceAspectRatio = referenceResolution.y / referenceResolution.x;
            var screenAspectRatio = (float)Screen.height / Screen.width;
            return referenceSize * screenAspectRatio / referenceAspectRatio;
        }

        private static bool PointWithinBounds(Vector2 point, Vector2 halfBounds)
        {
            return point.x < -halfBounds.x ||
                point.x > 1 + halfBounds.x ||
                point.y < -halfBounds.y ||
                point.y > 1 + halfBounds.y;
        }
    }
}