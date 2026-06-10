using System.Collections;
using UnityEngine;

public class ZonaGlitch : MonoBehaviour
{
    private const string RutaSpriteMesa = "Objetos/MesaBebidas";

    [SerializeField] private float duracionEfecto = 3f;
    [SerializeField] private int ordenVisual = -1;

    private GameObject visualMesa;

    private void Awake()
    {
        OcultarBloqueAmarillo();
        CrearVisualMesa();
    }

    private void OnDestroy()
    {
        if (visualMesa != null)
        {
            Destroy(visualMesa);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        PlayerMovement movimiento = other.GetComponent<PlayerMovement>();
        if (movimiento != null && !movimiento.controlesInvertidos)
        {
            Debug.Log("Tomaste una bebida de la mesa. Controles invertidos...");
            StartCoroutine(AplicarGlitch(movimiento));
        }
    }

    private void OcultarBloqueAmarillo()
    {
        Renderer[] renderers = GetComponents<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }
    }

    private void CrearVisualMesa()
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>(RutaSpriteMesa);
        if (sprites.Length == 0)
        {
            Debug.LogWarning("No se encontro el sprite de la mesa de bebidas.");
            return;
        }

        visualMesa = new GameObject(name + "_VisualMesa");
        visualMesa.transform.position = new Vector3(transform.position.x, transform.position.y, -0.1f);

        SpriteRenderer spriteRenderer = visualMesa.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprites[0];
        spriteRenderer.sortingOrder = ordenVisual;
    }

    private IEnumerator AplicarGlitch(PlayerMovement movimiento)
    {
        movimiento.controlesInvertidos = true;

        SpriteRenderer renderer = movimiento.GetComponent<SpriteRenderer>();
        Color colorOriginal = Color.white;
        if (renderer != null)
        {
            colorOriginal = renderer.color;
            renderer.color = Color.red;
        }

        yield return new WaitForSeconds(duracionEfecto);

        movimiento.controlesInvertidos = false;
        if (renderer != null)
        {
            renderer.color = colorOriginal;
        }

        Debug.Log("Efecto de la bebida terminado. Controles normales.");
    }
}
