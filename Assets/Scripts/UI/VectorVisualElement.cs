using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class VectorArrowElement : VisualElement
{
    [UxmlAttribute] public Color Color { get; set; } = Color.white;
    public Vector2 Vector
    {
        get => vector;
        set
        {
            if (vector == value)
                return;

            vector = value;
            MarkDirtyRepaint();
        }
    }
    public float MaxVectorMagnitude
    {
        get => maxVectorMagnitude;
        set
        {
            value = Mathf.Max(0.0001f, value);

            if (Mathf.Approximately(maxVectorMagnitude, value))
                return;

            maxVectorMagnitude = value;
            MarkDirtyRepaint();
        }
    }

    public float DotRadius { get; set; } = 4f;

    public float LineWidth { get; set; } = 2f;

    private Vector2 vector;
    private float maxVectorMagnitude = 6f;

    public VectorArrowElement()
    {
        generateVisualContent += GenerateVisualContent;
    }

    private void GenerateVisualContent(MeshGenerationContext context)
    {
        var painter = context.painter2D;

        painter.lineWidth = LineWidth;
        painter.strokeColor = Color;
        painter.fillColor = Color;


        Vector2 center = contentRect.center;

        painter.BeginPath();
        painter.Arc(center, DotRadius, 0f, 360f);
        painter.Fill();

        if (vector.sqrMagnitude < Mathf.Epsilon)
            return;

        Vector2 direction = new Vector2(vector.x, -vector.y).normalized;

        float magnitude = vector.magnitude;

        float normalizedMagnitude =
            Mathf.Clamp01(magnitude / maxVectorMagnitude);

        float maxArrowLength =
            Mathf.Min(contentRect.width, contentRect.height) * 0.5f;

        if (maxArrowLength <= 0f)
            return;

        float shaftLength = normalizedMagnitude * maxArrowLength;

        if (shaftLength <= 0f)
            return;

        Vector2 tip = center + direction * shaftLength;


        painter.BeginPath();
        painter.MoveTo(center);
        painter.LineTo(tip);
        painter.Stroke();
    }
}
