using UnityEngine;


public partial class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody2D rb;
    private Vector2 inputDirection;
    public bool controlesInvertidos = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Captura el movimiento de las flechas o WASD
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        inputDirection = new Vector2(moveX, moveY).normalized;
        // Creamos la dirección base
        Vector2 direccionFinal = new Vector2(moveX, moveY);

         // Si el personaje está bajo el efecto del glitch, damos vuelta el vector
         if (controlesInvertidos){
            direccionFinal = -direccionFinal;
        }

        // Finalmente normalizamos para que no camine más rápido en diagonal
        inputDirection = direccionFinal.normalized;
    }

    void FixedUpdate()
    {
        // Aplica el movimiento físico
        rb.MovePosition(rb.position + inputDirection * speed * Time.fixedDeltaTime);
    }
    // Se ejecuta cuando el jugador entra en el área de un Trigger (el amigo)
    private void OnTriggerEnter2D(Collider2D other)
{
    // Ahora preguntamos por el TAG. No importa si se llama Friend1 o Friend99.
    if (other.gameObject.CompareTag("Amigo"))
    {
        FollowPlayer scriptAmigo = other.GetComponent<FollowPlayer>();

        if (scriptAmigo != null && !scriptAmigo.isFollowing)
        {
            Debug.Log("¡Encontraste a un integrante del grupo!");
            scriptAmigo.StartFollowing(this.transform);
            
            // Efecto de motricidad: cada amigo te hace pesar más
            speed -= 0.5f; 
            if (speed < 1.5f) speed = 1.5f; 
        }
    }
}
}