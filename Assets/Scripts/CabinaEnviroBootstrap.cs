using UnityEngine;

public static class CabinaEnviroBootstrap
{
    public static void AplicarVisualCabina()
    {
        DesactivarCapasRuntimeViejas();

        RectTransform volante = ObtenerRectTransform("Volante");
        ConfigurarRolPersonaje("Conductor", true, volante);
        ConfigurarRolPersonaje("Acompanante", false, null);
        ConfigurarAsientosPrimerPlano();
        OrdenarAsientosPrimerPlano();
    }

    private static RectTransform ObtenerRectTransform(string nombre)
    {
        GameObject objeto = GameObject.Find(nombre);
        return objeto != null ? objeto.GetComponent<RectTransform>() : null;
    }

    private static void ConfigurarRolPersonaje(string nombre, bool conductor, RectTransform volante)
    {
        GameObject objeto = GameObject.Find(nombre);
        if (objeto == null)
        {
            return;
        }

        PersonajeCabinaVisual visual = objeto.GetComponent<PersonajeCabinaVisual>();
        if (visual == null)
        {
            visual = objeto.AddComponent<PersonajeCabinaVisual>();
        }

        if (conductor)
        {
            visual.ConfigurarComoConductor(volante);
        }
        else
        {
            visual.ConfigurarComoAcompanante();
        }
    }

    private static void DesactivarCapasRuntimeViejas()
    {
        Transform padre = ObtenerPadreCabina();
        if (padre == null)
        {
            return;
        }

        string[] nombres =
        {
            "CabinaRuntimeProcedural",
            "CabinaRuntimePilarIzquierdo",
            "CabinaRuntimePilarIzquierdoSombra",
            "CabinaRuntimePilarDerecho",
            "CabinaRuntimePilarDerechoSombra",
            "CabinaRuntimeTablero",
            "CabinaRuntimeBordeTablero",
            "CabinaRuntimeSombraVolante",
            "CabinaRuntimeAsientoDerecho",
            "CabinaRuntimeConsola",
            "CabinaRuntimeConsolaLuz"
        };

        foreach (string nombre in nombres)
        {
            DesactivarObjeto(padre, nombre);
        }

        GameObject cabina = GameObject.Find("CabinaFondo");
        CabinaEnviroVisuales visualCabina = cabina != null ? cabina.GetComponent<CabinaEnviroVisuales>() : null;
        if (visualCabina != null)
        {
            visualCabina.enabled = true;
        }
    }

    private static void ConfigurarAsientosPrimerPlano()
    {
        Transform padre = ObtenerPadreCabina();
        if (padre == null)
        {
            return;
        }

        ConfigurarAsientoPrimerPlano(
            padre,
            "CabinaRuntimeAsientoConductorFrente",
            new Vector2(-555f, -205f),
            true
        );
        ConfigurarAsientoPrimerPlano(
            padre,
            "CabinaRuntimeAsientoAcompananteFrente",
            new Vector2(440f, -205f),
            false
        );
    }

    private static void ConfigurarAsientoPrimerPlano(Transform padre, string nombre, Vector2 posicion, bool invertido)
    {
        Transform existente = padre.Find(nombre);
        GameObject objeto = existente != null
            ? existente.gameObject
            : new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(AsientoAcompananteVisual));
        objeto.transform.SetParent(padre, false);

        RectTransform rect = objeto.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicion;
        rect.sizeDelta = new Vector2(360f, 370f);
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one;

        AsientoAcompananteVisual visual = objeto.GetComponent<AsientoAcompananteVisual>();
        visual.ConfigurarInvertido(invertido);
        visual.raycastTarget = false;
        visual.enabled = true;
    }

    private static void OrdenarAsientosPrimerPlano()
    {
        GameObject conductor = GameObject.Find("Conductor");
        GameObject acompanante = GameObject.Find("Acompanante");
        GameObject asientoConductor = GameObject.Find("CabinaRuntimeAsientoConductorFrente");
        GameObject asientoAcompanante = GameObject.Find("CabinaRuntimeAsientoAcompananteFrente");

        if (conductor == null || acompanante == null || asientoConductor == null || asientoAcompanante == null)
        {
            return;
        }

        // Los respaldos quedan delante de los cuerpos, sin superar las capas de interfaz.
        int indiceDespuesDePersonajes = Mathf.Max(
            conductor.transform.GetSiblingIndex(),
            acompanante.transform.GetSiblingIndex()
        ) + 1;

        asientoConductor.transform.SetSiblingIndex(indiceDespuesDePersonajes);
        asientoAcompanante.transform.SetSiblingIndex(indiceDespuesDePersonajes + 1);
    }

    private static Transform ObtenerPadreCabina()
    {
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            return canvas.transform;
        }

        GameObject cabina = GameObject.Find("CabinaFondo");
        return cabina != null ? cabina.transform.parent : null;
    }

    private static void DesactivarObjeto(Transform padre, string nombre)
    {
        Transform existente = padre.Find(nombre);
        if (existente != null)
        {
            existente.gameObject.SetActive(false);
        }
    }
}
