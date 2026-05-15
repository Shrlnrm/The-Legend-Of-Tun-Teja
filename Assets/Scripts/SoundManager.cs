using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music Clips")]
    public AudioClip mainMenuMusic;
    public AudioClip levelMusic;
    public AudioClip lieCutsceneMusic;   // NEW
    public AudioClip truthCutsceneMusic; // NEW

    [Header("Scene Names (Type Exact Names)")]
    public string truthCutsceneSceneName = "Cutscene_Truth"; // NEW: Inspector slot
    public string lieCutsceneSceneName = "Cutscene_Lie";     // NEW: Inspector slot

    [Header("SFX Clips")]
    public AudioClip tokenPickupSFX;
    public AudioClip jumpSFX;
    public AudioClip dodgeSFX;
    public AudioClip buttonClickSFX;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
        }
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        float masterVol = PlayerPrefs.GetFloat("MasterVolume", 1f);
        AudioListener.volume = masterVol;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;

        // Logic to switch music based on scene name
        if (sceneName == "Main Menu" || sceneName == "MainMenu")
        {
            PlayMusic(mainMenuMusic);
        }
        else if (sceneName == truthCutsceneSceneName)
        {
            PlayMusic(truthCutsceneMusic);
        }
        else if (sceneName == lieCutsceneSceneName)
        {
            PlayMusic(lieCutsceneMusic);
        }
        else
        {
            // Default Level Music for everything else
            PlayMusic(levelMusic);
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;

        // Prevent restarting the same song if it's already playing
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayTokenSound() { PlaySFX(tokenPickupSFX); }
    public void PlayJumpSound() { PlaySFX(jumpSFX); }
    public void PlayDodgeSound() { PlaySFX(dodgeSFX); }
    public void PlayButtonSound() { PlaySFX(buttonClickSFX); }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
    }
}