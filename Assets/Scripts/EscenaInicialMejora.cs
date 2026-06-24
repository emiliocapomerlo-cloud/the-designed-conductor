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
    [SerializeField] private float amplitudMovimiento = 26f;
    [SerializeField] private float frecuenciaMovimiento = 3.2f;
    [SerializeField] private float amplitudBote = 10f;
    [SerializeField] private float frecuenciaBote = 5.5f;

    private Canvas canvas;
    private RectTransform panelIntro;
    private Text textoTitulo;
    private Text textoSubtitulo;
    private RectTransform[] companeros;
    private Image[] imagenesCompaneros;
    private float[] fases;
    private bool introActiva = true;
    private Button botonAceptar;

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
        if (!introActiva || companeros == null || imagenesCompaneros == null)
        {
            return;
        }

        float tiempo = Time.time;
        for (int i = 0; i < companeros.Length; i++)
        {
            RectTransform companero = companeros[i];
            if (companero == null)
            {
                continue;
            }

            float fase = fases[i];
            float x = Mathf.Sin(tiempo * frecuenciaMovimiento + fase) * amplitudMovimiento;
            float y = Mathf.Cos(tiempo * frecuenciaBote + fase * 0.7f) * amplitudBote;
            companero.anchoredPosition = new Vector2(x, y);

            float rotacion = Mathf.Sin(tiempo * 2.2f + fase) * 13f;
            companero.localRotation = Quaternion.Euler(0f, 0f, rotacion);

            if (imagenesCompaneros[i] != null)
            {
                Color color = new Color(0.2f + (i % 2) * 0.25f, 0.4f + (i % 3) * 0.12f, 0.75f - (i % 2) * 0.1f, 0.95f);
                imagenesCompaneros[i].color = color;
            }
        }

        if (textoTitulo != null)
        {
            textoTitulo.color = new Color(1f, 0.94f, 0.68f, 1f);
        }

        if (textoSubtitulo != null)
        {
            float alpha = 0.7f + Mathf.Sin(tiempo * 1.7f) * 0.12f;
            textoSubtitulo.color = new Color(0.95f, 0.95f, 0.95f, alpha);
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
        Texture2D texturaFondo = CrearTexturaPared(256, 256, new Color(0.06f, 0.08f, 0.13f, 1f), new Color(0.16f, 0.2f, 0.28f, 1f));
        fondoIntro.sprite = Sprite.Create(texturaFondo, new Rect(0f, 0f, texturaFondo.width, texturaFondo.height), new Vector2(0.5f, 0.5f));
        fondoIntro.type = Image.Type.Simple;
        fondoIntro.color = Color.white;
        fondoIntro.raycastTarget = true;

        textoTitulo = CrearTexto("TextoTituloInicio", panelIntro, fuente, 30, TextAnchor.MiddleCenter);
        textoTitulo.text = "La noche empieza";
        textoTitulo.rectTransform.anchorMin = new Vector2(0.5f, 0.82f);
        textoTitulo.rectTransform.anchorMax = new Vector2(0.5f, 0.82f);
        textoTitulo.rectTransform.sizeDelta = new Vector2(420f, 44f);
        textoTitulo.color = new Color(1f, 0.92f, 0.7f, 0.95f);

        textoSubtitulo = CrearTexto("TextoSubtituloInicio", panelIntro, fuente, 18, TextAnchor.MiddleCenter);
        textoSubtitulo.text = "Tus compañeros están listos para salir, pero la diversión puede volverse peligrosa.";
        textoSubtitulo.rectTransform.anchorMin = new Vector2(0.5f, 0.75f);
        textoSubtitulo.rectTransform.anchorMax = new Vector2(0.5f, 0.75f);
        textoSubtitulo.rectTransform.sizeDelta = new Vector2(760f, 50f);
        textoSubtitulo.color = new Color(0.95f, 0.95f, 0.95f, 0.85f);

        companeros = new RectTransform[cantidadCompaneros];
        imagenesCompaneros = new Image[cantidadCompaneros];
        fases = new float[cantidadCompaneros];

        for (int i = 0; i < cantidadCompaneros; i++)
        {
            GameObject companeroObj = new GameObject("CompaneroBailando" + i, typeof(RectTransform));
            companeroObj.transform.SetParent(panelIntro, false);
            RectTransform rect = companeroObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.18f);
            rect.anchorMax = new Vector2(0.5f, 0.18f);
            rect.sizeDelta = new Vector2(54f, 54f);
            rect.anchoredPosition = new Vector2((i - (cantidadCompaneros - 1) / 2f) * 88f, 0f);

            Color tint = new Color(0.22f + (i % 3) * 0.08f, 0.45f + (i % 2) * 0.12f, 0.9f - (i % 2) * 0.1f, 0.95f);
            Image imagen = companeroObj.AddComponent<Image>();
            Texture2D texturaCompanero = CrearTexturaCompanero(64, 64, tint);
            imagen.color = Color.white;
            imagen.sprite = Sprite.Create(texturaCompanero, new Rect(0f, 0f, texturaCompanero.width, texturaCompanero.height), new Vector2(0.5f, 0.5f));
            imagen.type = Image.Type.Simple;

            companeros[i] = rect;
            imagenesCompaneros[i] = imagen;
            fases[i] = i * 1.2f;
        }

        botonAceptar = new GameObject("BotonAceptarIntro", typeof(RectTransform)).AddComponent<Button>();
        RectTransform botonRect = botonAceptar.GetComponent<RectTransform>();
        botonRect.SetParent(panelIntro, false);
        botonRect.anchorMin = new Vector2(0.5f, 0.18f);
        botonRect.anchorMax = new Vector2(0.5f, 0.18f);
        botonRect.sizeDelta = new Vector2(220f, 54f);
        botonRect.anchoredPosition = Vector2.zero;

        Image botonImagen = botonAceptar.gameObject.AddComponent<Image>();
        botonImagen.color = new Color(0.24f, 0.56f, 0.9f, 1f);
        botonAceptar.targetGraphic = botonImagen;
        botonAceptar.onClick.AddListener(CerrarIntro);

        Text textoBoton = CrearTexto("TextoBotonAceptar", botonAceptar.transform, fuente, 22, TextAnchor.MiddleCenter);
        textoBoton.text = "Aceptar";
        textoBoton.color = Color.white;
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

    private Texture2D CrearTexturaPared(int ancho, int alto, Color colorBase, Color colorDetalle)
    {
        Texture2D textura = new Texture2D(ancho, alto, TextureFormat.RGBA32, false);
        Color[] pixeles = new Color[ancho * alto];

        for (int y = 0; y < alto; y++)
        {
            for (int x = 0; x < ancho; x++)
            {
                float ruido = Mathf.PerlinNoise(x * 0.03f, y * 0.04f);
                Color baseColor = Color.Lerp(colorBase, colorDetalle, ruido * 0.35f);
                bool rayas = (x + y) % 24 < 8;
                if (rayas)
                {
                    baseColor = Color.Lerp(baseColor, new Color(0.95f, 0.9f, 0.72f, 1f), 0.08f);
                }
                pixeles[y * ancho + x] = baseColor;
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

        for (int y = 0; y < alto; y++)
        {
            for (int x = 0; x < ancho; x++)
            {
                float distancia = Vector2.Distance(new Vector2(x, y), new Vector2(ancho * 0.5f, alto * 0.5f));
                float radio = Mathf.Min(ancho, alto) * 0.35f;
                float brillo = Mathf.Clamp01(1f - distancia / radio);
                Color color = Color.Lerp(new Color(0.12f, 0.14f, 0.18f, 1f), tint, brillo * 0.75f);
                color.a = Mathf.Clamp01(0.35f + brillo * 0.65f);
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
