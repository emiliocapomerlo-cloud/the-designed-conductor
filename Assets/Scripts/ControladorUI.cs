using UnityEngine;
using TMPro; // Clave para poder controlar TextMeshPro por código

public class ControladorUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textoContador;
    [SerializeField] private int totalAmigos = 4;

    void Update()
    {
        // Buscamos cuántos amigos están siguiendo actualmente
        FollowPlayer[] todosLosAmigos = FindObjectsByType<FollowPlayer>(FindObjectsSortMode.None);
        int amigosSiguiendo = 0;

        foreach (FollowPlayer amigo in todosLosAmigos)
        {
            if (amigo.isFollowing)
            {
                amigosSiguiendo++;
            }
        }

        // Actualizamos el texto en pantalla con los datos reales
        textoContador.text = "Integrantes: " + amigosSiguiendo + " / " + totalAmigos;

        // Opcional: Si ya tenés a los 4, ponemos el texto en verde
        if (amigosSiguiendo >= totalAmigos)
        {
            textoContador.color = Color.green;
        }
        else
        {
            textoContador.color = Color.white;
        }
    }
}