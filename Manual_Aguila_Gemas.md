# Manual: "El Águila y las Gemas" — Práctica 02 (Unity 6.6, platformer 2D)

Guía paso a paso para dejar listo el proyecto antes de la clase donde el profe explicará **triggers y colisiones**.

- **Proyecto:** `C:\Users\orope\Desafios_Videojuegos\test_2-AguilaGemas` (todavía NO abierto en Unity)
- **Recursos originales:** `C:\Users\orope\OneDrive\Escritorio\Cuatrimestre 10° SEP-DIC\Creación de Videojuegos ING\UNIDAD II\Desafio 1\El zorro y su mundo\Sunny-land-files\Sunny-land-files\Assets`

## Lo que ya se dejó listo (NO hagas nada aquí)

| Ruta en el proyecto | Qué es |
|---|---|
| `Assets/Images/eagle-attack-1.png` a `-4.png` | 4 frames del águila (sprites del vuelo) |
| `Assets/Images/tileset.png` | Tileset 400x368 (mismo de la rana) |
| `Assets/Images/gem-1.png` a `gem-5.png` | 5 frames de la gema (para la próxima clase) |
| `Assets/Backgrounds/back.png`, `middle.png`, `props.png` | 3 capas de fondo (paisaje) |
| `Assets/Scripts/Move.cs` | Script del movimiento (ya escrito, sin salto, sin collider) |
| `Assets/Scripts/ControlPlayer.inputactions` | Input Actions (WASD + flechas, sin Jump) |
| `Assets/Animations/`, `Assets/Scenes/`, `Assets/Palette/` | Carpetas vacías listas |

---

## 0. Crear el proyecto en Unity Hub (lo primero de todo)

La carpeta `test_2-AguilaGemas` ya existe con los Assets dentro, pero Unity todavía no la reconoce como proyecto.

