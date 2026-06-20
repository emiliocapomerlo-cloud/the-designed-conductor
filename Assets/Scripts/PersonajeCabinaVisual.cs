using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[ExecuteAlways]
public class PersonajeCabinaVisual : MaskableGraphic
{
    private enum RolPersonaje
    {
        Conductor,
        Acompanante
    }

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
    [SerializeField] private RolPersonaje rol = RolPersonaje.Acompanante;
    [SerializeField] private RectTransform volante;

    private const float TamanoLienzo = 256f;
    private const int SegmentosElipse = 28;

    public override Texture mainTexture => s_WhiteTexture;

    protected override void OnEnable()
    {
        base.OnEnable();
        raycastTarget = false;
        BuscarVolanteSiHaceFalta();
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        raycastTarget = false;
        SetVerticesDirty();
        SetMaterialDirty();
    }

    public void ConfigurarComoConductor(RectTransform volanteReferencia)
    {
        rol = RolPersonaje.Conductor;
        volante = volanteReferencia;
        mirandoIzquierda = false;
        raycastTarget = false;
        SetVerticesDirty();
        SetMaterialDirty();
    }

    public void ConfigurarComoAcompanante()
    {
        rol = RolPersonaje.Acompanante;
        volante = null;
        mirandoIzquierda = true;
        raycastTarget = false;
        SetVerticesDirty();
        SetMaterialDirty();
    }

    private void LateUpdate()
    {
        if (rol == RolPersonaje.Conductor)
        {
            BuscarVolanteSiHaceFalta();
        }

        if (Application.isPlaying)
        {
            SetVerticesDirty();
        }
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect rect = GetPixelAdjustedRect();
        float escala = Mathf.Min(rect.width, rect.height) / TamanoLienzo;
        float tiempo = Application.isPlaying ? Time.time : Time.realtimeSinceStartup;
        float direccion = mirandoIzquierda ? -1f : 1f;

        DibujarSombra(vh, rect, escala);

        if (rol == RolPersonaje.Acompanante)
        {
            DibujarAcompananteDeEspalda(vh, rect, escala, tiempo);
            return;
        }

        DibujarConductorDeEspalda(vh, rect, escala, tiempo);
    }

    private void DibujarSombra(VertexHelper vh, Rect rect, float escala)
    {
        AddEllipse(vh, Punto(rect, 128f, 24f), 74f * escala, 14f * escala, new Color(0f, 0f, 0f, 0.16f));
    }

    private void DibujarAcompananteDeEspalda(VertexHelper vh, Rect rect, float escala, float tiempo)
    {
        float respiracion = Mathf.Sin(tiempo * 1.35f + semillaTextura * 0.13f) * 2f;
        float movimientoBrazo = Mathf.Sin(tiempo * 1.9f + semillaTextura * 0.31f) * 2.5f;

        DibujarBrazoLateralAcompanante(vh, rect, escala, movimientoBrazo);

        Color ropaOscura = Sombrear(colorRopa, 0.68f);
        Color ropaMedia = Sombrear(colorRopa, 0.86f);
        AddPolygon(
            vh,
            new[]
            {
                Punto(rect, 67f, 8f),
                Punto(rect, 218f, 8f),
                Punto(rect, 218f, 113f + respiracion),
                Punto(rect, 190f, 137f + respiracion),
                Punto(rect, 139f, 144f + respiracion),
                Punto(rect, 91f, 119f + respiracion)
            },
            colorRopa
        );

        AddPolygon(
            vh,
            new[]
            {
                Punto(rect, 67f, 8f),
                Punto(rect, 112f, 8f),
                Punto(rect, 125f, 137f + respiracion),
                Punto(rect, 91f, 119f + respiracion)
            },
            ropaOscura
        );

        AddPolygon(
            vh,
            new[]
            {
                Punto(rect, 168f, 14f),
                Punto(rect, 218f, 8f),
                Punto(rect, 218f, 113f + respiracion),
                Punto(rect, 190f, 137f + respiracion)
            },
            ropaMedia
        );

        Color costura = Sombrear(colorRopa, 0.54f);
        AddSegment(vh, Punto(rect, 91f, 117f + respiracion), Punto(rect, 126f, 82f), 2.2f * escala, costura);
        AddSegment(vh, Punto(rect, 126f, 82f), Punto(rect, 112f, 14f), 1.8f * escala, costura);

        Color trama = Sombrear(colorRopa, 1.2f);
        trama.a = 0.12f;
        for (int i = 0; i < 7; i++)
        {
            float y = 28f + i * 13f;
            AddSegment(vh, Punto(rect, 119f, y), Punto(rect, 205f, y + 18f), 0.7f * escala, trama);
        }

        DibujarCapuchaAcompanante(vh, rect, escala, respiracion);
        DibujarCabezaAcompanante(vh, rect, escala, respiracion);
    }

