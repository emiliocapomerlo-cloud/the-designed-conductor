using UnityEngine;

/// <summary>
/// Manejador de eventos de teclado para decisiones del jugador.
/// Interpreta las acciones del jugador y dispara eventos correspondientes.
/// </summary>
public class PlayerDecisionHandler : MonoBehaviour
{
    [SerializeField] private KeyCode acceptKey = KeyCode.Space;
    [SerializeField] private KeyCode rejectKey = KeyCode.Escape;

    private EventManager eventManager;

    private void Start()
    {
        eventManager = EventManager.Instance;
    }

    private void Update()
    {
        if (Input.GetKeyDown(acceptKey))
        {
            HandlePlayerAccept();
        }

        if (Input.GetKeyDown(rejectKey))
        {
            HandlePlayerReject();
        }
    }

    private void HandlePlayerAccept()
    {
        if (eventManager != null)
        {
            eventManager.RegisterPlayerDecision("accept");
            
            // Efecto específico: si es una bebida, emborracharse
            GlitchManager.Instance?.AccumulateGlitch(3f);
            Debug.Log("[PlayerDecision] Player accepted offer - Glitch accumulated");
        }
    }

    private void HandlePlayerReject()
    {
        if (eventManager != null)
        {
            eventManager.RegisterPlayerDecision("reject");
            Debug.Log("[PlayerDecision] Player rejected offer");
        }
    }
}
