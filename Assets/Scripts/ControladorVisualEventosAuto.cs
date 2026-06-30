using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ControladorVisualEventosAuto : MonoBehaviour
{
    public static ControladorVisualEventosAuto Instancia { get; private set; }

    [Header("Visuales Calibrados")]
    [SerializeField] private float duracionVisual = 1.45f;
    [SerializeField] private Vector2 posicionPanel = new Vector2(0f, -120f);

    [Header("Cerveza")]
    [SerializeField] private float duracionEntregaCerveza = 1.05f;
    [SerializeField] private float alturaArcoCerveza = 70f;
    [SerializeField] private Vector2 posicionCervezaOrigen = new Vector2(370f, -95f);
    [SerializeField] private Vector2 posicionCervezaDestino = new Vector2(-245f, -92f);
    [SerializeField] private Vector2 posicionManoAcompanante = new Vector2(410f, -112f);
    [SerializeField] private Vector2 posicionManoConductor = new Vector2(-285f, -108f);

    // Cerca de la espina de pescado dibujada en la cabina, detras del conductor.
    [SerializeField] private Vector2 posicionCervezaPiso = new Vector2(-655f, -388f);

    [Header("Celular")]
    [SerializeField] private Vector2 posicionCelular = new Vector2(-185f, -215f);
    [SerializeField] private float duracionCelular = 1.8f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CrearSiHaceFalta()
    {
        if (FindAnyObjectByType<ControladorVisualEventosAuto>() != null) return;
        if (FindAnyObjectByType<ControladorPaisaje>() == null) return;

        GameObject controlador = new GameObject("ControladorVisualEventosAuto");
        controlador.AddComponent<ControladorVisualEventosAuto>();
    }

    private Canvas canvas;
    private RectTransform panel;
    private Image fondo;
    private Image overlayFlashImage;
    private RectTransform manoCompanero;
    private RectTransform manoConductor;
    private RectTransform cervezaMovil;
    private RectTransform celularVisual;
    private RectTransform brilloCelular;
    private RectTransform pantallaCelular;
    private Text textoCelular;
    private Coroutine rutinaEntregaCerveza;
    private Coroutine rutinaCelular;
    private int cervezasTiradas;
    private float tiempoRestante;
    private string eventoActual = "";
    private float tiempoEfectoEspecial;
    private float duracionEfectoEspecial;

    private void Awake()
    {
        Instancia = this;
        CrearInterfaz();
        Ocultar();
    }

    private void OnDestroy()
    {
        if (Instancia == this) Instancia = null;
    }

    private void Update()
    {
        if (tiempoRestante <= 0f)
        {
            return;
        }

        tiempoRestante -= Time.deltaTime;
        if (tiempoRestante <= 0f)
        {
            Ocultar();
            return;
        }

        if (tiempoEfectoEspecial > 0f)
        {
            tiempoEfectoEspecial -= Time.deltaTime;
            if (tiempoEfectoEspecial <= 0f)
            {
                tiempoEfectoEspecial = 0f;
                duracionEfectoEspecial = 0f;
            }
        }

        ActualizarAnimacion();
    }

    public void ActivarEvento(string idEvento)
    {
        if (string.IsNullOrEmpty(idEvento)) return;

        eventoActual = idEvento;
        tiempoRestante = duracionVisual;

        bool esCerveza = string.Equals(idEvento, "Cerveza", System.StringComparison.OrdinalIgnoreCase);
        bool esCelular = string.Equals(idEvento, "Celular", System.StringComparison.OrdinalIgnoreCase);

        if (esCerveza)
        {
            AnimarEntregaCerveza();
        }
        else if (esCelular)
        {
            AnimarCelular();
        }

        if (panel != null && !esCerveza && !esCelular)
        {
            panel.gameObject.SetActive(true);
            panel.SetAsLastSibling();
        }

        PrepararEfectoEspecial(idEvento);
        AplicarEstiloEvento(idEvento);
        ActualizarAnimacion();
    }

    public void MostrarCervezaEnCabina()
    {
        InstanciarCervezaTirada();
    }

    private void AnimarEntregaCerveza()
    {
        if (canvas == null) CrearInterfaz();

        if (rutinaEntregaCerveza != null)
        {
            StopCoroutine(rutinaEntregaCerveza);
        }

        rutinaEntregaCerveza = StartCoroutine(RutinaEntregaCerveza());
    }

    private IEnumerator RutinaEntregaCerveza()
    {
        PrepararEntregaCerveza();

        Vector2 origen = posicionCervezaOrigen;
        Vector2 destino = posicionCervezaDestino;
        Vector2 control = (origen + destino) * 0.5f + Vector2.up * alturaArcoCerveza;

        cervezaMovil.gameObject.SetActive(true);
        manoCompanero.gameObject.SetActive(true);
        manoConductor.gameObject.SetActive(true);

        float tiempo = 0f;
        while (tiempo < duracionEntregaCerveza)
        {
            float t = Mathf.Clamp01(tiempo / Mathf.Max(0.01f, duracionEntregaCerveza));
            float suave = t * t * (3f - 2f * t);
            Vector2 botella = Vector2.Lerp(Vector2.Lerp(origen, control, suave), Vector2.Lerp(control, destino, suave), suave);
            float vibracion = Mathf.Sin(Time.time * 24f) * 2f;

            cervezaMovil.anchoredPosition = botella + new Vector2(0f, vibracion * 0.25f);
            cervezaMovil.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(-10f, -58f, suave));
            cervezaMovil.localScale = Vector3.one * Mathf.Lerp(1.2f, 1.05f, suave);

            float salida = Mathf.Clamp01(t / 0.42f);
            manoCompanero.anchoredPosition = Vector2.Lerp(posicionManoAcompanante, posicionManoAcompanante + new Vector2(-95f, 10f), salida);
            manoCompanero.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(-8f, -18f, salida));
            manoCompanero.localScale = Vector3.one * Mathf.Lerp(1.25f, 1.05f, salida);

            float entrada = Mathf.Clamp01((t - 0.52f) / 0.38f);
            manoConductor.anchoredPosition = Vector2.Lerp(posicionManoConductor + new Vector2(-70f, -12f), posicionManoConductor, entrada);
            manoConductor.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(18f, 4f, entrada));
            manoConductor.localScale = Vector3.one * Mathf.Lerp(0.75f, 1.1f, entrada);

            SetAlpha(manoCompanero, 1f - Mathf.Clamp01((t - 0.55f) / 0.25f));
            SetAlpha(manoConductor, entrada);

            tiempo += Time.deltaTime;
            yield return null;
        }

        cervezaMovil.anchoredPosition = destino;
        cervezaMovil.localRotation = Quaternion.Euler(0f, 0f, -58f);
        SetAlpha(manoConductor, 1f);
        yield return new WaitForSeconds(0.12f);

        cervezaMovil.gameObject.SetActive(false);
        manoCompanero.gameObject.SetActive(false);
        manoConductor.gameObject.SetActive(false);
        SetAlpha(manoCompanero, 1f);
        SetAlpha(manoConductor, 1f);
        InstanciarCervezaTirada();
        rutinaEntregaCerveza = null;
    }

    private void PrepararEntregaCerveza()
    {
        Transform padre = ObtenerPadreCabinaEventos();

        if (manoCompanero == null)
        {
            manoCompanero = CrearMano("ManoAcompananteCerveza", padre, true);
        }

        if (manoConductor == null)
        {
            manoConductor = CrearMano("ManoConductorRecibeCerveza", padre, false);
        }

        if (cervezaMovil == null)
        {
            cervezaMovil = new GameObject("CervezaMovilEntrega", typeof(RectTransform)).GetComponent<RectTransform>();
            cervezaMovil.SetParent(padre, false);
            cervezaMovil.anchorMin = new Vector2(0.5f, 0.5f);
            cervezaMovil.anchorMax = new Vector2(0.5f, 0.5f);
            cervezaMovil.pivot = new Vector2(0.5f, 0.5f);
            cervezaMovil.sizeDelta = new Vector2(48f, 96f);
            cervezaMovil.localScale = Vector3.one;
            CrearCervezaVertical(cervezaMovil);
        }

        manoCompanero.SetAsLastSibling();
        manoConductor.SetAsLastSibling();
        cervezaMovil.SetAsLastSibling();
    }

    private void InstanciarCervezaTirada()
    {
        cervezasTiradas++;

        Transform padre = ObtenerPadreCabinaEventos();
        RectTransform botella = new GameObject("CervezaEnCabina", typeof(RectTransform)).GetComponent<RectTransform>();
        botella.SetParent(padre, false);
        botella.anchorMin = new Vector2(0.5f, 0.5f);
        botella.anchorMax = new Vector2(0.5f, 0.5f);
        botella.pivot = new Vector2(0.5f, 0.5f);
        botella.sizeDelta = new Vector2(96f, 52f);

        int indice = cervezasTiradas - 1;
        Vector2 offset = new Vector2((indice % 3) * 34f, (indice / 3) * 18f);
        botella.anchoredPosition = posicionCervezaPiso + offset;
        botella.localRotation = Quaternion.Euler(0f, 0f, -10f + indice * 7f);
        botella.localScale = Vector3.one;

        CrearCervezaAcostada(botella);
        botella.SetAsLastSibling();
    }

    private void AnimarCelular()
    {
        if (canvas == null) CrearInterfaz();

        if (rutinaCelular != null)
        {
            StopCoroutine(rutinaCelular);
        }

        rutinaCelular = StartCoroutine(RutinaCelular());
    }

    private IEnumerator RutinaCelular()
    {
        PrepararCelular();

        celularVisual.gameObject.SetActive(true);
        brilloCelular.gameObject.SetActive(true);
        celularVisual.SetAsLastSibling();

        float tiempo = 0f;
        while (tiempo < duracionCelular)
        {
            float t = Mathf.Clamp01(tiempo / Mathf.Max(0.01f, duracionCelular));
            float entrada = Mathf.Clamp01(t / 0.22f);
            float salida = Mathf.Clamp01((1f - t) / 0.18f);
            float visible = Mathf.Min(entrada, salida);
            float zumbido = Mathf.Sin(Time.time * 44f) * 6f;
            float salto = Mathf.Sin(t * Mathf.PI) * 42f;

            celularVisual.anchoredPosition = posicionCelular + new Vector2(zumbido, salto);
            celularVisual.localRotation = Quaternion.Euler(0f, 0f, -10f + Mathf.Sin(Time.time * 28f) * 5f);
            celularVisual.localScale = Vector3.one * Mathf.Lerp(0.85f, 1.2f, visible);
            brilloCelular.anchoredPosition = celularVisual.anchoredPosition;
            brilloCelular.localScale = Vector3.one * Mathf.Lerp(0.8f, 1.25f, visible);

            float pulso = Mathf.Abs(Mathf.Sin(Time.time * 12f));
            SetAlpha(celularVisual, visible);
            SetAlpha(brilloCelular, visible * (0.35f + pulso * 0.45f));
            SetAlpha(pantallaCelular, visible);
            if (textoCelular != null)
            {
                textoCelular.color = new Color(0.02f, 0.05f, 0.08f, visible);
                textoCelular.text = t < 0.48f ? "1 nuevo mensaje" : "veni? ahora";
            }

            if (overlayFlashImage != null)
            {
                float flash = Mathf.Sin(t * Mathf.PI) * (0.18f + pulso * 0.1f);
                overlayFlashImage.color = new Color(0.12f, 0.65f, 1f, flash * visible);
            }

            tiempo += Time.deltaTime;
            yield return null;
        }

        celularVisual.gameObject.SetActive(false);
        brilloCelular.gameObject.SetActive(false);
        if (overlayFlashImage != null)
        {
            overlayFlashImage.color = new Color(1f, 1f, 1f, 0f);
        }

        rutinaCelular = null;
    }

    private void PrepararCelular()
    {
        Transform padre = ObtenerPadreCabinaEventos();
        if (celularVisual != null)
        {
            return;
        }

        brilloCelular = CrearBloqueUI("BrilloCelularEvento", padre, new Vector2(190f, 190f), new Color(0.1f, 0.65f, 1f, 0.32f), out _);
        brilloCelular.gameObject.SetActive(false);

        celularVisual = new GameObject("CelularEvento", typeof(RectTransform)).GetComponent<RectTransform>();
        celularVisual.SetParent(padre, false);
        celularVisual.anchorMin = new Vector2(0.5f, 0.5f);
        celularVisual.anchorMax = new Vector2(0.5f, 0.5f);
        celularVisual.pivot = new Vector2(0.5f, 0.5f);
        celularVisual.sizeDelta = new Vector2(92f, 150f);
        celularVisual.localScale = Vector3.one;

        CrearBloqueUI("CuerpoCelular", celularVisual, new Vector2(92f, 150f), new Color(0.015f, 0.018f, 0.025f, 1f), out _);
        pantallaCelular = CrearBloqueUI("PantallaCelular", celularVisual, new Vector2(76f, 124f), new Color(0.46f, 0.9f, 1f, 1f), out _);
        pantallaCelular.anchoredPosition = new Vector2(0f, 2f);
        CrearBloqueUI("NotificacionCelular", celularVisual, new Vector2(64f, 36f), new Color(1f, 1f, 1f, 0.96f), out _).anchoredPosition = new Vector2(0f, 24f);

        Font fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (fuente == null)
        {
            fuente = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        GameObject textoGo = new GameObject("TextoCelular", typeof(RectTransform));
        textoGo.transform.SetParent(celularVisual, false);
        textoCelular = textoGo.AddComponent<Text>();
        textoCelular.font = fuente;
        textoCelular.fontSize = 10;
        textoCelular.alignment = TextAnchor.MiddleCenter;
        textoCelular.color = new Color(0.02f, 0.05f, 0.08f, 1f);
        RectTransform textoRect = textoCelular.rectTransform;
        textoRect.anchorMin = new Vector2(0.5f, 0.5f);
        textoRect.anchorMax = new Vector2(0.5f, 0.5f);
        textoRect.pivot = new Vector2(0.5f, 0.5f);
        textoRect.anchoredPosition = new Vector2(0f, 24f);
        textoRect.sizeDelta = new Vector2(62f, 32f);
        celularVisual.gameObject.SetActive(false);
    }

    private void CrearInterfaz()
    {
        canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObjeto = new GameObject("CanvasEventosVisuales", typeof(RectTransform));
            canvas = canvasObjeto.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 1000;
            canvasObjeto.AddComponent<CanvasScaler>();
            canvasObjeto.AddComponent<GraphicRaycaster>();
        }

        GameObject overlayFlashObjeto = new GameObject("OverlayFlash", typeof(RectTransform));
        overlayFlashImage = overlayFlashObjeto.AddComponent<Image>();
        RectTransform overlayFlashRect = overlayFlashObjeto.GetComponent<RectTransform>();
        overlayFlashRect.SetParent(canvas.transform, false);
        overlayFlashRect.anchorMin = Vector2.zero;
        overlayFlashRect.anchorMax = Vector2.one;
        overlayFlashRect.offsetMin = Vector2.zero;
        overlayFlashRect.offsetMax = Vector2.zero;
        overlayFlashImage.color = new Color(1f, 1f, 1f, 0f);
        overlayFlashImage.raycastTarget = false;
        overlayFlashRect.SetAsLastSibling();

        panel = new GameObject("PanelEventoVisual", typeof(RectTransform)).GetComponent<RectTransform>();
        panel.SetParent(canvas.transform, false);
        panel.anchorMin = new Vector2(0.5f, 0f);
        panel.anchorMax = new Vector2(0.5f, 0f);
        panel.pivot = new Vector2(0.5f, 0f);
        panel.anchoredPosition = posicionPanel;
        panel.sizeDelta = new Vector2(330f, 150f);

        fondo = panel.gameObject.AddComponent<Image>();
        fondo.color = new Color(0.03f, 0.04f, 0.06f, 0.85f);
        fondo.raycastTarget = false;

        panel.SetAsLastSibling();
    }

    private void PrepararEfectoEspecial(string id) { }
    private void AplicarEstiloEvento(string id) { }

    private void ActualizarAnimacion()
    {
        if (eventoActual.Equals("Celular", System.StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (overlayFlashImage != null)
        {
            overlayFlashImage.color = new Color(1f, 1f, 1f, 0f);
        }
    }

    private Transform ObtenerPadreCabinaEventos()
    {
        ControladorPaisaje paisaje = FindAnyObjectByType<ControladorPaisaje>();
        if (paisaje != null && paisaje.cabina != null && paisaje.cabina.parent != null)
        {
            return paisaje.cabina.parent;
        }

        if (canvas == null)
        {
            CrearInterfaz();
        }

        return canvas.transform;
    }

    private RectTransform CrearBloqueUI(string nombre, Transform padre, Vector2 tamano, Color color, out Image imagen)
    {
        GameObject objeto = new GameObject(nombre, typeof(RectTransform), typeof(Image));
        objeto.transform.SetParent(padre, false);

        RectTransform rect = objeto.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = tamano;
        rect.localScale = Vector3.one;

        imagen = objeto.GetComponent<Image>();
        imagen.color = color;
        imagen.raycastTarget = false;
        return rect;
    }

    private RectTransform CrearMano(string nombre, Transform padre, bool desdeDerecha)
    {
        RectTransform mano = new GameObject(nombre, typeof(RectTransform)).GetComponent<RectTransform>();
        mano.SetParent(padre, false);
        mano.anchorMin = new Vector2(0.5f, 0.5f);
        mano.anchorMax = new Vector2(0.5f, 0.5f);
        mano.pivot = new Vector2(0.5f, 0.5f);
        mano.sizeDelta = new Vector2(78f, 48f);
        mano.localScale = Vector3.one;

        CrearBloqueUI("Palma", mano, new Vector2(56f, 34f), new Color(0.5f, 0.24f, 0.1f, 1f), out _);
        float direccion = desdeDerecha ? -1f : 1f;
        for (int i = 0; i < 4; i++)
        {
            RectTransform dedo = CrearBloqueUI("Dedo", mano, new Vector2(24f, 8f), new Color(0.62f, 0.32f, 0.15f, 1f), out _);
            dedo.anchoredPosition = new Vector2(direccion * (18f + i * 4f), -13f + i * 7f);
            dedo.localRotation = Quaternion.Euler(0f, 0f, direccion * -10f);
        }

        return mano;
    }

    private void CrearCervezaVertical(RectTransform raiz)
    {
        CrearBloqueUI("Sombra", raiz, new Vector2(52f, 100f), new Color(0f, 0f, 0f, 0.22f), out _).anchoredPosition = new Vector2(4f, -4f);
        CrearBloqueUI("Cuerpo", raiz, new Vector2(42f, 70f), new Color(0.16f, 0.5f, 0.28f, 0.98f), out _).anchoredPosition = new Vector2(0f, -12f);
        CrearBloqueUI("Cuello", raiz, new Vector2(22f, 34f), new Color(0.15f, 0.45f, 0.25f, 0.98f), out _).anchoredPosition = new Vector2(0f, 38f);
        CrearBloqueUI("Etiqueta", raiz, new Vector2(45f, 22f), new Color(0.92f, 0.9f, 0.78f, 0.96f), out _).anchoredPosition = new Vector2(0f, -16f);
        CrearBloqueUI("Tapa", raiz, new Vector2(22f, 10f), new Color(0.9f, 0.72f, 0.12f, 1f), out _).anchoredPosition = new Vector2(0f, 59f);
    }

    private void CrearCervezaAcostada(RectTransform raiz)
    {
        CrearBloqueUI("Sombra", raiz, new Vector2(104f, 32f), new Color(0f, 0f, 0f, 0.2f), out _).anchoredPosition = new Vector2(3f, -6f);
        CrearBloqueUI("Cuerpo", raiz, new Vector2(66f, 32f), new Color(0.16f, 0.5f, 0.28f, 0.98f), out _).anchoredPosition = new Vector2(-13f, 0f);
        CrearBloqueUI("Cuello", raiz, new Vector2(30f, 17f), new Color(0.15f, 0.45f, 0.25f, 0.98f), out _).anchoredPosition = new Vector2(35f, 0f);
        CrearBloqueUI("Etiqueta", raiz, new Vector2(34f, 22f), new Color(0.92f, 0.9f, 0.78f, 0.96f), out _).anchoredPosition = new Vector2(-14f, 0f);
        CrearBloqueUI("Tapa", raiz, new Vector2(8f, 18f), new Color(0.9f, 0.72f, 0.12f, 1f), out _).anchoredPosition = new Vector2(52f, 0f);
    }

    private void SetAlpha(RectTransform raiz, float alpha)
    {
        if (raiz == null) return;

        Graphic[] graficos = raiz.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graficos.Length; i++)
        {
            Color color = graficos[i].color;
            color.a = alpha;
            graficos[i].color = color;
        }
    }

    private void Ocultar()
    {
        if (panel != null) panel.gameObject.SetActive(false);
        if (overlayFlashImage != null) overlayFlashImage.color = new Color(1f, 1f, 1f, 0f);
        eventoActual = "";
    }
}
