# Actividad Joystick + Menu de Pausa en Nivel1

## Archivos nuevos

- `Assets/Scripts/VirtualJoystick.cs` - **tal cual el codigo que dio el profe** en la actividad.
- `Assets/Scripts/AguilaController.cs` - **actualizado** para soportar joystick (sin romper el teclado).
- `Assets/Scripts/PausaMenu.cs` - script nuevo para el menu de pausa con ESC.
- `Assets/Editor/Nivel1Extras.cs` - automatiza la creacion del joystick y menu de pausa.

## Pasos en Unity

### 1. Abrir la escena Nivel1

`Assets/Scenes/Nivel1.unity` (NO la del Menu, la del juego).

### 2. Ejecutar el setup

`Menu Tools > Extras del Nivel > AGREGAR TODO (Joystick + Menu Pausa)`

Confirma. Esto hace 2 cosas:

**Joystick:**
- Crea un `CanvasUI` con `JoystickBackground` abajo izquierda (circulo grande) + `JoystickHandle` en el centro (circulo chico).
- Agrega el script `VirtualJoystick` al JoystickBackground y asigna las referencias.
- Lo conecta automaticamente al campo `Joystick` del `AguilaController` del Aguila.

**Menu de Pausa:**
- Crea un `CanvasPausa` con sortingOrder alto.
- `PausaPanel`: titulo "PAUSA" + 3 botones (Reanudar, Opciones, Salir al Menu).
- `OpcionesPausaPanel`: titulo "Opciones" + slider de volumen + boton Volver.
- `PausaController` GameObject con el script `PausaMenu` y referencias conectadas.
- Ambos paneles empiezan desactivados.

### 3. Guardar

**Ctrl+S**.

### 4. Play y probar

- Con **WASD** se mueve el aguila (como siempre).
- Con el **mouse** arrastra el joystick abajo izquierda → el aguila sigue el input.
- Si sueltas el joystick vuelve al centro y el aguila se detiene (o sigue con teclado).
- **ESC**: pausa el juego. El tiempo se congela (`Time.timeScale = 0`).
- En el menu de Pausa:
  - **Reanudar**: quita la pausa.
  - **Opciones**: muestra el subpanel con slider de volumen. Boton Volver arriba izquierda regresa al menu de pausa.
  - **Salir al Menu**: carga la escena `Menu`.
- Si vuelves a dar ESC estando pausado → reanuda. Si estas en Opciones → vuelve al menu de pausa.

## Si algo falla

| Problema | Solucion |
|---|---|
| Error "VirtualJoystick does not exist" | Asegurate que el archivo `VirtualJoystick.cs` esta en `Assets/Scripts/`. Unity recompila automatico. |
| El joystick no mueve al aguila | El campo Joystick del AguilaController esta vacio. Selecciona Aguila > en el Inspector arrastra JoystickBackground al slot Joystick. |
| ESC no pausa | El PausaController no esta en la escena, o no hay EventSystem. Rehaz el paso. |
| Al dar "Salir al Menu" no carga | La escena `Menu` no esta en Build Settings. Agregala en File > Build Profiles > Add Open Scenes. |
| El joystick se ve como cuadrado blanco | El sprite `button_round_320.png` no esta sliceado como Sprite (2D and UI). El script lo configura automatic, si fallo ve manual al Inspector del sprite. |
| El menu de pausa se ve encima del joystick (correcto) pero queda transparente | El fondo tiene alpha 0.92 para que veas el juego atras. Si quieres opaco, cambia alpha a 1.0 en Nivel1Extras.cs. |
| Al presionar Opciones en pausa, no aparece el slider | El OpcionesPausaPanel esta desactivado por error. El MostrarOpciones del script lo activa. |

## Como funciona el Joystick (resumen)

El script `VirtualJoystick` es un `MonoBehaviour` que implementa `IDragHandler`, `IPointerDownHandler`, `IPointerUpHandler` del EventSystem UI. Esto funciona IGUAL con mouse (PC) y con toque (movil) sin cambiar codigo.

- `OnDrag(eventData)`: calcula la direccion desde el centro del JoystickBackground hasta donde arrastras. Clampa la magnitud a 1. Mueve el JoystickHandle visualmente.
- `OnPointerUp`: resetea direccion a cero, regresa el handle al centro.
- `Direction`, `Horizontal`, `Vertical`: propiedades que otros scripts leen (como el AguilaController).

El `AguilaController` en su Update:
1. Lee WASD normal.
2. Si el joystick tiene magnitud > 0.01, SOBRESCRIBE la direccion con la del joystick.
3. En FixedUpdate aplica la velocidad al Rigidbody.

## Como funciona el Menu de Pausa

`PausaMenu` escucha `Input.GetKeyDown(KeyCode.Escape)` en cada frame:
- Si no esta pausado → `Pausar()` (timeScale=0, muestra pausaPanel).
- Si esta pausado y en Opciones → `VolverDeOpciones()`.
- Si esta pausado y en PausaPanel → `Reanudar()` (timeScale=1, oculta todo).

`SalirAlMenu()` restaura `Time.timeScale = 1` ANTES de cargar la escena Menu (porque si cargas con timeScale=0, la nueva escena queda congelada).

## Para explicar al profe

El joystick en movil funciona asi: Unity dispara el `IPointerDownHandler` cuando el usuario toca la pantalla. El `IDragHandler` se llama cada vez que mueve el dedo. El `IPointerUpHandler` cuando levanta el dedo. Como son eventos del EventSystem, no importa si es mouse o tactil - el mismo codigo funciona.

El menu de pausa es un patron comun: se usa `Time.timeScale = 0` para congelar toda la fisica y animaciones del juego. Para los menus que deben seguir activos (botones, slider), estos usan `Time.unscaledDeltaTime` internamente y funcionan sin problema.
