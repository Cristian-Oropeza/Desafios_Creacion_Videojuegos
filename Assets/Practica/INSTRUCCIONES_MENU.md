# Actividad: Creacion de Menu con Simple Button Set 01

## Estado del proyecto (lo que ya hice)

- Copie `MenuBuilder.cs` a `Assets/Editor/` (el script que automatiza TODO el menu).
- Dejamos tal cual: `Assets/FreeButtonSet/` (los sprites), `Assets/Scripts/MenuManager.cs` (el script del profe), `Assets/Practica/` (la carpeta que te pidio el profe).

## Antes de arrancar (importante)

Al abrir Unity debe aparecer un pop-up pidiendo importar **TextMeshPro Essentials**. Si te sale, acepta (`Import TMP Essentials`). Si no te sale pero el script de abajo falla con "TMP_Essential_Resources not found", ve manual a: `Window > TextMeshPro > Import TMP Essential Resources`.

## Pasos que te tocan en Unity

### 1. Nueva escena vacia

- `File > New Scene` → **Basic 2D (URP)** → Create.
- Asi arrancamos sin basura de escenas anteriores.

### 2. Correr el setup completo

- En la barra superior: `Menu Tools > REHACER TODO (limpiar escena y setup)`.
- Confirma con **Si**. Espera unos segundos.
- **Ctrl+S** para guardar (se guarda como `Assets/Scenes/Menu.unity`).

### 3. Agregar la escena de juego a Build Settings

Para que el boton Jugar funcione:

1. `File > Build Profiles` (o `Build Settings` en versiones viejas).
2. Clic en **Add Open Scenes** para agregar `Menu.unity`.
3. Abre tu escena `Nivel1.unity` y **Add Open Scenes** otra vez.
4. Asegurate que la primera escena sea **Menu** (arrastra para reordenar si no).

### 4. Arrastrar un AudioClip (opcional)

- En la Hierarchy selecciona `MusicPlayer`.
- En el Inspector busca el campo **AudioClip**.
- Arrastra cualquier `.mp3` o `.wav` de tu proyecto (si tienes uno).
- Si no tienes audio, dejalo vacio, funciona igual.

### 5. Darle Play

Deberias ver:
- Un titulo grande **"Game Scholar"**.
- 4 botones apilados: Jugar, Opciones, Creditos, Salir (cada uno con su icono).
- Al presionar **Opciones**: aparece OptionsPanel con el slider de volumen, toggle Musica, toggle Efectos y boton Volver arriba izquierda.
- Al presionar **Creditos**: aparece CreditsPanel con texto y Volver.
- El slider de volumen controla el AudioListener.
- El boton **Salir** cierra el juego (en editor, detiene el Play).
- El boton **Jugar** carga la escena `Nivel1`.

## Que cumple de la actividad (checklist)

| Lo que pide el PDF | Como lo cumplimos |
|---|---|
| Importar Simple Button Set 01 | Ya importado como `FreeButtonSet` |
| Sprites con Texture Type Sprite (2D and UI) | El script lo hace en el paso 1 |
| 9-slice en los botones | El script lo configura (button_100, 200, round_130, etc) |
| Canvas con Scale With Screen Size 1920x1080 Match 0.5 | Automatico |
| 3 paneles: MainPanel, OptionsPanel, CreditsPanel | Automatico con color panel 412402 |
| Vertical Layout Group en MainPanel | Spacing 20, Middle Center |
| Botones alargados 600x100 Sliced con texto y colores Normal/Highlighted/Pressed | Automatico |
| Icono 64x64 con Layout Element Preferred 48x48 antes del texto | Automatico |
| Fuente TMP 60pt para botones | Automatico |
| Slider con sprites del set (Background, Fill, Handle) | Automatico, value=0.7, min=0, max=1 |
| Toggle con Background y Checkmark del set | Automatico (Musica y Efectos, 2 toggles) |
| Boton Volver redondo en Top Left con Ignore Layout | Automatico, posicion 70,-70, tamaño 90x90 |
| OptionsPanel Vertical Layout Padding 120/120/80/80 Spacing 40 | Automatico |
| MenuManager con todas las referencias conectadas | El script las asigna automatico por reflection |
| AudioSource "MusicPlayer" | Automatico (solo falta el AudioClip) |
| Escena de juego en Build Settings | Lo tienes que hacer manual (paso 3 de arriba) |