    private void DibujarBrazoLateralAcompanante(VertexHelper vh, Rect rect, float escala, float movimiento)
    {
        Vector2 mano = Punto(rect, 73f, 111f + movimiento);
        Vector2 hombro = Punto(rect, 100f, 112f + movimiento * 0.35f);
        Vector2 codo = Punto(rect, 58f, 64f - movimiento * 0.4f);
        Vector2 extremoManga = Punto(rect, 43f, 37f + movimiento * 0.2f);

        AddSegment(vh, mano, hombro, 11f * escala, colorBrazos);
        AddEllipse(vh, mano, 12f * escala, 10f * escala, colorBrazos);
        AddSegment(vh, hombro, codo, 18f * escala, Sombrear(colorRopa, 0.9f));
        AddSegment(vh, codo, extremoManga, 19f * escala, Sombrear(colorRopa, 0.78f));

        Color costuraManga = Sombrear(colorRopa, 0.53f);
        AddSegment(vh, Punto(rect, 91f, 103f + movimiento * 0.35f), Punto(rect, 50f, 53f - movimiento * 0.25f), 1.8f * escala, costuraManga);
    }

    private void DibujarCapuchaAcompanante(VertexHelper vh, Rect rect, float escala, float respiracion)
    {
        Color capuchaOscura = Sombrear(colorRopa, 0.62f);
        Color capuchaMedia = Sombrear(colorRopa, 0.8f);

        AddEllipse(vh, Punto(rect, 157f, 145f + respiracion), 57f * escala, 32f * escala, capuchaOscura);
        AddPolygon(
            vh,
            new[]
            {
                Punto(rect, 105f, 146f + respiracion),
                Punto(rect, 137f, 126f + respiracion),
                Punto(rect, 188f, 132f + respiracion),
                Punto(rect, 205f, 151f + respiracion),
                Punto(rect, 166f, 159f + respiracion),
                Punto(rect, 128f, 157f + respiracion)
            },
            capuchaMedia
        );
        AddSegment(vh, Punto(rect, 115f, 149f + respiracion), Punto(rect, 165f, 135f + respiracion), 2f * escala, Sombrear(colorRopa, 0.5f));
    }

    private void DibujarCabezaAcompanante(VertexHelper vh, Rect rect, float escala, float respiracion)
    {
        Color pielSombra = Sombrear(colorPiel, 0.76f);
        Color pielLuz = Sombrear(colorPiel, 1.05f);

        AddEllipse(vh, Punto(rect, 139f, 168f + respiracion), 14f * escala, 24f * escala, pielSombra);
        AddEllipse(vh, Punto(rect, 112f, 183f + respiracion), 39f * escala, 50f * escala, colorPiel);
        AddEllipse(vh, Punto(rect, 82f, 181f + respiracion), 17f * escala, 31f * escala, pielLuz);

        AddPolygon(
            vh,
            new[]
            {
                Punto(rect, 77f, 184f + respiracion),
                Punto(rect, 65f, 177f + respiracion),
                Punto(rect, 80f, 171f + respiracion)
            },
            pielSombra
        );

        AddEllipse(vh, Punto(rect, 121f, 215f + respiracion), 52f * escala, 33f * escala, colorCabello);
        AddEllipse(vh, Punto(rect, 151f, 190f + respiracion), 29f * escala, 43f * escala, Sombrear(colorCabello, 0.82f));
        AddEllipse(vh, Punto(rect, 91f, 193f + respiracion), 24f * escala, 35f * escala, Sombrear(colorCabello, 0.9f));
        AddPolygon(
            vh,
            new[]
            {
                Punto(rect, 76f, 211f + respiracion),
                Punto(rect, 112f, 190f + respiracion),
                Punto(rect, 98f, 230f + respiracion)
            },
            colorCabello
        );

        Color hebra = Sombrear(colorCabello, 1.4f);
        hebra.a = 0.25f;
        for (int i = 0; i < 7; i++)
        {
            float x = 102f + i * 8f;
            AddSegment(vh, Punto(rect, x, 225f + respiracion), Punto(rect, x - 12f, 201f + respiracion), 0.8f * escala, hebra);
        }
    }

