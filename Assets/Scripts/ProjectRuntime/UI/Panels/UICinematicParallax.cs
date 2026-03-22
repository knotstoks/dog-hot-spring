using UnityEngine;

public class UICinematicParallax : MonoBehaviour
{
    [field: SerializeField, Header("Scene References")]
    private RectTransform ParallaxRT { get; set; }

    [field: SerializeField, Tooltip("The smaller it is the closer it is")]
    private float ParallaxDistance { get; set; }

    [field: SerializeField]
    private float MaxOffset { get; set; }

    private void Update()
    {
        this.UpdateParallax();
    }

    private void UpdateParallax()
    {
        var mousePos = Input.mousePosition;

        var screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        var delta = (Vector2)mousePos - screenCenter;
        delta.x /= screenCenter.x;
        delta.y /= screenCenter.y;

        // Invert movement for natural parallax feel (optional)
        delta *= -1f;

        // Apply layer depth scaling (further layers move less)
        var depthFactor = 5f / this.ParallaxDistance;

        var offset = new Vector3(
            delta.x * this.MaxOffset * depthFactor,
            delta.y * this.MaxOffset * depthFactor,
            0f
        );

        // Clamp final offset
        offset.x = Mathf.Clamp(offset.x, -this.MaxOffset, this.MaxOffset);
        offset.y = Mathf.Clamp(offset.y, -this.MaxOffset, this.MaxOffset);

        
        this.ParallaxRT.anchoredPosition = offset;
    }
}
