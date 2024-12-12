using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/**
Custon Unity Graphic component for rendering smooth line charts using Catmull-Rom splines.

Accepts a list of normalized Vector2 data points to plot the chart.
Configurable line thickness and color.
Utilizes Catmull-Rom spline interpolation formula for smooth transitions between data points.
Draws line segments with a defined number of segments per curve for adjustable smoothness.
Handles the rendering of lines directly in Unity's UI system via VertexHelper.
**/

[RequireComponent(typeof(RectTransform))]
public class LineChartRenderer : Graphic
{
    public List<Vector2> dataPoints; // Normalized data points (0-1)
    public float lineThickness = 2.5f;
    public Color lineColor = Color.white;
    private const int SEGMENTS_PER_CURVE = 20; // Number of segments for smoothness

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (dataPoints == null || dataPoints.Count < 2) return;

        for (int i = 0; i < dataPoints.Count - 1; i++)
        {
            // Determine points for the Catmull-Rom spline
            Vector2 p0 = i > 0 ? dataPoints[i - 1] : dataPoints[i];
            Vector2 p1 = dataPoints[i];
            Vector2 p2 = dataPoints[i + 1];
            Vector2 p3 = i < dataPoints.Count - 2 ? dataPoints[i + 2] : dataPoints[i + 1];

            DrawCurve(vh, p0, p1, p2, p3, lineThickness, lineColor);
        }
    }

    private void DrawCurve(VertexHelper vh, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float thickness, Color color)
    {
        Vector2 prevPoint = p1;

        for (int j = 1; j <= SEGMENTS_PER_CURVE; j++)
        {
            float t = j / (float)SEGMENTS_PER_CURVE;
            Vector2 point = CalculateCatmullRomPoint(t, p0, p1, p2, p3);

            // Draw a line segment between the previous and current point
            DrawLine(vh, prevPoint, point, thickness, color);
            prevPoint = point;
        }
    }

    private Vector2 CalculateCatmullRomPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        // Catmull-Rom spline formula
        return 0.5f * (
            (2 * p1) +
            (-p0 + p2) * t +
            (2 * p0 - 5 * p1 + 4 * p2 - p3) * t * t +
            (-p0 + 3 * p1 - 3 * p2 + p3) * t * t * t
        );
    }

    private void DrawLine(VertexHelper vh, Vector2 start, Vector2 end, float thickness, Color color)
    {
        Vector2 dir = (end - start).normalized;
        Vector2 perp = new Vector2(-dir.y, dir.x) * thickness * 0.5f;

        Vector3 v1 = start - perp;
        Vector3 v2 = start + perp;
        Vector3 v3 = end + perp;
        Vector3 v4 = end - perp;

        int index = vh.currentVertCount;

        vh.AddVert(v1, color, Vector2.zero);
        vh.AddVert(v2, color, Vector2.zero);
        vh.AddVert(v3, color, Vector2.zero);
        vh.AddVert(v4, color, Vector2.zero);

        vh.AddTriangle(index, index + 1, index + 2);
        vh.AddTriangle(index, index + 2, index + 3);
    }
}