    private void DibujarConductorDeEspalda(VertexHelper vh, Rect rect, float escala, float tiempo)
    {
        float respiracion = Mathf.Sin(tiempo * 1.5f + semillaTextura * 0.17f) * 1.5f;

        DibujarBrazoLejanoConductor(vh, rect, escala);

        Color ropaOscura = Sombrear(colorRopa, 0.66f);
        Color ropaMedia = Sombrear(colorRopa, 0.84f);
        AddPolygon(
            vh,
            new[]
            {
                Punto(rect, 38f, 5f),
                Punto(rect, 188f, 5f),
                Punto(rect, 184f, 108f + respiracion),
                Punto(rect, 159f, 132f + respiracion),
                Punto(rect, 105f, 141f + respiracion),
                Punto(rect, 58f, 119f + respiracion)
            },
            colorRopa
        );

        AddPolygon(
            vh,
            new[]
            {
                Punto(rect, 38f, 5f),
                Punto(rect, 93f, 5f),
                Punto(rect, 112f, 137f + respiracion),
                Punto(rect, 58f, 119f + respiracion)
            },
            ropaOscura
        );

        AddPolygon(
            vh,
            new[]
            {
                Punto(rect, 146f, 12f),
                Punto(rect, 188f, 5f),
                Punto(rect, 184f, 108f + respiracion),
                Punto(rect, 159f, 132f + respiracion)
            },
            ropaMedia
        );

        Color trama = Sombrear(colorRopa, 1.2f);
        trama.a = 0.12f;
        for (int i = 0; i < 7; i++)
        {
            float y = 25f + i * 13f;
            AddSegment(vh, Punto(rect, 66f, y), Punto(rect, 166f, y + 15f), 0.7f * escala, trama);
        }

        DibujarCapuchaConductor(vh, rect, escala, respiracion);
        DibujarCabezaConductor(vh, rect, escala, respiracion);
        DibujarBrazosConductorDeEspalda(vh, rect, escala);
    }

    private void DibujarBrazoLejanoConductor(VertexHelper vh, Rect rect, float escala)
    {
        Vector2 hombro = Punto(rect, 91f, 118f);
        Vector2 mano = ObtenerPuntoVolante(rect, 150f, Punto(rect, 174f, 117f));
        Vector2 codo = CalcularCodo(hombro, mano, 1f, escala, 9f, 24f);

        AddSegment(vh, hombro, codo, 15f * escala, Sombrear(colorRopa, 0.76f));
        AddSegment(vh, codo, mano, 11f * escala, Sombrear(colorBrazos, 0.82f));
        AddEllipse(vh, mano, 13f * escala, 10f * escala, Sombrear(colorBrazos, 0.82f));
    }

    private void DibujarCapuchaConductor(VertexHelper vh, Rect rect, float escala, float respiracion)
    {
        Color capuchaOscura = Sombrear(colorRopa, 0.58f);
        Color capuchaMedia = Sombrear(colorRopa, 0.77f);

        AddEllipse(vh, Punto(rect, 111f, 144f + respiracion), 58f * escala, 31f * escala, capuchaOscura);
        AddPolygon(
            vh,
            new[]
            {
                Punto(rect, 58f, 146f + respiracion),
                Punto(rect, 91f, 126f + respiracion),
                Punto(rect, 143f, 132f + respiracion),
                Punto(rect, 166f, 151f + respiracion),
                Punto(rect, 125f, 160f + respiracion),
                Punto(rect, 84f, 158f + respiracion)
            },
            capuchaMedia
        );

        AddSegment(vh, Punto(rect, 68f, 149f + respiracion), Punto(rect, 121f, 136f + respiracion), 2f * escala, Sombrear(colorRopa, 0.48f));
    }

