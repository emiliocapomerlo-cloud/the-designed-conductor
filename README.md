# The Designated Driver 🚗

Un simulador de conducción caótico para PC donde el jugador debe recopilar a sus amigos borrachos en un estacionamiento y llevarlos a casa lidiando con controles que fallan, eventos inesperados y un sistema dinámico de "glitches" que alteran la realidad del juego.

## 🎮 Descripción del Juego

**The Designated Driver** es un juego desarrollado en Unity 6 que presenta dos fases distintas:

### Fase 1: Recolección (Top-Down 2D)
- Perspectiva cenital estilo RPG clásico con gráficos Pixel Art 16-bit
- El jugador camina por un estacionamiento caótico
- Interactúa con 3+ amigos dispersos para que suban al auto
- Mecánica simple pero envolvente de exploración

### Fase 2: Conducción Caótica (Falsa Primera Persona)
- Vista "falsa primera persona" mediante sprites 2D superpuestos
- Controles manuales y torpes (acelerador, frenos, volante, marchas)
- Física inercial pesada y realista
- **El volante tiene memoria de giro**: no vuelve automáticamente al centro
- Sistema dinámico de glitches que alteran visión, sonido y controles
- Pasajeros actúan como obstáculos dinámicos dentro del auto
- Eventos programados con decisiones que tienen consecuencias

## 🏗️ Arquitectura del Proyecto

El proyecto está completamente implementado en C# con una arquitectura modular y escalable:

### Gestores Centrales (Singletons)
- **GameManager**: Control central de fases, tiempo y progreso
- **GlitchManager**: Sistema de errores de software como mecánicas
- **EventManager**: Gestor de eventos dinámicos con decisiones
- **PassengerManager**: Comportamiento caótico de pasajeros
- **AudioManager**: Gestión de sonido con distorsión por glitch
- **DifficultyManager**: Dificultad dinámica (Easy/Normal/Hard)

### Controladores de Fase
- **Phase1CollectionController**: Maneja recolección de amigos
- **Phase2DrivingController**: Maneja pantalla de conducción
- **PlayerTopDownController**: Movimiento en Fase 1
- **VehiclePhysicsController**: Física realista del vehículo
- **Phase2InputHandler**: Input con alteraciones por glitch

### Sistemas Especiales
- **GlitchManager**: Visión borrosa, inversión de controles, retrasos
- **EventManager**: offer_drink, falling_object, traffic_jam, police_chase
- **PassengerManager**: Pasajeros que se mueven caóticamente
- **CameraGlitchEffect**: Shader para efectos visuales

## 📁 Estructura de Archivos

```
the-designed-conductor/
├── Assets/
│   ├── Scripts/
│   │   ├── Managers/        (GameManager, AudioManager, DifficultyManager)
│   │   ├── Player/          (PlayerTopDownController, Friend)
│   │   ├── Vehicle/         (VehiclePhysicsController, Phase2InputHandler)
│   │   ├── Systems/         (GlitchManager, EventManager, PassengerManager)
│   │   └── UI/              (UIManager, EventDialogHandler)
│   ├── Scenes/              (MainMenu, Phase1_Parking, Phase2_Driving)
│   ├── Sprites/             (Carpeta para assets gráficos)
│   ├── Prefabs/             (Player, Friend, Vehicle prefabs)
│   ├── Audio/               (Música y efectos de sonido)
│   └── Shaders/             (GlitchEffect.shader)
├── GAME_DESIGN_DOCUMENT.md  (Documentación técnica completa)
├── SETUP_GUIDE.md           (Guía de configuración en Unity)
└── README.md                (Este archivo)
```

## 🎮 Controles

### Fase 1 (Recolección)
| Tecla | Acción |
|-------|--------|
| WASD / Flechas | Movimiento |
| ESPACIO | Interactuar con amigos |
| ENTER | Iniciar conducción |

### Fase 2 (Conducción)
| Tecla | Acción |
|-------|--------|
| W / ↑ | Acelerar |
| S / ↓ | Frenar |
| A / ← | Girar volante izquierda |
| D / → | Girar volante derecha |
| E | Marcha adelante |
| Q | Marcha reversa |
| R | Neutral |
| ESPACIO | Aceptar evento |
| ESC | Rechazar evento |

## ⚙️ Mecánicas Clave

### Sistema de Glitches
El **GlitchManager** simula errores de software que alteran progresivamente:
- **Visión**: Desenfoque y distorsión visual
- **Sonido**: Alteración de pitch y volumen
- **Controles**: Inversión aleatoria de entrada (A=D, W=S)
- **Tiempo de Reacción**: Retrasos en respuesta a input

4 niveles de glitch:
1. **Clean** (0%): Sin efectos
2. **Minor** (33%): Ligero desenfoque y pequeñas alteraciones
3. **Moderate** (66%): Desenfoque notable, mayor inversión de controles
4. **Severe** (100%): Caos total

### Física Realista del Vehículo
- **Inercia pesada**: El auto no detiene inmediatamente
- **Fricción**: Simulación de resistencia
- **Rotación con memoria**: El volante mantiene su posición tras soltar las teclas
- **Decaimiento lento**: El volante vuelve gradualmente al centro
- **Pasajeros como peso**: Afectan el movimiento del vehículo

