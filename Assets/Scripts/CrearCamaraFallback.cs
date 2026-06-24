using UnityEngine;

public class CrearCamaraFallback : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CrearSiHaceFalta()
    {
        Camera camaraPrincipal = Camera.main;
        if (camaraPrincipal != null && camaraPrincipal.gameObject.activeInHierarchy)
        {
            if (!camaraPrincipal.orthographic)
            {
                camaraPrincipal.orthographic = true;
                camaraPrincipal.orthographicSize = 5f;
            }

            if (!string.Equals(camaraPrincipal.tag, "MainCamera", System.StringComparison.Ordinal))
            {
                camaraPrincipal.tag = "MainCamera";
            }

            return;
        }

        Camera[] camaras = Camera.allCameras;
        foreach (Camera camara in camaras)
        {
            if (camara != null && camara.gameObject.activeInHierarchy)
            {
                camara.tag = "MainCamera";
                if (!camara.orthographic)
                {
                    camara.orthographic = true;
                    camara.orthographicSize = 5f;
                }
                return;
            }
        }

        GameObject objetoCamara = GameObject.Find("Main Camera");
        if (objetoCamara == null)
        {
            objetoCamara = new GameObject("Main Camera");
        }

        Camera nuevaCamara = objetoCamara.GetComponent<Camera>();
        if (nuevaCamara == null)
        {
            nuevaCamara = objetoCamara.AddComponent<Camera>();
        }

        nuevaCamara.tag = "MainCamera";
        nuevaCamara.orthographic = true;
        nuevaCamara.orthographicSize = 5f;
        nuevaCamara.clearFlags = CameraClearFlags.SolidColor;
        nuevaCamara.backgroundColor = Color.black;
        nuevaCamara.depth = -1;
        nuevaCamara.nearClipPlane = 0.3f;
        nuevaCamara.farClipPlane = 1000f;
        nuevaCamara.transform.position = new Vector3(0f, 0f, -10f);
        nuevaCamara.enabled = true;
    }
}
