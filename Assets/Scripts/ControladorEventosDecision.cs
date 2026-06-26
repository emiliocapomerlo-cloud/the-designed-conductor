using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ControladorEventosDecision : MonoBehaviour
{
    private struct EfectoManejo
    {
        public string nombre;
        public float duracion;
        public float multiplicadorGiro;
        public float multiplicadorVibracion;
        public float deriva;
        public float oscilacion;
        public float multiplicadorVelocidad;
        public float multiplicadorLimiteCamino;
        public float respuestaVolante;
        public float pulsoVelocidad;
        public float sacudidaCabina;
    }

    private struct OpcionDecision
    {
        public string texto;
        public EfectoManejo efecto;
        public bool aplicaEfecto;
        public float alcohol;
        public string idVisualEvento;
    }

    private struct EventoDecision
    {
        public string texto;
        public OpcionDecision opcionA;
        public OpcionDecision opcionB;
    }

    [SerializeField] private ControladorPaisaje paisaje;
    [SerializeField] private float demoraPrimerEvento = 2f;
    [SerializeField] private float intervaloEventos = 10f;
    [SerializeField] private float duracionDecision = 6f;
    [SerializeField] private AudioClip musicaCompanero;
    [SerializeField] private string rutaMusicaCompanero = "Audio/MusicaCompanero";
    [SerializeField, Range(0f, 1f)] private float volumenMusicaCompanero = 0.55f;

    private GameObject panelEvento;
    private Text textoEvento;
    private Text textoEstado;
    private Text textoTemporizador;
    private Button botonA;
    private Button botonB;
    private EventoDecision eventoActual;
    private bool esperandoDecision;
    private int indiceEvento;
    private AudioSource audioMusicaCompanero;
    private Coroutine rutinaDetenerMusicaCompanero;
    private bool avisoClipMusicaMostrado;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void IniciarAutomaticamente()
    {
        SceneManager.sceneLoaded -= CrearSiHaceFalta;
        SceneManager.sceneLoaded += CrearSiHaceFalta;
        CrearSiHaceFalta(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private static void CrearSiHaceFalta(Scene scene, LoadSceneMode mode)
    {
        if (FindAnyObjectByType<ControladorEventosDecision>() != null)
        {
            return;
        }

        if (FindAnyObjectByType<ControladorPaisaje>() == null)
        {
            return;
        }

        GameObject controlador = new GameObject("ControladorEventosDecision");
        controlador.AddComponent<ControladorEventosDecision>();
    }

    private void Awake()
    {
        if (paisaje == null)
        {
            paisaje = FindAnyObjectByType<ControladorPaisaje>();
        }

        ConfigurarAudioMusicaCompanero();
        CrearInterfaz();
        OcultarEvento();
    }

    private void Start()
    {
        StartCoroutine(CicloEventos());
    }

    private void Update()
    {
        if (textoEstado == null || paisaje == null)
        {
            return;
        }

        if (paisaje.HayEfectoDecision)
        {
            int segundos = Mathf.CeilToInt(paisaje.SegundosRestantesEfecto);
            textoEstado.text = paisaje.NombreEfectoDecision + ": " + segundos + "s";
            textoEstado.enabled = true;
        }
        else
        {
            textoEstado.enabled = false;
        }
    }

    private IEnumerator CicloEventos()
    {
        yield return new WaitForSeconds(demoraPrimerEvento);

        // Los siete eventos se reparten entre los segundos 2 y 74. No se repiten:
        // asi todos entran en la partida de un minuto y medio, incluso si hay un
        // efecto de manejo activo de una decision anterior.
        EventoDecision[] eventos = CrearEventos();
        for (int i = 0; i < eventos.Length; i++)
        {
            float inicioEvento = Time.time;
            if (paisaje != null)
            {
                yield return MostrarEvento(eventos[i]);
            }

            if (i < eventos.Length - 1)
            {
                float esperaRestante = Mathf.Max(0f, intervaloEventos - (Time.time - inicioEvento));
                yield return new WaitForSeconds(esperaRestante);
            }
        }
    }

    private EventoDecision[] CrearEventos()
    {
        return new EventoDecision[]
        {
            NuevoEvento(
                "Tu amigo te ofrece una cerveza antes de seguir manejando.",
                "Aceptar",
                Efecto("Reflejos alterados", 15f, 1.45f, 2.2f, 105f, 28f, 1f, 1f, 0.9f, 0f, 1f),
                45f,
                "Rechazar",
                SinEfecto(),
                "Cerveza"
            ),
            NuevoEvento(
                "Te llega un mensaje al celular y la pantalla se ilumina.",
                "Leer",
                Efecto("Mirada al celular", 7f, 1f, 0.7f, 0f, 0f, 0.78f, 1f, 0.18f, 0f, 0f),
                0f,
                "No mirar",
                SinEfecto(),
                "Celular"
            ),
            NuevoEvento(
                "Ves un control policial mas adelante.",
                "Bajar velocidad",
                Efecto("Manejo prudente", 8f, 0.65f, 0.3f, 0f, 0f, 0.48f, 1.12f, 1.35f, 0f, 0f),
                0f,
                "Seguir igual",
                Efecto("Nervios", 10f, 1.55f, 1.1f, 120f, 12f, 1.12f, 0.86f, 0.65f, 0.08f, 2f),
                "Policia"
            ),
            NuevoEvento(
                "Tu amigo te ofrece otra cerveza, esta vez con una sonrisa.",
                "Aceptar",
                Efecto("Reflejos alterados", 15f, 1.42f, 2.1f, 98f, 26f, 1f, 1f, 0.92f, 0f, 1f),
                45f,
                "Rechazar",
                SinEfecto(),
                "Cerveza"
            ),
            NuevoEvento(
                "El acompanante sube la musica justo cuando la ruta se angosta.",
                "Aceptar",
                Efecto("Musica fuerte", 12f, 1.05f, 2.8f, 12f, 8f, 1.04f, 0.92f, 1f, 0.18f, 5f),
                0f,
                "Bajar volumen",
                SinEfecto(),
                "Musica"
            ),
            NuevoEvento(
                "Tu acompanante vuelve a subir la musica y te distrae del camino.",
                "Aceptar",
                Efecto("Musica fuerte", 12f, 1.05f, 2.8f, 12f, 8f, 1.04f, 0.92f, 1f, 0.18f, 5f),
                0f,
                "Bajar volumen",
                SinEfecto(),
                "Musica"
            ),
            NuevoEvento(
                "Tu amigo insiste con tomar un atajo de tierra.",
                "Tomar atajo",
                Efecto("Atajo de tierra", 14f, 1.18f, 3.4f, 30f, 10f, 1.28f, 0.62f, 0.85f, 0.32f, 8f),
                0f,
                "Seguir ruta",
                Efecto("Ruta tranquila", 6f, 0.72f, 0.35f, 0f, 0f, 0.78f, 1f, 1.25f, 0f, 0f),
                "Atajo"
            ),
            NuevoEvento(
                "Tu amigo te ofrece una tercera cerveza para seguir la noche.",
                "Aceptar",
                Efecto("Reflejos alterados", 16f, 1.48f, 2.35f, 112f, 30f, 1f, 1f, 0.88f, 0f, 1.2f),
                50f,
                "Rechazar",
                SinEfecto(),
                "Cerveza"
            )
        };
    }

    private EventoDecision NuevoEvento(string texto, string textoA, EfectoManejo efectoA, float alcoholA, string textoB, EfectoManejo efectoB, string idVisualEvento)
    {
        EventoDecision evento = new EventoDecision();
        evento.texto = texto;
        evento.opcionA = NuevaOpcion(textoA, efectoA, alcoholA, idVisualEvento);
        evento.opcionB = NuevaOpcion(textoB, efectoB, 0f, "");
        return evento;
    }

    private OpcionDecision NuevaOpcion(string texto, EfectoManejo efecto, float alcohol, string idVisualEvento)
    {
        OpcionDecision opcion = new OpcionDecision();
        opcion.texto = texto;
        opcion.efecto = efecto;
        opcion.aplicaEfecto = efecto.duracion > 0f;
        opcion.alcohol = alcohol;
        opcion.idVisualEvento = idVisualEvento;
        return opcion;
    }

    private EfectoManejo Efecto(string nombre, float duracion, float giro, float vibracion, float deriva, float oscilacion, float velocidad, float limiteCamino, float respuestaVolante, float pulsoVelocidad, float sacudidaCabina)
    {
        EfectoManejo efecto = new EfectoManejo();
        efecto.nombre = nombre;
        efecto.duracion = duracion;
        efecto.multiplicadorGiro = giro;
        efecto.multiplicadorVibracion = vibracion;
        efecto.deriva = deriva;
        efecto.oscilacion = oscilacion;
        efecto.multiplicadorVelocidad = velocidad;
        efecto.multiplicadorLimiteCamino = limiteCamino;
        efecto.respuestaVolante = respuestaVolante;
        efecto.pulsoVelocidad = pulsoVelocidad;
        efecto.sacudidaCabina = sacudidaCabina;
        return efecto;
    }

    private EfectoManejo SinEfecto()
    {
        return Efecto("", 0f, 1f, 1f, 0f, 0f, 1f, 1f, 1f, 0f, 0f);
    }

    private IEnumerator MostrarEvento(EventoDecision evento)
    {
        if (panelEvento == null || paisaje == null)
        {
            yield break;
        }

        eventoActual = evento;
        esperandoDecision = true;
        panelEvento.SetActive(true);
        textoEvento.text = evento.texto;
        botonA.GetComponentInChildren<Text>().text = evento.opcionA.texto;
        botonB.GetComponentInChildren<Text>().text = evento.opcionB.texto;

        float tiempoRestante = duracionDecision;
        while (esperandoDecision && tiempoRestante > 0f)
        {
            if (textoTemporizador != null)
            {
                textoTemporizador.text = Mathf.CeilToInt(tiempoRestante).ToString();
            }

            tiempoRestante -= Time.deltaTime;
            yield return null;
        }

        if (esperandoDecision)
        {
            ElegirOpcion(eventoActual.opcionB);
        }
    }

    private void ElegirA()
    {
        ElegirOpcion(eventoActual.opcionA);
    }

    private void ElegirB()
    {
        ElegirOpcion(eventoActual.opcionB);
    }

    private void ElegirOpcion(OpcionDecision opcion)
    {
        if (!esperandoDecision)
        {
            return;
        }

        esperandoDecision = false;
        OcultarEvento();

        bool eventoDeMusica = EventoActualEsMusica();
        bool opcionMantieneMusica = string.Equals(opcion.idVisualEvento, "Musica", StringComparison.OrdinalIgnoreCase);

        if (opcionMantieneMusica)
        {
            ReproducirMusicaCompanero(opcion.efecto.duracion, false);
        }
        else if (eventoDeMusica)
        {
            DetenerMusicaCompanero();
        }

        if (opcion.alcohol > 0f && ControladorFinJuego.Instancia != null)
        {
            ControladorFinJuego.Instancia.SumarAlcohol(opcion.alcohol);
        }

        ControladorVisualEventosAuto visualEventos = FindAnyObjectByType<ControladorVisualEventosAuto>();
        if (opcion.alcohol > 0f && string.Equals(opcion.idVisualEvento, "Cerveza", StringComparison.OrdinalIgnoreCase) && visualEventos != null)
        {
            visualEventos.MostrarCervezaEnCabina();
        }

        if (!string.IsNullOrEmpty(opcion.idVisualEvento) && visualEventos != null)
        {
            visualEventos.ActivarEvento(opcion.idVisualEvento);
        }

        if (opcion.aplicaEfecto && paisaje != null)
        {
            EfectoManejo efecto = opcion.efecto;
            paisaje.AplicarEfectoDecision(
                efecto.nombre,
                efecto.duracion,
                efecto.multiplicadorGiro,
                efecto.multiplicadorVibracion,
                efecto.deriva,
                efecto.oscilacion,
                efecto.multiplicadorVelocidad,
                efecto.multiplicadorLimiteCamino,
                efecto.respuestaVolante,
                efecto.pulsoVelocidad,
                efecto.sacudidaCabina
            );
        }
    }

    private void ConfigurarAudioMusicaCompanero()
    {
        if (musicaCompanero == null && !string.IsNullOrEmpty(rutaMusicaCompanero))
        {
            musicaCompanero = Resources.Load<AudioClip>(rutaMusicaCompanero);
        }

        audioMusicaCompanero = GetComponent<AudioSource>();
        if (audioMusicaCompanero == null)
        {
            audioMusicaCompanero = gameObject.AddComponent<AudioSource>();
        }

        audioMusicaCompanero.playOnAwake = false;
        audioMusicaCompanero.loop = true;
        audioMusicaCompanero.spatialBlend = 0f;
        audioMusicaCompanero.volume = volumenMusicaCompanero;
        audioMusicaCompanero.clip = musicaCompanero;
    }

    private void ReproducirMusicaCompanero(float duracion, bool reiniciar)
    {
        if (audioMusicaCompanero == null)
        {
            ConfigurarAudioMusicaCompanero();
        }

        if (musicaCompanero == null && !string.IsNullOrEmpty(rutaMusicaCompanero))
        {
            musicaCompanero = Resources.Load<AudioClip>(rutaMusicaCompanero);
        }

        if (musicaCompanero == null)
        {
            if (!avisoClipMusicaMostrado)
            {
                Debug.LogWarning("No se encontro el audio del evento de musica. Ponelo en Assets/Resources/Audio/MusicaCompanero con extension .mp3, .wav u .ogg.");
                avisoClipMusicaMostrado = true;
            }

            return;
        }

        audioMusicaCompanero.clip = musicaCompanero;
        audioMusicaCompanero.volume = volumenMusicaCompanero;
        audioMusicaCompanero.loop = true;

        if (reiniciar || !audioMusicaCompanero.isPlaying)
        {
            audioMusicaCompanero.Stop();
            audioMusicaCompanero.Play();
        }

        ProgramarDetencionMusicaCompanero(duracion);
    }

    private void ProgramarDetencionMusicaCompanero(float duracion)
    {
        if (rutinaDetenerMusicaCompanero != null)
        {
            StopCoroutine(rutinaDetenerMusicaCompanero);
        }

        rutinaDetenerMusicaCompanero = StartCoroutine(DetenerMusicaCompaneroDespuesDe(duracion));
    }

    private IEnumerator DetenerMusicaCompaneroDespuesDe(float duracion)
    {
        yield return new WaitForSeconds(Mathf.Max(0.1f, duracion));
        rutinaDetenerMusicaCompanero = null;

        if (audioMusicaCompanero != null && audioMusicaCompanero.isPlaying)
        {
            audioMusicaCompanero.Stop();
        }
    }

    private void DetenerMusicaCompanero()
    {
        if (rutinaDetenerMusicaCompanero != null)
        {
            StopCoroutine(rutinaDetenerMusicaCompanero);
            rutinaDetenerMusicaCompanero = null;
        }

        if (audioMusicaCompanero != null && audioMusicaCompanero.isPlaying)
        {
            audioMusicaCompanero.Stop();
        }
    }

    private bool EventoActualEsMusica()
    {
        return EventoEsMusica(eventoActual);
    }

    private bool EventoEsMusica(EventoDecision evento)
    {
        return string.Equals(evento.opcionA.idVisualEvento, "Musica", StringComparison.OrdinalIgnoreCase);
    }

    private void CrearInterfaz()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObjeto = new GameObject("Canvas", typeof(RectTransform));
            canvas = canvasObjeto.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 250;
            canvasObjeto.AddComponent<CanvasScaler>();
            canvasObjeto.AddComponent<GraphicRaycaster>();
        }
        else
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = Mathf.Max(canvas.sortingOrder, 250);
        }

        Font fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (fuente == null)
        {
            fuente = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        panelEvento = new GameObject("PanelEventoDecision", typeof(RectTransform));
        panelEvento.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = panelEvento.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0f, 36f);
        panelRect.sizeDelta = new Vector2(660f, 168f);

        Image fondo = panelEvento.AddComponent<Image>();
        fondo.color = new Color(0.06f, 0.07f, 0.08f, 0.92f);

        textoEvento = CrearTexto("TextoEventoDecision", panelEvento.transform, fuente, 22, TextAnchor.MiddleCenter);
        RectTransform textoRect = textoEvento.rectTransform;
        textoRect.anchorMin = new Vector2(0f, 0.45f);
        textoRect.anchorMax = new Vector2(1f, 1f);
        textoRect.offsetMin = new Vector2(58f, 0f);
        textoRect.offsetMax = new Vector2(-24f, -12f);

        textoTemporizador = CrearTexto("TextoTemporizadorDecision", panelEvento.transform, fuente, 28, TextAnchor.MiddleCenter);
        RectTransform timerRect = textoTemporizador.rectTransform;
        timerRect.anchorMin = new Vector2(0f, 0.45f);
        timerRect.anchorMax = new Vector2(0f, 1f);
        timerRect.pivot = new Vector2(0f, 0.5f);
        timerRect.anchoredPosition = new Vector2(18f, -10f);
        timerRect.sizeDelta = new Vector2(42f, 58f);
        textoTemporizador.color = new Color(1f, 0.82f, 0.25f, 1f);

        botonA = CrearBoton("BotonDecisionA", panelEvento.transform, fuente, "Opcion A", new Color(0.16f, 0.48f, 0.22f, 1f));
        PosicionarBoton(botonA.GetComponent<RectTransform>(), new Vector2(-115f, 24f));
        botonA.onClick.AddListener(ElegirA);

        botonB = CrearBoton("BotonDecisionB", panelEvento.transform, fuente, "Opcion B", new Color(0.45f, 0.16f, 0.14f, 1f));
        PosicionarBoton(botonB.GetComponent<RectTransform>(), new Vector2(115f, 24f));
        botonB.onClick.AddListener(ElegirB);

        textoEstado = CrearTexto("TextoEstadoDecision", canvas.transform, fuente, 20, TextAnchor.MiddleCenter);
        RectTransform estadoRect = textoEstado.rectTransform;
        estadoRect.anchorMin = new Vector2(0.5f, 1f);
        estadoRect.anchorMax = new Vector2(0.5f, 1f);
        estadoRect.pivot = new Vector2(0.5f, 1f);
        estadoRect.anchoredPosition = new Vector2(0f, -24f);
        estadoRect.sizeDelta = new Vector2(420f, 36f);
        textoEstado.color = new Color(1f, 0.82f, 0.25f, 1f);
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
        texto.verticalOverflow = VerticalWrapMode.Truncate;
        return texto;
    }

    private Button CrearBoton(string nombre, Transform padre, Font fuente, string etiqueta, Color color)
    {
        GameObject objeto = new GameObject(nombre, typeof(RectTransform));
        objeto.transform.SetParent(padre, false);
        Image imagen = objeto.AddComponent<Image>();
        imagen.color = color;
        Button boton = objeto.AddComponent<Button>();

        Text texto = CrearTexto("Texto", objeto.transform, fuente, 18, TextAnchor.MiddleCenter);
        texto.text = etiqueta;
        RectTransform textoRect = texto.rectTransform;
        textoRect.anchorMin = Vector2.zero;
        textoRect.anchorMax = Vector2.one;
        textoRect.offsetMin = Vector2.zero;
        textoRect.offsetMax = Vector2.zero;

        return boton;
    }

    private void PosicionarBoton(RectTransform rectTransform, Vector2 posicion)
    {
        rectTransform.anchorMin = new Vector2(0.5f, 0f);
        rectTransform.anchorMax = new Vector2(0.5f, 0f);
        rectTransform.pivot = new Vector2(0.5f, 0f);
        rectTransform.anchoredPosition = posicion;
        rectTransform.sizeDelta = new Vector2(190f, 46f);
    }

    private void OcultarEvento()
    {
        if (panelEvento != null)
        {
            panelEvento.SetActive(false);
        }
    }
}
