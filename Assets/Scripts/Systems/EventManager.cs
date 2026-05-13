using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gestor de eventos dinámicos que dispara consecuencias programadas.
/// Ejemplos: ofertas de bebidas, situaciones de tráfico, cambios de clima, etc.
/// </summary>
public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    [System.Serializable]
    public class GameEvent
    {
        public string eventId;
        public string eventTitle;
        public string eventDescription;
        public float probability; // 0 a 1
        public float triggerTime; // Tiempo en el que se activa
        public System.Action onTrigger;
    }

    [SerializeField] private List<GameEvent> availableEvents = new List<GameEvent>();
    [SerializeField] private float eventCheckInterval = 5f;

    private float eventCheckTimer = 0f;
    private float gameStartTime = 0f;
    private HashSet<string> triggeredEvents = new HashSet<string>();

    public event System.Action<GameEvent> OnEventTriggered;
    public event System.Action<string> OnPlayerDecisionMade;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        gameStartTime = Time.time;
        InitializeDefaultEvents();
    }

    private void Update()
    {
        eventCheckTimer -= Time.deltaTime;
        
        if (eventCheckTimer <= 0)
        {
            CheckForEvents();
            eventCheckTimer = eventCheckInterval;
        }
    }

    private void CheckForEvents()
    {
        foreach (var gameEvent in availableEvents)
        {
            if (!triggeredEvents.Contains(gameEvent.eventId))
            {
                float elapsedTime = Time.time - gameStartTime;
                
                // Verificar si el evento debe dispararse
                if (elapsedTime >= gameEvent.triggerTime)
                {
                    if (Random.value < gameEvent.probability)
                    {
                        TriggerEvent(gameEvent);
                        triggeredEvents.Add(gameEvent.eventId);
                    }
                }
            }
        }
    }

    public void TriggerEvent(GameEvent gameEvent)
    {
        OnEventTriggered?.Invoke(gameEvent);
        gameEvent.onTrigger?.Invoke();
    }

    public void RegisterPlayerDecision(string decisionId)
    {
        OnPlayerDecisionMade?.Invoke(decisionId);
    }

    private void InitializeDefaultEvents()
    {
        availableEvents.Clear();

        // Evento: Ofrecer bebida
        availableEvents.Add(new GameEvent
        {
            eventId = "offer_drink",
            eventTitle = "Oferta de bebida",
            eventDescription = "Un pasajero te ofrece una cerveza",
            probability = 0.3f,
            triggerTime = 10f,
            onTrigger = () => HandleDrinkOffer()
        });

        // Evento: Objeto cayendo en el auto
        availableEvents.Add(new GameEvent
        {
            eventId = "falling_object",
            eventTitle = "¡Objeto cayendo!",
            eventDescription = "Un objeto cae dentro del auto",
            probability = 0.2f,
            triggerTime = 20f,
            onTrigger = () => HandleFallingObject()
        });

        // Evento: Atasco de tráfico
        availableEvents.Add(new GameEvent
        {
            eventId = "traffic_jam",
            eventTitle = "Atasco de tráfico",
            eventDescription = "Te encuentras con tráfico denso",
            probability = 0.4f,
            triggerTime = 30f,
            onTrigger = () => HandleTrafficJam()
        });

        // Evento: Policia persiguiendo
        availableEvents.Add(new GameEvent
        {
            eventId = "police_chase",
            eventTitle = "¡Policía!",
            eventDescription = "La policía te persigue",
            probability = 0.15f,
            triggerTime = 60f,
            onTrigger = () => HandlePoliceChase()
        });
    }

    private void HandleDrinkOffer()
    {
        Debug.Log("[EVENT] Drink offered to player");
        GlitchManager.Instance?.AccumulateGlitch(2f);
    }

    private void HandleFallingObject()
    {
        Debug.Log("[EVENT] Object falling in car");
        PassengerManager.Instance?.CausePassengerChaos(1f);
    }

    private void HandleTrafficJam()
    {
        Debug.Log("[EVENT] Traffic jam encountered");
        VehiclePhysicsController.Instance?.ApplyExternalForce(Vector2.back * 50f);
    }

    private void HandlePoliceChase()
    {
        Debug.Log("[EVENT] Police chase started");
        GlitchManager.Instance?.AccumulateGlitch(5f);
    }
}
