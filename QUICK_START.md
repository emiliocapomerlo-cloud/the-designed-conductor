# ⚡ Quick Start - The Designated Driver

## 🚀 Iniciar en 15 Minutos

### Paso 1: Abrir el Proyecto (1 min)
- [ ] Abrir Unity 6
- [ ] File → Open Project
- [ ] Seleccionar `the-designed-conductor`
- [ ] Esperar a que se carguen los scripts

### Paso 2: Crear Escenas Base (3 min)
```
1. Right-click en Assets/Scenes
2. Create → Scene → Nombre: "Phase1_Parking"
3. Right-click en Assets/Scenes
4. Create → Scene → Nombre: "Phase2_Driving"
```

### Paso 3: Setup Fase 1 (5 min)

#### Crear Player
```
1. Create → 3D Object → Cube (será sprite placeholder)
2. Renombrar: "Player"
3. Eliminar Box Collider, agregar Circle Collider 2D
4. Agregar Rigidbody2D (Dynamic, Gravity: 0)
5. Asignar script: PlayerTopDownController
6. Position: (0, 0, 0)
7. Scale: (0.5, 0.5, 1)
```

#### Crear 3 Amigos
```
Repetir 3 veces:
1. Create → 3D Object → Cube
2. Renombrar: "Friend_1" (cambiar número)
3. Agregar Circle Collider 2D
4. Agregar Rigidbody2D (Dynamic, Gravity: 0)
5. Asignar script: Friend
6. En Inspector → Friend.cs → Friend Name: "Friend_1"
7. Posicionar diferentes lugares (x: -5 a 5, y: -5 a 5)
8. Scale: (0.4, 0.4, 1)
```

#### Crear GameManager
```
1. Create Empty → Renombrar: "GameManager"
2. Asignar script: GameManager
3. No mover (el script lo maneja)
```

#### Crear Otros Managers
```
Repetir para cada manager:
1. Create Empty
2. Renombrar: GlitchManager, EventManager, PassengerManager, AudioManager
3. Asignar scripts correspondientes
```

#### Crear UI
```
1. Right-click en Hierarchy → UI → Canvas
2. En el Canvas → Create → Text → Renombrar: "FriendCountText"
3. Asignar script: Phase1CollectionController
   en el Canvas o en un GameObject padre
4. Configurar referencias en Inspector
```

### Paso 4: Setup Fase 2 (5 min)

#### Crear Vehicle
```
1. Create → 3D Object → Cube
2. Renombrar: "Vehicle"
3. Eliminar Box Collider, agregar BoxCollider 2D
4. Agregar Rigidbody2D (Dynamic, Gravity: 0)
5. Asignar script: VehiclePhysicsController
6. Scale: (1, 2, 1)
```

#### Crear Canvas de Conducción
```
1. Create → Canvas
2. En el Canvas → Create → Text → "SpeedText"
3. En el Canvas → Create → Text → "GearText"
4. En el Canvas → Create → Text → "TimeText"
5. En el Canvas → Create → Text → "GlitchIndicator"
6. Asignar script: Phase2DrivingController al Canvas
7. Conectar referencias en Inspector
```

### Paso 5: Prueba Rápida (1 min)
```
1. Abrir escena "Phase1_Parking"
2. Presionar PLAY
3. Mover con WASD → ¿Se mueve el Player?
4. Acercarse a Friend → Presionar SPACE
5. ¿Se recolecta el amigo?
6. Si todo está bien → ¡ÉXITO!
```

---

## ⚙️ Parámetros Iniciales

### VehiclePhysicsController (Inspector)
```
Engine Power: 100
Brake Power: 150
Max Speed: 20
Max Reverse Speed: -10
Friction: 0.1
Steering Response: 2
Steering Decay: 5
Wheel Rotation Memory: 0.8 ← IMPORTANTE!
Max Wheel Rotation: 45
```

### GameManager (Inspector)
```
Total Friends Needed: 3
Time Limit: 600
```

---

## 🎮 Probar Controles

### En Fase 1
| Tecla | Acción | Resultado |
|-------|--------|-----------|
| W | Arriba | Player sube |
| S | Abajo | Player baja |
| A | Izquierda | Player izq |
| D | Derecha | Player der |
| ESPACIO | Interactuar | Friend desaparece |
| ENTER | Iniciar | Ir a Fase 2 |

### En Fase 2
| Tecla | Acción | Resultado |
|-------|--------|-----------|
| W | Acelerar | Velocidad aumenta |
| S | Frenar | Velocidad disminuye |
| A | Girar IZQ | Vehículo rota |
| D | Girar DER | Vehículo rota |
| E | Marcha | Cambiar a "D" |
| Q | Reversa | Cambiar a "R" |
| R | Neutral | Cambiar a "N" |

---

## 🔍 Checklist de Verificación

### Después de Setup Básico
- [ ] Scripts compilan sin errores
- [ ] Player se mueve con WASD
- [ ] Friends pueden ser recolectados
- [ ] Se ve contador de amigos
- [ ] Vehicle se mueve con W/S
- [ ] Vehicle gira con A/D
- [ ] Se ve velocímetro