1. Abre **Unity Hub**.
2. Pestaña **Projects** → **New project**.
3. Editor Version: **6000.6.0f1** (Unity 6.6).
4. Template: **Universal 2D** (el mismo que usamos para la rana).
5. Project name: **`test_2-AguilaGemas`**.
6. Location: `C:\Users\orope\Desafios_Videojuegos\` ⚠️ **ojo**: Unity va a crear la carpeta ahí dentro con el `Project name`. Como la carpeta YA existe con tus Assets, Unity Hub puede quejarse ("Folder already exists").
   - **Solución A (recomendada):** cambia Location temporal a otro lado (por ejemplo `C:\Users\orope\Desktop\`), dale **Create**. Cuando termine, cierra Unity, abre el Explorador, y **copia solo las carpetas** `Assets`, `Packages`, `ProjectSettings`, `UserSettings` del proyecto recién creado dentro de `C:\Users\orope\Desafios_Videojuegos\test_2-AguilaGemas\` (sobre-escribiendo si pregunta, salvo la carpeta `Assets` que debe **fusionarse**, no reemplazarse). Luego abres `test_2-AguilaGemas` desde Unity Hub con **Add → Add project from disk**.
   - **Solución B (más fácil pero destructiva):** renombra tu `test_2-AguilaGemas` a `test_2-AguilaGemas-backup` antes de crear el proyecto en Hub. Hub lo crea limpio. Luego copias de `-backup\Assets\` → `test_2-AguilaGemas\Assets\` todas las subcarpetas.
7. Una vez abierto el proyecto en Unity, en Project (abajo) debes ver `Assets/Images`, `Assets/Scripts`, `Assets/Backgrounds`, etc. Si no aparecen, repite el paso 6.

> 💡 Si no quieres batallar, puedes dejar que Unity cree el proyecto en una carpeta diferente (`test_2-AguilaGemas-UnityProj`) y después manualmente copias dentro los Assets ya preparados.

---

## 1. Verificar packages

`Window → Package Manager → Packages: Unity Registry`. Verifica que estén instalados (ya vienen con el template 2D):

- Input System
- 2D Tilemap Editor
- 2D Pixel Perfect
- 2D Sprite
- **Cinemachine**

### Si Cinemachine NO aparece:
- Arriba, cambia el filtro a **Unity Registry** (puede estar en "In Project").
- Si aún no aparece, clic en el **+** arriba izquierda → **Install package by name...** → escribe `com.unity.cinemachine` → **Install**.

---

## 2. Activar el New Input System

1. `Edit → Project Settings → Player → Other Settings → Active Input Handling`.
2. Elige **Input System Package (New)** o **Both**.
3. Si pide reiniciar, dale **Apply**. Unity se reinicia.

---

## 3. Limpiar el template

En el Project:
- Borra la carpeta `Assets/Welcome` completa (clic derecho → Delete).
- Borra el archivo `Assets/Settings/InputSystem_Actions.inputactions` (el default). Usamos el nuestro.
- Deja `Assets/Settings` con lo demás (URP configuración).

---

## 4. Configurar la importación de los sprites

### 4.1 Los 4 del águila (`eagle-attack-1` a `-4.png`)

Selecciona los 4 juntos en `Assets/Images/` (clic en el primero, Shift+clic en el último). En el Inspector:

- Texture Type: **Sprite (2D and UI)**
- Sprite Mode: **Single**
- Pixels Per Unit: **32**
- Filter Mode: **Point (no filter)**
- Compression: **None**
- Pivot: **Center**
- → **Apply**

### 4.2 `tileset.png`
- Texture Type: **Sprite (2D and UI)**
- Sprite Mode: **Multiple**
- Pixels Per Unit: **16**
- Mesh Type: **Full Rect**
- Filter Mode: **Point (no filter)**
- Compression: **None**
- → **Apply**
- Clic en **Sprite Editor** (botón azul):
  - **Slice → Type: Grid By Cell Size**
  - Pixel Size: **X=16, Y=16**
  - Pivot: **Center**
  - Clic **Slice** → te divide en 25×23 celdas.
  - Clic **Apply** arriba derecha. Cierra la ventana.

### 4.3 Los 3 backgrounds (`back.png`, `middle.png`, `props.png`)

Selecciona los 3 juntos en `Assets/Backgrounds/`. En el Inspector:

- Texture Type: **Sprite (2D and UI)**
- Sprite Mode: **Single**
- Pixels Per Unit: **16** (prueba con 16; si se ven muy grandes/chicos cámbialo a 32)
- Filter Mode: **Point (no filter)**
- Compression: **None**
- → **Apply**

### 4.4 Los 5 sprites de gema (`gem-1` a `gem-5.png`)

Selecciona los 5 juntos. En el Inspector:

- Texture Type: **Sprite (2D and UI)**
- Sprite Mode: **Single**
- Pixels Per Unit: **32**
- Filter Mode: **Point (no filter)**
- Compression: **None**
- Pivot: **Center**
- → **Apply**

> 💡 **No las uses todavía**, solo déjalas importadas. Las vamos a necesitar cuando el profe explique triggers en clase.

---

## 5. Generar la clase C# del Input Actions

1. En Project selecciona `Assets/Scripts/ControlPlayer.inputactions`.
2. En el Inspector palomea **Generate C# Class**.
3. Click **Apply**. Se crea automáticamente `ControlPlayer.cs` al lado — déjalo ahí, lo usa el Move.cs.

---

## 6. Crear el GameObject `Aguila`

1. Hierarchy → clic derecho → **Create Empty**. Renómbralo `Aguila`.
2. Transform → Position **(0, 0, 0)**.
3. **Add Component → Sprite Renderer**:
   - Campo **Sprite**: arrastra `eagle-attack-1` desde Project.
   - Order in Layer: **5** (para que quede encima del Tilemap y los backgrounds).

> ⚠️ Ojo: **NO** le pongas Rigidbody todavía. En el siguiente paso.

---

## 7. Agregar Rigidbody 2D al águila (sin Collider)

1. Con `Aguila` seleccionada → **Add Component → Rigidbody 2D**.
2. Configura exactamente así:
   - Body Type: **Dynamic**
   - Mass: **1**
   - Linear Drag: **0**
   - Angular Drag: **0.05**
   - **Gravity Scale: 0** ⚠️ (clave: el águila NO cae)
   - Collision Detection: **Discrete**
   - Constraints → palomea **Freeze Rotation Z**

> 🚫 **NO agregues Collider 2D al águila.** Esta es la indicación explícita del profe para esta etapa. El águila debe atravesar todo (tilemap, bordes, etc.). En clase le vamos a poner Collider con `Is Trigger` para detectar gemas, pero por ahora NADA de colliders.

---

## 8. Adjuntar el script `Move.cs` al águila

1. Selecciona `Aguila` en Hierarchy.
2. **Add Component → Move**.
3. Llena los campos:
   - **Velocidad**: `4`

Eso es todo. Este script es más simple que el de la rana: no tiene campos de CheckSuelo ni LayerMask porque no hay colisiones.

---

## 9. Crear el Animator Controller del águila

1. Project → entra a `Assets/Animations`.
2. Clic derecho área vacía → **Create → Animator Controller**. Nómbralo `Aguila`.
3. Selecciona `Aguila` en Hierarchy → **Add Component → Animator**.
4. En el Animator del Aguila, campo **Controller**: arrastra `Aguila.controller`.

---

## 10. Crear la animación `aguila_vuela`

Con el `Aguila` seleccionado en Hierarchy:

1. En Project `Assets/Images`, selecciona los 4 sprites del águila:
   - Clic en `eagle-attack-1`, Shift+clic en `eagle-attack-4`.
2. **Arrástralos encima del GameObject `Aguila`** en la Hierarchy.
3. Te abre la ventana "Create New Animation":
   - Navega a `Assets/Animations/`
   - Nombre: `aguila_vuela.anim`
   - Save.
4. Si te pregunta si quieres reemplazar el controller, dile **NO** (queremos que se agregue al controller existente).

---

## 11. Configurar el Animator (súper simple: un solo estado)

Abre `Window → Animation → Animator` con el `Aguila` seleccionado.

1. Verás **un solo estado**: `aguila_vuela`, probablemente ya en color naranja (default).
   - Si no está naranja, clic derecho sobre él → **Set as Layer Default State**.
2. **NO necesitas parámetros**: la pestaña Parameters queda vacía.
3. **NO necesitas transiciones**: con un solo estado no hay nada entre qué cambiar.
4. Haz clic sobre `aguila_vuela` → Inspector:
   - ✅ **Loop Time** debe estar palomeado (así se repite el batir de alas infinitamente). Si no está, ve al archivo `aguila_vuela.anim` en el Project y palomea **Loop Time** ahí.
   - **Speed = 1** (si el batir de alas se ve muy lento súbelo a 1.5 o 2; si muy rápido bájalo a 0.5).

Dale Play y verifica que el águila batea alas incluso quieta. ✅

---

## 12. Crear el Tilemap y la Tile Palette (igual que en la rana)

> Resumen (si quieres el detalle completo revisa el `Manual_La_Rana_y_su_Mundo.md`).

### 12.1 Grilla y tilemap
1. Hierarchy → clic derecho → **2D Object → Tilemap → Rectangular**.
2. Se crea un `Grid` con hijo `Tilemap`. Renombra el `Tilemap` a `Suelo`.
3. **NO le pongas Tilemap Collider 2D** (nos la ahorramos esta vez; el águila no choca con nada).

### 12.2 Crear la paleta
1. `Window → 2D → Tile Palette`.
2. **Create New Palette**:
   - Name: `TilesetAguila`
   - Grid: **Rectangle**
   - Cell Size: **Automatic**
   - Guárdalo en `Assets/Palette/` → **Create**.
3. Arrastra `tileset.png` (desde `Assets/Images/`) encima de la ventana Tile Palette.
4. Cuando pregunte dónde guardar los tiles → `Assets/Palette/` → **Choose**. Se crean ~575 archivos .asset.

### 12.3 Pintar el escenario
En la ventana Tile Palette:
- Paleta activa: `TilesetAguila`
- Active Tilemap: `Suelo`

Pinta en la Scene:
- **Un suelo con pasto** (línea horizontal continua, aprox Y=0 hacia abajo).
- **Al menos 2 plataformas** a distinta altura (ej. Y=3 y Y=5).
- **1 o 2 elementos decorativos** (hierba, arbusto, troncos).

> El águila va a atravesar todo esto, pero sirve como referencia visual del nivel.

---

## 13. Agregar los backgrounds (paisaje de fondo)

Vamos a crear 3 GameObjects estáticos detrás del Tilemap.

### 13.1 `BG_Back`
1. Hierarchy → clic derecho → **Create Empty**. Nombre: `BG_Back`.
2. Transform → Position **(0, 0, 1)** (Z=1 lo pone detrás).
3. **Add Component → Sprite Renderer**:
   - Sprite: `back` (de `Assets/Backgrounds/`)
   - **Order in Layer: -30**

### 13.2 `BG_Middle`
1. Create Empty → nombre `BG_Middle`.
2. Position **(0, 0, 1)**.
3. Sprite Renderer:
   - Sprite: `middle`
   - **Order in Layer: -20**

### 13.3 `BG_Props`
1. Create Empty → nombre `BG_Props`.
2. Position **(0, 0, 1)**.
3. Sprite Renderer:
   - Sprite: `props`
   - **Order in Layer: -10**

### 13.4 Orden de capas final
| GameObject | Order in Layer |
|---|---|
| BG_Back | -30 |
| BG_Middle | -20 |
| BG_Props | -10 |
| Suelo (Tilemap) | 0 |
| Aguila | 5 |

### 13.5 Posicionar los backgrounds
Mueve cada BG en X/Y (en la Scene) para que llenen la vista de la cámara. Si se ven muy pequeños, ajusta la escala Transform (ej. Scale (2, 2, 1)) o cambia su PPU en el Import Settings a un valor menor y haz Apply.

> 💡 Si quieres que se vea "profundidad real" con parallax (back se mueve menos que props), eso lo haces en una clase posterior. Por ahora que se vea bonito es suficiente.

---

## 14. Cinemachine (cámara sigue al águila)

1. Hierarchy → clic derecho → **Cinemachine → 2D Camera** (en Unity 6 aparece como `CinemachineCamera`).
2. Se crea un objeto nuevo (ej. `CM vcam1` o `CinemachineCamera`).
3. Selecciónalo. En el Inspector:
   - **Follow** (o Tracking Target): arrastra `Aguila`.
4. La `Main Camera` recibe automáticamente un componente `CinemachineBrain`. Déjalo.
5. Ajusta `Lens → Orthographic Size`: **5** suele estar bien; súbelo a 7 u 8 si quieres ver más del nivel.

---

## 15. Guardar la escena

1. `File → Save Scene` → nombre: `Nivel1.unity` → guarda en `Assets/Scenes/`.
2. `File → Save Project` (por si acaso).

---

## 16. Probar que todo funciona

Dale **Play** ▶.

Checklist de lo que debes ver:
- ✅ El águila bate alas incluso quieta (animación en loop).
- ✅ Con **WASD** o **flechas** el águila vuela libre en los 4 ejes (arriba, abajo, izq, der).
- ✅ Cuando mueves hacia la izquierda, el águila se voltea (flipX).
- ✅ Cuando mueves hacia la derecha, el águila mira a la derecha.
- ✅ El águila **atraviesa el tilemap y las plataformas** sin chocar (porque no tiene Collider). ESTO ESTÁ BIEN. Es exactamente lo que el profe pidió.
- ✅ La cámara sigue al águila a donde vaya.
- ✅ Los backgrounds se ven detrás del tilemap y del águila.

Si todo esto pasa → **estás listo para la clase**. 🎉

---

## 17. Troubleshooting

| Problema | Causa / solución |
|---|---|
| El águila no se mueve | No palomeaste **Generate C# Class** en el inputactions (paso 5). O no tiene Rigidbody 2D. |
| El águila cae al suelo | Rigidbody 2D → **Gravity Scale** no está en 0. |
| El águila rebota / se traba contra el tilemap | Le pusiste un Collider 2D por error. Quítalo. **NO debe tener Collider en esta etapa.** |
| El águila no bate alas | El Animator Controller no está asignado, o el estado no tiene Loop Time, o el Animator no está adjunto al GameObject. |
| El sprite se ve borroso | Sprite → Filter Mode = **Point**, Compression = **None**. |
| Los backgrounds se ven encima del águila | Order in Layer mal. El águila debe tener el valor más alto (5). Los BG en negativo. |
| El Move.cs marca error en `rb.linearVelocity` | Tu Unity es muy viejo. Cambia la línea a `rb.velocity = movimiento * velocidad;`. |
| Error "ControlPlayer does not contain Player.Move" | No palomeaste **Generate C# Class** en el inputactions. Vuelve al paso 5. |
| Cinemachine no aparece en el menú | Instalalo (paso 1, nota de Cinemachine). |
| El águila se sale de la pantalla y se pierde | Normal, no tiene límites. En clase podemos poner Colliders de borde si quieres (`Edge Collider 2D` en un GameObject vacío). |

---

## 18. Scripts (contenido completo por si necesitas rehacerlos)

### `Assets/Scripts/Move.cs`

```csharp
using UnityEngine;

