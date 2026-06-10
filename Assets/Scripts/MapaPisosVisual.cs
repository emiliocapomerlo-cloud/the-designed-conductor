using UnityEngine;
using UnityEngine.SceneManagement;

public static class MapaPisosVisual
{
    private const string NombreEscena = "SampleScene";
    private const string NombreContenedor = "PisosMapaVisuales";
    private const string RutaMadera = "Pisos/PisoMadera";
    private const string RutaEstacionamiento = "Pisos/PisoEstacionamiento";
    private const float LimiteExteriorPredeterminado = 4.2f;
    private const float MargenCamara = 1f;
    private const float AspectoMaximoCubierto = 4f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CrearPisos()
    {
        if (SceneManager.GetActiveScene().name != NombreEscena ||
            GameObject.Find(NombreContenedor) != null)
        {
            return;
        }

        Camera camara = Camera.main;
        if (camara == null || !camara.orthographic)
        {
            Debug.LogWarning("No se pudo crear el piso: falta una camara ortografica.");
            return;
        }

        Sprite madera = Resources.Load<Sprite>(RutaMadera);
        Sprite estacionamiento = Resources.Load<Sprite>(RutaEstacionamiento);
        if (madera == null || estacionamiento == null)
        {
            Debug.LogWarning("No se encontraron las texturas de los pisos.");
            return;
        }

        OcultarSueloOriginal();

        float alto = camara.orthographicSize * 2f + MargenCamara * 2f;
        // Al cargar la escena, Unity puede informar temporalmente un aspecto 1:1.
        // Usamos una cobertura amplia para que nunca asome el fondo cuadriculado.
        float aspectoCubierto = Mathf.Max(camara.aspect, AspectoMaximoCubierto);
        float semiancho = camara.orthographicSize * aspectoCubierto;
        float izquierda = camara.transform.position.x - semiancho - MargenCamara;
        float derecha = camara.transform.position.x + semiancho + MargenCamara;
        float centroY = camara.transform.position.y;
        float limiteExterior = DetectarLimiteExterior();

        limiteExterior = Mathf.Clamp(limiteExterior, izquierda + 1f, derecha - 1f);

        GameObject contenedor = new GameObject(NombreContenedor);
        CrearZona(
            contenedor.transform,
            "PisoSalonMadera",
            madera,
            izquierda,
            limiteExterior,
            centroY,
            alto);
        CrearZona(
            contenedor.transform,
            "PisoEstacionamiento",
            estacionamiento,
            limiteExterior,
            derecha,
            centroY,
            alto);
    }

    private static void OcultarSueloOriginal()
    {
        Renderer[] renderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        foreach (Renderer renderer in renderers)
        {
            string nombreObjeto = renderer.gameObject.name;
            if (nombreObjeto == "SueloAsfalto" ||
                nombreObjeto == "SueloVereda" ||
                nombreObjeto.StartsWith("SueloVereda ("))
            {
                renderer.enabled = false;
            }
        }
    }

    private static float DetectarLimiteExterior()
    {
        float limite = LimiteExteriorPredeterminado;
        bool encontroMuro = false;

        Transform[] objetos = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
        foreach (Transform objeto in objetos)
        {
            if (!objeto.name.StartsWith("ObstaculoGlitch") ||
                Mathf.Abs(objeto.lossyScale.y) <= Mathf.Abs(objeto.lossyScale.x) * 1.2f)
            {
                continue;
            }

            if (!encontroMuro || objeto.position.x > limite)
            {
                limite = objeto.position.x;
                encontroMuro = true;
            }
        }

        return limite;
    }

    private static void CrearZona(
        Transform padre,
        string nombre,
        Sprite sprite,
        float bordeIzquierdo,
        float bordeDerecho,
        float centroY,
        float alto)
    {
        float ancho = bordeDerecho - bordeIzquierdo;
        GameObject zona = new GameObject(nombre);
        zona.transform.SetParent(padre);
        zona.transform.position = new Vector3((bordeIzquierdo + bordeDerecho) * 0.5f, centroY, 0.2f);

        SpriteRenderer renderer = zona.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.drawMode = SpriteDrawMode.Tiled;
        renderer.size = new Vector2(ancho, alto);
        renderer.sortingOrder = -100;
    }
}
