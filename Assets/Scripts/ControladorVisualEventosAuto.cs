using UnityEngine;
using UnityEngine.UI;

public class ControladorVisualEventosAuto : MonoBehaviour
{
    public static ControladorVisualEventosAuto Instancia { get; private set; }

    [Header("Visuales Calibrados")]
    [SerializeField] private float duracionVisual = 1.45f;
    [SerializeField] private Vector2 posicionPanel = new Vector2(0f, -120f);
    
    // CORRECCIÓN: Posición real en el piso del coche para evitar el velocímetro
    [SerializeField] private Vector2 posicionCervezaPiso = new Vector2(120f, -360f);

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
    private RectTransform contenedorTexto;
    private Text texto;
    private RectTransform contenedorIcono;
    private Image icono;
    private Image overlayFlashImage;
    private RectTransform manoCompanero;
    private Image manoCompaneroImage;
    private RectTransform cervezaMovil;
    private Image cervezaMovilImage;
    private RectTransform cervezaEnCabina;
    private Image cervezaEnCabinaImage;
    private bool cervezaEnCabinaVisible;
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
            if (cervezaEnCabinaVisible && cervezaEnCabina != null)
            {
                // Animación de sutil movimiento en el piso
                cervezaEnCabina.anchoredPosition = new Vector2(posicionCervezaPiso.x, posicionCervezaPiso.y + Mathf.Sin(Time.time * 3.2f) * 4f);
                cervezaEnCabina.localScale = Vector3.one * (0.95f + Mathf.Sin(Time.time * 4f) * 0.04f);
            }
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
        
        if (string.Equals(idEvento, "Cerveza", System.StringComparison.OrdinalIgnoreCase))
        {
            MostrarCervezaEnCabina();
        }
        
        if (panel != null)
        {
            panel.gameObject.SetActive(true);
            panel.SetAsLastSibling(); // Fuerza el frente al activarse
        }

        PrepararEfectoEspecial(idEvento);
        AplicarEstiloEvento(idEvento);
        ActualizarAnimacion();
    }

    public void MostrarCervezaEnCabina()
    {
        cervezaEnCabinaVisible = true;

        if (cervezaEnCabina != null)
        {
            cervezaEnCabina.gameObject.SetActive(true);
            cervezaEnCabina.SetAsLastSibling();
            return;
        }

        if (canvas == null) CrearInterfaz();

        ControladorPaisaje paisaje = FindAnyObjectByType<ControladorPaisaje>();
        Transform padre = (paisaje != null && paisaje.cabina != null) ? paisaje.cabina : canvas.transform;

        cervezaEnCabina = new GameObject("CervezaEnCabina", typeof(RectTransform)).GetComponent<RectTransform>();
        cervezaEnCabina.SetParent(padre, false);
        cervezaEnCabina.anchorMin = new Vector2(0.5f, 0.5f);
        cervezaEnCabina.anchorMax = new Vector2(0.5f, 0.5f);
        cervezaEnCabina.sizeDelta = new Vector2(40f, 65f); // Tamaño pulido
        cervezaEnCabina.anchoredPosition = posicionCervezaPiso; // Ubicación corregida en el suelo
        
        cervezaEnCabinaImage = cervezaEnCabina.gameObject.AddComponent<Image>();
        cervezaEnCabinaImage.color = new Color(0.18f, 0.55f, 0.34f, 0.95f); // Verde porrón
        cervezaEnCabinaImage.raycastTarget = false;
        
        cervezaEnCabina.SetAsLastSibling(); // AL FRENTE DE LA CABINA
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

        // Crear el Overlay del Flash (Celular/Efectos)
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
        
        // CORRECCIÓN CLAVE: El flash debe tapar absolutamente toda la pantalla
        overlayFlashRect.SetAsLastSibling();

        // Crear Panel de Notificación de Eventos
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
        
        panel.SetAsLastSibling(); // Adelante del fondo gris de la cabina
    }

    private void PrepararEfectoEspecial(string id) { }
    private void AplicarEstiloEvento(string id) { }
    
    private void ActualizarAnimacion() 
    {
        // Lógica de Codex para interpolar opacidades/posiciones del flash del celular
        if (eventoActual.Equals("Celular", System.StringComparison.OrdinalIgnoreCase) && overlayFlashImage != null)
        {
            // Hace que la pantalla destelle o se oscurezca al mirar el celular
            float t = tiempoRestante / duracionVisual;
            overlayFlashImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(0f, 0.75f, t)); 
        }
    }

    private void Ocultar()
    {
        if (panel != null) panel.gameObject.SetActive(false);
        if (overlayFlashImage != null) overlayFlashImage.color = new Color(1f, 1f, 1f, 0f);
        eventoActual = "";
    }
}