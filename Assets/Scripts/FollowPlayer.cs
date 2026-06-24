using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform target;
    public float speed = 5.8f;
    public float stoppingDistance = 1.2f;
    public bool isFollowing = false;

    [SerializeField] private float amplitudBaile = 0.12f;
    [SerializeField] private float frecuenciaBaile = 7f;
    [SerializeField] private float margenColision = 0.03f;

    private Vector3 posicionBase;
    private Collider2D colliderSeguidor;
    private readonly RaycastHit2D[] resultadosColision = new RaycastHit2D[8];
    private ContactFilter2D filtroColisiones;

    private void Start()
    {
        posicionBase = transform.position;
        colliderSeguidor = GetComponent<Collider2D>();
        filtroColisiones = new ContactFilter2D();
        filtroColisiones.useTriggers = false;
        filtroColisiones.useLayerMask = false;
    }

    private void Update()
    {
        if (isFollowing && target != null)
        {
            float distancia = Vector2.Distance(transform.position, target.position);
            if (distancia > stoppingDistance)
            {
                Vector2 actual = transform.position;
                Vector2 siguiente = Vector2.MoveTowards(actual, target.position, speed * Time.deltaTime);
                MoverConColisiones(siguiente - actual);
            }
        }
        else
        {
            float oscilacionX = Mathf.Sin(Time.time * frecuenciaBaile + transform.GetInstanceID() * 0.17f) * amplitudBaile;
            float oscilacionY = Mathf.Cos(Time.time * (frecuenciaBaile * 1.15f) + transform.GetInstanceID() * 0.11f) * amplitudBaile * 0.75f;
            Vector3 destino = posicionBase + new Vector3(oscilacionX, oscilacionY, 0f);
            Vector2 movimientoIdle = Vector3.Lerp(transform.position, destino, 0.18f) - transform.position;
            MoverConColisiones(movimientoIdle);
        }

        float giro = Mathf.Sin(Time.time * frecuenciaBaile * 0.9f + transform.GetInstanceID() * 0.23f) * 8f;
        transform.rotation = Quaternion.Euler(0f, 0f, giro);
    }

    public void StartFollowing(Transform playerTransform)
    {
        target = playerTransform;
        isFollowing = true;

        colliderSeguidor = GetComponent<Collider2D>();
        if (colliderSeguidor != null)
        {
            colliderSeguidor.enabled = true;
            colliderSeguidor.isTrigger = true;
        }
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
        if (colliderSeguidor == null || desplazamiento.sqrMagnitude <= 0.000001f)
        {
            return true;
        }

        int impactos = colliderSeguidor.Cast(
            desplazamiento.normalized,
            filtroColisiones,
            resultadosColision,
            desplazamiento.magnitude + margenColision);

        for (int i = 0; i < impactos; i++)
        {
            Collider2D impacto = resultadosColision[i].collider;
            if (impacto == null || impacto.isTrigger || (target != null && impacto.transform == target))
            {
                continue;
            }

            return false;
        }

        return true;
    }
}
