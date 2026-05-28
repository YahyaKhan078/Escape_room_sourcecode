using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sound Effects")]
    public AudioClip wireConnectSound;
    public AudioClip wireDeleteSound;
    public AudioClip levelPassSound;
    public AudioClip levelFailSound;
    public AudioClip buttonClickSound;
    public AudioClip inputToggleSound;

    [Header("Background Music")]
    public AudioClip backgroundMusic;

    private AudioSource sfxSource;
    private AudioSource musicSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }

        // Two audio sources — one for SFX, one for music
        sfxSource = gameObject.AddComponent<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = 0.3f;
    }

    void Start()
    {
        if (backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlayWireConnect() => PlaySFX(wireConnectSound);
    public void PlayWireDelete() => PlaySFX(wireDeleteSound);
    public void PlayLevelPass() => PlaySFX(levelPassSound);
    public void PlayLevelFail() => PlaySFX(levelFailSound);
    public void PlayButton() => PlaySFX(buttonClickSound);
    public void PlayInputToggle() => PlaySFX(inputToggleSound);
}