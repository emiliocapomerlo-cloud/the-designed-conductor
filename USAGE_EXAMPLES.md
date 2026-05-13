# Ejemplos de Uso - The Designated Driver

## Ejemplos de Integración de Scripts

### 1. Cómo Crear un Evento Personalizado

```csharp
// En EventManager o en una clase especializada
public class CustomEventCreator
{
    public static void CreateRainEvent()
    {
        var rainEvent = new EventManager.GameEvent
        {
            eventId = "heavy_rain",
            eventTitle = "Lluvia Torrencial",
            eventDescription = "¡La lluvia hace que sea difícil ver! La visibilidad se reduce.",
            probability = 0.25f,
            triggerTime = 45f,
            onTrigger = () => 
            {
                // Aumentar glitch por lluvia
                GlitchManager.Instance?.AccumulateGlitch(2.5f);
                Debug.Log("[Event] Heavy rain started!");
            }
        };
        
        EventManager.Instance?.TriggerEvent(rainEvent);
    }
}
```

### 2. Alternar Dificultad en Tiempo Real

```csharp
// Script para cambiar dificultad con teclas
public class DifficultyToggle : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
            DifficultyManager.Instance?.SetDifficulty(DifficultyManager.Difficulty.Easy);
        if (Input.GetKeyDown(KeyCode.F2))
            DifficultyManager.Instance?.SetDifficulty(DifficultyManager.Difficulty.Normal);
        if (Input.GetKeyDown(KeyCode.F3))
            DifficultyManager.Instance?.SetDifficulty(DifficultyManager.Difficulty.Hard);
    }
}
```

### 3. Emborrachar a un Pasajero Específico

```csharp
// Hacer que el primer pasajero esté borracho
public class PassengerInteraction : MonoBehaviour
{
    public void MakeFirstPassengerDrunk()
    {
        PassengerManager passengerManager = PassengerManager.Instance;
        if (passengerManager != null)
        {
            // Parámetro 0 = primer pasajero
            passengerManager.MakePassengerDrunk(0);
            Debug.Log("First passenger is now drunk!");
        }
    }
}
```

### 4. Escuchar Eventos de Glitch

```csharp
// Script que reacciona a cambios de glitch
public class GlitchReactor : MonoBehaviour
{
    private void Start()
    {
        GlitchManager glitchManager = GlitchManager.Instance;
        if (glitchManager != null)
        {
            glitchManager.OnGlitchIntensityChanged += HandleGlitchChange;
            glitchManager.OnControlsInverted += HandleControlInversion;
        }
    }

    private void HandleGlitchChange(float intensity)
    {
        if (intensity > 0.66f)
        {
            Debug.Log("WARNING: Severe glitch!");
            // Mostrar efecto visual
            // Reproducir sonido de alerta
        }
    }

    private void HandleControlInversion(bool inverted)
    {
        Debug.Log(inverted ? "Controls INVERTED!" : "Controls normal");
        // Cambiar color de UI para avisar al jugador
    }

    private void OnDestroy()
    {
        GlitchManager glitchManager = GlitchManager.Instance;
        if (glitchManager != null)
        {
            glitchManager.OnGlitchIntensityChanged -= HandleGlitchChange;
            glitchManager.OnControlsInverted -= HandleControlInversion;
        }
    }
}
```

### 5. Sistemas de Guardado/Carga (Extensión)

```csharp
// Sistema de guardado para el futuro
[System.Serializable]
public class GameSaveData
{
    public int friendsCollected;
    public float gameTime;
    public float glitchIntensity;
    public bool[] passengersInCar;
    
    public static void Save(string filename)
    {
        GameSaveData data = new GameSaveData
        {
            friendsCollected = GameManager.Instance.GetCollectedFriendsCount(),
            gameTime = GameManager.Instance.GetElapsedTime(),
            glitchIntensity = GlitchManager.Instance.GetCurrentGlitchIntensity(),
        };
        
        string json = JsonUtility.ToJson(data);
        System.IO.File.WriteAllText(filename, json);
    }
}
```

### 6. Crear Evento de Decisión Interactiva

```csharp
// Evento que permite decisiones del jugador
public class DecisionEvent
{
    public static void ShowOfferBeerEvent()
    {
        var beerEvent = new EventManager.GameEvent
        {
            eventId = "beer_offer",
            eventTitle = "¿Una cervecita?",
            eventDescription = "Tu amigo ofrece una cerveza. ¿Aceptas?",
            probability = 1f, // Garantizado para demo
            triggerTime = 5f,
            onTrigger = () =>
            {
                // El evento se muestra mediante EventDialogHandler
                // El jugador puede aceptar (ESPACIO) o rechazar (ESC)
            }
        };

        EventManager.Instance?.TriggerEvent(beerEvent);
        
        // Escuchar la decisión
        EventManager eventManager = EventManager.Instance;
        if (eventManager != null)
        {
            eventManager.OnPlayerDecisionMade += (decision) =>
            {
                if (decision == "accept")
                {
                    Debug.Log("Player accepted beer!");
                    // Efecto: embriaguez
                    PassengerManager.Instance?.MakePassengerDrunk(0);
                    GlitchManager.Instance?.AccumulateGlitch(3f);
                }
                else
                {
                    Debug.Log("Player refused beer");
                }
            };
        }
    }
}
```

### 7. Monitorear Estadísticas en Tiempo Real

