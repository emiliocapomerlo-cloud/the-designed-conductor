using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ManejadorCervezaAutomatico : MonoBehaviour
{
    [Header("Referencias Opcionales")]
    [SerializeField] private RectTransform referenciaAcompanante;
    [SerializeField] private RectTransform referenciaConductor;
    [SerializeField] private RectTransform referenciaPiso;

    [Header("Animación")]
    [SerializeField] private float duracionAnimacion = 0.75f;
    [SerializeField] private float alturaArco = 130f;
    [SerializeField] private float tiempoCaida = 0.18f;
    
    // Calibración exacta para el piso de tu auto (abajo de todo)
    [SerializeField] private Vector2 coordenadasPisoDefecto = new Vector2(100f, -420f);

    private Canvas canvas;
    private RectTransform contenedor;
    private RectTransform acompananteVisual;
    private RectTransform conductorVisual;
    private RectTransform cervezaFlotante;
    private RectTransform pisoVisual;
    private bool animacionEnCurso;

    private void Start()
    {
        PrepararContenedor();
        ConstruirEscena();
    }

    private void PrepararContenedor()
    {
        if (TryGetComponent(out RectTransform rectTransformActual))
        {
            contenedor = rectTransformActual;
        }
        else
        {
            contenedor = gameObject.AddComponent<RectTransform>();
        }

        contenedor.anchorMin = Vector2.zero;
        contenedor.anchorMax = Vector2.one;
        contenedor.sizeDelta = Vector2.zero;
        contenedor.anchoredPosition = Vector2.zero;
        contenedor.localScale = Vector3.one;

        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = Object.FindFirstObjectByType<Canvas>();
        }

        if (canvas != null && contenedor.parent != canvas.transform)
        {
            contenedor.SetParent(canvas.transform, false);
        }

        contenedor.SetAsLastSibling();
    }

    private void ConstruirEscena()
    {
        CrearAcompanante();
        CrearConductor();
        CrearCervezaFlotante();
        CrearPisoCerveza();
    }

    private void CrearAcompanante()
    {
        // CORRECCIÓN: El acompañante está a la DERECHA en tu dibujo
        Vector2 posicion = referenciaAcompanante != null ? referenciaAcompanante.anchoredPosition : new Vector2(250f, -40f);
        
        // Hacemos el recuadro invisible (Alpha = 0) para que no tape tu dibujo pero reciba el clic
        acompananteVisual = CrearElementoUI("AcompananteCerveza", posicion, new Vector2(140f, 200f), new Color(1f, 1f, 1f, 0f));

        Image imagenAcompanante = acompananteVisual.GetComponent<Image>();
        imagenAcompanante.raycastTarget = true;

        EventTrigger trigger = acompananteVisual.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = acompananteVisual.gameObject.AddComponent<EventTrigger>();
        }

        trigger.triggers.Clear();
        EventTrigger.Entry entradaClick = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerClick
        };
        entradaClick.callback.AddListener(_ => OnAcompananteClick());
        trigger.triggers.Add(entradaClick);
    }

    private void CrearConductor()
    {
        // CORRECCIÓN: El conductor está a la IZQUIERDA en tu dibujo
        Vector2 posicion = referenciaConductor != null ? referenciaConductor.anchoredPosition : new Vector2(-250f, -40f);
        
        // Invisible también
        conductorVisual = CrearElementoUI("ConductorCerveza", posicion, new Vector2(140f, 200f), new Color(1f, 1f, 1f, 0f));
    }

    private void CrearCervezaFlotante()
    {
        GameObject raiz = new GameObject("CervezaFlotante", typeof(RectTransform));
        raiz.transform.SetParent(contenedor, false);
        cervezaFlotante = raiz.GetComponent<RectTransform>();
        cervezaFlotante.anchorMin = new Vector2(0.5f, 0.5f);
        cervezaFlotante.anchorMax = new Vector2(0.5f, 0.5f);
        cervezaFlotante.pivot = new Vector2(0.5f, 0.5f);
        
        // Reducimos un poco el tamaño para que sea una botellita normal
        cervezaFlotante.sizeDelta = new Vector2(30f, 50f);
        cervezaFlotante.localScale = Vector3.one;
        cervezaFlotante.gameObject.SetActive(false);

        CrearParteCerveza(raiz.transform, "Sombra", new Vector2(0f, -4f), new Vector2(32f, 54f), new Color(0f, 0f, 0f, 0.25f));
        CrearParteCerveza(raiz.transform, "Cuerpo", Vector2.zero, new Vector2(26f, 46f), new Color(0.18f, 0.55f, 0.34f, 0.95f)); // Verde porrón
        CrearParteCerveza(raiz.transform, "Tapa", new Vector2(0f, 26f), new Vector2(14f, 8f), new Color(0.9f, 0.7f, 0.1f, 1f));
        CrearParteCerveza(raiz.transform, "Etiqueta", new Vector2(0f, -4f), new Vector2(28f, 16f), new Color(0.9f, 0.9f, 0.9f, 0.95f));
    }

    private void CrearPisoCerveza()
    {
        Vector2 posicion = referenciaPiso != null ? referenciaPiso.anchoredPosition : coordenadasPisoDefecto;
        pisoVisual = CrearElementoUI("PisoCerveza", posicion, new Vector2(16f, 16f), new Color(1f, 1f, 1f, 0f));
        pisoVisual.gameObject.SetActive(false);
    }

    private RectTransform CrearElementoUI(string nombre, Vector2 posicion, Vector2 tamaño, Color color)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(contenedor, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicion;
        rect.sizeDelta = tamaño;
        rect.localScale = Vector3.one;

        Image imagen = go.GetComponent<Image>();
        imagen.color = color;
        imagen.raycastTarget = false;

        return rect;
    }

    private void CrearParteCerveza(Transform padre, string nombre, Vector2 posicion, Vector2 tamaño, Color color)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(padre, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicion;
        rect.sizeDelta = tamaño;
        rect.localScale = Vector3.one;

        Image imagen = go.GetComponent<Image>();
        imagen.color = color;
        imagen.raycastTarget = false;
    }

    private void OnAcompananteClick()
    {
        if (animacionEnCurso || cervezaFlotante == null || acompananteVisual == null || conductorVisual == null)
        {
            return;
        }

        StartCoroutine(AnimarEntregaCerveza());
    }

    private IEnumerator AnimarEntregaCerveza()
    {
        animacionEnCurso = true;
        cervezaFlotante.gameObject.SetActive(true);
        cervezaFlotante.anchoredPosition = acompananteVisual.anchoredPosition;

        Vector2 origen = acompananteVisual.anchoredPosition;
        Vector2 destino = conductorVisual.anchoredPosition;
        Vector2 puntoControl = (origen + destino) * 0.5f + Vector2.up * alturaArco;

        float tiempo = 0f;
        while (tiempo < duracionAnimacion)
        {
            float t = tiempo / duracionAnimacion;
            float suave = t * t * (3f - 2f * t);
            Vector2 posicionInterpolada = Vector2.Lerp(Vector2.Lerp(origen, puntoControl, suave), Vector2.Lerp(puntoControl, destino, suave), suave);
            cervezaFlotante.anchoredPosition = posicionInterpolada;
            tiempo += Time.deltaTime;
            yield return null;
        }

        cervezaFlotante.anchoredPosition = destino;
        yield return new WaitForSeconds(tiempoCaida);
        InstanciarCervezaTirada();
        cervezaFlotante.gameObject.SetActive(false);
        animacionEnCurso = false;
    }

    private void InstanciarCervezaTirada()
    {
        GameObject cervezaTiradaGo = new GameObject("CervezaTirada", typeof(RectTransform));
        cervezaTiradaGo.transform.SetParent(contenedor, false);

        RectTransform rect = cervezaTiradaGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        
        // Destino en el piso corregido
        rect.anchoredPosition = pisoVisual != null ? pisoVisual.anchoredPosition : coordenadasPisoDefecto;
        rect.sizeDelta = new Vector2(50f, 30f); // Invertimos ancho por alto para que parezca acostada
        rect.localScale = Vector3.one;

        // Dibujamos la botellita acostada en el suelo
        CrearParteCerveza(cervezaTiradaGo.transform, "Sombra", new Vector2(0f, -3f), new Vector2(54f, 34f), new Color(0f, 0f, 0f, 0.2f));
        CrearParteCerveza(cervezaTiradaGo.transform, "Cuerpo", new Vector2(-6f, 0f), new Vector2(36f, 24f), new Color(0.18f, 0.55f, 0.34f, 0.95f));
        CrearParteCerveza(cervezaTiradaGo.transform, "Cuello", new Vector2(16f, 0f), new Vector2(14f, 12f), new Color(0.18f, 0.55f, 0.34f, 0.95f));
        CrearParteCerveza(cervezaTiradaGo.transform, "Tapa", new Vector2(24f, 0f), new Vector2(4f, 14f), new Color(0.9f, 0.7f, 0.1f, 1f));
    }
}