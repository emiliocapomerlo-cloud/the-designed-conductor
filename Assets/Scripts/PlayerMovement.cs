using UnityEngine;

public partial class PlayerMovement : MonoBehaviour
{
    public float speed = 3.6f;
    public bool controlesInvertidos = false;

    [SerializeField] private float margenColision = 0.03f;

    private Rigidbody2D rb;
    private Collider2D colliderJugador;
    private Vector2 inputDirection;
    private readonly RaycastHit2D[] resultadosColision = new RaycastHit2D[8];
    private ContactFilter2D filtroColisiones;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        colliderJugador = GetComponent<Collider2D>();
        filtroColisiones = new ContactFilter2D();
        filtroColisiones.useTriggers = false;
        filtroColisiones.useLayerMask = false;

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    private void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        Vector2 direccionFinal = new Vector2(moveX, moveY);
        if (controlesInvertidos)
        {
            direccionFinal = -direccionFinal;
        }

        inputDirection = direccionFinal.normalized;
    }

    private void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        MoverConColisiones(inputDirection * speed * Time.fixedDeltaTime);
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

        if (movimiento.sqrMagnitude > 0.000001f)
        {
            rb.MovePosition(rb.position + movimiento);
        }
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
        if (colliderJugador == null || desplazamiento.sqrMagnitude <= 0.000001f)
        {
            return true;
        }

        int impactos = colliderJugador.Cast(
            desplazamiento.normalized,
            filtroColisiones,
            resultadosColision,
            desplazamiento.magnitude + margenColision);

        for (int i = 0; i < impactos; i++)
        {
            Collider2D impacto = resultadosColision[i].collider;
            if (impacto != null && !impacto.isTrigger)
            {
                return false;
            }
        }

        return true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Amigo"))
        {
            return;
        }

        FollowPlayer scriptAmigo = other.GetComponent<FollowPlayer>();
        if (scriptAmigo == null || scriptAmigo.isFollowing)
        {
            return;
        }

        Debug.Log("Encontraste a un integrante del grupo.");
        scriptAmigo.StartFollowing(transform);

        speed -= 0.35f;
        if (speed < 1.8f)
        {
            speed = 1.8f;
        }
    }
}
