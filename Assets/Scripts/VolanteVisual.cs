using UnityEngine;
using UnityEngine.UI;

public class VolanteVisual : MaskableGraphic
{
    [SerializeField] private Color colorAro = new Color(0.78f, 0.74f, 0.07f, 1f);
    [SerializeField] private Color colorCentro = new Color(0.75f, 0.75f, 0.72f, 1f);
    [SerializeField] private Color colorRadio = new Color(0.18f, 0.18f, 0.18f, 1f);
    [SerializeField] private int segmentos = 64;
    [SerializeField] private float grosorAro = 0.18f;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect rect = GetPixelAdjustedRect();
        Vector2 centro = rect.center;
        float radio = Mathf.Min(rect.width, rect.height) * 0.5f;
        float radioInterior = radio * (1f - grosorAro);

        AddRing(vh, centro, radio, radioInterior, colorAro);
        AddSpoke(vh, centro, radioInterior * 0.9f, 0f, colorRadio);
        AddSpoke(vh, centro, radioInterior * 0.9f, 120f, colorRadio);
        AddSpoke(vh, centro, radioInterior * 0.9f, 240f, colorRadio);
        AddCircle(vh, centro, radio * 0.18f, colorCentro);
    }

    private void AddRing(VertexHelper vh, Vector2 centro, float externo, float interno, Color32 ringColor)
    {
        int steps = Mathf.Max(12, segmentos);

        for (int i = 0; i < steps; i++)
        {
            float a0 = Mathf.PI * 2f * i / steps;
            float a1 = Mathf.PI * 2f * (i + 1) / steps;
            Vector2 outer0 = centro + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * externo;
            Vector2 outer1 = centro + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * externo;
            Vector2 inner1 = centro + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * interno;
            Vector2 inner0 = centro + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * interno;
            AddQuad(vh, outer0, outer1, inner1, inner0, ringColor);
        }
    }

    private void AddSpoke(VertexHelper vh, Vector2 centro, float largo, float grados, Color32 spokeColor)
    {
        Vector2 dir = new Vector2(Mathf.Cos(grados * Mathf.Deg2Rad), Mathf.Sin(grados * Mathf.Deg2Rad));
        Vector2 normal = new Vector2(-dir.y, dir.x);
        float ancho = largo * 0.12f;
        Vector2 end = centro + dir * largo;

        AddQuad(
            vh,
            centro - normal * ancho,
            centro + normal * ancho,
            end + normal * ancho * 0.55f,
            end - normal * ancho * 0.55f,
            spokeColor
        );
    }

    private void AddCircle(VertexHelper vh, Vector2 centro, float radio, Color32 circleColor)
    {
        int start = vh.currentVertCount;
        vh.AddVert(centro, circleColor, Vector2.zero);

        for (int i = 0; i <= segmentos; i++)
        {
            float angle = Mathf.PI * 2f * i / segmentos;
            vh.AddVert(centro + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radio, circleColor, Vector2.zero);
        }

        for (int i = 1; i <= segmentos; i++)
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
}