    private void DibujarCabezaConductor(VertexHelper vh, Rect rect, float escala, float respiracion)
    {
        Color pielSombra = Sombrear(colorPiel, 0.74f);
        Color pielLuz = Sombrear(colorPiel, 1.05f);

        AddEllipse(vh, Punto(rect, 112f, 166f + respiracion), 15f * escala, 25f * escala, pielSombra);
        AddEllipse(vh, Punto(rect, 145f, 183f + respiracion), 40f * escala, 51f * escala, colorPiel);
        AddEllipse(vh, Punto(rect, 176f, 181f + respiracion), 17f * escala, 31f * escala, pielLuz);

        AddPolygon(
            vh,
            new[]
            {
                Punto(rect, 177f, 184f + respiracion),
                Punto(rect, 191f, 177f + respiracion),
                Punto(rect, 176f, 171f + respiracion)
            },
            pielSombra
        );

        AddEllipse(vh, Punto(rect, 134f, 216f + respiracion), 53f * escala, 33f * escala, colorCabello);
        AddEllipse(vh, Punto(rect, 104f, 191f + respiracion), 29f * escala, 43f * escala, Sombrear(colorCabello, 0.82f));
        AddEllipse(vh, Punto(rect, 165f, 193f + respiracion), 24f * escala, 35f * escala, Sombrear(colorCabello, 0.9f));
        AddPolygon(
            vh,
            new[]
            {
                Punto(rect, 179f, 211f + respiracion),
                Punto(rect, 143f, 190f + respiracion),
                Punto(rect, 157f, 231f + respiracion)
            },
            colorCabello
        );

        AddSegment(vh, Punto(rect, 169f, 170f + respiracion), Punto(rect, 184f, 170f + respiracion), 1.6f * escala, new Color(0.035f, 0.026f, 0.018f, 1f));
        AddSegment(vh, Punto(rect, 166f, 140f + respiracion), Punto(rect, 183f, 137f + respiracion), 2.1f * escala, new Color(0.26f, 0.07f, 0.055f, 1f));
    }

    private void DibujarBrazosConductorDeEspalda(VertexHelper vh, Rect rect, float escala)
    {
        Vector2 hombro = Punto(rect, 158f, 119f);
        Vector2 mano = ObtenerPuntoVolante(rect, 24f, Punto(rect, 216f, 124f));
        Vector2 codo = Vector2.Lerp(hombro, mano, 0.42f);
        codo += new Vector2(-10f * escala, -20f * escala);

        DibujarAntebrazoConductor(vh, codo, mano, escala);
        DibujarBrazoSuperiorConductor(vh, hombro, codo, escala);
        DibujarManoConductor(vh, mano, escala);
    }

    private void DibujarBrazoSuperiorConductor(VertexHelper vh, Vector2 hombro, Vector2 codo, float escala)
    {
        Color colorManga = Sombrear(colorRopa, 0.94f);
        AddSegment(vh, hombro, codo, 17f * escala, colorManga);
        AddEllipse(vh, codo, 19f * escala, 17f * escala, colorManga);
    }

    private void DibujarAntebrazoConductor(VertexHelper vh, Vector2 codo, Vector2 mano, float escala)
    {
        AddSegment(vh, codo, mano, 12f * escala, colorBrazos);
    }

    private void DibujarManoConductor(VertexHelper vh, Vector2 mano, float escala)
    {
        AddEllipse(vh, mano, 15f * escala, 12f * escala, colorBrazos);
    }

    private void DibujarTorso(VertexHelper vh, Rect rect, float direccion)
    {
        float inclinacionConductor = rol == RolPersonaje.Conductor ? 10f : 0f;
        Vector2 hombroEspalda = Punto(rect, 128f - direccion * 70f, 116f);
        Vector2 hombroFrente = Punto(rect, 128f + direccion * 38f, 128f + inclinacionConductor);
        Vector2 cinturaFrente = Punto(rect, 128f + direccion * 58f, 4f);
        Vector2 cinturaEspalda = Punto(rect, 128f - direccion * 84f, 2f);

        AddPolygon(vh, new[] { cinturaEspalda, cinturaFrente, hombroFrente, hombroEspalda }, colorRopa);

        Color sombraLateral = Sombrear(colorRopa, 0.66f);
        AddPolygon(
            vh,
            new[]
            {
                Punto(rect, 128f + direccion * 8f, 14f),
                cinturaFrente,
                hombroFrente,
                Punto(rect, 128f + direccion * 3f, 112f)
            },
            sombraLateral
        );

        Color brilloTela = colorDetalleRopa;
        brilloTela.a = 0.92f;
        AddSegment(vh, Punto(rect, 128f - direccion * 18f, 108f), Punto(rect, 128f + direccion * 2f, 18f), 3f * Escala(rect), brilloTela);
        AddSegment(vh, Punto(rect, 128f + direccion * 24f, 112f), Punto(rect, 128f + direccion * 34f, 18f), 2.3f * Escala(rect), Sombrear(brilloTela, 0.82f));

        Color trama = Sombrear(colorRopa, 1.22f);
        trama.a = 0.18f;
        for (int i = 0; i < 8; i++)
        {
            float y = 26f + i * 11f;
            AddSegment(vh, Punto(rect, 68f, y), Punto(rect, 188f, y + 10f), 0.7f * Escala(rect), trama);
        }
    }

