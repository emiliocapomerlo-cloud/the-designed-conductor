using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Vigila las condiciones que cortan o terminan la partida durante la fase de manejo:
//  - Accidente grave (por mucho alcohol o por irse del camino) -> GAME OVER, corta el juego.
//  - Pasar el tiempo limite (2 minutos) o llegar a casa -> FINAL de la partida.
// Se crea solo cuando existe un ControladorPaisaje en la escena (es decir, en la escena de manejo).
public class ControladorFinJuego : MonoBehaviour
{
    public static ControladorFinJuego Instancia { get; private set; }

    [Header("Condiciones de FINAL del juego")]
    [Tooltip("Tiempo maximo de la partida en segundos (2 minutos = 120).")]
    [SerializeField] private float duracionPartidaSegundos = 120f;
    [Tooltip("Distancia que hay que recorrer hacia adelante para llegar a casa.")]
    [SerializeField] private float distanciaParaLlegarACasa = 20000f;

    [Header("Condiciones de ACCIDENTE GRAVE (corta el juego)")]
    [Tooltip("Nivel de alcohol a partir del cual el conductor pierde el control.")]
    [SerializeField] private float limiteAlcohol = 100f;
    [Tooltip("Segundos pegado al borde del camino antes de chocar (estando sobrio).")]
    [SerializeField] private float tiempoEnBordeParaChocar = 2.5f;

    private float tiempoTranscurrido;
    private float nivelAlcohol;
    private float tiempoEnBorde;
    private bool juegoTerminado;

    private ControladorPaisaje paisaje;

