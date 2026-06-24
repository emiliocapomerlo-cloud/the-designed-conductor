using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ControladorEscapeInicial : MonoBehaviour
{
    public static ControladorEscapeInicial Instancia { get; private set; }

    [SerializeField] private float tiempoLimiteSegundos = 60f;

    private float tiempoRestante;
    private bool partidaTerminada;
    private Text textoTimer;
    private GameObject panelFinal;
    private Text textoFinal;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void IniciarAutomaticamente()
    {
        SceneManager.sceneLoaded -= CrearSiHaceFalta;
        SceneManager.sceneLoaded += CrearSiHaceFalta;
        CrearSiHaceFalta(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private static void CrearSiHaceFalta(Scene scene, LoadSceneMode mode)
    {
        if (FindAnyObjectByType<ControladorEscapeInicial>() != null || FindAnyObjectByType<ControladorPaisaje>() != null)
        {
            return;
        }

        if (GameObject.FindGameObjectWithTag("Player") == null || FindAnyObjectByType<ControlarAuto>() == null)
        {
            return;
        }

        GameObject controlador = new GameObject("ControladorEscapeInicial");
        controlador.AddComponent<ControladorEscapeInicial>();
    }

    private void Awake()
    {
        Instancia = this;
        Time.timeScale = 1f;
        tiempoRestante = tiempoLimiteSegundos;
        CrearInterfaz();
    }

    private void OnDestroy()
    {
        if (Instancia == this)
        {
            Instancia = null;
        }
    }

    private void Update()
    {
        if (partidaTerminada)
        {
            return;
        }

        tiempoRestante -= Time.deltaTime;
        ActualizarTimer();

        if (tiempoRestante <= 0f)
        {
            TerminarPartida("SE ACABO EL TIEMPO\nNo llegaste al auto antes de 1 minuto.");
        }
    }

    public static void TerminarPorPolicia()
    {
        if (Instancia != null)
        {
            Instancia.TerminarPartida("TE ATRAPO LA POLICIA\nEl juego termino antes de llegar al auto.");
        }
    }

    private void TerminarPartida(string mensaje)
    {
        if (partidaTerminada)
        {
            return;
        }

        partidaTerminada = true;
        Time.timeScale = 0f;

        if (panelFinal != null)
        {
            panelFinal.transform.SetAsLastSibling();
            panelFinal.SetActive(true);
        }

        if (textoFinal != null)
        {
            textoFinal.text = mensaje;
        }

        Debug.Log(mensaje.Replace("\n", " "));
    }

    private void Reiniciar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ActualizarTimer()
    {
        if (textoTimer == null)
        {
            return;
        }

        int segundos = Mathf.Max(0, Mathf.CeilToInt(tiempoRestante));
        textoTimer.text = "Auto en: " + segundos + "s";
        textoTimer.color = segundos <= 10 ? new Color(1f, 0.42f, 0.28f, 1f) : Color.white;
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

        textoTimer = CrearTexto("TextoTimerEscapeInicial", canvas.transform, fuente, 26, TextAnchor.UpperRight);
        RectTransform timerRect = textoTimer.rectTransform;
        timerRect.anchorMin = new Vector2(1f, 1f);
        timerRect.anchorMax = new Vector2(1f, 1f);
        timerRect.pivot = new Vector2(1f, 1f);
        timerRect.anchoredPosition = new Vector2(-20f, -18f);
        timerRect.sizeDelta = new Vector2(260f, 42f);
        ActualizarTimer();

        panelFinal = new GameObject("PanelFinEscapeInicial", typeof(RectTransform));
        panelFinal.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = panelFinal.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image fondo = panelFinal.AddComponent<Image>();
        fondo.color = new Color(0f, 0f, 0f, 0.82f);

        textoFinal = CrearTexto("TextoFinEscapeInicial", panelFinal.transform, fuente, 38, TextAnchor.MiddleCenter);
        RectTransform textoRect = textoFinal.rectTransform;
        textoRect.anchorMin = new Vector2(0.5f, 0.5f);
        textoRect.anchorMax = new Vector2(0.5f, 0.5f);
        textoRect.pivot = new Vector2(0.5f, 0.5f);
        textoRect.anchoredPosition = new Vector2(0f, 60f);
        textoRect.sizeDelta = new Vector2(740f, 220f);
        textoFinal.color = new Color(1f, 0.58f, 0.48f, 1f);

        Button botonReiniciar = CrearBoton("BotonReiniciarEscapeInicial", panelFinal.transform, fuente, "Reiniciar", new Color(0.16f, 0.4f, 0.55f, 1f));
        RectTransform botonRect = botonReiniciar.GetComponent<RectTransform>();
        botonRect.anchorMin = new Vector2(0.5f, 0.5f);
        botonRect.anchorMax = new Vector2(0.5f, 0.5f);
        botonRect.pivot = new Vector2(0.5f, 0.5f);
        botonRect.anchoredPosition = new Vector2(0f, -90f);
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
