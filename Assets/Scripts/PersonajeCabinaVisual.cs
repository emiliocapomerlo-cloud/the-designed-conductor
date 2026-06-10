using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PersonajeCabinaVisual : MaskableGraphic
{
    [FormerlySerializedAs("colorCabeza")]
    [SerializeField] private Color colorPiel = new Color(0.58f, 0.28f, 0.12f, 1f);

    [FormerlySerializedAs("colorTorso")]
    [SerializeField] private Color colorRopa = new Color(0.12f, 0.2f, 0.48f, 1f);

    [FormerlySerializedAs("colorBrazo")]
    [SerializeField] private Color colorBrazos = new Color(0.45f, 0.2f, 0.08f, 1f);

    [SerializeField] private Color colorCabello = new Color(0.06f, 0.035f, 0.02f, 1f);
    [SerializeField] private Color colorDetalleRopa = new Color(0.93f, 0.88f, 0.72f, 1f);
    [SerializeField] private bool mirandoIzquierda;
    [SerializeField] private int semillaTextura;

    private const int AnchoTextura = 256;
    private const int AltoTextura = 256;

    private Texture2D texturaPersonaje;
    private bool texturaSucia = true;

    public override Texture mainTexture
    {
        get
        {
            AsegurarTextura();
            return texturaPersonaje != null ? texturaPersonaje : s_WhiteTexture;
        }
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        texturaSucia = true;
        SetVerticesDirty();
        SetMaterialDirty();
    }

    protected override void OnDestroy()
    {
        if (texturaPersonaje != null)
        {
            if (Application.isPlaying)
            {
                Destroy(texturaPersonaje);
            }
            else
            {
                DestroyImmediate(texturaPersonaje);
            }
        }

        base.OnDestroy();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        AsegurarTextura();
        vh.Clear();

        Rect rect = GetPixelAdjustedRect();
        Color32 colorVertice = color;
        int start = vh.currentVertCount;

        vh.AddVert(new Vector3(rect.xMin, rect.yMin), colorVertice, new Vector2(0f, 0f));
        vh.AddVert(new Vector3(rect.xMin, rect.yMax), colorVertice, new Vector2(0f, 1f));
        vh.AddVert(new Vector3(rect.xMax, rect.yMax), colorVertice, new Vector2(1f, 1f));
        vh.AddVert(new Vector3(rect.xMax, rect.yMin), colorVertice, new Vector2(1f, 0f));
        vh.AddTriangle(start, start + 1, start + 2);
        vh.AddTriangle(start, start + 2, start + 3);
    }

    private void AsegurarTextura()
    {
        if (texturaPersonaje == null)
        {
            texturaPersonaje = new Texture2D(AnchoTextura, AltoTextura, TextureFormat.RGBA32, false);
            texturaPersonaje.name = "TexturaPersonajeCabina";
            texturaPersonaje.filterMode = FilterMode.Bilinear;
            texturaPersonaje.wrapMode = TextureWrapMode.Clamp;
            texturaSucia = true;
        }

        if (!texturaSucia)
        {
            return;
        }

        GenerarTextura();
        texturaSucia = false;
    }

    private void GenerarTextura()
    {
        Color transparente = new Color(0f, 0f, 0f, 0f);
        for (int y = 0; y < AltoTextura; y++)
        {
            for (int x = 0; x < AnchoTextura; x++)
            {
                texturaPersonaje.SetPixel(x, y, transparente);
            }
        }

        DibujarSombra();
        DibujarTorso();
        DibujarCuello();
        DibujarBrazos();
        DibujarCabeza();
        DibujarCabello();
        DibujarRostro();
        DibujarGranoTextura();
        texturaPersonaje.Apply();
    }

    private void DibujarSombra()
    {
        DibujarElipse(new Vector2(128f, 24f), 72f, 16f, new Color(0f, 0f, 0f, 0.16f));
    }

    private void DibujarTorso()
    {
        DibujarPoligono(
            new Vector2[]
            {
                new Vector2(54f, 12f),
                new Vector2(202f, 12f),
                new Vector2(178f, 118f),
                new Vector2(78f, 118f)
            },
            colorRopa
        );

        Color sombra = colorRopa * 0.68f;
        sombra.a = 1f;
        DibujarPoligono(
            new Vector2[]
            {
                new Vector2(128f, 12f),
                new Vector2(202f, 12f),
                new Vector2(178f, 118f),
                new Vector2(134f, 118f)
            },
            sombra
        );

        DibujarPoligono(new Vector2[] { new Vector2(82f, 118f), new Vector2(124f, 84f), new Vector2(116f, 126f) }, colorDetalleRopa);
        DibujarPoligono(new Vector2[] { new Vector2(174f, 118f), new Vector2(132f, 84f), new Vector2(140f, 126f) }, colorDetalleRopa);
        DibujarRectangulo(124, 18, 132, 94, colorDetalleRopa * 0.85f);
        DibujarTela(56, 15, 200, 116);
    }

    private void DibujarCuello()
    {
        DibujarRectangulo(108, 102, 148, 144, colorPiel * 0.9f);
    }

    private void DibujarBrazos()
    {
        DibujarSegmento(new Vector2(72f, 102f), new Vector2(42f, 50f), 13f, colorRopa * 0.9f);
        DibujarSegmento(new Vector2(184f, 102f), new Vector2(214f, 50f), 13f, colorRopa * 0.9f);
        DibujarSegmento(new Vector2(42f, 50f), new Vector2(72f, 35f), 10f, colorBrazos * 0.95f);
        DibujarSegmento(new Vector2(214f, 50f), new Vector2(184f, 35f), 10f, colorBrazos * 0.95f);
        DibujarElipse(new Vector2(73f, 34f), 14f, 10f, colorBrazos);
        DibujarElipse(new Vector2(183f, 34f), 14f, 10f, colorBrazos);
    }

    private void DibujarCabeza()
    {
        DibujarElipse(new Vector2(84f, 158f), 12f, 18f, colorPiel * 0.85f);
        DibujarElipse(new Vector2(172f, 158f), 12f, 18f, colorPiel * 0.85f);
        DibujarElipse(new Vector2(128f, 160f), 47f, 55f, colorPiel);
        DibujarElipse(new Vector2(143f, 168f), 25f, 36f, new Color(1f, 1f, 1f, 0.08f));
    }

    private void DibujarCabello()
    {
        DibujarElipse(new Vector2(128f, 204f), 48f, 28f, colorCabello);
        DibujarElipse(new Vector2(104f, 185f), 22f, 35f, colorCabello * 0.85f);
        DibujarElipse(new Vector2(152f, 185f), 22f, 35f, colorCabello * 0.78f);

        DibujarPoligono(new Vector2[] { new Vector2(86f, 194f), new Vector2(126f, 176f), new Vector2(112f, 224f) }, colorCabello);
        DibujarPoligono(new Vector2[] { new Vector2(170f, 194f), new Vector2(130f, 176f), new Vector2(145f, 224f) }, colorCabello * 0.9f);
        DibujarHebras();
    }

    private void DibujarRostro()
    {
        float mirada = mirandoIzquierda ? -1f : 1f;
        Color ojo = new Color(0.035f, 0.026f, 0.018f, 1f);
        Color brillo = new Color(1f, 1f, 1f, 0.75f);
        Color boca = new Color(0.28f, 0.08f, 0.065f, 1f);

        DibujarElipse(new Vector2(111f + mirada * 4f, 165f), 6f, 4f, ojo);
        DibujarElipse(new Vector2(145f + mirada * 4f, 165f), 6f, 4f, ojo);
        DibujarElipse(new Vector2(113f + mirada * 4f, 167f), 2f, 1.5f, brillo);
        DibujarElipse(new Vector2(147f + mirada * 4f, 167f), 2f, 1.5f, brillo);
        DibujarSegmento(new Vector2(101f, 177f), new Vector2(120f, 180f), 2f, colorCabello);
        DibujarSegmento(new Vector2(136f, 180f), new Vector2(155f, 177f), 2f, colorCabello);
        DibujarPoligono(
            new Vector2[]
            {
                new Vector2(128f + mirada * 2f, 158f),
                new Vector2(139f + mirada * 2f, 142f),
                new Vector2(126f + mirada * 2f, 144f)
            },
            colorPiel * 0.72f
        );
        DibujarSegmento(new Vector2(110f, 130f), new Vector2(145f, 128f), 3f, boca);
    }

    private void DibujarTela(int minX, int minY, int maxX, int maxY)
    {
        Color hilo = colorRopa * 1.18f;
        hilo.a = 0.16f;

        for (int y = minY; y <= maxY; y += 8)
        {
            DibujarSegmento(new Vector2(minX, y), new Vector2(maxX, y + 8), 1f, hilo);
        }
    }

    private void DibujarHebras()
    {
        Color hebra = colorCabello * 1.45f;
        hebra.a = 0.35f;

        for (int i = 0; i < 12; i++)
        {
            float x = 92f + i * 6f;
            DibujarSegmento(new Vector2(x, 209f), new Vector2(x + 12f, 185f), 1f, hebra);
        }
    }

    private void DibujarGranoTextura()
    {
        int semilla = semillaTextura + (mirandoIzquierda ? 91 : 17);

        for (int y = 0; y < AltoTextura; y++)
        {
            for (int x = 0; x < AnchoTextura; x++)
            {
                Color pixel = texturaPersonaje.GetPixel(x, y);
                if (pixel.a <= 0f)
                {
                    continue;
                }

                float ruido = Mathf.PerlinNoise((x + semilla) * 0.19f, (y - semilla) * 0.19f) - 0.5f;
                float luz = 1f + ruido * 0.08f;
                pixel.r *= luz;
                pixel.g *= luz;
                pixel.b *= luz;
                texturaPersonaje.SetPixel(x, y, pixel);
            }
        }
    }

    private void DibujarRectangulo(int minX, int minY, int maxX, int maxY, Color colorRect)
    {
        for (int y = Mathf.Max(0, minY); y <= Mathf.Min(AltoTextura - 1, maxY); y++)
        {
            for (int x = Mathf.Max(0, minX); x <= Mathf.Min(AnchoTextura - 1, maxX); x++)
            {
                PintarPixel(x, y, colorRect);
            }
        }
    }

    private void DibujarElipse(Vector2 centro, float radioX, float radioY, Color colorElipse)
    {
        int minX = Mathf.Max(0, Mathf.FloorToInt(centro.x - radioX));
        int maxX = Mathf.Min(AnchoTextura - 1, Mathf.CeilToInt(centro.x + radioX));
        int minY = Mathf.Max(0, Mathf.FloorToInt(centro.y - radioY));
        int maxY = Mathf.Min(AltoTextura - 1, Mathf.CeilToInt(centro.y + radioY));

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                float dx = (x - centro.x) / radioX;
                float dy = (y - centro.y) / radioY;
                float distancia = dx * dx + dy * dy;
                if (distancia <= 1f)
                {
                    PintarPixel(x, y, colorElipse);
                }
            }
        }
    }

    private void DibujarSegmento(Vector2 a, Vector2 b, float grosor, Color colorSegmento)
    {
        int minX = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.x, b.x) - grosor));
        int maxX = Mathf.Min(AnchoTextura - 1, Mathf.CeilToInt(Mathf.Max(a.x, b.x) + grosor));
        int minY = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.y, b.y) - grosor));
        int maxY = Mathf.Min(AltoTextura - 1, Mathf.CeilToInt(Mathf.Max(a.y, b.y) + grosor));
        Vector2 ab = b - a;
        float largoCuadrado = ab.sqrMagnitude;

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                Vector2 punto = new Vector2(x, y);
                float t = largoCuadrado <= 0f ? 0f : Mathf.Clamp01(Vector2.Dot(punto - a, ab) / largoCuadrado);
                Vector2 cercano = a + ab * t;
                if ((punto - cercano).sqrMagnitude <= grosor * grosor)
                {
                    PintarPixel(x, y, colorSegmento);
                }
            }
        }
    }

    private void DibujarPoligono(Vector2[] puntos, Color colorPoligono)
    {
        if (puntos == null || puntos.Length < 3)
        {
            return;
        }

        float minXf = puntos[0].x;
        float maxXf = puntos[0].x;
        float minYf = puntos[0].y;
        float maxYf = puntos[0].y;

        for (int i = 1; i < puntos.Length; i++)
        {
            minXf = Mathf.Min(minXf, puntos[i].x);
            maxXf = Mathf.Max(maxXf, puntos[i].x);
            minYf = Mathf.Min(minYf, puntos[i].y);
            maxYf = Mathf.Max(maxYf, puntos[i].y);
        }

        int minX = Mathf.Max(0, Mathf.FloorToInt(minXf));
        int maxX = Mathf.Min(AnchoTextura - 1, Mathf.CeilToInt(maxXf));
        int minY = Mathf.Max(0, Mathf.FloorToInt(minYf));
        int maxY = Mathf.Min(AltoTextura - 1, Mathf.CeilToInt(maxYf));

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                if (PuntoEnPoligono(new Vector2(x, y), puntos))
                {
                    PintarPixel(x, y, colorPoligono);
                }
            }
        }
    }

    private bool PuntoEnPoligono(Vector2 punto, Vector2[] puntos)
    {
        bool adentro = false;
        int j = puntos.Length - 1;

        for (int i = 0; i < puntos.Length; i++)
        {
            if ((puntos[i].y > punto.y) != (puntos[j].y > punto.y) &&
                punto.x < (puntos[j].x - puntos[i].x) * (punto.y - puntos[i].y) / (puntos[j].y - puntos[i].y) + puntos[i].x)
            {
                adentro = !adentro;
            }

            j = i;
        }

        return adentro;
    }

    private void PintarPixel(int x, int y, Color colorNuevo)
    {
        Color actual = texturaPersonaje.GetPixel(x, y);
        Color mezclado = Color.Lerp(actual, colorNuevo, colorNuevo.a);
        mezclado.a = Mathf.Clamp01(actual.a + colorNuevo.a * (1f - actual.a));
        texturaPersonaje.SetPixel(x, y, mezclado);
    }
}
