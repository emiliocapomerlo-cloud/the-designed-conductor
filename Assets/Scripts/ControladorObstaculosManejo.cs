using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Obstaculos de manejo hechos con UI estandar. Cada auto enemigo es un
// RectTransform con Image; la profundidad se simula moviendo Y y escalando.
public class ControladorObstaculosManejo : MonoBehaviour
{
    private const float DuracionAuto = 6f;

    private static readonly float[] TiemposAparicion = { 12f, 26f, 40f, 56f, 72f };
    private static readonly int[] CarrilesDisponibles = { -1, 0, 1 };

    [SerializeField] private ControladorPaisaje paisaje;
    [SerializeField] private RectTransform zonaImpactoJugador;
    [SerializeField] private Vector2 tamanoBaseAuto = new Vector2(108f, 150f);
    [SerializeField] private Vector2 tamanoZonaImpacto = new Vector2(170f, 96f);
    [SerializeField] private float escalaInicial = 0.22f;
    [SerializeField] private float escalaFinal = 2.75f;
    [SerializeField] private float posicionYInicioNormalizada = 0.26f;
    [SerializeField] private float posicionYImpactoNormalizada = -0.18f;
    [SerializeField] private float separacionCarrilesLejos = 0.08f;
    [SerializeField] private float separacionCarrilesCerca = 0.32f;
    [SerializeField] private float factorCompensacionJugador = 1f;

    private RectTransform capaAutos;
    private AutoEnemigoUI[] autos;
    private float tiempoTranscurrido;
    private bool accidenteRegistrado;
    private readonly Vector3[] esquinasAuto = new Vector3[4];
    private readonly Vector3[] esquinasZona = new Vector3[4];

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
        if (paisaje == null)
        {
            paisaje = FindAnyObjectByType<ControladorPaisaje>();
        }

        CrearInterfaz();
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

        if (capaAutos == null || autos == null)
        {
            CrearInterfaz();
            if (capaAutos == null || autos == null)
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
        for (int i = 0; i < autos.Length; i++)
        {
            ActualizarAuto(i);
        }
    }

    private void ActualizarAuto(int indice)
    {
        AutoEnemigoUI auto = autos[indice];
        float tiempoDesdeAparicion = tiempoTranscurrido - TiemposAparicion[indice];

        if (tiempoDesdeAparicion < 0f)
        {
            return;
        }

        if (!auto.CarrilAsignado)
        {
            auto.Carril = CarrilesDisponibles[Random.Range(0, CarrilesDisponibles.Length)];
            auto.CarrilAsignado = true;
        }

        float progreso = tiempoDesdeAparicion / DuracionAuto;
        if (progreso >= 1f)
        {
            auto.Ocultar();
            return;
        }

        ActualizarTransformAuto(auto, progreso);

        if (!accidenteRegistrado && RectsSeSuperponen(auto.RectTransform, zonaImpactoJugador))
        {
            RegistrarAccidente("Chocaste contra un auto de frente.");
        }
    }