    private void DibujarCuello(VertexHelper vh, Rect rect, float direccion, float tiempo)
    {
        float respiracion = Mathf.Sin(tiempo * 1.7f + semillaTextura) * 1.5f;
        AddEllipse(
            vh,
            Punto(rect, 128f + direccion * 13f, 118f + respiracion),
            17f * Escala(rect),
            23f * Escala(rect),
            Sombrear(colorPiel, 0.88f)
        );
    }

    private void DibujarCabeza(VertexHelper vh, Rect rect, float direccion, float tiempo)
    {
        float idle = rol == RolPersonaje.Acompanante ? Mathf.Sin(tiempo * 1.35f + semillaTextura * 0.13f) * 2f : 0f;
        float avance = rol == RolPersonaje.Conductor ? 32f : 24f;
        Vector2 centroCabeza = Punto(rect, 128f + direccion * avance, 166f + idle);

        AddEllipse(vh, Punto(rect, 128f - direccion * 24f, 162f + idle), 13f * Escala(rect), 18f * Escala(rect), Sombrear(colorPiel, 0.76f));
        AddEllipse(vh, centroCabeza, 39f * Escala(rect), 53f * Escala(rect), colorPiel);
        AddEllipse(vh, Punto(rect, 128f + direccion * 50f, 168f + idle), 17f * Escala(rect), 33f * Escala(rect), Sombrear(colorPiel, 1.08f));

        Color sombraNuca = Sombrear(colorPiel, 0.74f);
        sombraNuca.a = 0.55f;
        AddEllipse(vh, Punto(rect, 128f - direccion * 12f, 154f + idle), 18f * Escala(rect), 32f * Escala(rect), sombraNuca);
    }

    private void DibujarCabello(VertexHelper vh, Rect rect, float direccion, float tiempo)
    {
        float idle = rol == RolPersonaje.Acompanante ? Mathf.Sin(tiempo * 1.35f + semillaTextura * 0.13f) * 2f : 0f;
        float avance = rol == RolPersonaje.Conductor ? 14f : 6f;

        AddEllipse(vh, Punto(rect, 128f + direccion * avance, 201f + idle), 50f * Escala(rect), 31f * Escala(rect), colorCabello);
        AddEllipse(vh, Punto(rect, 128f - direccion * 18f, 179f + idle), 29f * Escala(rect), 41f * Escala(rect), Sombrear(colorCabello, 0.86f));
        AddEllipse(vh, Punto(rect, 128f + direccion * 43f, 185f + idle), 19f * Escala(rect), 32f * Escala(rect), Sombrear(colorCabello, 0.74f));

        AddPolygon(
            vh,
            new[]
            {
                Punto(rect, 128f - direccion * 46f, 196f + idle),
                Punto(rect, 128f - direccion * 2f, 174f + idle),
                Punto(rect, 128f - direccion * 22f, 224f + idle)
            },
            colorCabello
        );

        Color hebra = Sombrear(colorCabello, 1.45f);
        hebra.a = 0.35f;
        for (int i = 0; i < 8; i++)
        {
            float x = 98f + i * 7f;
            AddSegment(vh, Punto(rect, x, 211f + idle), Punto(rect, x + direccion * 17f, 186f + idle), 0.9f * Escala(rect), hebra);
        }
    }

