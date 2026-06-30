using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EscenaInicialMejora : MonoBehaviour
{
    [Header("Activacion")]
    [Tooltip("Se activa solo en escenas que no sean la de manejo.")]
    [SerializeField] private bool activarEnEscenasSinManejo = true;

    [Header("Visuales")]
    [SerializeField] private int cantidadCompaneros = 4;
    [SerializeField] private float amplitudMovimiento = 18f;
    [SerializeField] private float frecuenciaMovimiento = 2.2f;
    [SerializeField] private float amplitudBote = 8f;
    [SerializeField] private float frecuenciaBote = 4.6f;

    private Canvas canvas;
    private RectTransform panelIntro;
    private RectTransform botonRect;
    private RectTransform autoIntro;
    private RectTransform[] companeros;
    private RectTransform[] luces;
    private Image[] imagenesCompaneros;
    private Image[] imagenesLuces;
    private Vector2[] posicionesBaseCompaneros;
    private float[] fases;
    private bool introActiva = true;
    private Button botonAceptar;
    private Text textoTitulo;
    private Text textoSubtitulo;
    private Text textoObjetivo;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CrearSiHaceFalta()
    {
        if (FindAnyObjectByType<EscenaInicialMejora>() != null)
        {
            return;
        }

        if (FindAnyObjectByType<ControladorPaisaje>() != null)
        {
            return;
        }

        GameObject controlador = new GameObject("EscenaInicialMejora");
        controlador.AddComponent<EscenaInicialMejora>();
    }

    private void Start()
    {
        if (activarEnEscenasSinManejo && FindAnyObjectByType<ControladorPaisaje>() != null)
        {
            Destroy(gameObject);
            return;
        }

        CrearInterfaz();
    }

    private void Update()
    {
        if (!introActiva)
        {
            return;
        }

        float tiempo = Time.time;
        AnimarGrupo(tiempo);
        AnimarLuces(tiempo);

        if (autoIntro != null)
        {
            autoIntro.anchoredPosition = new Vector2(0f, -230f + Mathf.Sin(tiempo * 1.8f) * 2.5f);
        }

        if (botonRect != null)
        {
            float escala = 1f + Mathf.Sin(tiempo * 2.4f) * 0.025f;
            botonRect.localScale = new Vector3(escala, escala, 1f);
        }

        if (textoTitulo != null)
        {
            float brillo = 0.88f + Mathf.Sin(tiempo * 1.3f) * 0.08f;
            textoTitulo.color = new Color(1f, brillo, 0.58f, 1f);
        }

        if (textoObjetivo != null)
        {
            textoObjetivo.color = new Color(0.82f, 0.94f, 1f, 0.78f + Mathf.Sin(tiempo * 1.7f) * 0.12f);
        }
    }

    private void AnimarGrupo(float tiempo)
    {
        if (companeros == null || imagenesCompaneros == null)
        {
            return;
        }

        for (int i = 0; i < companeros.Length; i++)
        {
            RectTransform companero = companeros[i];
            if (companero == null)
            {
                continue;
            }

            float fase = fases[i];
            Vector2 basePos = posicionesBaseCompaneros[i];
            float x = Mathf.Sin(tiempo * frecuenciaMovimiento + fase) * amplitudMovimiento;
            float y = Mathf.Cos(tiempo * frecuenciaBote + fase * 0.7f) * amplitudBote;
            companero.anchoredPosition = basePos + new Vector2(x, y);
            companero.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(tiempo * 2.1f + fase) * 8f);

            if (imagenesCompaneros[i] != null)
            {
                Color color = imagenesCompaneros[i].color;
                color.a = 0.9f + Mathf.Sin(tiempo * 2.5f + fase) * 0.08f;
                imagenesCompaneros[i].color = color;
            }
        }
    }

    private void AnimarLuces(float tiempo)
    {
        if (luces == null || imagenesLuces == null)
        {
            return;
        }

        for (int i = 0; i < luces.Length; i++)
        {
            if (luces[i] == null || imagenesLuces[i] == null)
            {
                continue;
            }

            float pulso = 0.5f + Mathf.Sin(tiempo * (1.4f + i * 0.23f) + i) * 0.5f;
            Color color = imagenesLuces[i].color;
            color.a = 0.18f + pulso * 0.42f;
            imagenesLuces[i].color = color;
            float escala = 0.88f + pulso * 0.18f;
            luces[i].localScale = new Vector3(escala, escala, 1f);
        }
    }

    private void CrearInterfaz()
    {
        canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObjeto = new GameObject("CanvasEscenaInicial", typeof(RectTransform));
            canvas = canvasObjeto.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObjeto.AddComponent<CanvasScaler>();
            canvasObjeto.AddComponent<GraphicRaycaster>();
        }

        Font fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (fuente == null)
        {
            fuente = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        panelIntro = new GameObject("PanelIntro", typeof(RectTransform)).GetComponent<RectTransform>();
        panelIntro.SetParent(canvas.transform, false);
        panelIntro.anchorMin = Vector2.zero;
        panelIntro.anchorMax = Vector2.one;
        panelIntro.offsetMin = Vector2.zero;
        panelIntro.offsetMax = Vector2.zero;

        Image fondoIntro = panelIntro.gameObject.AddComponent<Image>();
        Texture2D texturaFondo = CrearTexturaNoche(320, 220);
        fondoIntro.sprite = Sprite.Create(texturaFondo, new Rect(0f, 0f, texturaFondo.width, texturaFondo.height), new Vector2(0.5f, 0.5f));
        fondoIntro.type = Image.Type.Simple;
        fondoIntro.color = Color.white;
        fondoIntro.raycastTarget = true;

        CrearEscenaDecorativa(panelIntro);
        CrearTextos(panelIntro, fuente);
        CrearGrupo(panelIntro);
        CrearBoton(panelIntro, fuente);
    }

    private void CrearEscenaDecorativa(RectTransform padre)
    {
        CrearBloque("BandaCieloInicio", padre, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(980f, 170f), new Vector2(0f, -48f), new Color(0.02f, 0.035f, 0.08f, 0.58f));
        CrearBloque("LunaInicio", padre, new Vector2(0.82f, 0.82f), new Vector2(0.82f, 0.82f), new Vector2(78f, 78f), Vector2.zero, new Color(1f, 0.88f, 0.48f, 0.88f));

        luces = new RectTransform[6];
        imagenesLuces = new Image[6];
        Vector2[] posiciones =
        {
            new Vector2(-360f, 40f),
            new Vector2(-220f, 78f),
            new Vector2(-40f, 26f),
            new Vector2(170f, 92f),
            new Vector2(315f, 42f),
            new Vector2(430f, 118f)
        };

        Color[] colores =
        {
            new Color(0.1f, 0.76f, 1f, 0.35f),
            new Color(1f, 0.68f, 0.2f, 0.35f),
            new Color(0.38f, 1f, 0.72f, 0.35f),
            new Color(1f, 0.28f, 0.44f, 0.35f),
            new Color(0.18f, 0.72f, 1f, 0.35f),
            new Color(1f, 0.86f, 0.36f, 0.35f)
        };

        for (int i = 0; i < luces.Length; i++)
        {
            luces[i] = CrearBloque("LuzCiudadInicio" + i, padre, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(90f, 90f), posiciones[i], colores[i]);
            imagenesLuces[i] = luces[i].GetComponent<Image>();
        }

        RectTransform ruta = CrearBloque("RutaInicio", padre, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(960f, 290f), new Vector2(0f, 0f), new Color(0.055f, 0.06f, 0.07f, 0.96f));
        ruta.pivot = new Vector2(0.5f, 0f);
        CrearBloque("LineaRutaInicio", ruta, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(14f, 260f), new Vector2(0f, -8f), new Color(1f, 0.78f, 0.28f, 0.9f));
        CrearBloque("VeredaInicio", padre, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(980f, 84f), new Vector2(0f, 276f), new Color(0.16f, 0.18f, 0.2f, 0.96f));

        autoIntro = new GameObject("AutoIntro", typeof(RectTransform)).GetComponent<RectTransform>();
        autoIntro.SetParent(padre, false);
        autoIntro.anchorMin = new Vector2(0.5f, 0f);
        autoIntro.anchorMax = new Vector2(0.5f, 0f);
        autoIntro.pivot = new Vector2(0.5f, 0.5f);
        autoIntro.sizeDelta = new Vector2(250f, 118f);
        autoIntro.anchoredPosition = new Vector2(0f, -230f);

        CrearBloque("SombraAutoInicio", autoIntro, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(270f, 26f), new Vector2(0f, -52f), new Color(0f, 0f, 0f, 0.34f));
        CrearBloque("CuerpoAutoInicio", autoIntro, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(230f, 62f), new Vector2(0f, -12f), new Color(0.08f, 0.42f, 0.56f, 1f));
        CrearBloque("CabinaAutoInicio", autoIntro, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(132f, 52f), new Vector2(-12f, 28f), new Color(0.11f, 0.68f, 0.82f, 1f));
        CrearBloque("VidrioAutoInicio", autoIntro, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(92f, 34f), new Vector2(-14f, 30f), new Color(0.64f, 0.9f, 1f, 0.7f));
        CrearBloque("FaroIzquierdoInicio", autoIntro, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(42f, 16f), new Vector2(-108f, -12f), new Color(1f, 0.86f, 0.32f, 1f));
        CrearBloque("FaroDerechoInicio", autoIntro, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(42f, 16f), new Vector2(108f, -12f), new Color(1f, 0.86f, 0.32f, 1f));
        CrearBloque("RuedaIzquierdaInicio", autoIntro, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(52f, 52f), new Vector2(-76f, -46f), new Color(0.015f, 0.017f, 0.02f, 1f));
        CrearBloque("RuedaDerechaInicio", autoIntro, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(52f, 52f), new Vector2(76f, -46f), new Color(0.015f, 0.017f, 0.02f, 1f));
    }

    private void CrearTextos(RectTransform padre, Font fuente)
    {
        textoTitulo = CrearTexto("TextoTituloInicio", padre, fuente, 44, TextAnchor.MiddleCenter);
        textoTitulo.text = "La noche empieza";
        textoTitulo.rectTransform.anchorMin = new Vector2(0.5f, 0.84f);
        textoTitulo.rectTransform.anchorMax = new Vector2(0.5f, 0.84f);
        textoTitulo.rectTransform.sizeDelta = new Vector2(660f, 62f);
        textoTitulo.color = new Color(1f, 0.88f, 0.58f, 1f);

        textoSubtitulo = CrearTexto("TextoSubtituloInicio", padre, fuente, 20, TextAnchor.MiddleCenter);
        textoSubtitulo.text = "Junta a tus amigos, llega al auto y cuidate de lo que pasa en el camino.";
        textoSubtitulo.rectTransform.anchorMin = new Vector2(0.5f, 0.765f);
        textoSubtitulo.rectTransform.anchorMax = new Vector2(0.5f, 0.765f);
        textoSubtitulo.rectTransform.sizeDelta = new Vector2(760f, 46f);
        textoSubtitulo.color = new Color(0.92f, 0.96f, 1f, 0.9f);

        RectTransform bandaObjetivo = CrearBloque("BandaObjetivoInicio", padre, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(680f, 52f), new Vector2(0f, 98f), new Color(0.02f, 0.03f, 0.04f, 0.64f));
        textoObjetivo = CrearTexto("TextoObjetivoInicio", bandaObjetivo, fuente, 18, TextAnchor.MiddleCenter);
        textoObjetivo.text = "Objetivo: encontrá a los 4 amigos antes de que se termine el tiempo";
        textoObjetivo.rectTransform.anchorMin = Vector2.zero;
        textoObjetivo.rectTransform.anchorMax = Vector2.one;
        textoObjetivo.rectTransform.offsetMin = new Vector2(12f, 0f);
        textoObjetivo.rectTransform.offsetMax = new Vector2(-12f, 0f);
    }

    private void CrearGrupo(RectTransform padre)
    {
        companeros = new RectTransform[cantidadCompaneros];
        imagenesCompaneros = new Image[cantidadCompaneros];
        posicionesBaseCompaneros = new Vector2[cantidadCompaneros];
        fases = new float[cantidadCompaneros];

        for (int i = 0; i < cantidadCompaneros; i++)
        {
            GameObject companeroObj = new GameObject("CompaneroInicio" + i, typeof(RectTransform));
            companeroObj.transform.SetParent(padre, false);
            RectTransform rect = companeroObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(72f, 92f);

            float baseX = (i - (cantidadCompaneros - 1) / 2f) * 94f;
            Vector2 basePos = new Vector2(baseX, 210f + (i % 2) * 16f);
            rect.anchoredPosition = basePos;

            Color tint = new Color(0.2f + (i % 3) * 0.12f, 0.42f + (i % 2) * 0.18f, 0.84f - (i % 2) * 0.12f, 0.95f);
            Image imagen = companeroObj.AddComponent<Image>();
            Texture2D texturaCompanero = CrearTexturaCompanero(72, 92, tint);
            imagen.color = Color.white;
            imagen.sprite = Sprite.Create(texturaCompanero, new Rect(0f, 0f, texturaCompanero.width, texturaCompanero.height), new Vector2(0.5f, 0.5f));
            imagen.type = Image.Type.Simple;

            companeros[i] = rect;
            imagenesCompaneros[i] = imagen;
            posicionesBaseCompaneros[i] = basePos;
            fases[i] = i * 1.25f;
        }
    }

    private void CrearBoton(RectTransform padre, Font fuente)
    {
        GameObject botonObj = new GameObject("BotonAceptarIntro", typeof(RectTransform), typeof(Image), typeof(Button));
        botonObj.transform.SetParent(padre, false);
        botonRect = botonObj.GetComponent<RectTransform>();
        botonRect.anchorMin = new Vector2(0.5f, 0f);
        botonRect.anchorMax = new Vector2(0.5f, 0f);
        botonRect.pivot = new Vector2(0.5f, 0.5f);
        botonRect.sizeDelta = new Vector2(280f, 58f);
        botonRect.anchoredPosition = new Vector2(0f, 38f);

        Image botonImagen = botonObj.GetComponent<Image>();
        botonImagen.color = new Color(0.98f, 0.62f, 0.18f, 1f);

        botonAceptar = botonObj.GetComponent<Button>();
        botonAceptar.targetGraphic = botonImagen;
        botonAceptar.onClick.AddListener(CerrarIntro);

        Text textoBoton = CrearTexto("TextoBotonAceptar", botonObj.transform, fuente, 22, TextAnchor.MiddleCenter);
        textoBoton.text = "Salir a buscar al grupo";
        textoBoton.color = new Color(0.04f, 0.045f, 0.05f, 1f);
        RectTransform textoBotonRect = textoBoton.rectTransform;
        textoBotonRect.anchorMin = Vector2.zero;
        textoBotonRect.anchorMax = Vector2.one;
        textoBotonRect.offsetMin = Vector2.zero;
        textoBotonRect.offsetMax = Vector2.zero;
    }

    private void CerrarIntro()
    {
        introActiva = false;
        if (panelIntro != null)
        {
            panelIntro.gameObject.SetActive(false);
        }
    }

    private RectTransform CrearBloque(string nombre, Transform padre, Vector2 anchorMin, Vector2 anchorMax, Vector2 tamano, Vector2 posicion, Color color)
    {
        GameObject objeto = new GameObject(nombre, typeof(RectTransform), typeof(Image));
        objeto.transform.SetParent(padre, false);

        RectTransform rect = objeto.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = tamano;
        rect.anchoredPosition = posicion;

        Image imagen = objeto.GetComponent<Image>();
        imagen.color = color;
        imagen.raycastTarget = false;
        return rect;
    }

    private Texture2D CrearTexturaNoche(int ancho, int alto)
    {
        Texture2D textura = new Texture2D(ancho, alto, TextureFormat.RGBA32, false);
        Color[] pixeles = new Color[ancho * alto];

        Color arriba = new Color(0.015f, 0.025f, 0.06f, 1f);
        Color medio = new Color(0.055f, 0.09f, 0.13f, 1f);
        Color abajo = new Color(0.09f, 0.08f, 0.105f, 1f);

        for (int y = 0; y < alto; y++)
        {
            float v = y / (float)(alto - 1);
            Color baseColor = v < 0.6f
                ? Color.Lerp(abajo, medio, v / 0.6f)
                : Color.Lerp(medio, arriba, (v - 0.6f) / 0.4f);

            for (int x = 0; x < ancho; x++)
            {
                float ruido = Mathf.PerlinNoise(x * 0.025f, y * 0.035f);
                Color color = Color.Lerp(baseColor, new Color(0.14f, 0.22f, 0.26f, 1f), ruido * 0.12f);
                bool estrella = y > alto * 0.58f && ((x * 17 + y * 31) % 197 == 0);
                if (estrella)
                {
                    color = Color.Lerp(color, new Color(1f, 0.9f, 0.62f, 1f), 0.85f);
                }

                pixeles[y * ancho + x] = color;
            }
        }

        textura.SetPixels(pixeles);
        textura.Apply();
        return textura;
    }

    private Texture2D CrearTexturaCompanero(int ancho, int alto, Color tint)
    {
        Texture2D textura = new Texture2D(ancho, alto, TextureFormat.RGBA32, false);
        Color[] pixeles = new Color[ancho * alto];

        Vector2 centroCabeza = new Vector2(ancho * 0.5f, alto * 0.72f);
        Vector2 centroTorso = new Vector2(ancho * 0.5f, alto * 0.36f);

        for (int y = 0; y < alto; y++)
        {
            for (int x = 0; x < ancho; x++)
            {
                Vector2 p = new Vector2(x, y);
                float cabeza = Mathf.Clamp01(1f - Vector2.Distance(p, centroCabeza) / (ancho * 0.22f));
                float torso = Mathf.Clamp01(1f - Vector2.Distance(p, centroTorso) / (ancho * 0.34f));
                float piernas = (y < alto * 0.28f && Mathf.Abs(x - ancho * 0.38f) < ancho * 0.08f) || (y < alto * 0.28f && Mathf.Abs(x - ancho * 0.62f) < ancho * 0.08f) ? 0.9f : 0f;
                float cuerpo = Mathf.Max(cabeza, Mathf.Max(torso, piernas));

                Color color = Color.Lerp(new Color(0.02f, 0.025f, 0.03f, 0f), tint, cuerpo);
                if (cabeza > 0.2f)
                {
                    color = Color.Lerp(color, new Color(0.72f, 0.42f, 0.22f, 1f), cabeza * 0.8f);
                }

                color.a = Mathf.Clamp01(cuerpo);
                pixeles[y * ancho + x] = color;
            }
        }

        textura.SetPixels(pixeles);
        textura.Apply();
        return textura;
    }

    private Text CrearTexto(string nombre, Transform padre, Font fuente, int tamano, TextAnchor alineacion)
    {
        GameObject objeto = new GameObject(nombre, typeof(RectTransform));
        objeto.transform.SetParent(padre, false);
        Text texto = objeto.AddComponent<Text>();
        texto.font = fuente;
        texto.fontSize = tamano;
        texto.alignment = alineacion;
        texto.color = Color.white;
        texto.horizontalOverflow = HorizontalWrapMode.Wrap;
        texto.verticalOverflow = VerticalWrapMode.Overflow;
        return texto;
    }
}