    private void ActualizarTransformAuto(AutoEnemigoUI auto, float progreso)
    {
        Rect rectCapa = ObtenerRectCapa();
        float profundidad = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progreso));
        float yInicio = rectCapa.height * posicionYInicioNormalizada;
        float yImpacto = ObtenerYImpacto(rectCapa);
        float y = Mathf.Lerp(yInicio, yImpacto, profundidad);
        float separacionCarril = rectCapa.width * Mathf.Lerp(separacionCarrilesLejos, separacionCarrilesCerca, profundidad);

        // El carril del auto existe en el camino. La posicion del jugador se
        // resta para compensar el desplazamiento del paisaje y permitir esquive.
        float jugador = paisaje != null ? paisaje.PosicionLateralNormalizada : 0f;
        float x = (auto.Carril - jugador * factorCompensacionJugador) * separacionCarril;
        float escala = Mathf.Lerp(escalaInicial, escalaFinal, profundidad);

        auto.RectTransform.anchoredPosition = new Vector2(x, y);
        auto.RectTransform.localScale = new Vector3(escala, escala, 1f);
        auto.Mostrar();
    }

    private Rect ObtenerRectCapa()
    {
        Rect rect = capaAutos.rect;
        if (rect.width < 1f || rect.height < 1f)
        {
            return new Rect(0f, 0f, Screen.width, Screen.height);
        }

        return rect;
    }

    private float ObtenerYImpacto(Rect rectCapa)
    {
        if (zonaImpactoJugador == null)
        {
            return rectCapa.height * posicionYImpactoNormalizada;
        }

        Vector3 centroMundo = zonaImpactoJugador.TransformPoint(zonaImpactoJugador.rect.center);
        Vector3 centroLocal = capaAutos.InverseTransformPoint(centroMundo);
        return centroLocal.y;
    }

    private bool RectsSeSuperponen(RectTransform a, RectTransform b)
    {
        if (a == null || b == null || !a.gameObject.activeInHierarchy || !b.gameObject.activeInHierarchy)
        {
            return false;
        }

        Rect rectA = ObtenerRectMundo(a, esquinasAuto);
        Rect rectB = ObtenerRectMundo(b, esquinasZona);
        return rectA.Overlaps(rectB);
    }

    private static Rect ObtenerRectMundo(RectTransform rectTransform, Vector3[] esquinas)
    {
        rectTransform.GetWorldCorners(esquinas);
        float xMin = esquinas[0].x;
        float yMin = esquinas[0].y;
        float xMax = esquinas[2].x;
        float yMax = esquinas[2].y;
        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
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

    private void CrearInterfaz()
    {
        if (paisaje == null || paisaje.cabina == null || paisaje.cabina.parent == null)
        {
            return;
        }

        RectTransform padre = paisaje.cabina.parent as RectTransform;
        if (padre == null)
        {
            return;
        }

        capaAutos = CrearCapaAutos(padre);
        if (zonaImpactoJugador == null)
        {
            zonaImpactoJugador = BuscarZonaImpactoExistente(padre);
        }

        if (zonaImpactoJugador == null)
        {
            zonaImpactoJugador = CrearZonaImpacto(capaAutos);
        }

        autos = new AutoEnemigoUI[TiemposAparicion.Length];
        for (int i = 0; i < autos.Length; i++)
        {
            autos[i] = CrearAuto("AutoDeFrente_" + (i + 1), capaAutos);
            autos[i].Ocultar();
        }
    }

    private RectTransform CrearCapaAutos(RectTransform padre)
    {
        GameObject objeto = new GameObject("CapaAutosEnemigosUI", typeof(RectTransform));
        objeto.transform.SetParent(padre, false);

        RectTransform rect = objeto.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);

        objeto.transform.SetSiblingIndex(paisaje.cabina.GetSiblingIndex());
        return rect;
    }

    private RectTransform BuscarZonaImpactoExistente(RectTransform padre)
    {
        Transform existente = padre.Find("ZonaImpactoJugador");
        if (existente == null)
        {
            return null;
        }

        return existente as RectTransform;
    }

    private RectTransform CrearZonaImpacto(RectTransform padre)
    {
        GameObject objeto = new GameObject("ZonaImpactoJugador", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        objeto.transform.SetParent(padre, false);

        RectTransform rect = objeto.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = tamanoZonaImpacto;

        Rect rectCapa = ObtenerRectCapa();
        rect.anchoredPosition = new Vector2(0f, rectCapa.height * posicionYImpactoNormalizada);

        Image imagen = objeto.GetComponent<Image>();
        imagen.color = new Color(1f, 0f, 0f, 0f);
        imagen.raycastTarget = false;
        return rect;
    }

    private AutoEnemigoUI CrearAuto(string nombre, RectTransform padre)
    {
        GameObject objeto = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        objeto.transform.SetParent(padre, false);

        RectTransform rect = objeto.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = tamanoBaseAuto;

        Image carroceria = objeto.GetComponent<Image>();
        carroceria.color = new Color(0.72f, 0.10f, 0.08f, 1f);
        carroceria.raycastTarget = false;

        CrearParteAuto("Parabrisas", rect, new Vector2(0.25f, 0.58f), new Vector2(0.75f, 0.86f), new Color(0.48f, 0.78f, 0.88f, 1f));
        CrearParteAuto("Parrilla", rect, new Vector2(0.16f, 0.28f), new Vector2(0.84f, 0.44f), new Color(0.28f, 0.03f, 0.03f, 1f));
        CrearParteAuto("LuzIzquierda", rect, new Vector2(0.10f, 0.12f), new Vector2(0.34f, 0.26f), new Color(1f, 0.92f, 0.55f, 1f));
        CrearParteAuto("LuzDerecha", rect, new Vector2(0.66f, 0.12f), new Vector2(0.90f, 0.26f), new Color(1f, 0.92f, 0.55f, 1f));

        return new AutoEnemigoUI(rect);
    }

    private void CrearParteAuto(string nombre, RectTransform padre, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        GameObject objeto = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        objeto.transform.SetParent(padre, false);

        RectTransform rect = objeto.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image imagen = objeto.GetComponent<Image>();
        imagen.color = color;
        imagen.raycastTarget = false;
    }

    private void OcultarTodos()
    {
        if (autos == null)
        {
            return;
        }

        foreach (AutoEnemigoUI auto in autos)
        {
            if (auto != null)
            {
                auto.Ocultar();
            }
        }
    }

    private class AutoEnemigoUI
    {
        public readonly RectTransform RectTransform;
        public int Carril;
        public bool CarrilAsignado;

        public AutoEnemigoUI(RectTransform rectTransform)
        {
            RectTransform = rectTransform;
        }

        public void Mostrar()
        {
            if (!RectTransform.gameObject.activeSelf)
            {
                RectTransform.gameObject.SetActive(true);
            }
        }

        public void Ocultar()
        {
            if (RectTransform.gameObject.activeSelf)
            {
                RectTransform.gameObject.SetActive(false);
            }
        }
    }
}
