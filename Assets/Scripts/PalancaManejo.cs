using UnityEngine;
using UnityEngine.EventSystems;

public class PalancaManejo : MonoBehaviour, IPointerClickHandler
{
    // Esta variable la va a leer el script del paisaje para saber el sentido de la marcha
    [HideInInspector]
    public bool enMarchaAdelante = true; 

    private RectTransform rectTransform;
    
    // Guardamos las posiciones locales en Y para el movimiento visual
    private float posicionDriveY = -300f;
    private float posicionReverseY = -330f;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        // Arranca en Drive por defecto
        ActualizarPosicionVisual();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Conmutamos el estado: si estaba en Drive pasa a Reverse, y viceversa
        enMarchaAdelante = !enMarchaAdelante;

        ActualizarPosicionVisual();
    }

    private void ActualizarPosicionVisual()
    {
        if (enMarchaAdelante)
        {
            // Empujamos la palanca hacia ARRIBA (Drive)
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, posicionDriveY);
            Debug.Log("Palanca en: DRIVE (D) - El auto irá hacia adelante");
        }
        else
        {
            // Tiramos la palanca hacia ABAJO (Reverse)
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, posicionReverseY);
            Debug.Log("Palanca en: REVERSE (R) - El auto irá marcha atrás");
        }
    }
}