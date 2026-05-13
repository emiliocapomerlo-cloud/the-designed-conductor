using UnityEngine;

/// <summary>
/// Controlador del jugador en la Fase 1 (Vista Superior - Top Down).
/// Maneja el movimiento del jugador en el estacionamiento.
/// </summary>
public class PlayerTopDownController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float interactionDistance = 1.5f;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;
    private Vector2 moveDirection = Vector2.zero;
    private Vector2 lastFacingDirection = Vector2.down;
    private bool isMoving = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void Update()
    {
        HandleInput();
        UpdateAnimation();
        CheckForInteractions();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void HandleInput()
    {
        moveDirection = Vector2.zero;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            moveDirection += Vector2.up;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            moveDirection += Vector2.down;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            moveDirection += Vector2.left;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            moveDirection += Vector2.right;

        moveDirection.Normalize();

        if (moveDirection != Vector2.zero)
        {
            lastFacingDirection = moveDirection;
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }

        // Flip sprite según dirección
        if (moveDirection.x != 0)
        {
            spriteRenderer.flipX = moveDirection.x < 0;
        }
    }

    private void Move()
    {
        rb.velocity = moveDirection * moveSpeed;
    }

    private void UpdateAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("isMoving", isMoving);
            animator.SetFloat("moveX", moveDirection.x);
            animator.SetFloat("moveY", moveDirection.y);
        }
    }

    private void CheckForInteractions()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactionDistance);
            
            foreach (Collider2D collider in colliders)
            {
                Friend friend = collider.GetComponent<Friend>();
                if (friend != null && !friend.IsCollected())
                {
                    friend.Interact();
                }
            }
        }
    }

    public Vector2 GetFacingDirection() => lastFacingDirection;
    public bool IsMoving() => isMoving;
}
