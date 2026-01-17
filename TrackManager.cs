using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Управляет записанными треками для каждого инструмента
/// </summary>
public class TrackManager : MonoBehaviour
{
    public static TrackManager I { get; private set; }

    private Dictionary<InstrumentType, AudioClip> recordedTracks = new Dictionary<InstrumentType, AudioClip>();
    private Dictionary<InstrumentType, AudioSource> playbackSources = new Dictionary<InstrumentType, AudioSource>();

    [Header("Playback Settings")]
    public AudioMixerGroup masterMixerGroup;
    public bool autoCreatePlaybackSources = true;

    void Awake()
    {
        if (I == null)
        {
            I = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Сохраняет записанный трек для инструмента
    /// </summary>
    public void SetTake(InstrumentType instrumentType, AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning($"Attempted to save null clip for {instrumentType}");
            return;
        }

        recordedTracks[instrumentType] = clip;
        Debug.Log($"Track saved for {instrumentType}: {clip.name} ({clip.length:F2}s)");

        // Создаем AudioSource для воспроизведения, если нужно
        if (autoCreatePlaybackSources && !playbackSources.ContainsKey(instrumentType))
        {
            CreatePlaybackSource(instrumentType);
        }
    }

    /// <summary>
    /// Получает записанный трек для инструмента
    /// </summary>
    public AudioClip GetTrack(InstrumentType instrumentType)
    {
        recordedTracks.TryGetValue(instrumentType, out AudioClip clip);
        return clip;
    }

    /// <summary>
    /// Проверяет, есть ли записанный трек для инструмента
    /// </summary>
    public bool HasTrack(InstrumentType instrumentType)
    {
        return recordedTracks.ContainsKey(instrumentType) && recordedTracks[instrumentType] != null;
    }

    /// <summary>
    /// Воспроизводит трек для конкретного инструмента
    /// </summary>
    public void PlayTrack(InstrumentType instrumentType)
    {
        if (!HasTrack(instrumentType))
        {
            Debug.LogWarning($"No track recorded for {instrumentType}");
            return;
        }

        if (!playbackSources.ContainsKey(instrumentType))
        {
            CreatePlaybackSource(instrumentType);
        }

        var source = playbackSources[instrumentType];
        source.clip = recordedTracks[instrumentType];
        source.Play();
        Debug.Log($"Playing track: {instrumentType}");
    }

    /// <summary>
    /// Останавливает воспроизведение трека
    /// </summary>
    public void StopTrack(InstrumentType instrumentType)
    {
        if (playbackSources.TryGetValue(instrumentType, out AudioSource source))
        {
            source.Stop();
        }
    }

    /// <summary>
    /// Воспроизводит все записанные треки одновременно
    /// </summary>
    public void PlayAllTracks()
    {
        foreach (var kvp in recordedTracks)
        {
            if (kvp.Value != null)
            {
                PlayTrack(kvp.Key);
            }
        }
        Debug.Log("Playing all tracks");
    }

    /// <summary>
    /// Останавливает все треки
    /// </summary>
    public void StopAllTracks()
    {
        foreach (var source in playbackSources.Values)
        {
            if (source != null)
            {
                source.Stop();
            }
        }
        Debug.Log("Stopped all tracks");
    }

    /// <summary>
    /// Проверяет, играет ли трек
    /// </summary>
    public bool IsTrackPlaying(InstrumentType instrumentType)
    {
        if (playbackSources.TryGetValue(instrumentType, out AudioSource source))
        {
            return source.isPlaying;
        }
        return false;
    }

    /// <summary>
    /// Создает AudioSource для воспроизведения трека
    /// </summary>
    private void CreatePlaybackSource(InstrumentType instrumentType)
    {
        GameObject sourceObj = new GameObject($"PlaybackSource_{instrumentType}");
        sourceObj.transform.SetParent(transform);
        
        AudioSource source = sourceObj.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        
        if (masterMixerGroup != null)
        {
            source.outputAudioMixerGroup = masterMixerGroup;
        }

        playbackSources[instrumentType] = source;
    }

    /// <summary>
    /// Очищает все треки
    /// </summary>
    public void ClearAllTracks()
    {
        StopAllTracks();
        recordedTracks.Clear();
        
        foreach (var source in playbackSources.Values)
        {
            if (source != null)
            {
                Destroy(source.gameObject);
            }
        }
        playbackSources.Clear();
        
        Debug.Log("All tracks cleared");
    }

    /// <summary>
    /// Получает список всех записанных инструментов
    /// </summary>
    public List<InstrumentType> GetRecordedInstruments()
    {
        List<InstrumentType> instruments = new List<InstrumentType>();
        foreach (var kvp in recordedTracks)
        {
            if (kvp.Value != null)
            {
                instruments.Add(kvp.Key);
            }
        }
        return instruments;
    }
}
