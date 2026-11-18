using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Menu de pausa que aparece al presionar ESC.
// Tiene Reanudar, Opciones (con slider de volumen) y Salir al Menu.
public class PausaMenu : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject pausaPanel;
    [SerializeField] private GameObject opcionesPanel;

    [Header("Botones")]
    [SerializeField] private Button btnReanudar;
    [SerializeField] private Button btnOpciones;
    [SerializeField] private Button btnSalirMenu;
    [SerializeField] private Button btnVolverDeOpciones;

    [Header("Opciones")]
    [SerializeField] private Slider volumeSlider;

    [Header("Escena del Menu principal")]
    [SerializeField] private string escenaMenu = "Menu";

    private bool pausado = false;
    private const string VolumeKey = "MasterVolume";

    void Start()
    {
        // Estado inicial: todo apagado
        if (pausaPanel != null) pausaPanel.SetActive(false);
        if (opcionesPanel != null) opcionesPanel.SetActive(false);

        if (btnReanudar != null) btnReanudar.onClick.AddListener(Reanudar);
        if (btnOpciones != null) btnOpciones.onClick.AddListener(MostrarOpciones);
        if (btnSalirMenu != null) btnSalirMenu.onClick.AddListener(SalirAlMenu);
        if (btnVolverDeOpciones != null) btnVolverDeOpciones.onClick.AddListener(VolverDeOpciones);

        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat(VolumeKey, 1f);
            volumeSlider.onValueChanged.AddListener(SetVolume);
            AudioListener.volume = volumeSlider.value;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (opcionesPanel != null && opcionesPanel.activeSelf) VolverDeOpciones();
            else if (pausado) Reanudar();
            else Pausar();
        }
    }

    void Pausar()
    {
        pausado = true;
        if (pausaPanel != null) pausaPanel.SetActive(true);
        if (opcionesPanel != null) opcionesPanel.SetActive(false);
        Time.timeScale = 0f;
    }

    public void Reanudar()
    {
        pausado = false;
        if (pausaPanel != null) pausaPanel.SetActive(false);
        if (opcionesPanel != null) opcionesPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void MostrarOpciones()
    {
        if (pausaPanel != null) pausaPanel.SetActive(false);
        if (opcionesPanel != null) opcionesPanel.SetActive(true);
    }

    public void VolverDeOpciones()
    {
        if (opcionesPanel != null) opcionesPanel.SetActive(false);
        if (pausaPanel != null) pausaPanel.SetActive(true);
    }

    public void SalirAlMenu()
    {
        Time.timeScale = 1f; // CRITICO: si no, la nueva escena queda con timeScale=0
        SceneManager.LoadScene(escenaMenu);
    }

    void SetVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat(VolumeKey, value);
    }
}
