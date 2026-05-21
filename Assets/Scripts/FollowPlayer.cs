using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform target; // A quién debe seguir (el Player)
    public float speed = 3f;  // Velocidad de seguimiento
    public float stoppingDistance = 1.2f; // Distancia para no chocar al Player
    public bool isFollowing = false; // Solo se mueve si fue "tocado"

    void Update()
    {
        if (isFollowing && target != null)
        {
            float distance = Vector2.Distance(transform.position, target.position);

            if (distance > stoppingDistance)
            {
                // Se mueve hacia el jugador
                transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
            }
        }
    }

    public void StartFollowing(Transform playerTransform)
    {
        target = playerTransform;
        isFollowing = true;
        // Quitamos el collider para que no trabe al jugador mientras lo sigue
        GetComponent<Collider2D>().enabled = false; 
    }
}