```csharp
// Script para mostrar debug info
public class GameStatistics : MonoBehaviour
{
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        
        GUILayout.Label("=== Game Statistics ===");
        
        if (GameManager.Instance != null)
        {
            GUILayout.Label($"Phase: {GameManager.Instance.GetCurrentPhase()}");
            GUILayout.Label($"Friends: {GameManager.Instance.GetCollectedFriendsCount()}/{GameManager.Instance.GetTotalFriendsNeeded()}");
            GUILayout.Label($"Time: {GameManager.Instance.GetElapsedTime():F1}s");
        }
        
        if (GlitchManager.Instance != null)
        {
            GUILayout.Label($"Glitch: {GlitchManager.Instance.GetCurrentGlitchIntensity():P0}");
            GUILayout.Label($"Vision Blur: {GlitchManager.Instance.GetCurrentVisionBlur():F2}");
        }
        
        if (VehiclePhysicsController.Instance != null)
        {
            GUILayout.Label($"Speed: {VehiclePhysicsController.Instance.GetCurrentSpeed():F1}");
            GUILayout.Label($"Gear: {VehiclePhysicsController.Instance.GetCurrentGear()}");
            GUILayout.Label($"Wheel Rot: {VehiclePhysicsController.Instance.GetWheelRotation():F1}°");
        }
        
        if (PassengerManager.Instance != null)
        {
            GUILayout.Label($"Chaos Level: {PassengerManager.Instance.GetTotalChaosLevel():P0}");
        }
        
        GUILayout.EndArea();
    }
}
```

### 8. Aplicar Fuerza Física al Vehículo (Obstáculos)

```csharp
// Colisión con obstáculo que afecta el vehículo
public class ObstacleCollider : MonoBehaviour
{
    [SerializeField] private Vector2 impactForce = new Vector2(0, -30);
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<VehiclePhysicsController>() != null)
        {
            VehiclePhysicsController vehicle = collision.gameObject.GetComponent<VehiclePhysicsController>();
            vehicle.ApplyExternalForce(impactForce);
            
            Debug.Log("Vehicle hit obstacle!");
            
            // Aumentar glitch por impacto
            GlitchManager.Instance?.AccumulateGlitch(1f);
        }
    }
}
```

### 9. Crear Secuencia de Eventos Encadenados

```csharp
// Eventos que desencadenan otros eventos
public class EventChain
{
    public static void StartWeatherSequence()
    {
        // Evento 1: Lluvia
        EventManager.Instance?.TriggerEvent(new EventManager.GameEvent
        {
            eventId = "rain",
            eventTitle = "Comienza a llover",
            eventDescription = "La visibilidad disminuye",
            probability = 1f,
            triggerTime = 30f,
            onTrigger = () =>
            {
                GlitchManager.Instance?.AccumulateGlitch(1.5f);
                // Después de 10 segundos, trueno
                StartCoroutine(TriggerThunderAfterDelay(10f));
            }
        });
    }
    
    private static System.Collections.IEnumerator TriggerThunderAfterDelay(float delay)
    {
        yield return new System.Collections.WaitForSeconds(delay);
        
        EventManager.Instance?.TriggerEvent(new EventManager.GameEvent
        {
            eventId = "thunder",
            eventTitle = "¡TRUENO!",
            eventDescription = "Un relámpago cegador",
            probability = 1f,
            triggerTime = 0f,
            onTrigger = () =>
            {
                GlitchManager.Instance?.AccumulateGlitch(3f);
                // Efecto sonoro de trueno
            }
        });
    }
}
```

### 10. Crear Mapa Dinámico con Eventos Locales

```csharp
// Zonas de riesgo que activan eventos
public class DangerZone : MonoBehaviour
{
    [SerializeField] private string zoneId;
    [SerializeField] private Vector2 zoneCenter;
    [SerializeField] private float zoneRadius = 10f;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<VehiclePhysicsController>() != null)
        {
            TriggerZoneEvent();
        }
    }
    
    private void TriggerZoneEvent()
    {
        switch(zoneId)
        {
            case "construction":
                EventManager.Instance?.TriggerEvent(new EventManager.GameEvent
                {
                    eventId = "construction_zone",
                    eventTitle = "¡Obra en construcción!",
                    eventDescription = "Debes navegar alrededor de conos",
                    probability = 1f,
                    triggerTime = 0f,
                    onTrigger = () => PassengerManager.Instance?.CausePassengerChaos(0.5f)
                });
                break;
                
            case "pothole":
                // Efecto de bache en la carretera
                VehiclePhysicsController.Instance?.ApplyExternalForce(Vector2.up * 20f);
                GlitchManager.Instance?.AccumulateGlitch(0.5f);
                break;
        }
    }
}
```

## Tips de Debugging

### Modo Debug Visual
```csharp
// Habilitar en Update para ver información en pantalla
private void OnGUI()
{
    if (GUILayout.Button("Trigger Random Event"))
        EventManager.Instance?.TriggerEvent(/* */);
        
    if (GUILayout.Button("Add Glitch"))
        GlitchManager.Instance?.AccumulateGlitch(1f);
        
    if (GUILayout.Button("Make Passenger Drunk"))
        PassengerManager.Instance?.MakePassengerDrunk(0);
}
```

### Verificar Conexión de Eventos
```csharp
// En Start() de cualquier manager
private void Start()
{
    Debug.Log($"[Manager] Initialization check:");
    Debug.Log($"  GameManager: {(GameManager.Instance != null ? "OK" : "MISSING")}");
    Debug.Log($"  GlitchManager: {(GlitchManager.Instance != null ? "OK" : "MISSING")}");
    Debug.Log($"  EventManager: {(EventManager.Instance != null ? "OK" : "MISSING")}");
}
```

---

**¡Estos ejemplos te permiten personalizar y extender el juego a tu gusto!**
