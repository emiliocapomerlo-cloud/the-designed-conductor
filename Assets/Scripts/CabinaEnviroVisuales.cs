using UnityEngine;
using UnityEngine.UI;

public class CabinaEnviroVisuales : MaskableGraphic
{
    [SerializeField] private Color colorTablero = new Color(0.24f, 0.08f, 0.025f, 1f);
    [SerializeField] private Color colorTableroClaro = new Color(0.42f, 0.16f, 0.055f, 1f);
    [SerializeField] private Color colorSombra = new Color(0.08f, 0.025f, 0.01f, 1f);
    [SerializeField] private Color colorMarco = new Color(0.26f, 0.09f, 0.025f, 1f);
    [SerializeField] private Color colorMarcoOscuro = new Color(0.13f, 0.035f, 0.012f, 1f);
    [SerializeField] private Color colorMetal = new Color(0.48f, 0.5f, 0.48f, 1f);
    [SerializeField] private Color colorHueso = new Color(0.86f, 0.82f, 0.78f, 1f);

    private const int Segmentos = 20;
    private ControladorPaisaje controladorPaisaje;
    private float velocidadIndicada;

    protected override void OnEnable()
    {
        base.OnEnable();
        raycastTarget = false;
        BuscarControladorPaisaje();
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        raycastTarget = false;
        SetVerticesDirty();
    }

    private void LateUpdate()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        BuscarControladorPaisaje();
        float velocidadObjetivo = controladorPaisaje != null ? controladorPaisaje.VelocidadActual : 0f;
        velocidadIndicada = Mathf.Lerp(velocidadIndicada, velocidadObjetivo, 1f - Mathf.Exp(-Time.unscaledDeltaTime * 5f));
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect rect = GetPixelAdjustedRect();

