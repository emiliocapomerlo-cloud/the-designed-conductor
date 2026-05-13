# The Designated Driver - Documentación del Proyecto

## Descripción General

**The Designated Driver** es un juego de simulación de conducción caótico para PC diseñado para teclado y ratón. El jugador debe recopilar a sus amigos dispersos en un estacionamiento (Fase 1) y luego llevarlos a casa en una conducción caótica llena de controles que fallan y eventos inesperados (Fase 2).

### Características Principales

- **Fase 1 - Recolección (Top-Down 2D)**
  - Perspectiva cenital (Top-Down) con estética Pixel Art de 16 bits
  - Inspiración visual en RPGs clásicos
  - Mecánica de caminar e interactuar con amigos

- **Fase 2 - Conducción Caótica**
  - Perspectiva de "falsa primera persona" con sprites 2D
  - Física inercial pesada y realista
  - Controles manuales y torpes (acelerador, frenos, marchas, volante)
  - Volante con "memoria de giro" (no vuelve al centro automáticamente)

- **Sistemas de Dificultad Dinámica**
  - GlitchManager: Simula errores de software como mecánicas
  - Sistema de caos cooperativo: Pasajeros como obstáculos dinámicos
  - Event Manager: Eventos programados con consecuencias dinámicas

---

## Estructura del Proyecto

```
Assets/
├── Scripts/
│   ├── Managers/
│   │   ├── GameManager.cs          # Controlador central del juego
│   │   ├── AudioManager.cs         # Gestor de audio y música
│   │   ├── Phase1CollectionController.cs
│   │   └── DifficultyManager.cs    # Dificultad dinámica
│   ├── Player/
│   │   ├── PlayerTopDownController.cs  # Movimiento en Fase 1
│   │   └── Friend.cs               # Comportamiento de amigos
│   ├── Vehicle/
│   │   ├── VehiclePhysicsController.cs # Física del vehículo
│   │   ├── Phase2DrivingController.cs  # Control de conducción
│   │   └── Phase2InputHandler.cs   # Input con glitches
│   ├── Systems/
│   │   ├── GlitchManager.cs        # Sistema de glitches
│   │   ├── EventManager.cs         # Gestor de eventos
│   │   ├── PassengerManager.cs     # Gestor de pasajeros
│   │   ├── CameraGlitchEffect.cs   # Efectos visuales de glitch
│   │   └── PlayerDecisionHandler.cs
│   └── UI/
│       ├── UIManager.cs            # Gestor de UI
│       └── EventDialogHandler.cs   # Diálogos de eventos
├── Scenes/
│   ├── MainMenu.unity
│   ├── Phase1_Parking.unity
│   └── Phase2_Driving.unity
├── Sprites/
│   ├── Player/
│   ├── Friends/
│   ├── Vehicle/
│   ├── Environment/
│   └── UI/
├── Prefabs/
│   ├── Player.prefab
│   ├── Friend.prefab
│   ├── Vehicle.prefab
│   └── EventDialog.prefab
├── Audio/
│   ├── Music/
│   ├── SFX/
│   └── Ambient/
└── Shaders/
    └── GlitchEffect.shader
```

---

## Scripts Principales

### GameManager.cs
**Responsabilidad:** Controlador central que gestiona el estado global del juego.

```csharp
// Propiedades principales:
- CurrentPhase: Estado actual (Menu, Phase1, Phase2, Victory, GameOver)
- FriendsCollected: Lista de amigos recolectados
- TotalFriendsNeeded: Número de amigos a recopilar (default: 3)
- TimeLimit: Límite de tiempo para Phase 2 (default: 600s = 10 min)

// Eventos:
OnPhaseChanged(GamePhase)
OnFriendsCountChanged(int collected, int total)
OnTimeChanged(float elapsed, float limit)

// Métodos principales:
StartPhase1()           // Inicia la Fase 1
TransitionToPhase2()    // Transiciona a Fase 2
CollectFriend(string)   // Recolecta un amigo
EndGame(bool victory)   // Finaliza el juego
```

### VehiclePhysicsController.cs
**Responsabilidad:** Gestiona la física y controles del vehículo.

```csharp
// Parámetros de Física:
- EnginePower: Potencia del motor (default: 100)
- BrakePower: Potencia del freno (default: 150)
- MaxSpeed: Velocidad máxima (default: 20)
- MaxReverseSpeed: Velocidad reversa máxima (default: -10)
- Friction: Fricción del vehículo (default: 0.1)
- SteeringResponse: Respuesta del volante (default: 2)
- SteeringDecay: Velocidad de retorno del volante (default: 5)
- WheelRotationMemory: Memoria del volante 0-1 (default: 0.8)
- MaxWheelRotation: Ángulo máximo del volante (default: 45°)

// Controles:
W/↑: Acelerar
S/↓: Frenar
A/←: Girar izquierda
D/→: Girar derecha
E: Marcha adelante
Q: Marcha reversa
R: Neutral

// Eventos:
OnSpeedChanged(float)
OnGearChanged(int)
OnWheelRotationChanged(float)
```

