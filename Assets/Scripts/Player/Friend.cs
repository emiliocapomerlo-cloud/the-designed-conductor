using UnityEngine;

/// <summary>
/// Representa a un amigo en la Fase 1 que puede ser recolectado.
/// </summary>
public class Friend : MonoBehaviour
{
    [SerializeField] private string friendName;
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite collectedSprite;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;

    private bool isCollected = false;
    private Vector2 wanderDirection = Vector2.zero;
    private float wanderChangeTimer = 0f;
    private float wanderChangeInterval = 3f;
    private float wanderSpeed = 0.5f;

    private void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
        
        SetRandomWanderDirection();
    }

    private void Update()
    {
        if (!isCollected)
        {
            Wander();
        }
    }

    private void Wander()
    {
        wanderChangeTimer -= Time.deltaTime;
        if (wanderChangeTimer <= 0)
        {
            SetRandomWanderDirection();
            wanderChangeTimer = wanderChangeInterval;
        }

        if (rb != null)
        {
            rb.velocity = wanderDirection * wanderSpeed;
        }

        if (animator != null)
        {
            animator.SetFloat("moveX", wanderDirection.x);
            animator.SetFloat("moveY", wanderDirection.y);
        }
    }

    private void SetRandomWanderDirection()
    {
        wanderDirection = Random.insideUnitCircle.normalized;
    }

    public void Interact()
    {
        if (!isCollected)
        {
            Collect();
        }
    }

    public void Collect()
    {
        isCollected = true;
        rb.velocity = Vector2.zero;
        
        if (animator != null)
        {
            animator.SetBool("collected", true);
        }

        GetComponent<Collider2D>().enabled = false;
        GameManager.Instance?.CollectFriend(friendName);
    }

    public bool IsCollected() => isCollected;
    public string GetFriendName() => friendName;
}
