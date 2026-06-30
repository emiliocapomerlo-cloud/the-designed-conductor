using UnityEngine;

public class ManejadorCervezaAutomatico : MonoBehaviour
{
    public void ReproducirEntrega()
    {
        ControladorVisualEventosAuto visual = FindAnyObjectByType<ControladorVisualEventosAuto>();
        if (visual != null)
        {
            visual.ActivarEvento("Cerveza");
        }
    }
}