    private void DibujarPerfil(VertexHelper vh, Rect rect, float escala, float direccion)
    {
        Color detalle = Sombrear(colorPiel, 0.78f);
        float frenteX = rol == RolPersonaje.Conductor ? 72f : 64f;
        Vector2 frente = Punto(rect, 128f + direccion * frenteX, 163f);

        AddPolygon(
            vh,
            new[]
            {
                frente,
                Punto(rect, 128f + direccion * (frenteX + 13f), 157f),
                Punto(rect, 128f + direccion * (frenteX - 2f), 151f)
            },
            detalle
        );

        AddSegment(vh, Punto(rect, 128f + direccion * 45f, 170f), Punto(rect, 128f + direccion * 59f, 171f), 1.7f * escala, new Color(0.035f, 0.026f, 0.018f, 1f));
        AddSegment(vh, Punto(rect, 128f + direccion * 43f, 139f), Punto(rect, 128f + direccion * 59f, 137f), 2.2f * escala, new Color(0.26f, 0.07f, 0.055f, 1f));
    }

    private void DibujarBrazosFrontales(VertexHelper vh, Rect rect, float escala, float tiempo, float direccion)
    {
        if (rol == RolPersonaje.Conductor)
        {
            DibujarBrazosConductor(vh, rect, escala, direccion);
            return;
        }

        DibujarBrazosAcompanante(vh, rect, escala, tiempo, direccion);
    }

    private void DibujarBrazosConductor(VertexHelper vh, Rect rect, float escala, float direccion)
    {
        Vector2 hombroCercano = Punto(rect, 128f + direccion * 34f, 116f);
        Vector2 manoCercana = ObtenerPuntoVolante(rect, 18f, Punto(rect, 128f + direccion * 122f, 130f));
        Vector2 codoCercano = CalcularCodo(hombroCercano, manoCercana, direccion, escala, 20f, 22f);

        AddSegment(vh, hombroCercano, codoCercano, 15f * escala, Sombrear(colorRopa, 0.9f));
        AddSegment(vh, codoCercano, manoCercana, 12f * escala, colorBrazos);
        AddEllipse(vh, manoCercana, 16f * escala, 12f * escala, colorBrazos);

        Color nudillo = Sombrear(colorBrazos, 1.18f);
        nudillo.a = 0.38f;
        AddSegment(vh, manoCercana + new Vector2(-4f * escala * direccion, 2f * escala), manoCercana + new Vector2(5f * escala * direccion, 3f * escala), 1f * escala, nudillo);
    }

    private void DibujarBrazosAcompanante(VertexHelper vh, Rect rect, float escala, float tiempo, float direccion)
    {
        float idle = Mathf.Sin(tiempo * 2f + semillaTextura * 0.31f) * 5f;
        float sacudida = Mathf.Sin(tiempo * 6.8f + semillaTextura) * 1.6f;

        Vector2 hombroLejano = Punto(rect, 128f - direccion * 45f, 106f + idle * 0.2f);
        Vector2 codoLejano = Punto(rect, 128f - direccion * 72f, 63f - idle * 0.45f + sacudida);
        Vector2 manoLejana = Punto(rect, 128f - direccion * 44f, 39f + idle * 0.2f);

        Vector2 hombroCercano = Punto(rect, 128f + direccion * 38f, 110f - idle * 0.15f);
        Vector2 codoCercano = Punto(rect, 128f + direccion * 70f, 66f + idle * 0.35f - sacudida);
        Vector2 manoCercana = Punto(rect, 128f + direccion * 40f, 42f - idle * 0.15f);

        AddSegment(vh, hombroLejano, codoLejano, 11f * escala, Sombrear(colorRopa, 0.75f));
        AddSegment(vh, codoLejano, manoLejana, 8f * escala, Sombrear(colorBrazos, 0.86f));
        AddEllipse(vh, manoLejana, 11f * escala, 8f * escala, Sombrear(colorBrazos, 0.86f));

        AddSegment(vh, hombroCercano, codoCercano, 12f * escala, Sombrear(colorRopa, 0.95f));
        AddSegment(vh, codoCercano, manoCercana, 9f * escala, colorBrazos);
        AddEllipse(vh, manoCercana, 13f * escala, 9f * escala, colorBrazos);
    }

