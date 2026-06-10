using UnityEngine;
using UnityEngine.SceneManagement;

public class ControlarAuto : MonoBehaviour
{
    [SerializeField] private string nombreEscenaManejo = "EscenaManejo";
    [SerializeField] private int amigosRequeridos = 4;
    [SerializeField] private Vector2 tamanoZonaInteraccion = new Vector2(2.2f, 1.1f);

    private void Awake()
    {
        BoxCollider2D zonaInteraccion = GetComponent<BoxCollider2D>();
        if (zonaInteraccion != null)
        {
            zonaInteraccion.isTrigger = true;
            zonaInteraccion.offset = Vector2.zero;
            zonaInteraccion.size = tamanoZonaInteraccion;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        FollowPlayer[] todosLosAmigos = FindObjectsByType<FollowPlayer>(FindObjectsSortMode.None);
        int amigosSiguiendo = 0;

        foreach (FollowPlayer amigo in todosLosAmigos)
        {
            if (amigo.isFollowing)
            {
                amigosSiguiendo++;
            }
        }

        Debug.Log("Intentando subir al auto. Amigos listos: " + amigosSiguiendo + " de " + amigosRequeridos);

        if (amigosSiguiendo >= amigosRequeridos)
        {
            Debug.Log("La banda esta completa. Cargando la escena de manejo...");
            SceneManager.LoadScene(nombreEscenaManejo);
            return;
        }

        int faltantes = amigosRequeridos - amigosSiguiendo;
        Debug.Log("No podes irte todavia. Te faltan " + faltantes + " integrantes del grupo.");
    }
}
