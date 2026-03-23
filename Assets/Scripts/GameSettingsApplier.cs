using UnityEngine;

/// <summary>
/// Применяет настройки (volume, brightness) сохранённые в меню
/// при загрузке игровой сцены.
///
/// Ставить на XR Origin в SampleScene (или любой постоянный GO).
/// </summary>
public class GameSettingsApplier : MonoBehaviour
{
    const string PREF_VOLUME     = "MasterVolume";
    const string PREF_BRIGHTNESS = "Brightness";

    [SerializeField] float minLightIntensity = 0.0f;
    [SerializeField] float maxLightIntensity = 3.0f;

    void Start()
    {
        float vol    = PlayerPrefs.GetFloat(PREF_VOLUME,     0.8f);
        float bright = PlayerPrefs.GetFloat(PREF_BRIGHTNESS, 0.33f);

        // Volume
        AudioListener.volume = vol;

        // Brightness
        var dl = FindDirectionalLight();
        if (dl) dl.intensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, bright);
        RenderSettings.ambientIntensity = Mathf.Lerp(0.1f, 1.5f, bright);
    }

    Light FindDirectionalLight()
    {
        foreach (var l in FindObjectsOfType<Light>())
            if (l.type == LightType.Directional) return l;
        return null;
    }
}
