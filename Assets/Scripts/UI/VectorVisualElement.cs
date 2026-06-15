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

    public float HeadLength { get; set; } = 10f;

    public float HeadWidth { get; set; } = 5f;

    public float DotRadius { get; set; } = 4f;

    public float Padding { get; set; } = 4f;

    public float LineWidth { get; set; } = 2f;

    private Vector2 vector;
    private float maxVectorMagnitude = 1f;

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

        Vector2 direction = vector.normalized;

        float magnitude = vector.magnitude;

        float normalizedMagnitude =
            Mathf.Clamp01(magnitude / maxVectorMagnitude);

        float maxArrowLength =
            Mathf.Min(contentRect.width, contentRect.height) * 0.5f
            - Padding
            - HeadLength;

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


        Vector2 perpendicular = new Vector2(-direction.y, direction.x);

        Vector2 headBase = tip - direction * HeadLength;

        Vector2 left = headBase + perpendicular * HeadWidth;
        Vector2 right = headBase - perpendicular * HeadWidth;

        painter.BeginPath();
        painter.MoveTo(tip);
        painter.LineTo(left);

        painter.MoveTo(tip);
        painter.LineTo(right);
        painter.Stroke();
    }
}
