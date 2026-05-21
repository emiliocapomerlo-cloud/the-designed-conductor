using UnityEngine;
using UnityEngine.SceneManagement;

public class ControlarAuto : MonoBehaviour
{
    [SerializeField] private string nombreEscenaManejo = "EscenaManejo";
    [SerializeField] private int amigosRequeridos = 4;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Si el que toca el auto es el jugador
        if (other.CompareTag("Player"))
        {
            // Buscamos todos los scripts 'FollowPlayer' que haya en la escena
            FollowPlayer[] todosLosAmigos = FindObjectsByType<FollowPlayer>(FindObjectsSortMode.None);
            
            int amigosSiguiendo = 0;

            // Recorremos cada amigo y chequeamos tu variable 'isFollowing'
            foreach (FollowPlayer amigo in todosLosAmigos)
            {
                if (amigo.isFollowing) 
                {
                    amigosSiguiendo++;
                }
            }

            Debug.Log("Intentando subir al auto. Amigos listos: " + amigosSiguiendo + " de " + amigosRequeridos);

            // Si están los 4, avanzamos de fase
            if (amigosSiguiendo >= amigosRequeridos)
            {
                Debug.Log("¡La banda está completa! Cargando la escena de manejo...");
                SceneManager.LoadScene(nombreEscenaManejo);
            }
            else
            {
                int faltantes = amigosRequeridos - amigosSiguiendo;
                Debug.Log("No podés irte todavía. Te faltan " + faltantes + " integrantes del grupo.");
            }
        }
    }
}