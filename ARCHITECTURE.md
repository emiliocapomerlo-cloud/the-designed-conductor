# Arquitectura del Sistema - The Designated Driver

## Diagrama de Flujo General

```
┌─────────────────────────────────────────────────┐
│           GAME INITIALIZATION                    │
│    (Scene Load → Managers Instantiate)           │
└───────────────────┬─────────────────────────────┘
                    │
        ┌───────────┴───────────┐
        │                       │
   ┌────▼─────┐          ┌─────▼────┐
   │ Phase 1   │   ────▶ │ Phase 2   │
   │Collection │          │ Driving   │
   └──────────┘          └──────────┘
```

## Arquitectura de Componentes

### Núcleo del Juego (Game Core)

```
GameManager (Singleton)
├── Mantiene estado global
├── Controla transiciones de fase
├── Maneja tiempo límite
└── Dispara eventos de fase
```

### Sistemas de Alteración

```
GlitchManager (Singleton)
├── Acumula glitch de eventos
├── Invierte controles temporalmente
├── Altera visión (blur)
└── Retrasa tiempo de reacción

PassengerManager (Singleton)
├── Mantiene lista de pasajeros
├── Genera movimiento caótico
├── Simula embriaguez
└── Afecta física del vehículo
```

### Gestión de Eventos

```
EventManager (Singleton)
├── Almacena eventos programados
├── Verifica disparo de eventos
├── Registra decisiones del jugador
└── Ejecuta callbacks de eventos
```

### Fase 1: Recolección

```
Phase1CollectionController
├── PlayerTopDownController
│   ├── Maneja movimiento (WASD)
│   ├── Detecta colisiones
│   └── Interacciona con Friends (SPACE)
│
└── Friend (múltiples instancias)
    ├── Comportamiento de paseo
    ├── Detección de interacción
    └── Notificación a GameManager
```

### Fase 2: Conducción

```
Phase2DrivingController
├── VehiclePhysicsController
│   ├── Rigidbody2D (física real)
│   ├── Cálculo de aceleración
│   ├── Giro del volante con memoria
│   └── Cambio de marchas
│
├── Phase2InputHandler
│   ├── Verifica estado de glitch
│   ├── Invierte input si es necesario
│   └── Aplica retrasos de reacción
│
└── Pasajeros (desde PassengerManager)
    ├── Se mueven dentro del auto
    └── Afectan movimiento del vehículo
```

### Sistemas Visuales y Audio

```
CameraGlitchEffect
├── Aplica shader de glitch
├── Modifica FOV
└── Invierte pantalla si es necesario

AudioManager (Singleton)
├── Reprodución de música
├── Efectos de sonido
└── Distorsión por glitch
```

### UI

```
UIManager (Singleton)
├── Actualización de valores
└── Manejo de colores/advertencias

EventDialogHandler
├── Muestra diálogos de eventos
├── Botones de decisión
└── Registra decisiones
```

## Patrones de Comunicación

### Patrón de Singleton
```csharp
public static GameManager Instance { get; private set; }

// Asegurar una sola instancia
if (Instance != null) Destroy(gameObject);
Instance = this;
DontDestroyOnLoad(gameObject);
```

### Patrón de Eventos (Event Broadcasting)
```csharp
// Publicador
public event System.Action<float> OnGlitchIntensityChanged;
OnGlitchIntensityChanged?.Invoke(intensity);

// Suscriptor
glitchManager.OnGlitchIntensityChanged += HandleGlitchChange;
```

## Flujo de Control Completo

### Inicialización
```
1. Scene Load
2. GameManager.Awake() → Singleton setup
3. Otros Managers.Awake() → Singletons setup
4. GameManager.Start() → Inicializar amigos
5. Managers.Start() → Suscribirse a eventos
6. Ready para input
```

### Fase 1: Recolección
```
Input del Jugador
    ↓
PlayerTopDownController.HandleInput()
    ↓
Move() / Interactuar con Friend
    ↓
Friend.Collect() (si SPACE y cerca)
    ↓
GameManager.CollectFriend()
    ↓
Friend count actualiza UI
    ↓
¿Todos recolectados?
    → Sí: Permitir ENTER para Fase 2
    → No: Continuar esperando
```

### Fase 2: Conducción
```
Event Check (cada 5s)
    ↓
¿Evento debe dispararse?
    → Sí: EventManager.TriggerEvent()
        ├── Ejecutar callback del evento
        ├── GlitchManager.AccumulateGlitch()
        ├── PassengerManager effects
        └── EventDialogHandler muestra diálogo
    → No: Continuar
    ↓
Input del Jugador (cada frame)
    ↓
Phase2InputHandler.GetInput()
    ├── Verifica si controles invertidos
    └── Aplica retraso de reacción
    ↓
VehiclePhysicsController.ApplyPhysics()
    ├── Calcula fuerzas
    ├── Aplica fricción
    ├── Gira en base a volante
    └── Actualiza Rigidbody
    ↓
PassengerManager.UpdatePassengers()
    ├── Reduce caos naturalmente
    └── Aplica movimiento caótico
    ↓
GlitchManager.Update()
    ├── Reduce intensidad naturalmente
    └── Actualiza nivel de glitch
    ↓
Render
    ├── CameraGlitchEffect aplica shader
    ├── Phase2DrivingController actualiza UI
    └── AudioManager aplica distorsión
    ↓
¿Tiempo agotado?
    → Sí: GameManager.EndGame(false) → Game Over
    → No: Continuar
```