## Si algo sale mal

| Problema | Solucion |
|---|---|
| Menu Tools no aparece | Error de compilacion. Mira la consola. El script usa TMPro, asegurate de importar TMP Essentials. |
| Botones se ven como cuadros blancos | El 9-slice fallo. En el paso 1 el script configura los borders de cada sprite. Si fallo, ve manual a cada sprite y en el Inspector dale Sprite Editor > Border L/R/T/B. |
| El titulo "Game Scholar" no sale o sale en cuadrados | TMP Essentials no importado. Window > TextMeshPro > Import TMP Essential Resources. |
| Al presionar Jugar sale error | La escena Nivel1 no esta en Build Settings. Paso 3. |
| El slider no se mueve | El Fill Rect o Handle Rect no se asigno. Selecciona el Slider, en el Inspector verifica que Fill Rect apunte al hijo Fill y Handle Rect al hijo Handle. |
| El toggle no toggles | El Target Graphic o Graphic no se asigno. Selecciona el Toggle y verifica en Inspector: Target Graphic = Background, Graphic = Checkmark. |
| El boton Volver no vuelve al main | El backButtons[] del MenuManager no incluye ese boton. Revisa el Inspector del MenuManager > Back Buttons debe tener 2 elementos. |
| Error "enableWordWrapping deprecated" | Si tu version de TMP es muy nueva, cambia esa linea en MenuBuilder.cs por `tmp.textWrappingMode = TextWrappingModes.NoWrap;` |

## Para explicar al profe

El `MenuManager.cs` (que el profe te dio) tiene:

- **Awake():** conecta los `onClick` de cada boton a su accion (Play, Show otro panel, Quit). Los back buttons (array) todos vuelven al mainPanel.
- **Start():** muestra el mainPanel (oculta los otros).
- **Show(target):** activa solo el panel que recibe, desactiva los otros.
- **Play():** carga la escena por nombre (`SceneManager.LoadScene`).
- **SetVolume(value):** cambia el volumen global (`AudioListener.volume`) y lo guarda en `PlayerPrefs` para que persista entre sesiones.
- **Quit():** en editor detiene el Play, en build cierra la app.
- **Click():** reproduce un sonido corto al hacer click (si hay clip).
- **muteToggle:** cuando lo activas/desactivas, mutea el AudioSource de UI.

El `MenuBuilder.cs` (que YO escribi) **no** forma parte del juego, es solo una herramienta de Editor que arma la UI por ti con un clic. En el build final del juego no se incluye (va dentro de `#if UNITY_EDITOR`).

## Pasos individuales (si quieres debuggear uno por uno)

- `Menu Tools > Pasos individuales > 1. Configurar imports de sprites (9-slice)`
- `Menu Tools > Pasos individuales > 2. Crear Canvas`
- `Menu Tools > Pasos individuales > 3. Crear 3 Paneles (Main, Options, Credits)`
- `Menu Tools > Pasos individuales > 4. Crear botones del MainPanel`
- `Menu Tools > Pasos individuales > 5. Crear elementos del OptionsPanel`
- `Menu Tools > Pasos individuales > 6. Crear elementos del CreditsPanel`
- `Menu Tools > Pasos individuales > 7. Crear MenuManager + MusicPlayer`
- `Menu Tools > Pasos individuales > 8. Conectar referencias del MenuManager`
- `Menu Tools > Pasos individuales > 9. Guardar escena como Menu`