### Si Algo Falla
1. ✅ Verificar que scripts están en carpeta correcta
2. ✅ Verificar que GameObjects tienen Rigidbody2D
3. ✅ Verificar que scripts están asignados
4. ✅ Revisar Console para errores (Ctrl+Shift+C)
5. ✅ Ver SETUP_GUIDE.md sección "Troubleshooting"

---

## 📁 Archivos Críticos

### Scripts que DEBEN existir
```
✅ GameManager.cs → Assets/Scripts/Managers/
✅ PlayerTopDownController.cs → Assets/Scripts/Player/
✅ VehiclePhysicsController.cs → Assets/Scripts/Vehicle/
✅ GlitchManager.cs → Assets/Scripts/Systems/
✅ EventManager.cs → Assets/Scripts/Systems/
```

### Verificar Carpeta
```
Assets/
├── Scripts/
│   ├── Managers/ (6 scripts)
│   ├── Player/ (2 scripts)
│   ├── Vehicle/ (3 scripts)
│   ├── Systems/ (4 scripts)
│   └── UI/ (2 scripts)
```

---

## 🎨 Sprites Placeholder

Para empezar sin arte real:

### Opción 1: Cuadrados de Color (MÁS FÁCIL)
```csharp
// Unity crea automáticamente un sprite blanco
// Solo asigna colores diferentes:
// Player: Rojo
// Friends: Azul
// Vehicle: Verde
```

### Opción 2: Sprites Simples
- Descargar desde Unity Asset Store (gratis)
- Buscar: "Free 16-bit Pixel Art"
- Importar en Assets/Sprites/

---

## 🚨 Errores Comunes y Soluciones

### Error: "PlayerTopDownController requires a Rigidbody2D"
**Solución:**
- Seleccionar Player → Add Component → Rigidbody2D
- Configurar: Body Type = Dynamic, Gravity Scale = 0

### Error: "NullReferenceException in GameManager"
**Solución:**
- Verificar que existe un GameObject llamado "GameManager"
- Verificar que tiene script GameManager asignado
- Verificar que no hay 2 GameManagers en la escena

### Vehicle no se mueve
**Solución:**
- Verificar que VehiclePhysicsController tiene referencia a Rigidbody2D
- Si no, asignar en el script: `vehicleRB = GetComponent<Rigidbody2D>();`

### Amigos no desaparecen al recolectar
**Solución:**
- Verificar que Friend tiene script Friend.cs
- Verificar que Player está lo suficientemente cerca (< 1.5 unidades)
- Verificar que tiene Collider asignado

---

## 📊 Diagrama de Escenas

```
MainMenu
    ↓ [Play Button]
Phase1_Parking (Recolección)
    ├─ Managers (GameManager, GlitchManager, etc)
    ├─ Player
    ├─ Friend_1, Friend_2, Friend_3
    ├─ Canvas UI
    └─ [ENTER] ↓
Phase2_Driving (Conducción)
    ├─ Managers
    ├─ Vehicle
    ├─ Camera
    ├─ Canvas UI
    └─ [10 minutos] ↓ Victory/GameOver
```

---

## 🎯 Objetivo Mínimo Viable (MVP)

Para que funcione el juego básicamente:

1. ✅ Player se mueve (10 minutos)
2. ✅ Friends se recolectan (5 minutos)
3. ✅ Vehicle conduce (10 minutos)
4. ✅ Glitch afecta visión (automático)
5. ✅ Tiempo corre (automático)

**Total: ~30 minutos de setup**

---

## 🎨 Mejoras Opcionales (Después)

Una vez que funciona básicamente:

1. **Animaciones** (20 min)
   - Agregar Animator a Player/Friends

2. **Más Eventos** (30 min)
   - Crear eventos personalizados

3. **Efectos de Glitch** (30 min)
   - Activar shader GlitchEffect
   - Configurar post-processing

4. **Arte Real** (1-2 horas)
   - Reemplazar cuadrados con sprites

5. **Audio** (1 hora)
   - Agregar música y SFX

---

## 📞 Soporte

Si algo no funciona:
1. Ver **SETUP_GUIDE.md** - sección Troubleshooting
2. Ver **GAME_DESIGN_DOCUMENT.md** - para entender sistema
3. Ver **USAGE_EXAMPLES.md** - para ejemplos de código
4. Ver **ARCHITECTURE.md** - para flujo de control

---

## ✅ Checklist Final Pre-Juego

- [ ] Scripts compilan sin errores
- [ ] Escenas creadas (Phase1 y Phase2)
- [ ] Player en Fase 1
- [ ] 3+ Friends en Fase 1
- [ ] Vehicle en Fase 2
- [ ] Managers en ambas escenas
- [ ] UI Canvases creados
- [ ] Referencias conectadas
- [ ] Parámetros ajustados
- [ ] Play → Sin errores en Console

**¡Si todo está verde = LISTO PARA JUGAR! 🎮**

---

## 🕐 Timeline Realista

```
Setup básico:           15 minutos
Sprites placeholder:    5 minutos
Pruebas iniciales:      10 minutos
Ajustes de parámetros:  10 minutos
                        ─────────────
Total:                  40 minutos

Después: Agregar arte real, sonido, más contenido
```

---

**¡Diviértete creando tu juego! 🚗💨**

Todos los scripts están listos. Solo necesitas un poco de configuración visual en Unity.

