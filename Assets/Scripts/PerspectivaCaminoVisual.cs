using UnityEngine;
using UnityEngine.UI;

public class PerspectivaCaminoVisual : MaskableGraphic
{
    [SerializeField] private Color colorCielo = new Color(0.45f, 0.85f, 0.9f, 1f);
    [SerializeField] private Color colorBosque = new Color(0.1f, 0.42f, 0.16f, 1f);
    [SerializeField] private Color colorBanquina = new Color(0.18f, 0.48f, 0.12f, 1f);
    [SerializeField] private Color colorCamino = new Color(0.12f, 0.12f, 0.13f, 1f);
    [SerializeField] private Color colorLinea = new Color(0.95f, 0.82f, 0.12f, 1f);

    [Range(0.05f, 0.45f)]
    [SerializeField] private float anchoHorizonte = 0.16f;

    [Range(0.4f, 1f)]
    [SerializeField] private float anchoFrente = 0.9f;

    [Range(0.45f, 0.85f)]
    [SerializeField] private float alturaHorizonte = 0.62f;

    private float avance;
    private float desvioHorizontal;

    public void ActualizarMovimiento(float nuevoAvance, float nuevoDesvioHorizontal)
    {
        avance = nuevoAvance;
        desvioHorizontal = nuevoDesvioHorizontal;
        SetVerticesDirty();
    }

    public void ConfigurarEncuadreCabina(float nuevoAnchoHorizonte, float nuevoAnchoFrente, float nuevaAlturaHorizonte)
    {
        anchoHorizonte = Mathf.Clamp(nuevoAnchoHorizonte, 0.05f, 0.45f);
        anchoFrente = Mathf.Clamp(nuevoAnchoFrente, 0.4f, 1f);
        alturaHorizonte = Mathf.Clamp(nuevaAlturaHorizonte, 0.45f, 0.85f);
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect rect = GetPixelAdjustedRect();
        float left = rect.xMin;
        float right = rect.xMax;
        float bottom = rect.yMin;
        float top = rect.yMax;
        float width = rect.width;
        float height = rect.height;
        float center = Mathf.Lerp(left, right, 0.5f) + desvioHorizontal;
        float horizonY = Mathf.Lerp(bottom, top, alturaHorizonte);

        AddQuad(vh, new Vector2(left, horizonY), new Vector2(right, horizonY), new Vector2(right, top), new Vector2(left, top), colorCielo);
        AddQuad(vh, new Vector2(left, bottom), new Vector2(right, bottom), new Vector2(right, horizonY), new Vector2(left, horizonY), colorBanquina);

        AddJaggedTreeLine(vh, rect, horizonY);

        float topHalf = width * anchoHorizonte * 0.5f;
        float bottomHalf = width * anchoFrente * 0.5f;
        Vector2 roadTopLeft = new Vector2(center - topHalf, horizonY);
        Vector2 roadTopRight = new Vector2(center + topHalf, horizonY);
        Vector2 roadBottomRight = new Vector2(center + bottomHalf, bottom);
        Vector2 roadBottomLeft = new Vector2(center - bottomHalf, bottom);
        AddQuad(vh, roadBottomLeft, roadBottomRight, roadTopRight, roadTopLeft, colorCamino);

        AddPerspectiveStripes(vh, center, bottom, horizonY, width, height);
    }

    private void AddJaggedTreeLine(VertexHelper vh, Rect rect, float horizonY)
    {
        int count = 18;
        float step = rect.width / count;

        for (int i = 0; i < count; i++)
        {
            float x = rect.xMin + i * step;
            float h = rect.height * (0.08f + 0.08f * Mathf.PerlinNoise(i * 0.8f, 3f));
            Vector2 a = new Vector2(x, horizonY);
            Vector2 b = new Vector2(x + step * 0.5f, horizonY + h);
            Vector2 c = new Vector2(x + step, horizonY);
            AddTriangle(vh, a, b, c, colorBosque);
        }
    }

    private void AddPerspectiveStripes(VertexHelper vh, float center, float bottom, float horizonY, float width, float height)
    {
        int stripeCount = 9;
        float scroll = Mathf.Repeat(avance * 0.0025f, 1f);

        for (int i = 0; i < stripeCount; i++)
        {
            float t = (i + scroll) / stripeCount;
            float nextT = Mathf.Min(t + 0.035f + t * 0.035f, 0.98f);

            float yNear = Mathf.Lerp(bottom, horizonY, t);
            float yFar = Mathf.Lerp(bottom, horizonY, nextT);
            float perspectiveNear = 1f - t;
            float perspectiveFar = 1f - nextT;

            float stripeNear = Mathf.Lerp(32f, 5f, t);
            float stripeFar = Mathf.Lerp(32f, 5f, nextT);
            float xNearOffset = Mathf.Sin(t * Mathf.PI * 0.75f) * width * 0.03f * perspectiveNear;
            float xFarOffset = Mathf.Sin(nextT * Mathf.PI * 0.75f) * width * 0.03f * perspectiveFar;

            Vector2 a = new Vector2(center - stripeNear * 0.5f + xNearOffset, yNear);
            Vector2 b = new Vector2(center + stripeNear * 0.5f + xNearOffset, yNear);
            Vector2 c = new Vector2(center + stripeFar * 0.5f + xFarOffset, yFar);
            Vector2 d = new Vector2(center - stripeFar * 0.5f + xFarOffset, yFar);
            AddQuad(vh, a, b, c, d, colorLinea);
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
