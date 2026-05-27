using UnityEngine;
using UnityEngine.UI;

public class PersonajeCabinaVisual : MaskableGraphic
{
    [SerializeField] private Color colorCabeza = new Color(0.58f, 0.28f, 0.12f, 1f);
    [SerializeField] private Color colorTorso = new Color(0.12f, 0.2f, 0.48f, 1f);
    [SerializeField] private Color colorBrazo = new Color(0.45f, 0.2f, 0.08f, 1f);
    [SerializeField] private Color colorCabello = new Color(0.06f, 0.035f, 0.02f, 1f);
    [SerializeField] private bool mirandoIzquierda;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect rect = GetPixelAdjustedRect();
        float w = rect.width;
        float h = rect.height;
        Vector2 center = rect.center;

        AddBody(vh, rect, colorTorso);
        AddArm(vh, center + new Vector2((mirandoIzquierda ? -0.18f : 0.18f) * w, -0.08f * h), mirandoIzquierda ? -1f : 1f, w, h, colorBrazo);
        AddCircle(vh, center + new Vector2(0f, h * 0.23f), Mathf.Min(w, h) * 0.2f, colorCabeza);
        AddHair(vh, center + new Vector2(0f, h * 0.34f), w, h, colorCabello);
    }

    private void AddBody(VertexHelper vh, Rect rect, Color32 bodyColor)
    {
        Vector2 a = new Vector2(rect.xMin + rect.width * 0.2f, rect.yMin);
        Vector2 b = new Vector2(rect.xMax - rect.width * 0.2f, rect.yMin);
        Vector2 c = new Vector2(rect.xMax - rect.width * 0.31f, rect.yMin + rect.height * 0.55f);
        Vector2 d = new Vector2(rect.xMin + rect.width * 0.31f, rect.yMin + rect.height * 0.55f);
        AddQuad(vh, a, b, c, d, bodyColor);
    }

    private void AddArm(VertexHelper vh, Vector2 shoulder, float side, float w, float h, Color32 armColor)
    {
        Vector2 elbow = shoulder + new Vector2(side * w * 0.2f, -h * 0.18f);
        Vector2 hand = elbow + new Vector2(side * w * 0.18f, -h * 0.05f);
        float thickness = w * 0.06f;

        AddSegment(vh, shoulder, elbow, thickness, armColor);
        AddSegment(vh, elbow, hand, thickness, armColor);
        AddCircle(vh, hand, thickness * 1.25f, armColor);
    }

    private void AddHair(VertexHelper vh, Vector2 top, float w, float h, Color32 hairColor)
    {
        Vector2 a = top + new Vector2(-w * 0.16f, -h * 0.05f);
        Vector2 b = top + new Vector2(w * 0.16f, -h * 0.05f);
        Vector2 c = top + new Vector2(0f, h * 0.11f);
        AddTriangle(vh, a, b, c, hairColor);
    }

    private void AddSegment(VertexHelper vh, Vector2 a, Vector2 b, float thickness, Color32 segmentColor)
    {
        Vector2 dir = (b - a).normalized;
        Vector2 normal = new Vector2(-dir.y, dir.x) * thickness;
        AddQuad(vh, a - normal, a + normal, b + normal, b - normal, segmentColor);
    }

    private void AddCircle(VertexHelper vh, Vector2 centro, float radio, Color32 circleColor)
    {
        int steps = 32;
        int start = vh.currentVertCount;
        vh.AddVert(centro, circleColor, Vector2.zero);

        for (int i = 0; i <= steps; i++)
        {
            float angle = Mathf.PI * 2f * i / steps;
            vh.AddVert(centro + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radio, circleColor, Vector2.zero);
        }

        for (int i = 1; i <= steps; i++)
        {
            vh.AddTriangle(start, start + i, start + i + 1);
        }
    }

    private void AddQuad(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color32 quadColor)
    {
        int start = vh.currentVertCount;
        vh.AddVert(a, quadColor, Vector2.zero);
        vh.AddVert(b, quadColor, Vector2.zero);
        vh.AddVert(c, quadColor, Vector2.zero);
        vh.AddVert(d, quadColor, Vector2.zero);
        vh.AddTriangle(start, start + 1, start + 2);
        vh.AddTriangle(start, start + 2, start + 3);
    }

    private void AddTriangle(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Color32 triangleColor)
    {
        int start = vh.currentVertCount;
        vh.AddVert(a, triangleColor, Vector2.zero);
        vh.AddVert(b, triangleColor, Vector2.zero);
        vh.AddVert(c, triangleColor, Vector2.zero);
        vh.AddTriangle(start, start + 1, start + 2);
    }
}