        DibujarTablero(vh, rect);
        DibujarMarcosParabrisas(vh, rect);
        DibujarConsola(vh, rect);
        DibujarInstrumentos(vh, rect);
        DibujarDetalles(vh, rect);
    }

    private void DibujarTablero(VertexHelper vh, Rect r)
    {
        AddQuad(vh, P(r, 0f, 0f), P(r, 1f, 0f), P(r, 1f, 0.52f), P(r, 0f, 0.52f), colorTablero);
        AddQuad(vh, P(r, 0f, 0.46f), P(r, 1f, 0.46f), P(r, 1f, 0.55f), P(r, 0f, 0.55f), colorTableroClaro);
        AddQuad(vh, P(r, 0f, 0.45f), P(r, 1f, 0.45f), P(r, 1f, 0.48f), P(r, 0f, 0.48f), Sombrear(colorTablero, 0.62f));
        AddQuad(vh, P(r, 0.04f, 0.05f), P(r, 0.38f, 0.05f), P(r, 0.34f, 0.35f), P(r, 0.09f, 0.32f), colorSombra);
        AddQuad(vh, P(r, 0.62f, 0.05f), P(r, 0.96f, 0.05f), P(r, 0.91f, 0.32f), P(r, 0.66f, 0.35f), colorSombra);
    }

    private void DibujarMarcosParabrisas(VertexHelper vh, Rect r)
    {
        AddPolygon(vh, new[] { P(r, -0.03f, 1.05f), P(r, 0.07f, 1.05f), P(r, 0.22f, 0.52f), P(r, 0.16f, 0.48f), P(r, -0.03f, 0.85f) }, colorMarco);
        AddPolygon(vh, new[] { P(r, 1.03f, 1.05f), P(r, 0.93f, 1.05f), P(r, 0.78f, 0.52f), P(r, 0.84f, 0.48f), P(r, 1.03f, 0.85f) }, colorMarco);
        AddPolygon(vh, new[] { P(r, 0.055f, 1.05f), P(r, 0.09f, 1.05f), P(r, 0.24f, 0.52f), P(r, 0.205f, 0.52f) }, colorMarcoOscuro);
        AddPolygon(vh, new[] { P(r, 0.945f, 1.05f), P(r, 0.91f, 1.05f), P(r, 0.76f, 0.52f), P(r, 0.795f, 0.52f) }, colorMarcoOscuro);
        AddQuad(vh, P(r, 0.16f, 0.48f), P(r, 0.84f, 0.48f), P(r, 0.86f, 0.53f), P(r, 0.14f, 0.53f), Sombrear(colorMarco, 0.72f));
    }

    private void DibujarConsola(VertexHelper vh, Rect r)
    {
        AddPolygon(vh, new[] { P(r, 0.39f, 0f), P(r, 0.61f, 0f), P(r, 0.56f, 0.47f), P(r, 0.46f, 0.47f) }, Sombrear(colorTablero, 0.84f));
        AddPolygon(vh, new[] { P(r, 0.45f, 0.02f), P(r, 0.55f, 0.02f), P(r, 0.53f, 0.42f), P(r, 0.47f, 0.42f) }, Sombrear(colorTableroClaro, 0.62f));
    }

    private void DibujarInstrumentos(VertexHelper vh, Rect r)
    {
        AddQuad(vh, P(r, 0.47f, 0.36f), P(r, 0.54f, 0.36f), P(r, 0.54f, 0.44f), P(r, 0.47f, 0.44f), new Color(0.015f, 0.015f, 0.018f, 1f));
        AddEllipse(vh, P(r, 0.505f, 0.4f), r.width * 0.028f, r.height * 0.036f, new Color(0.08f, 0.08f, 0.085f, 1f));

        for (int i = 0; i < 9; i++)
        {
            float a = Mathf.Lerp(205f, -25f, i / 8f) * Mathf.Deg2Rad;
            Vector2 centro = P(r, 0.505f, 0.4f);
            Vector2 dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
            AddSegment(vh, centro + dir * r.height * 0.028f, centro + dir * r.height * 0.034f, r.height * 0.0025f, Color.white);
        }

        DibujarAgujaVelocimetro(vh, r);
    }

    private void DibujarAgujaVelocimetro(VertexHelper vh, Rect r)
    {
        float velocidadMaxima = controladorPaisaje != null ? controladorPaisaje.VelocidadMaximaVisual : 375f;
        float velocidadNormalizada = Mathf.Clamp01(velocidadIndicada / velocidadMaxima);
        float angulo = Mathf.Lerp(205f, -25f, velocidadNormalizada) * Mathf.Deg2Rad;
        Vector2 centro = P(r, 0.505f, 0.4f);
        Vector2 direccion = new Vector2(Mathf.Cos(angulo), Mathf.Sin(angulo));

        AddSegment(vh, centro, centro + direccion * r.height * 0.027f, r.height * 0.004f, new Color(0.94f, 0.94f, 0.92f, 1f));
        AddEllipse(vh, centro, r.height * 0.006f, r.height * 0.006f, colorMetal);
    }

    private void BuscarControladorPaisaje()
    {
        if (controladorPaisaje == null)
        {
            controladorPaisaje = FindFirstObjectByType<ControladorPaisaje>();
        }
    }

    private void DibujarDetalles(VertexHelper vh, Rect r)
    {
        DibujarHuesoPescado(vh, r);
    }

    private void DibujarHuesoPescado(VertexHelper vh, Rect r)
    {
        Vector2 cola = P(r, 0.06f, 0.08f);
        Vector2 cabeza = P(r, 0.18f, 0.085f);
        AddSegment(vh, cola, cabeza, r.height * 0.0045f, colorHueso);
        AddPolygon(vh, new[] { P(r, 0.035f, 0.06f), P(r, 0.065f, 0.08f), P(r, 0.035f, 0.105f) }, colorHueso);
        AddPolygon(vh, new[] { P(r, 0.18f, 0.058f), P(r, 0.215f, 0.085f), P(r, 0.18f, 0.112f) }, colorHueso);
        for (int i = 0; i < 6; i++)
        {
            float x = 0.085f + i * 0.016f;
            AddSegment(vh, P(r, x, 0.085f), P(r, x - 0.018f, 0.116f), r.height * 0.003f, colorHueso);
            AddSegment(vh, P(r, x, 0.085f), P(r, x - 0.018f, 0.055f), r.height * 0.003f, colorHueso);
        }
    }

    private Vector2 P(Rect r, float x, float y)
    {
        return new Vector2(Mathf.Lerp(r.xMin, r.xMax, x), Mathf.Lerp(r.yMin, r.yMax, y));
    }

    private Color Sombrear(Color baseColor, float factor)
    {
        Color c = baseColor * factor;
        c.a = baseColor.a;
        return c;
    }

    private void AddSegment(VertexHelper vh, Vector2 a, Vector2 b, float radio, Color32 segmentColor)
    {
        Vector2 delta = b - a;
        if (delta.sqrMagnitude <= 0.001f)
        {
            AddEllipse(vh, a, radio, radio, segmentColor);
            return;
        }

        Vector2 normal = new Vector2(-delta.y, delta.x).normalized * radio;
        AddQuad(vh, a - normal, a + normal, b + normal, b - normal, segmentColor);
        AddEllipse(vh, a, radio, radio, segmentColor);
        AddEllipse(vh, b, radio, radio, segmentColor);
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

    private void AddPolygon(VertexHelper vh, Vector2[] points, Color32 polygonColor)
    {
        if (points == null || points.Length < 3)
        {
            return;
        }

        int start = vh.currentVertCount;
        for (int i = 0; i < points.Length; i++)
        {
            vh.AddVert(points[i], polygonColor, Vector2.zero);
        }

        for (int i = 1; i < points.Length - 1; i++)
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