## Flujo de Decisiones

```
EventManager detecta evento
    ↓
EventManager.OnEventTriggered dispara
    ↓
EventDialogHandler muestra diálogo UI
    ↓
Jugador presiona SPACE (aceptar) o ESC (rechazar)
    ↓
EventManager.RegisterPlayerDecision(decisión)
    ↓
EventManager.OnPlayerDecisionMade dispara
    ↓
Consecuencias según decisión:
- "accept_drink" → PassengerManager.MakePassengerDrunk() → GlitchManager.AccumulateGlitch()
- "reject_danger" → Sin efecto negativo
- etc.
```

## Interacciones entre Sistemas

### Glitch afecta todo
```
GlitchManager.AccumulateGlitch()
    ├── VisionBlur → CameraGlitchEffect
    ├── ControlInversion → Phase2InputHandler
    ├── ReactionDelay → Phase2InputHandler
    └── AudioDistortion → AudioManager
```

### Eventos desencadenan caos
```
EventManager.TriggerEvent()
    ├── "falling_object" → PassengerManager.CausePassengerChaos()
    ├── "traffic_jam" → VehiclePhysicsController.ApplyExternalForce()
    ├── "beer_offer" → GlitchManager.AccumulateGlitch()
    └── "police_chase" → GlitchManager.AccumulateGlitch(severo)
```

### Pasajeros afectan vehículo
```
PassengerManager.UpdatePassengers()
    └── ApplyChaosMovement()
        └── AddForce() al Rigidbody del vehículo
```

## Capas de Abstracción

```
┌─────────────────────────────────────┐
│         Presentation Layer          │
│  (UI, Canvas, RawImages, Sprites)   │
└────────────────┬────────────────────┘
                 │
┌────────────────▼────────────────────┐
│        Controller Layer              │
│ (Phase1/2Controllers, InputHandlers) │
└────────────────┬────────────────────┘
                 │
┌────────────────▼────────────────────┐
│         System Layer                │
│ (GlitchManager, EventManager, etc)  │
└────────────────┬────────────────────┘
                 │
┌────────────────▼────────────────────┐
│         Physics Layer                │
│ (Rigidbody2D, Colliders, Transforms) │
└─────────────────────────────────────┘
```

## Dependencias entre Scripts

```
VehiclePhysicsController
    └── Independiente (requiere Rigidbody2D)

Phase2InputHandler
    └── Depende de: GlitchManager

Phase2DrivingController
    └── Depende de: VehiclePhysicsController, GameManager, GlitchManager

CameraGlitchEffect
    └── Depende de: GlitchManager

PassengerManager
    └── Independiente

GlitchManager
    └── Independiente

EventManager
    └── Depende de: GlitchManager, PassengerManager, VehiclePhysicsController

GameManager
    └── Independiente

AudioManager
    └── Depende de: GlitchManager
```

## Configuración Recomendada de Orden de Ejecución

```
Script Execution Order (en Project Settings):
1. GameManager.cs (-200)
2. GlitchManager.cs (-100)
3. EventManager.cs (-100)
4. PassengerManager.cs (-100)
5. AudioManager.cs (-100)
6. Phase2InputHandler.cs (0)
7. VehiclePhysicsController.cs (100)
8. UIManager.cs (200)
9. Otros scripts (default 0)
```

## State Transitions

### Game States
```
START
    ↓
MENU → [Play Button]
    ↓
PHASE1_COLLECTION
    ├─ CollectingFriends...
    └─ [All Collected] → AllFriendsInCar
        ↓
        [Press ENTER]
        ↓
PHASE2_DRIVING
    ├─ Driving with time limit...
    └─ [Time Up] → GAME_OVER (Lost)
            ↓
            [Reach destination with friends] → VICTORY
                ↓
                [View Results]
                    ↓
                [Play Again / Quit]
```

## Memory Management

```
Singleton Objects (Persistent):
- GameManager (DontDestroyOnLoad)
- GlitchManager (DontDestroyOnLoad)
- EventManager (DontDestroyOnLoad)
- PassengerManager (DontDestroyOnLoad)
- AudioManager (DontDestroyOnLoad)
- DifficultyManager (DontDestroyOnLoad)

Scene Objects (Destroyed on Load):
- UI Canvases
- Controllers
- Visuals
```

---

Esta arquitectura permite:
- ✅ Fácil extensión de eventos
- ✅ Desacoplamiento entre sistemas
- ✅ Reutilización de código
- ✅ Testing individual de componentes
- ✅ Modificación de parámetros sin cambiar código
