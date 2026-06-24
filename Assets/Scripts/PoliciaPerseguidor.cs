using UnityEngine;
using UnityEngine.SceneManagement;

public class PoliciaPerseguidor : MonoBehaviour
{
    [SerializeField] private float multiplicadorVelocidadJugador = 2f / 3f;
    [SerializeField] private float velocidadFallback = 1f;
    [SerializeField] private float distanciaCaptura = 0.72f;
    [SerializeField] private float demoraAntesDePerseguir = 1.5f;
    [SerializeField] private Vector2 tamanoCuerpo = new Vector2(0.78f, 0.92f);

    private Transform objetivo;
    private PlayerMovement movimientoJugador;
    private Collider2D colliderPolicia;
    private BoxCollider2D cajaPolicia;
    private SpriteRenderer spritePrincipal;
    private float tiempoInicio;
    private bool capturoJugador;
    private readonly Collider2D[] resultadosBloqueo = new Collider2D[12];

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void IniciarAutomaticamente()
    {
        SceneManager.sceneLoaded -= PrepararPolicia;
        SceneManager.sceneLoaded += PrepararPolicia;
        PrepararPolicia(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private static void PrepararPolicia(Scene scene, LoadSceneMode mode)
    {
        if (FindAnyObjectByType<ControladorPaisaje>() != null)
        {
            return;
        }

        GameObject policia = GameObject.Find("Policia");
        if (policia == null || policia.GetComponent<PoliciaPerseguidor>() != null)
        {
            return;
        }

        policia.AddComponent<PoliciaPerseguidor>();
    }

    private void Awake()
    {
        tiempoInicio = Time.time;
        objetivo = BuscarJugador();
        movimientoJugador = objetivo != null ? objetivo.GetComponent<PlayerMovement>() : null;
        gameObject.tag = "Untagged";

        FollowPlayer seguidorDeAmigo = GetComponent<FollowPlayer>();
        if (seguidorDeAmigo != null)
        {
            Destroy(seguidorDeAmigo);
        }

        colliderPolicia = GetComponent<Collider2D>();
        if (colliderPolicia != null)
        {
            // El policia no debe comportarse como un obstaculo fisico: al tocar al
            // jugador, la captura termina la partida. Si fuera solido, ambos
            // colliders se empujan y el jugador puede terminar fuera del mapa.
            colliderPolicia.isTrigger = true;
        }
        cajaPolicia = colliderPolicia as BoxCollider2D;

        AplicarSkinPolicia();
    }

    private void Update()
    {
        if (capturoJugador || Time.time - tiempoInicio < demoraAntesDePerseguir)
        {
            return;
        }

        if (objetivo == null)
        {
            objetivo = BuscarJugador();
            movimientoJugador = objetivo != null ? objetivo.GetComponent<PlayerMovement>() : null;
            if (objetivo == null)
            {
                return;
            }
        }

        Vector3 posicionObjetivo = objetivo.position;
        posicionObjetivo.z = transform.position.z;

        Vector2 actual = transform.position;
        Vector2 siguiente = Vector2.MoveTowards(actual, posicionObjetivo, ObtenerVelocidadPersecucion() * Time.deltaTime);
        MoverConColisiones(siguiente - actual);

        Vector3 direccion = posicionObjetivo - transform.position;
        if (direccion.sqrMagnitude > 0.0001f)
        {
            spritePrincipal.flipX = direccion.x < 0f;
        }

        if (Vector2.Distance(transform.position, objetivo.position) <= distanciaCaptura)
        {
            CapturarJugador();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!capturoJugador && other.CompareTag("Player"))
        {
            CapturarJugador();
        }
    }

    private void CapturarJugador()
    {
        capturoJugador = true;
        ControladorEscapeInicial.TerminarPorPolicia();
    }

    private static Transform BuscarJugador()
    {
        GameObject jugador = GameObject.FindGameObjectWithTag("Player");
        return jugador != null ? jugador.transform : null;
    }

    private float ObtenerVelocidadPersecucion()
    {
        if (movimientoJugador == null && objetivo != null)
        {
            movimientoJugador = objetivo.GetComponent<PlayerMovement>();
        }

        if (movimientoJugador == null)
        {
            return velocidadFallback;
        }

        return Mathf.Max(0.1f, movimientoJugador.speed * multiplicadorVelocidadJugador);
    }

    private void MoverConColisiones(Vector2 desplazamiento)
    {
        if (desplazamiento.sqrMagnitude <= 0.000001f)
        {
            return;
        }

        Vector2 movimiento = Vector2.zero;
        if (PuedeMover(desplazamiento))
        {
            movimiento = desplazamiento;
        }
        else
        {
            movimiento = CalcularDeslizamiento(desplazamiento);
        }

        transform.position += new Vector3(movimiento.x, movimiento.y, 0f);
    }

    private Vector2 CalcularDeslizamiento(Vector2 desplazamiento)
    {
        Vector2 movimientoX = new Vector2(desplazamiento.x, 0f);
        Vector2 movimientoY = new Vector2(0f, desplazamiento.y);
        bool probarXPrimero = Mathf.Abs(desplazamiento.x) >= Mathf.Abs(desplazamiento.y);

        Vector2 primero = probarXPrimero ? movimientoX : movimientoY;
        Vector2 segundo = probarXPrimero ? movimientoY : movimientoX;

        if (PuedeMover(primero))
        {
            return primero;
        }

        return PuedeMover(segundo) ? segundo : Vector2.zero;
    }

    private bool PuedeMover(Vector2 desplazamiento)
    {
        if (desplazamiento.sqrMagnitude <= 0.000001f)
        {
            return true;
        }

        Vector2 posicionCandidata = (Vector2)transform.position + desplazamiento;
        Vector2 centroCaja = posicionCandidata;
        Vector2 tamanoCaja = tamanoCuerpo;

        if (cajaPolicia != null)
        {
            Vector2 escala = new Vector2(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y));
            centroCaja += Vector2.Scale(cajaPolicia.offset, escala);
            tamanoCaja = Vector2.Scale(cajaPolicia.size, escala);
        }

        int impactos = Physics2D.OverlapBoxNonAlloc(centroCaja, tamanoCaja, 0f, resultadosBloqueo);

        for (int i = 0; i < impactos; i++)
        {
            Collider2D impacto = resultadosBloqueo[i];
            if (impacto == null ||
                impacto == colliderPolicia ||
                impacto.isTrigger ||
                impacto.transform == transform ||
                (objetivo != null && impacto.transform == objetivo))
            {
                continue;
            }

            return false;
        }

        return true;
    }

    private void AplicarSkinPolicia()
    {
        spritePrincipal = GetComponent<SpriteRenderer>();
        if (spritePrincipal == null)
        {
            spritePrincipal = gameObject.AddComponent<SpriteRenderer>();
        }

        spritePrincipal.color = new Color(0.08f, 0.18f, 0.42f, 1f);
        int ordenBase = spritePrincipal.sortingOrder;

        CrearPiezaSkin("GorraPolicia", new Vector2(0f, 0.55f), new Vector2(0.72f, 0.2f), new Color(0.02f, 0.08f, 0.24f, 1f), ordenBase + 2);
        CrearPiezaSkin("ViseraPolicia", new Vector2(0.14f, 0.48f), new Vector2(0.36f, 0.09f), new Color(0.01f, 0.04f, 0.12f, 1f), ordenBase + 3);
        CrearPiezaSkin("InsigniaPolicia", new Vector2(-0.18f, 0.1f), new Vector2(0.16f, 0.16f), new Color(1f, 0.82f, 0.18f, 1f), ordenBase + 3);
        CrearPiezaSkin("CinturonPolicia", new Vector2(0f, -0.18f), new Vector2(0.62f, 0.09f), new Color(0.02f, 0.02f, 0.025f, 1f), ordenBase + 3);
    }

    private void CrearPiezaSkin(string nombre, Vector2 posicionLocal, Vector2 escala, Color color, int sortingOrder)
    {
        Transform existente = transform.Find(nombre);
        if (existente != null)
        {
            Destroy(existente.gameObject);
        }

        GameObject pieza = new GameObject(nombre);
        pieza.transform.SetParent(transform, false);
        pieza.transform.localPosition = new Vector3(posicionLocal.x, posicionLocal.y, -0.02f);
        pieza.transform.localScale = new Vector3(escala.x, escala.y, 1f);

        SpriteRenderer renderer = pieza.AddComponent<SpriteRenderer>();
        renderer.sprite = SpriteCuadrado;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
    }

    private static Sprite spriteCuadrado;
    private static Sprite SpriteCuadrado
    {
        get
        {
            if (spriteCuadrado != null)
            {
                return spriteCuadrado;
            }

            Texture2D textura = new Texture2D(8, 8);
            Color[] pixeles = new Color[64];
            for (int i = 0; i < pixeles.Length; i++)
            {
                pixeles[i] = Color.white;
            }

            textura.SetPixels(pixeles);
            textura.Apply();
            spriteCuadrado = Sprite.Create(textura, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 8f);
            return spriteCuadrado;
        }
    }
}