### GlitchManager.cs
**Responsabilidad:** Simula errores de software y altera la mecánica del juego.

```csharp
// Niveles de Glitch: Clean, Minor, Moderate, Severe
// Cada nivel tiene:
- Intensity: 0-1
- VisionBlur: Cantidad de desenfoque
- ControlInversion: Probabilidad de inversión
- ReactionTimeDelay: Retraso en respuesta
- AudioDistortion: Distorsión del audio

// Métodos principales:
AccumulateGlitch(float amount)      // Acumula glitch
TriggerControlInversion(float time) // Invierte controles por X segundos
GetCurrentGlitchIntensity()         // Retorna intensidad actual

// Eventos:
OnGlitchIntensityChanged(float)
OnControlsInverted(bool)
OnVisionBlurChanged(float)
```

### EventManager.cs
**Responsabilidad:** Dispara eventos dinámicos y maneja decisiones del jugador.

```csharp
// Eventos disponibles:
- offer_drink: Ofrece bebida (acumula glitch)
- falling_object: Objeto cae en el auto (caos de pasajeros)
- traffic_jam: Atasco de tráfico (fuerza externa)
- police_chase: Persecución policial (glitch severo)

// Métodos principales:
TriggerEvent(GameEvent)              // Dispara evento
RegisterPlayerDecision(string)       // Registra decisión del jugador

// Eventos:
OnEventTriggered(GameEvent)
OnPlayerDecisionMade(string)
```

### PassengerManager.cs
**Responsabilidad:** Gestiona el comportamiento caótico de los pasajeros.

```csharp
// Propiedades de Pasajero:
- name: Nombre del pasajero
- chaosLevel: Nivel de caos 0-1
- isDrunk: Si está embriagado
- currentLocalPosition: Posición en el auto

// Métodos principales:
CausePassengerChaos(float amount)    // Aumenta caos de pasajeros
MakePassengerDrunk(int index)        // Emborracha a un pasajero
GetTotalChaosLevel()                 // Nivel de caos promedio

// Eventos:
OnPassengerAction(string)
OnChaosLevelChanged(int index, float level)
```

---

## Setup de Escenas

### Escena 1: Main Menu
**Componentes necesarios:**
- Canvas con botones: "New Game", "Settings", "Quit"
- Script: Conectar botones a `GameManager.StartPhase1()`

### Escena 2: Phase 1 - Parking
**Componentes necesarios:**
- Player GameObject con:
  - Sprite Renderer (Player sprite)
  - Rigidbody2D (Body Type: Dynamic)
  - Collider2D (Circle)
  - PlayerTopDownController.cs
  - Animator (opcional)

- 3+ Friends dispersos en el mapa, cada uno con:
  - Sprite Renderer
  - Rigidbody2D
  - Collider2D
  - Friend.cs
  - Animator (opcional)

- Canvas con:
  - Texto de conteo de amigos
  - Botón "Start Game" para pasar a Phase 2

- Gestor central:
  - GameObject con GameManager.cs
  - Objeto con Phase1CollectionController.cs

### Escena 3: Phase 2 - Driving
**Componentes necesarios:**
- Vehicle GameObject con:
  - Sprite Renderer (vista falsa-primera-persona)
  - Rigidbody2D (Body Type: Dynamic)
  - BoxCollider2D
  - VehiclePhysicsController.cs

- Cámara configurada para:
  - Seguir el vehículo
  - CameraGlitchEffect.cs adjunto

- Canvas con:
  - Velocímetro (actualización en tiempo real)
  - Indicador de marcha
  - Temporizador
  - Indicador de glitch
  - RawImages para efecto de carretera scrolling

- Gestores:
  - GlitchManager
  - EventManager
  - PassengerManager
  - AudioManager
  - Phase2DrivingController

---

## Integración de Sistemas

### Flujo de Control de Entrada (Fase 2)
```
Input del Jugador
    ↓
Phase2InputHandler (verifica glitches/retrasos)
    ↓
VehiclePhysicsController (aplica física)
    ↓
PassengerManager (pasajeros afectan movimiento)
    ↓
Renderer (muestra resultado)
```

### Flujo de Glitches
```
EventManager (dispara evento)
    ↓
GlitchManager (acumula glitch)
    ↓
CameraGlitchEffect (aplica visual blur)
    ↓
Phase2InputHandler (invierte/retrasa input)
    ↓
AudioManager (distorsión de audio)
```

### Flujo de Decisiones
```
EventManager (muestra evento)
    ↓
EventDialogHandler (muestra diálogo)
    ↓
Jugador (acepta/rechaza)
    ↓
Consecuencia (glitch, pasajero ebrio, etc.)
```

---

## Cómo Configurar en Unity

### 1. Crear el Proyecto
```
- Crear nuevo proyecto 3D (Unity 6)
- Seleccionar resolución 1920x1080
- Importar 2D Sprite package si no está incluido
```

