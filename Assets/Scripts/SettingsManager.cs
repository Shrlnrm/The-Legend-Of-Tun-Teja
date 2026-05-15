using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("Audio Settings")]
    public Slider volumeSlider;

    // Optional: Set this in Inspector to limit max loudness (e.g., 0.5)
    // If kept at 1, it behaves normally.
    [Range(0f, 1f)] public float maxVolumeCap = 1f;

    void Start()
    {
        // 1. Determine what the "Max" volume should be. 
        // If you want "Current Volume" as Max, we capture it here.
        // Or we use the maxVolumeCap variable. 
        // Let's assume you want the slider to range from 0 to 1, but 1 maps to maxVolumeCap.

        // Load saved volume (normalized 0-1). Default to 0.5 if no key exists.
        float savedNormalizedVolume = PlayerPrefs.GetFloat("MasterVolume", 0.5f);

        if (volumeSlider != null)
        {
            // Set slider to display 0-1 range regardless of actual volume cap
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = savedNormalizedVolume;

            volumeSlider.onValueChanged.RemoveAllListeners();
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        // Apply the volume immediately
        ApplyVolume(savedNormalizedVolume);
    }

    void OnVolumeChanged(float normalizedValue)
    {
        ApplyVolume(normalizedValue);

        // Save the normalized value (0-1)
        PlayerPrefs.SetFloat("MasterVolume", normalizedValue);
    }

    void ApplyVolume(float normalizedValue)
    {
        // Map 0-1 slider to 0-maxVolumeCap actual volume
        float actualVolume = normalizedValue * maxVolumeCap;

        if (SoundManager.instance != null)
        {
            SoundManager.instance.SetVolume(actualVolume);
        }
        else
        {
            AudioListener.volume = actualVolume;
        }
    }
}