// The AudioManager class is responsible for managing audio playback in the game.
// It uses a pool of AudioSource objects to play sound effects and music efficiently.
// This class extends the SingletonManager to ensure only one instance exists in the scene.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AudioPriority
{
    High = 0,
    Medium = 128,
    Low = 256
}

[System.Serializable]
public class AudioSettings
{
    public AudioClip[] Clips;
    public float Volume = 1.0f;
    public float Pitch = 1.0f;
    public bool Loop = false;
    public float SpatialBlend = 1.0f;
    public float MinDistance = 1f;
    public float MaxDistance = 15f;
    public AudioPriority Priority = AudioPriority.Medium;
    public AudioRolloffMode RolloffMode = AudioRolloffMode.Linear;
}

public class AudioManager : SingletonManager<AudioManager>
{
    // The AudioSource used for playing background music.
    [SerializeField] private AudioSource m_MusicSource;
    // The initial size of the audio source pool.
    [SerializeField] private int m_InitialPoolSize = 10;
    // The audio settings for UI click sounds.
    [SerializeField] private AudioSettings m_UiClickAudioSettings;

    // A queue to store available AudioSource objects for reuse.
    private Queue<AudioSource> m_AudioSourcePool;
    // A list to keep track of active AudioSource objects.
    private List<AudioSource> m_ActiveSources;

    // This method is called when the script instance is being loaded.
    // It initializes the audio source pool and ensures the object is not destroyed on scene load.
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        InitializeAudioPool();
    }

    // Plays the UI click sound using the predefined settings.
    public void PlayBtnClick()
    {
        PlaySound(m_UiClickAudioSettings, Vector3.zero);
    }

    // Plays background music using the provided audio settings.
    public void PlayMusic(AudioSettings settings)
    {
        if (settings == null || settings.Clips.Length == 0) return;

        ConfigureAudioSource(m_MusicSource, settings);
        m_MusicSource.Play();
    }

    // Plays a sound effect at the specified position using the provided audio settings.
    public void PlaySound(AudioSettings audioSettings, Vector3 position)
    {
        if (audioSettings == null || audioSettings.Clips.Length == 0) return;

        var source = GetAvailableAudioSource();
        ConfigureAudioSource(source, audioSettings);
        source.transform.position = position;
        source.Play();

        if (!source.loop)
        {
            StartCoroutine(ReturnToPoolWhenDone(source));
        }
    }

    // Coroutine to return the AudioSource to the pool when it is done playing.
    IEnumerator ReturnToPoolWhenDone(AudioSource source)
    {
        yield return new WaitWhile(() => source.isPlaying);
        StopAndReturnToPool(source);
    }

    // Stops the AudioSource and returns it to the pool.
    void StopAndReturnToPool(AudioSource source)
    {
        source.Stop();
        m_ActiveSources.Remove(source);
        m_AudioSourcePool.Enqueue(source);
    }

    // Configures the AudioSource with the provided audio settings.
    void ConfigureAudioSource(AudioSource source, AudioSettings settings)
    {
        source.clip = settings.Clips[Random.Range(0, settings.Clips.Length)];
        source.volume = settings.Volume;
        source.pitch = settings.Pitch;
        source.loop = settings.Loop;
        source.spatialBlend = settings.SpatialBlend;
        source.minDistance = settings.MinDistance;
        source.maxDistance = settings.MaxDistance;
        source.priority = (int)settings.Priority;
        source.rolloffMode = settings.RolloffMode;
    }

    // Retrieves an available AudioSource from the pool or creates new ones if the pool is empty.
    AudioSource GetAvailableAudioSource()
    {
        if (m_AudioSourcePool.Count <= 0)
        {
            for (int i = 0; i < m_InitialPoolSize; i++)
            {
                CreateAudioSourceObject();
            }
        }

        AudioSource source = m_AudioSourcePool.Dequeue();
        m_ActiveSources.Add(source);
        return source;
    }

    // Initializes the audio source pool with the initial pool size.
    void InitializeAudioPool()
    {
        m_AudioSourcePool = new();
        m_ActiveSources = new();

        for (int i = 0; i < m_InitialPoolSize; i++)
        {
            CreateAudioSourceObject();
        }
    }

    // Creates a new AudioSource object and adds it to the pool.
    void CreateAudioSourceObject()
    {
        GameObject audioObject = new("PooledAudioSource");
        audioObject.transform.SetParent(transform);
        AudioSource audioSource = audioObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        m_AudioSourcePool.Enqueue(audioSource);
    }
}
