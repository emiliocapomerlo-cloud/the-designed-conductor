using UnityEngine;
using UnityEngine.UI;

public class PalancaVisual : MaskableGraphic
{
    [SerializeField] private Color colorBase = new Color(0.05f, 0.04f, 0.035f, 1f);
    [SerializeField] private Color colorMetal = new Color(0.45f, 0.46f, 0.44f, 1f);
    [SerializeField] private Color colorPerilla = new Color(0.88f, 0.78f, 0.08f, 1f);

    private const int Segmentos = 18;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect rect = GetPixelAdjustedRect();
        float ancho = rect.width;
        float alto = rect.height;
        Vector2 centro = rect.center;

        AddQuad(
            vh,
            new Vector2(centro.x - ancho * 0.14f, rect.yMin + alto * 0.08f),
            new Vector2(centro.x + ancho * 0.14f, rect.yMin + alto * 0.08f),
            new Vector2(centro.x + ancho * 0.11f, rect.yMin + alto * 0.72f),
            new Vector2(centro.x - ancho * 0.11f, rect.yMin + alto * 0.72f),
            colorMetal
        );

        AddQuad(
            vh,
            new Vector2(centro.x - ancho * 0.28f, rect.yMin + alto * 0.02f),
            new Vector2(centro.x + ancho * 0.28f, rect.yMin + alto * 0.02f),
            new Vector2(centro.x + ancho * 0.22f, rect.yMin + alto * 0.16f),
            new Vector2(centro.x - ancho * 0.22f, rect.yMin + alto * 0.16f),
            colorBase
        );

        AddQuad(
            vh,
            new Vector2(centro.x - ancho * 0.42f, rect.yMin + alto * 0.68f),
            new Vector2(centro.x + ancho * 0.42f, rect.yMin + alto * 0.68f),
            new Vector2(centro.x + ancho * 0.32f, rect.yMin + alto * 0.88f),
            new Vector2(centro.x - ancho * 0.32f, rect.yMin + alto * 0.88f),
            colorPerilla
        );

        AddEllipse(vh, new Vector2(centro.x, rect.yMin + alto * 0.88f), ancho * 0.31f, alto * 0.08f, colorPerilla);
    }

    private void AddEllipse(VertexHelper vh, Vector2 centro, float radioX, float radioY, Color32 ellipseColor)
    {
        int start = vh.currentVertCount;
        vh.AddVert(centro, ellipseColor, Vector2.zero);
        for (int i = 0; i <= Segmentos; i++)
        {
            float angle = Mathf.PI * 2f * i / Segmentos;
            vh.AddVert(centro + new Vector2(Mathf.Cos(angle) * radioX, Mathf.Sin(angle) * radioY), ellipseColor, Vector2.zero);
        }

        for (int i = 1; i <= Segmentos; i++)
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
