using UnityEngine;
using UnityEngine.EventSystems;

// Usamos las interfaces nativas de Unity para arrastrar elementos de UI
public class VolanteManejo : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    private float anguloBase = 0f;

    public void OnPointerDown(PointerEventData eventData)
    {
        // Detecta el ángulo inicial entre el mouse y el centro del volante al hacer clic
        Vector2 direccion = eventData.position - (Vector2)transform.position;
        anguloBase = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg - transform.eulerAngles.z;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Mientras arrastrás con el mouse, el volante rota
        Vector2 direccion = eventData.position - (Vector2)transform.position;
        float anguloMouse = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, anguloMouse - anguloBase);
    }
}