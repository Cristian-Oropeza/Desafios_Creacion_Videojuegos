using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestor simple de menús con paneles, navegación y botones.
/// Uso: añádelo a un GameObject vacío, asigna los paneles y botones en el Inspector.
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Botones menú principal")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;

    [Header("Botones 'Volver'")]
    [SerializeField] private Button[] backButtons;

    [Header("Opciones")]
    [SerializeField] private Slider volumeSlider;

    [Header("Escena de juego")]
    [SerializeField] private string gameSceneName = "Game";

    [Header("Audio de UI (opcional)")]
    [SerializeField] private Toggle muteToggle;
    [SerializeField] private AudioSource uiAudio;
    [SerializeField] private AudioClip clickClip;

    private const string VolumeKey = "MasterVolume";

    private void Awake()
    {
        playButton.onClick.AddListener(() => { Click(); Play(); });
        optionsButton.onClick.AddListener(() => { Click(); Show(optionsPanel); });
        creditsButton.onClick.AddListener(() => { Click(); Show(creditsPanel); });
        quitButton.onClick.AddListener(() => { Click(); Quit(); });

        muteToggle.onValueChanged.AddListener(isOn => uiAudio.mute = !isOn);
        foreach (var b in backButtons)
            b.onClick.AddListener(() => { Click(); Show(mainPanel); });

        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat(VolumeKey, 1f);
            volumeSlider.onValueChanged.AddListener(SetVolume);
            AudioListener.volume = volumeSlider.value;
        }
    }

    private void Start() => Show(mainPanel);

    private void Show(GameObject target)
    {
        mainPanel.SetActive(target == mainPanel);
        optionsPanel.SetActive(target == optionsPanel);
        creditsPanel.SetActive(target == creditsPanel);
    }

    private void Play() => SceneManager.LoadScene(gameSceneName);

    private void SetVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat(VolumeKey, value);
    }

    private void Click()
    {
        if (uiAudio != null && clickClip != null)
            uiAudio.PlayOneShot(clickClip);
    }

    private void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
