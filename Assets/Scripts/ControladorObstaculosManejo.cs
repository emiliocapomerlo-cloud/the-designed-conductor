using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Programa los autos de frente de la escena de manejo y comprueba su impacto.
// Se usa geometria UI porque el manejo se representa dentro de un Canvas.
public class ControladorObstaculosManejo : MonoBehaviour
{
    private const float DuracionAuto = 6f;
    private const float InicioZonaImpacto = 0.45f;
    private const float FinZonaImpacto = 0.96f;
    // Pixeles extra a cada lado de la zona central del jugador. Hace que el
    // impacto sea indulgente visualmente, sin cubrir por completo la ruta.
    private const float MargenImpactoAutoPixeles = 28f;

    // Los tres autos entran y terminan su recorrido antes del final a los 90 s.
    private static readonly float[] TiemposAparicion = { 18f, 46f, 74f };
    // Los autos alternan sus carriles: derecha, izquierda y derecha.
    // Ninguno aparece en el centro, por lo que siempre hay un lateral libre.
    private static readonly float[] CarrilesAutos = { 0.68f, -0.68f, 0.68f };

    private ControladorPaisaje paisaje;
    private ObstaculoManejoVisual[] visuales;
    private float tiempoTranscurrido;
    private bool accidenteRegistrado;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void IniciarAutomaticamente()
    {
        SceneManager.sceneLoaded -= CrearSiHaceFalta;
        SceneManager.sceneLoaded += CrearSiHaceFalta;
        CrearSiHaceFalta(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private static void CrearSiHaceFalta(Scene scene, LoadSceneMode mode)
    {
        if (FindAnyObjectByType<ControladorObstaculosManejo>() != null ||
            FindAnyObjectByType<ControladorPaisaje>() == null)
        {
            return;
        }

        new GameObject("ControladorObstaculosManejo").AddComponent<ControladorObstaculosManejo>();
    }

    private void Awake()
    {
        paisaje = FindAnyObjectByType<ControladorPaisaje>();
        CrearVisuales();
    }

    private void Update()
    {
        if (paisaje == null)
        {
            paisaje = FindAnyObjectByType<ControladorPaisaje>();
            if (paisaje == null)
            {
                return;
            }
        }

        if (visuales == null)
        {
            CrearVisuales();
            if (visuales == null)
            {
                return;
            }
        }

        if (ControladorFinJuego.Instancia != null && ControladorFinJuego.Instancia.PartidaTerminada)
        {
            OcultarTodos();
            return;
        }

        tiempoTranscurrido += Time.deltaTime;
        for (int i = 0; i < TiemposAparicion.Length; i++)
        {
            ActualizarAuto(i);
        }
    }

    private void ActualizarAuto(int indice)
    {
        if (tiempoTranscurrido < TiemposAparicion[indice])
        {
            return;
        }

        float progreso = (tiempoTranscurrido - TiemposAparicion[indice]) / DuracionAuto;
        if (progreso >= 1f)
        {
            visuales[indice].Ocultar();
            return;
        }

        // El auto enemigo conserva su carril. El volante mueve solamente la
        // zona de impacto del jugador, para que el auto no lo siga en espejo.
        visuales[indice].Mostrar(progreso, CarrilesAutos[indice]);

        // La colision usa la misma geometria que el dibujo, no solo el carril.
        // La zona del jugador es deliberadamente ancha para que un auto que se
        // ve centrado choque, pero permite salvarse moviendolo a un lateral.
        if (progreso >= InicioZonaImpacto && progreso <= FinZonaImpacto &&
            visuales[indice].SeSuperponeConZonaJugador(
                MargenImpactoAutoPixeles,
                paisaje.PosicionLateralNormalizada
            ))
        {
            RegistrarAccidente("Chocaste contra un auto de frente.");
        }
    }

    private void RegistrarAccidente(string motivo)
    {
        if (accidenteRegistrado)
        {
            return;
        }

        accidenteRegistrado = true;
        OcultarTodos();

        if (ControladorFinJuego.Instancia != null)
        {
            ControladorFinJuego.Instancia.RegistrarAccidenteGrave(motivo);
        }
        else
        {
            Debug.LogWarning("No se encontro ControladorFinJuego para registrar el accidente: " + motivo);
        }
    }

    private void CrearVisuales()
    {
        if (paisaje == null || paisaje.cabina == null || paisaje.cabina.parent == null)
        {
            return;
        }

        Transform padre = paisaje.cabina.parent;
        visuales = new ObstaculoManejoVisual[TiemposAparicion.Length];
        for (int i = 0; i < visuales.Length; i++)
        {
            GameObject objeto = new GameObject("AutoDeFrente_" + (i + 1), typeof(RectTransform), typeof(CanvasRenderer));
            objeto.transform.SetParent(padre, false);

            RectTransform rect = objeto.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            visuales[i] = objeto.AddComponent<ObstaculoManejoVisual>();
            objeto.transform.SetSiblingIndex(paisaje.cabina.GetSiblingIndex());
            visuales[i].Ocultar();
        }
    }

    private void OcultarTodos()
    {
        if (visuales == null)
        {
            return;
        }

        foreach (ObstaculoManejoVisual visual in visuales)
        {
            if (visual != null)
            {
                visual.Ocultar();
            }
        }
    }
}

// Dibuja un auto de frente que se agranda al acercarse por el parabrisas.
public class ObstaculoManejoVisual : MaskableGraphic
{
    private float progreso;
    private float lateralRelativo;

    protected override void OnEnable()
    {
        base.OnEnable();
        raycastTarget = false;
    }

    public void Mostrar(float nuevoProgreso, float nuevoLateralRelativo)
    {
        progreso = Mathf.Clamp01(nuevoProgreso);
        lateralRelativo = nuevoLateralRelativo;
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        SetVerticesDirty();
    }

    public void Ocultar()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

    public bool SeSuperponeConZonaJugador(float margenExtra, float posicionLateralJugador)
    {
        Rect rectanguloAuto = ObtenerRectanguloAuto();
        Rect zonaJugador = ObtenerZonaJugador(posicionLateralJugador);
        zonaJugador.xMin -= margenExtra;
        zonaJugador.xMax += margenExtra;
        return rectanguloAuto.Overlaps(zonaJugador);
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect rect = GetPixelAdjustedRect();
        CalcularGeometriaAuto(rect, out float x, out float y, out float ancho);
        DibujarAuto(vh, x, y, ancho);
    }

    private Rect ObtenerRectanguloAuto()
    {
        Rect rect = GetPixelAdjustedRect();
        CalcularGeometriaAuto(rect, out float x, out float y, out float ancho);
        float alto = ancho * 0.62f;

        // Incluye carroceria, parabrisas y luces: es el contorno que se ve.
        return Rect.MinMaxRect(
            x - ancho * 0.5f,
            y - alto * 0.36f,
            x + ancho * 0.5f,
            y + alto * 0.52f
        );
    }

    private Rect ObtenerZonaJugador(float posicionLateralJugador)
    {
        Rect rect = GetPixelAdjustedRect();
        float ancho = rect.width * 0.10f;
        float yMin = Mathf.Lerp(rect.yMin, rect.yMax, 0.50f);
        float yMax = Mathf.Lerp(rect.yMin, rect.yMax, 0.67f);
        float yNormalizada = Mathf.Lerp(0.74f, 0.49f, progreso);
        float profundidadCamino = Mathf.InverseLerp(0f, 0.74f, yNormalizada);
        float mitadCamino = Mathf.Lerp(rect.width * 0.41f, rect.width * 0.06f, profundidadCamino);
        float xJugador = rect.center.x + Mathf.Clamp(posicionLateralJugador, -1f, 1f) * mitadCamino;
        return Rect.MinMaxRect(xJugador - ancho * 0.5f, yMin, xJugador + ancho * 0.5f, yMax);
    }

    private void CalcularGeometriaAuto(Rect rect, out float x, out float y, out float ancho)
    {
        float yNormalizada = Mathf.Lerp(0.74f, 0.49f, progreso);
        y = Mathf.Lerp(rect.yMin, rect.yMax, yNormalizada);
        float profundidadCamino = Mathf.InverseLerp(0f, 0.74f, yNormalizada);
        float mitadCamino = Mathf.Lerp(rect.width * 0.41f, rect.width * 0.06f, profundidadCamino);
        x = rect.center.x + lateralRelativo * mitadCamino;
        float escala = Mathf.Lerp(0.12f, 1f, Mathf.SmoothStep(0f, 1f, progreso));

        // Cada mitad del camino es un carril. Al limitar el auto a una fraccion
        // de ese ancho, queda claramente de un solo lado de la linea central.
        float anchoPorEscala = rect.width * 0.25f * escala;
        float anchoMaximoPorCarril = mitadCamino * 0.82f;
        ancho = Mathf.Min(anchoPorEscala, anchoMaximoPorCarril);
    }

    private void DibujarAuto(VertexHelper vh, float x, float y, float ancho)
    {
        float alto = ancho * 0.62f;
        Color carroceria = new Color(0.72f, 0.12f, 0.1f, 1f);
        Color carroceriaOscura = new Color(0.36f, 0.045f, 0.035f, 1f);
        Color vidrio = new Color(0.48f, 0.78f, 0.88f, 1f);

        AddQuad(vh, new Vector2(x - ancho * 0.5f, y - alto * 0.36f), new Vector2(x + ancho * 0.5f, y - alto * 0.36f), new Vector2(x + ancho * 0.4f, y + alto * 0.3f), new Vector2(x - ancho * 0.4f, y + alto * 0.3f), carroceria);
        AddQuad(vh, new Vector2(x - ancho * 0.3f, y + alto * 0.28f), new Vector2(x + ancho * 0.3f, y + alto * 0.28f), new Vector2(x + ancho * 0.2f, y + alto * 0.52f), new Vector2(x - ancho * 0.2f, y + alto * 0.52f), carroceriaOscura);
        AddQuad(vh, new Vector2(x - ancho * 0.18f, y + alto * 0.3f), new Vector2(x + ancho * 0.18f, y + alto * 0.3f), new Vector2(x + ancho * 0.12f, y + alto * 0.46f), new Vector2(x - ancho * 0.12f, y + alto * 0.46f), vidrio);
        AddQuad(vh, new Vector2(x - ancho * 0.4f, y - alto * 0.12f), new Vector2(x + ancho * 0.4f, y - alto * 0.12f), new Vector2(x + ancho * 0.34f, y + alto * 0.02f), new Vector2(x - ancho * 0.34f, y + alto * 0.02f), carroceriaOscura);
        AddQuad(vh, new Vector2(x - ancho * 0.36f, y - alto * 0.24f), new Vector2(x - ancho * 0.18f, y - alto * 0.24f), new Vector2(x - ancho * 0.18f, y - alto * 0.08f), new Vector2(x - ancho * 0.36f, y - alto * 0.08f), new Color(1f, 0.92f, 0.55f, 1f));
        AddQuad(vh, new Vector2(x + ancho * 0.18f, y - alto * 0.24f), new Vector2(x + ancho * 0.36f, y - alto * 0.24f), new Vector2(x + ancho * 0.36f, y - alto * 0.08f), new Vector2(x + ancho * 0.18f, y - alto * 0.08f), new Color(1f, 0.92f, 0.55f, 1f));
    }

    private void AddQuad(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color colorQuad)
    {
        int inicio = vh.currentVertCount;
        vh.AddVert(a, colorQuad, Vector2.zero);
        vh.AddVert(b, colorQuad, Vector2.zero);
        vh.AddVert(c, colorQuad, Vector2.zero);
        vh.AddVert(d, colorQuad, Vector2.zero);
        vh.AddTriangle(inicio, inicio + 1, inicio + 2);
        vh.AddTriangle(inicio, inicio + 2, inicio + 3);
    }
}
