using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Контроллер сцены главного меню.
/// 
/// Архитектура:
/// - MenuCanvas стоит на фиксированной точке в мире (z=1.5 от спавна)
///   и НИКУДА не следует — это позволяет игроку видеть её подойдя.
/// - MainPanel: Title + кнопки START / CONTROLS / EXIT
/// - ControlsPanel: появляется по кнопке CONTROLS, закрывается крестиком
///   содержит слайдеры Volume и Brightness
/// </summary>
public class MenuSceneManager : MonoBehaviour
{
    // ── Panels ─────────────────────────────────────────────────────
    [Header("Panels")]
    [SerializeField] GameObject mainPanel;       // Title + кнопки
    [SerializeField] GameObject controlsPanel;   // Слайдеры — скрыт по умолчанию

    // ── Main panel buttons ──────────────────────────────────────────
    [Header("Main Buttons")]
    [SerializeField] Button startButton;
    [SerializeField] Button controlsButton;
    [SerializeField] Button exitButton;

    // ── Controls panel ──────────────────────────────────────────────
    [Header("Controls Panel")]
    [SerializeField] Button closeControlsButton;
    [SerializeField] Slider volumeSlider;
    [SerializeField] Slider brightnessSlider;
    [SerializeField] TextMeshProUGUI volumeLabel;
    [SerializeField] TextMeshProUGUI brightnessLabel;

    // ── Settings ────────────────────────────────────────────────────
    [Header("Scene")]
    [SerializeField] string gameSceneName = "SampleScene";

    [Header("Brightness")]
    [SerializeField] Light directionalLight;
    [SerializeField] float minLightIntensity = 0f;
    [SerializeField] float maxLightIntensity = 3f;

    const string PREF_VOLUME     = "MasterVolume";
    const string PREF_BRIGHTNESS = "Brightness";

    void Start()
    {
        // Controls panel hidden at start
        if (controlsPanel) controlsPanel.SetActive(false);

        // Auto-find directional light
        if (!directionalLight)
            foreach (var l in FindObjectsOfType<Light>())
                if (l.type == LightType.Directional) { directionalLight = l; break; }

        // Restore saved settings
        float savedVol    = PlayerPrefs.GetFloat(PREF_VOLUME,     0.8f);
        float savedBright = PlayerPrefs.GetFloat(PREF_BRIGHTNESS, 0.33f);

        // Wire sliders
        if (volumeSlider)
        {
            volumeSlider.value = savedVol;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
        if (brightnessSlider)
        {
            brightnessSlider.value = savedBright;
            brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
        }

        // Apply saved values immediately
        AudioListener.volume = savedVol;
        ApplyBrightness(savedBright);
        UpdateLabels(savedVol, savedBright);

        // Wire buttons
        if (startButton)         startButton.onClick.AddListener(OnStart);
        if (controlsButton)      controlsButton.onClick.AddListener(OpenControls);
        if (exitButton)          exitButton.onClick.AddListener(OnExit);
        if (closeControlsButton) closeControlsButton.onClick.AddListener(CloseControls);
    }

    // ── Button callbacks ────────────────────────────────────────────
    void OnStart()
    {
        PlayerPrefs.Save();
        SceneManager.LoadScene(gameSceneName);
    }

    void OpenControls()
    {
        if (mainPanel)     mainPanel.SetActive(false);
        if (controlsPanel) controlsPanel.SetActive(true);
    }

    void CloseControls()
    {
        if (controlsPanel) controlsPanel.SetActive(false);
        if (mainPanel)     mainPanel.SetActive(true);
    }

    void OnExit()
    {
        PlayerPrefs.Save();
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // ── Slider callbacks ────────────────────────────────────────────
    void OnVolumeChanged(float val)
    {
        AudioListener.volume = val;
        PlayerPrefs.SetFloat(PREF_VOLUME, val);
        if (volumeLabel) volumeLabel.text = $"Volume: {Mathf.RoundToInt(val * 100)}%";
    }

    void OnBrightnessChanged(float val)
    {
        ApplyBrightness(val);
        PlayerPrefs.SetFloat(PREF_BRIGHTNESS, val);
        if (brightnessLabel) brightnessLabel.text = $"Brightness: {Mathf.RoundToInt(val * 100)}%";
    }

    void ApplyBrightness(float t)
    {
        if (directionalLight)
            directionalLight.intensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, t);
        RenderSettings.ambientIntensity = Mathf.Lerp(0.1f, 1.5f, t);
    }

    void UpdateLabels(float vol, float bright)
    {
        if (volumeLabel)     volumeLabel.text     = $"Volume: {Mathf.RoundToInt(vol   * 100)}%";
        if (brightnessLabel) brightnessLabel.text = $"Brightness: {Mathf.RoundToInt(bright * 100)}%";
    }
}
