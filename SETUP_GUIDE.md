# The Designated Driver - Guía de Configuración Rápida

## 1. Requerimientos
- Unity 6.0 o superior
- Windows 10/11
- Visual Studio Code o Visual Studio para edición de scripts

## 2. Estructura de Carpetas Creada

✓ Assets/Scripts/ - Todos los scripts C#
✓ Assets/Scenes/ - Archivos de escenas
✓ Assets/Sprites/ - Gráficos y sprites (pendiente)
✓ Assets/Prefabs/ - Prefabs reutilizables
✓ Assets/Audio/ - Sonidos y música
✓ Assets/Shaders/ - Shaders personalizados

## 3. Scripts Disponibles

### Managers (Singletons)
- ✓ GameManager.cs - Control central del juego
- ✓ AudioManager.cs - Gestión de audio
- ✓ DifficultyManager.cs - Dificultad dinámica

### Sistemas Dinámicos
- ✓ GlitchManager.cs - Sistema de glitches
- ✓ EventManager.cs - Gestor de eventos
- ✓ PassengerManager.cs - Comportamiento de pasajeros

### Fase 1 (Top-Down)
- ✓ PlayerTopDownController.cs - Movimiento del jugador
- ✓ Friend.cs - Comportamiento de amigos
- ✓ Phase1CollectionController.cs - Control de Fase 1

### Fase 2 (Conducción)
- ✓ VehiclePhysicsController.cs - Física del vehículo
- ✓ Phase2DrivingController.cs - Control de conducción
- ✓ Phase2InputHandler.cs - Input con glitches
- ✓ CameraGlitchEffect.cs - Efectos visuales

### UI
- ✓ UIManager.cs - Gestor de UI
- ✓ EventDialogHandler.cs - Diálogos de eventos
- ✓ PlayerDecisionHandler.cs - Decisiones del jugador

## 4. Próximos Pasos en Unity

### Paso 1: Crear Escenas Básicas
```
1. Abrir Unity
2. Crear escena: Assets > Create > Folder "Scenes"
3. Crear 3 escenas:
   - MainMenu.unity
   - Phase1_Parking.unity
   - Phase2_Driving.unity
```

### Paso 2: Crear Gestores (cada escena)
```
1. Crear Empty GameObject
2. Renombrar (GameManager, GlitchManager, etc.)
3. Arrastar script correspondiente al inspector
4. Marcar como DontDestroyOnLoad (scripts lo hacen automáticamente)
```

### Paso 3: Sprites Placeholder
Para comenzar, puedes usar cuadrados de colores:
- Crear un sprite blanco simple
- Usar colores diferentes para diferentes elementos

### Paso 4: Setup de Fase 1
```
1. Crear Player GameObject
2. Añadir Sprite (rojo para identificar)
3. Añadir Rigidbody2D (Dynamic, Gravity: 0)
4. Añadir BoxCollider2D
5. Añadir script PlayerTopDownController.cs
6. Crear 3 Friends con sprite diferente (azul)
7. Añadir script Friend.cs a cada uno
```

### Paso 5: Setup de Fase 2
```
1. Crear Vehicle GameObject
2. Añadir Sprite grande (será la vista del auto)
3. Añadir Rigidbody2D (Dynamic, Gravity: 0)
4. Añadir BoxCollider2D
5. Añadir script VehiclePhysicsController.cs
6. Crear Canvas para UI
7. Añadir Texts para: velocidad, marcha, tiempo, glitch
```

## 5. Parámetros Recomendados Iniciales

### VehiclePhysicsController
```
Engine Power: 100
Brake Power: 150
Max Speed: 20
Max Reverse Speed: -10
Friction: 0.1
Steering Response: 2
Steering Decay: 5
Wheel Rotation Memory: 0.8
Max Wheel Rotation: 45
```

### GlitchManager
Se inicializa automáticamente con 4 niveles (Clean, Minor, Moderate, Severe)

### GameManager
```
Total Friends Needed: 3
Time Limit: 600 segundos (10 minutos)
```

## 6. Prueba Rápida

1. Configurar Fase 1 básica
2. Play - Verificar movimiento del jugador (WASD)
3. Spacebar cerca de Friend - Verificar recolección
4. Presionar Enter - Transición a Fase 2
5. En Fase 2 verificar:
   - Aceleración (W)
   - Frenado (S)
   - Giro (A/D) - debe "recordar" la posición del volante
   - Cambios de marcha (E/Q/R)

## 7. Estructura de Assets Necesarios

### Sprites (Pendientes de crear)
```
- player.png (16x16 px recomendado)
- friend_1.png, friend_2.png, friend_3.png
- vehicle_interior.png (vista falsa 1ª persona)
- road_side.png (para scrolling)
```

### Audio (Pendientes de crear)
```
- background_music.mp3
- event_alert.mp3
- glitch_sound.mp3
- whoosh.mp3 (efectos)
```

## 8. Script Integration Map

```
GameManager
├── Phase1CollectionController
│   ├── PlayerTopDownController
│   └── Friend (multiple)
├── Phase2DrivingController
│   ├── VehiclePhysicsController
│   ├── Phase2InputHandler
│   └── PassengerManager
├── GlitchManager
├── EventManager
├── AudioManager
└── DifficultyManager
```

## 9. Variables Clave a Monitorear

- GlitchManager.GetCurrentGlitchIntensity() → 0-1
- VehiclePhysicsController.GetCurrentSpeed() → km/h
- GameManager.GetCollectedFriendsCount() → número
- PassengerManager.GetTotalChaosLevel() → 0-1

## 10. Debugging Tips

```csharp
// En cualquier script:
Debug.Log($"[Sistema] Mensaje: {variable}");

// Ver en Console de Unity mientras Play
// Usar breakpoints en scripts
// Inspector muestra valores en tiempo real
```

---

## Checklist de Configuración

- [ ] Proyecto Unity 6 creado
- [ ] Scripts importados en Assets/Scripts
- [ ] 3 escenas creadas (Menu, Phase1, Phase2)
- [ ] Gestores asignados a GameObjects
- [ ] Player y Friends en Fase 1
- [ ] Vehicle en Fase 2
- [ ] Canvas con textos de UI
- [ ] Rigidbodies configurados correctamente
- [ ] Collideres en place
- [ ] Scripts asignados a GameObjects
- [ ] Build Settings actualizado (si planning hacer build)

---

**¡Listo para empezar! 🎮**

Todos los scripts están listos. Solo necesitas:
1. Agregar sprites (pueden ser simples cuadrados para test)
2. Crear las escenas en Unity
3. Asignar los scripts a los GameObjects
4. Ajustar parámetros en el inspector según prefieras