    private Vector2 ObtenerPuntoVolante(Rect rect, float anguloBase, Vector2 fallback)
    {
        if (volante == null)
        {
            return fallback;
        }

        float radio = Mathf.Min(volante.rect.width, volante.rect.height) * 0.38f;
        float angulo = anguloBase * Mathf.Deg2Rad;
        Vector3 puntoLocalVolante = new Vector3(Mathf.Cos(angulo) * radio, Mathf.Sin(angulo) * radio, 0f);
        Vector3 puntoMundo = volante.TransformPoint(puntoLocalVolante);
        return rectTransform.InverseTransformPoint(puntoMundo);
    }

    private Vector2 CalcularCodo(Vector2 hombro, Vector2 mano, float direccion, float escala, float apertura, float caida)
    {
        Vector2 medio = Vector2.Lerp(hombro, mano, 0.48f);
        Vector2 brazo = mano - hombro;
        Vector2 normal = brazo.sqrMagnitude > 0.01f ? new Vector2(-brazo.y, brazo.x).normalized : Vector2.up;
        if (Vector2.Dot(normal, new Vector2(direccion, -0.35f)) < 0f)
        {
            normal = -normal;
        }

        return medio + normal * apertura * escala + Vector2.down * caida * escala;
    }

    private void BuscarVolanteSiHaceFalta()
    {
        if (volante != null || rol != RolPersonaje.Conductor)
        {
            return;
        }

        GameObject objetoVolante = GameObject.Find("Volante");
        if (objetoVolante != null)
        {
            volante = objetoVolante.GetComponent<RectTransform>();
        }
    }

    private Vector2 Punto(Rect rect, float x, float y)
    {
        return new Vector2(
            Mathf.Lerp(rect.xMin, rect.xMax, x / TamanoLienzo),
            Mathf.Lerp(rect.yMin, rect.yMax, y / TamanoLienzo)
        );
    }

    private float Escala(Rect rect)
    {
        return Mathf.Min(rect.width, rect.height) / TamanoLienzo;
    }

    private Color Sombrear(Color colorBase, float multiplicador)
    {
        Color resultado = colorBase * multiplicador;
        resultado.a = colorBase.a;
        return resultado;
    }

    private void AddSegment(VertexHelper vh, Vector2 a, Vector2 b, float radio, Color32 colorSegmento)
    {
        Vector2 delta = b - a;
        if (delta.sqrMagnitude <= 0.001f)
        {
            AddEllipse(vh, a, radio, radio, colorSegmento);
            return;
        }

        Vector2 normal = new Vector2(-delta.y, delta.x).normalized * radio;
        AddQuad(vh, a - normal, a + normal, b + normal, b - normal, colorSegmento);
        AddEllipse(vh, a, radio, radio, colorSegmento);
        AddEllipse(vh, b, radio, radio, colorSegmento);
    }

    private void AddEllipse(VertexHelper vh, Vector2 centro, float radioX, float radioY, Color32 colorElipse)
    {
        int start = vh.currentVertCount;
        vh.AddVert(centro, colorElipse, Vector2.zero);

        for (int i = 0; i <= SegmentosElipse; i++)
        {
            float angulo = Mathf.PI * 2f * i / SegmentosElipse;
            Vector2 punto = centro + new Vector2(Mathf.Cos(angulo) * radioX, Mathf.Sin(angulo) * radioY);
            vh.AddVert(punto, colorElipse, Vector2.zero);
        }

        for (int i = 1; i <= SegmentosElipse; i++)
        {
            vh.AddTriangle(start, start + i, start + i + 1);
        }
    }

    private void AddPolygon(VertexHelper vh, Vector2[] puntos, Color32 colorPoligono)
    {
        if (puntos == null || puntos.Length < 3)
        {
            return;
        }

        int start = vh.currentVertCount;
        for (int i = 0; i < puntos.Length; i++)
        {
            vh.AddVert(puntos[i], colorPoligono, Vector2.zero);
        }

        for (int i = 1; i < puntos.Length - 1; i++)
        {
            vh.AddTriangle(start, start + i, start + i + 1);
        }
    }

    private void AddQuad(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color32 colorCuad)
    {
        int start = vh.currentVertCount;
        vh.AddVert(a, colorCuad, Vector2.zero);
        vh.AddVert(b, colorCuad, Vector2.zero);
        vh.AddVert(c, colorCuad, Vector2.zero);
        vh.AddVert(d, colorCuad, Vector2.zero);
        vh.AddTriangle(start, start + 1, start + 2);
        vh.AddTriangle(start, start + 2, start + 3);
    }
}