### 2. Importar Scripts
- Copiar todos los scripts en `Assets/Scripts/` según la estructura

### 3. Crear Prefabs Esenciales

**Player Prefab:**
- Crear GameObject vacío
- Añadir Sprite Renderer
- Añadir Rigidbody2D (Dynamic, Gravity Scale: 0)
- Añadir CircleCollider2D
- Añadir PlayerTopDownController.cs
- Nombrar como "Player"

**Friend Prefab:**
- Crear GameObject vacío
- Añadir Sprite Renderer
- Añadir Rigidbody2D (Dynamic, Gravity Scale: 0)
- Añadir CircleCollider2D
- Añadir Friend.cs
- Asignar friendName en inspector
- Nombrar como "Friend"

**Vehicle Prefab:**
- Crear GameObject vacío
- Añadir Sprite Renderer (para vista del interior)
- Añadir Rigidbody2D (Dynamic, Gravity Scale: 0)
- Añadir BoxCollider2D
- Añadir VehiclePhysicsController.cs
- Nombrar como "Vehicle"

### 4. Crear Gestores (Singleton Pattern)

Crear GameObject vacío en cada escena:
- "GameManager" → GameManager.cs
- "GlitchManager" → GlitchManager.cs
- "EventManager" → EventManager.cs
- "PassengerManager" → PassengerManager.cs
- "AudioManager" → AudioManager.cs (en DontDestroyOnLoad)
- "DifficultyManager" → DifficultyManager.cs

### 5. Configurar Canvas de UI

Para Phase 2:
- Text "SpeedText" para velocidad
- Text "GearText" para marcha
- Text "TimeText" para tiempo
- Text "GlitchIndicator" para intensidad de glitch
- RawImage para scroll de carretera

---

## Control de Juego

### Fase 1 (Recolección)
- **WASD o Flechas:** Movimiento del jugador
- **ESPACIO:** Interactuar con amigos
- **ENTER:** Iniciar conducción (después de recolectar todos)

### Fase 2 (Conducción)
- **W o ↑:** Acelerar
- **S o ↓:** Frenar
- **A o ←:** Girar volante izquierda
- **D o →:** Girar volante derecha
- **E:** Marcha adelante
- **Q:** Marcha reversa
- **R:** Neutral
- **ESPACIO/Mouse Click:** Aceptar evento
- **ESC:** Rechazar evento

---

## Parámetros Ajustables (Difficulty)

### Easy
- Glitch Accumulation Rate: 0.05
- Passenger Chaos Rate: 0.02
- Event Frequency: 0.2
- Time Limit: 900s (15 min)

### Normal
- Glitch Accumulation Rate: 0.1
- Passenger Chaos Rate: 0.05
- Event Frequency: 0.3
- Time Limit: 600s (10 min)

### Hard
- Glitch Accumulation Rate: 0.2
- Passenger Chaos Rate: 0.1
- Event Frequency: 0.5
- Time Limit: 420s (7 min)

---

## Extensiones Futuras

### Mecánicas Adicionales
- [ ] Sistema de tráfico con vehículos IA
- [ ] Daño progresivo del vehículo
- [ ] Sistema de combustible
- [ ] Minijuegos durante conducción
- [ ] Diferentes caminos y mapas

### Eventos Adicionales
- [ ] Cambios de clima
- [ ] Accidentes cercanos
- [ ] Llamadas telefónicas
- [ ] Cambios de música
- [ ] Efectos de tiempo

### UI/UX
- [ ] Pantalla de pausa mejorada
- [ ] Sistema de guardado/carga
- [ ] Tabla de puntuaciones
- [ ] Tutorial interactivo

---

## Troubleshooting

### El jugador no se mueve en Fase 1
- Verificar que Rigidbody2D tiene Body Type: Dynamic
- Verificar que Gravity Scale = 0
- Verificar que PlayerTopDownController está asignado

### El vehículo se comporta extraño
- Verificar que steeringResponse está entre 1-5
- Verificar que friction está entre 0.05-0.2
- Ajustar wheelRotationMemory (0.7-0.9 recomendado)

### Glitches no se aplican
- Verificar que GlitchManager existe en la escena
- Verificar que Phase2InputHandler está recibiendo input
- Verificar que CameraGlitchEffect tiene el shader correcto

### Eventos no disparan
- Verificar que EventManager existe en la escena
- Verificar que triggerTime es menor que timeLimit
- Aumentar probability para debug

---

## Notas de Desarrollo

- Usar Vector2 para todo (proyecto 2D)
- Mantener rigidbodies en Dynamic para efectos de físicas
- Usar DontDestroyOnLoad solo para gestores persistentes
- Todos los singletons deben tener patrón Instance que verifica duplicados

---

**Versión:** 1.0  
**Última actualización:** 13 de Mayo de 2026  
**Desarrollado para:** Unity 6  
**Requisitos:** .NET Framework 4.7.1+, Editor 6.0+