public class Move : MonoBehaviour
{
    private ControlPlayer controlPlayer;
    private Vector2 movimiento;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    [Header("Movimiento")]
    [SerializeField] private float velocidad = 4f;

    void Awake()
    {
        controlPlayer = new ControlPlayer();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        controlPlayer.Enable();
    }

    void OnDisable()
    {
        controlPlayer.Disable();
    }

    void FixedUpdate()
    {
        movimiento = controlPlayer.Player.Move.ReadValue<Vector2>();

        rb.linearVelocity = movimiento * velocidad;

        if (movimiento.x > 0.01f) sr.flipX = false;
        else if (movimiento.x < -0.01f) sr.flipX = true;
    }
}
```

### `Assets/Scripts/ControlPlayer.inputactions`

Ya está escrito en el proyecto. Contiene:
- Action Map: **Player**
- Actions:
  - **Move** (Value / Vector2): WASD + flechas
- **NO tiene Jump** (porque el águila vuela libre).

---

## 19. Lo que viene en clase (prepárate mentalmente)

En la clase del profe van a cubrir **triggers y colisiones** — justamente lo que falta para que el águila pueda **recoger** las gemas que ya tienes importadas. Lo que probablemente te va a enseñar es:

1. **Collider 2D con `Is Trigger` palomeado**: le vas a agregar al águila un Collider 2D (probablemente Circle o Box) y lo marcas como **Is Trigger**. Esto hace que el collider detecte "entradas" sin que el objeto físicamente choque con nada (sigue atravesando todo). Luego a cada gema le pondrás también un Collider 2D con Is Trigger.
2. **El método `OnTriggerEnter2D(Collider2D other)`**: es una función que Unity llama automáticamente cada vez que algo con un Collider 2D entra en tu trigger. Dentro de esa función comparas la etiqueta (`if (other.CompareTag("Gema"))`) y haces lo que quieras: destruir la gema (`Destroy(other.gameObject)`), sumar un contador, etc.

El flujo típico sería: creas un script `RecogerGemas.cs` que vive en el águila, con un `int gemasRecogidas = 0`, y en `OnTriggerEnter2D` sumas 1 y destruyes la gema. Opcionalmente lo muestras en pantalla con UI (TextMeshPro). Como el águila YA tiene Rigidbody 2D y las gemas tendrán Collider 2D, el motor de Unity se encarga del resto. Por eso en esta etapa dejamos el Rigidbody listo con Gravity Scale = 0 — así solo tienes que agregarle el Collider con Is Trigger en clase y ya está.

---

## 20. Entrega

Cuando termines, en tu reporte al profe menciona brevemente:
- Qué dejaste listo: platformer con águila que vuela libre en 4 ejes (sin gravedad), tilemap pintado + 3 capas de background, animación de aleteo en loop, flip horizontal, cámara con Cinemachine, SIN colliders (como indicaste).
- Qué queda para clase: agregar Collider 2D con Is Trigger al águila y a las gemas, script `OnTriggerEnter2D` para recoger gemas, contador.

Suerte bro 🦅✨