### Eventos Dinámicos
Eventos programados que disparan en tiempos específicos:
- **Oferta de Bebida**: Acumula glitch (embriaguez)
- **Objeto Cayendo**: Aumenta caos de pasajeros
- **Atasco de Tráfico**: Aplica fuerza externa al vehículo
- **Persecución Policial**: Glitch severo

### Caos Cooperativo
Los pasajeros:
- Se mueven aleatoriamente dentro del auto
- Afectan el equilibrio del vehículo
- Pueden ser emborrachados (incrementa caos)
- Su nivel de caos aumenta con eventos

## 🚀 Cómo Empezar

### Requisitos
- Unity 6.0 o superior
- Windows 10/11
- .NET Framework 4.7.1+

### Pasos Rápidos
1. Abrir el proyecto en Unity 6
2. Ver **SETUP_GUIDE.md** para instrucciones de configuración
3. Ver **GAME_DESIGN_DOCUMENT.md** para documentación técnica completa
4. Crear sprites placeholder (cuadrados de colores para test)
5. Crear 3 escenas: MainMenu, Phase1_Parking, Phase2_Driving
6. Asignar scripts a GameObjects
7. ¡Jugar!

### Parámetros Ajustables (Inspector de Unity)

**VehiclePhysicsController**
- Engine Power: 100
- Brake Power: 150
- Max Speed: 20
- Steering Response: 2
- Wheel Rotation Memory: 0.8 (clave para la mecánica de volante)

**GameManager**
- Total Friends Needed: 3
- Time Limit: 600 (segundos)

**GlitchManager**
- Glitch Accumulation Rate: 0.1
- Glitch Decay Rate: 0.05

## 📊 Dificultad Dinámica

El **DifficultyManager** ofrece 3 niveles:

| Parámetro | Easy | Normal | Hard |
|-----------|------|--------|------|
| Glitch Rate | 0.05 | 0.1 | 0.2 |
| Event Frequency | 0.2 | 0.3 | 0.5 |
| Time Limit | 15 min | 10 min | 7 min |

## 📜 Scripts Principales

### GameManager.cs
Controlador central que mantiene estado global:
```csharp
public enum GamePhase { Phase1_Collection, Phase2_Driving, Victory, GameOver }
public void StartPhase1()
public void TransitionToPhase2()
public void CollectFriend(string friendName)
public void EndGame(bool victory)
```

### VehiclePhysicsController.cs
Simula física realista del vehículo:
```csharp
public float GetCurrentSpeed()
public float GetWheelRotation()
public void ApplyExternalForce(Vector2 force)
public void ChangeGear(int gear)
```

### GlitchManager.cs
Sistema de alteraciones visuales y de control:
```csharp
public void AccumulateGlitch(float amount)
public void TriggerControlInversion(float duration)
public float GetCurrentGlitchIntensity()
public bool AreControlsInverted()
```

### EventManager.cs
Gestor de eventos con decisiones del jugador:
```csharp
public void TriggerEvent(GameEvent gameEvent)
public void RegisterPlayerDecision(string decisionId)
```

## 🎨 Sistema Visual

- **Fase 1**: Pixel Art 16-bit en perspectiva cenital
- **Fase 2**: Sprites 2D superpuestos simulando vista interior del auto
- **Efectos de Glitch**: Shader personalizado (GlitchEffect.shader)
- **Roadside Scrolling**: RawImage con offset UV para simular movimiento

## 🔊 Audio

El **AudioManager**:
- Gestiona música de fondo
- Aplica distorsión según intensidad de glitch
- Varía pitch y volumen dinámicamente
- Reproduce SFX para eventos

## 📈 Extensibilidad

La arquitectura permite fácilmente agregar:
- Nuevos eventos (hereda de GameEvent)
- Nuevas mecánicas de glitch (en GlitchManager.glitchLevels)
- Nuevos pasajeros con comportamientos específicos
- Diferentes mapas y escenas
- Sistema de guardado/carga

## 🐛 Solución de Problemas

Ver **SETUP_GUIDE.md** sección "Troubleshooting" para:
- Problemas de movimiento del jugador
- Comportamiento anómalo del vehículo
- Glitches no aplicándose
- Eventos sin disparar

## 📝 Documentación

- **GAME_DESIGN_DOCUMENT.md**: Documentación técnica completa (500+ líneas)
- **SETUP_GUIDE.md**: Guía paso a paso de configuración
- **Comentarios en código**: Cada script tiene docstrings detallados

## 🎯 Objetivos del Juego

**Victoria**: Recolectar los 3 amigos en Fase 1 y llevarlos a casa en Fase 2 antes de que se agote el tiempo (10 minutos en dificultad normal).

**Desafíos**:
- Superar la dificultad dinámica
- Adaptar conducción a glitches progresivos
- Evitar que los pasajeros causen caos
- Tomar decisiones sabias en eventos

## 🏆 Créditos

Desarrollado como proyecto de estudio en Unity 6.

## 📄 Licencia

Proyecto educativo - Libre para uso y modificación.

---

**Versión**: 1.0  
**Última actualización**: 13 de Mayo de 2026  
**Motor**: Unity 6  
**Lenguaje**: C#

**¡Bienvenido, Conductor Designado! 🚗💨**
