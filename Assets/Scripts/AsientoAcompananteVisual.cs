using UnityEngine;
using UnityEngine.UI;

public class AsientoAcompananteVisual : MaskableGraphic
{
    [SerializeField] private Color colorAsiento = new Color(0.18f, 0.055f, 0.018f, 1f);
    [SerializeField] private Color colorBorde = new Color(0.3f, 0.1f, 0.032f, 1f);
    [SerializeField] private Color colorSombra = new Color(0.075f, 0.018f, 0.008f, 1f);
    [SerializeField] private bool invertido;

    protected override void OnEnable()
    {
        base.OnEnable();
        raycastTarget = false;
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        raycastTarget = false;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect rect = GetPixelAdjustedRect();

        AddPolygon(
            vh,
            new[]
            {
                Punto(rect, 0.04f, 0f),
                Punto(rect, 1f, 0f),
                Punto(rect, 1f, 0.92f),
                Punto(rect, 0.91f, 0.98f),
                Punto(rect, 0.19f, 0.98f),
                Punto(rect, 0.08f, 0.91f)
            },
            colorAsiento
        );

        AddPolygon(
            vh,
            new[]
            {
                Punto(rect, 0.82f, 0f),
                Punto(rect, 1f, 0f),
                Punto(rect, 1f, 0.92f),
                Punto(rect, 0.91f, 0.98f),
                Punto(rect, 0.86f, 0.9f)
            },
            colorSombra
        );

        AddPolygon(
            vh,
            new[]
            {
                Punto(rect, 0.08f, 0.91f),
                Punto(rect, 0.19f, 0.98f),
                Punto(rect, 0.91f, 0.98f),
                Punto(rect, 0.86f, 0.91f),
                Punto(rect, 0.22f, 0.91f)
            },
            colorBorde
        );

        Color costura = colorBorde;
        costura.a = 0.55f;
        AddQuad(
            vh,
            Punto(rect, 0.13f, 0.08f),
            Punto(rect, 0.15f, 0.08f),
            Punto(rect, 0.22f, 0.86f),
            Punto(rect, 0.2f, 0.86f),
            costura
        );
    }

    public void ConfigurarInvertido(bool nuevoInvertido)
    {
        invertido = nuevoInvertido;
        raycastTarget = false;
        SetVerticesDirty();
    }

    private Vector2 Punto(Rect rect, float x, float y)
    {
        if (invertido)
        {
            x = 1f - x;
        }

        return new Vector2(Mathf.Lerp(rect.xMin, rect.xMax, x), Mathf.Lerp(rect.yMin, rect.yMax, y));
    }

    private void AddPolygon(VertexHelper vh, Vector2[] puntos, Color32 colorPoligono)
    {
        int inicio = vh.currentVertCount;
        for (int i = 0; i < puntos.Length; i++)
        {
            vh.AddVert(puntos[i], colorPoligono, Vector2.zero);
        }

        for (int i = 1; i < puntos.Length - 1; i++)
        {
            vh.AddTriangle(inicio, inicio + i, inicio + i + 1);
        }
    }

    private void AddQuad(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color32 colorCuad)
    {
        int inicio = vh.currentVertCount;
        vh.AddVert(a, colorCuad, Vector2.zero);
        vh.AddVert(b, colorCuad, Vector2.zero);
        vh.AddVert(c, colorCuad, Vector2.zero);
        vh.AddVert(d, colorCuad, Vector2.zero);
        vh.AddTriangle(inicio, inicio + 1, inicio + 2);
        vh.AddTriangle(inicio, inicio + 2, inicio + 3);
    }
}
