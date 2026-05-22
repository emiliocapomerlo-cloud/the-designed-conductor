using System.Collections;
using UnityEngine;

public class ZonaGlitch : MonoBehaviour
{
    [SerializeField] private float duracionEfecto = 3f; // Cuánto dura el mareo

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Buscamos el componente de movimiento del jugador
            PlayerMovement movimiento = other.GetComponent<PlayerMovement>();

            if (movimiento != null && !movimiento.controlesInvertidos)
            {
                Debug.Log("¡Pisaste un Glitch! Controles invertidos...");
                StartCoroutine(AplicarGlitch(movimiento));
            }
        }
    }

    // Rutina de tiempo para activar y desactivar el efecto sola
    private IEnumerator AplicarGlitch(PlayerMovement mov)
    {
        mov.controlesInvertidos = true;

        // Opcional: Podés cambiar el color del player a rojo temporalmente para dar feedback
        SpriteRenderer renderer = mov.GetComponent<SpriteRenderer>();
        Color colorOriginal = Color.white;
        if (renderer != null)
        {
            colorOriginal = renderer.color;
            renderer.color = Color.red; // Se pone rojo de bug
        }

        // Esperamos los segundos configurados
        yield return new WaitForSeconds(duracionEfecto);

        // Devolvemos todo a la normalidad
        mov.controlesInvertidos = false;
        if (renderer != null)
        {
            renderer.color = colorOriginal;
        }
        Debug.Log("Efecto Glitch terminado. Controles normales.");
    }
}