    private GameObject panelFinal;
    private Text textoFinal;
    private Text textoEstado;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void IniciarAutomaticamente()
    {
        SceneManager.sceneLoaded -= CrearSiHaceFalta;
        SceneManager.sceneLoaded += CrearSiHaceFalta;
        CrearSiHaceFalta(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private static void CrearSiHaceFalta(Scene scene, LoadSceneMode mode)
    {
        // Solo corre en la escena de manejo, donde existe el ControladorPaisaje.
        if (FindAnyObjectByType<ControladorFinJuego>() != null)
        {
            return;
        }

        if (FindAnyObjectByType<ControladorPaisaje>() == null)
        {
            return;
        }

        GameObject controlador = new GameObject("ControladorFinJuego");
        controlador.AddComponent<ControladorFinJuego>();
    }

    private void Awake()
    {
        Instancia = this;
        Time.timeScale = 1f;

        if (paisaje == null)
        {
            paisaje = FindAnyObjectByType<ControladorPaisaje>();
        }

        CrearInterfaz();
    }

    private void OnDestroy()
    {
        if (Instancia == this)
        {
            Instancia = null;
        }
    }

    // La llama el controlador de eventos cuando el jugador acepta un trago.
    public void SumarAlcohol(float cantidad)
    {
        if (juegoTerminado)
        {
            return;
        }

        nivelAlcohol = Mathf.Max(0f, nivelAlcohol + cantidad);
    }

    private void Update()
    {
        if (juegoTerminado)
        {
            return;
        }

        tiempoTranscurrido += Time.deltaTime;

        ActualizarEstado();

        // 1) Accidente grave por demasiado alcohol -> corta el juego.
        if (nivelAlcohol >= limiteAlcohol)
        {
            TerminarPartida("ACCIDENTE GRAVE\nManejaste con demasiado alcohol.", false);
            return;
        }

        // 2) Accidente grave por irse del camino. El alcohol hace que choques mas rapido.
        if (paisaje != null && paisaje.DesvioNormalizado >= 0.98f)
        {
            tiempoEnBorde += Time.deltaTime;
        }
        else
        {
            tiempoEnBorde = Mathf.Max(0f, tiempoEnBorde - Time.deltaTime * 0.5f);
        }

        float factorAlcohol = limiteAlcohol > 0f ? Mathf.Clamp01(nivelAlcohol / limiteAlcohol) : 0f;
        float umbralBorde = tiempoEnBordeParaChocar * (1f - factorAlcohol * 0.6f);
        if (tiempoEnBorde >= umbralBorde)
        {
            TerminarPartida("ACCIDENTE GRAVE\nTe saliste del camino.", false);
            return;
        }

        // 3) Llegaste a casa -> final feliz.
        if (paisaje != null && paisaje.AvanceAcumulado >= distanciaParaLlegarACasa)
        {
            TerminarPartida("¡LLEGASTE A CASA!\nMision cumplida, todos a salvo.", true);
            return;
        }

        // 4) Se acabaron los 2 minutos -> final del recorrido.
        if (tiempoTranscurrido >= duracionPartidaSegundos)
        {
            TerminarPartida("FIN DEL RECORRIDO\nSe acabo el tiempo.", true);
        }
    }

    private void TerminarPartida(string mensaje, bool esVictoria)
    {
        juegoTerminado = true;
        Time.timeScale = 0f;

        if (textoEstado != null)
        {
            textoEstado.enabled = false;
        }

        if (panelFinal != null)
        {
            panelFinal.SetActive(true);
        }

        if (textoFinal != null)
        {
            textoFinal.text = mensaje;
            textoFinal.color = esVictoria ? new Color(0.55f, 1f, 0.6f, 1f) : new Color(1f, 0.5f, 0.45f, 1f);
        }

        Debug.Log("Fin de la partida: " + mensaje.Replace("\n", " "));
    }

    private void Reiniciar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ActualizarEstado()
    {
        if (textoEstado == null)
        {
            return;
        }

        int segundosRestantes = Mathf.Max(0, Mathf.CeilToInt(duracionPartidaSegundos - tiempoTranscurrido));
        int alcoholPorcentaje = limiteAlcohol > 0f ? Mathf.RoundToInt(Mathf.Clamp01(nivelAlcohol / limiteAlcohol) * 100f) : 0;
        textoEstado.text = "Tiempo: " + segundosRestantes + "s   Alcohol: " + alcoholPorcentaje + "%";
        textoEstado.color = alcoholPorcentaje >= 60 ? new Color(1f, 0.5f, 0.45f, 1f) : Color.white;
    }

    private void CrearInterfaz()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObjeto = new GameObject("Canvas", typeof(RectTransform));
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

        // HUD: tiempo restante y nivel de alcohol (arriba a la izquierda).
        textoEstado = CrearTexto("TextoEstadoFinJuego", canvas.transform, fuente, 22, TextAnchor.UpperLeft);
        RectTransform estadoRect = textoEstado.rectTransform;
        estadoRect.anchorMin = new Vector2(0f, 1f);
        estadoRect.anchorMax = new Vector2(0f, 1f);
        estadoRect.pivot = new Vector2(0f, 1f);
        estadoRect.anchoredPosition = new Vector2(20f, -20f);
        estadoRect.sizeDelta = new Vector2(420f, 36f);

        // Panel final (oculto hasta que termina la partida).
        panelFinal = new GameObject("PanelFinJuego", typeof(RectTransform));
        panelFinal.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = panelFinal.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image fondo = panelFinal.AddComponent<Image>();
        fondo.color = new Color(0f, 0f, 0f, 0.82f);

        textoFinal = CrearTexto("TextoFinJuego", panelFinal.transform, fuente, 40, TextAnchor.MiddleCenter);
        RectTransform textoRect = textoFinal.rectTransform;
        textoRect.anchorMin = new Vector2(0.5f, 0.5f);
        textoRect.anchorMax = new Vector2(0.5f, 0.5f);
        textoRect.pivot = new Vector2(0.5f, 0.5f);
        textoRect.anchoredPosition = new Vector2(0f, 60f);
        textoRect.sizeDelta = new Vector2(720f, 220f);

        Button botonReiniciar = CrearBoton("BotonReiniciar", panelFinal.transform, fuente, "Reiniciar", new Color(0.16f, 0.4f, 0.55f, 1f));
        RectTransform botonRect = botonReiniciar.GetComponent<RectTransform>();
        botonRect.anchorMin = new Vector2(0.5f, 0.5f);
        botonRect.anchorMax = new Vector2(0.5f, 0.5f);
        botonRect.pivot = new Vector2(0.5f, 0.5f);
        botonRect.anchoredPosition = new Vector2(0f, -80f);
        botonRect.sizeDelta = new Vector2(220f, 56f);
        botonReiniciar.onClick.AddListener(Reiniciar);

        panelFinal.SetActive(false);
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

    private Button CrearBoton(string nombre, Transform padre, Font fuente, string etiqueta, Color color)
    {
        GameObject objeto = new GameObject(nombre, typeof(RectTransform));
        objeto.transform.SetParent(padre, false);
        Image imagen = objeto.AddComponent<Image>();
        imagen.color = color;
        Button boton = objeto.AddComponent<Button>();

        Text texto = CrearTexto("Texto", objeto.transform, fuente, 22, TextAnchor.MiddleCenter);
        texto.text = etiqueta;
        RectTransform textoRect = texto.rectTransform;
        textoRect.anchorMin = Vector2.zero;
        textoRect.anchorMax = Vector2.one;
        textoRect.offsetMin = Vector2.zero;
        textoRect.offsetMax = Vector2.zero;

        return boton;
    }
}
