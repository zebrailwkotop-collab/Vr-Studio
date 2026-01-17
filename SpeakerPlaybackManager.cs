using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Менеджер для воспроизведения записанных треков через динамики
/// Интегрируется с TrackManager и ConnectionManager
/// </summary>
public class SpeakerPlaybackManager : MonoBehaviour
{
    public static SpeakerPlaybackManager I { get; private set; }

    [Header("Playback Settings")]
    [Tooltip("Автоматически находить динамики в сцене")]
    public bool autoFindSpeakers = true;

    [Tooltip("Список динамиков для воспроизведения (если не используется autoFindSpeakers)")]
    public Speaker[] speakers = new Speaker[0];

    [Tooltip("Громкость воспроизведения по умолчанию")]
    [Range(0f, 1f)]
    public float defaultVolume = 1f;

    private Dictionary<InstrumentType, Speaker> instrumentSpeakerMap = new Dictionary<InstrumentType, Speaker>();

    void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        I = this;
    }

    void Start()
    {
        if (autoFindSpeakers)
        {
            FindAllSpeakers();
        }
    }

    /// <summary>
    /// Находит все динамики в сцене
    /// </summary>
    private void FindAllSpeakers()
    {
        speakers = FindObjectsOfType<Speaker>();
        Debug.Log($"SpeakerPlaybackManager: Найдено {speakers.Length} динамиков");
    }

    /// <summary>
    /// Воспроизводит трек инструмента через подключенный динамик
    /// </summary>
    public void PlayTrack(InstrumentType instrumentType)
    {
        if (TrackManager.I == null)
        {
            Debug.LogError("SpeakerPlaybackManager: TrackManager не найден!");
            return;
        }

        AudioClip clip = TrackManager.I.GetTrack(instrumentType);
        if (clip == null)
        {
            Debug.LogWarning($"SpeakerPlaybackManager: Трек для {instrumentType} не найден!");
            return;
        }

        // Находим динамик, к которому подключен инструмент
        Speaker speaker = GetSpeakerForInstrument(instrumentType);
        if (speaker == null)
        {
            Debug.LogWarning($"SpeakerPlaybackManager: Динамик для {instrumentType} не найден или не подключен!");
            return;
        }

        // Воспроизводим через динамик
        speaker.PlayClip(clip, defaultVolume);
        Debug.Log($"SpeakerPlaybackManager: Воспроизведение {instrumentType} через {speaker.speakerName}");
    }

    /// <summary>
    /// Останавливает воспроизведение трека инструмента
    /// </summary>
    public void StopTrack(InstrumentType instrumentType)
    {
        Speaker speaker = GetSpeakerForInstrument(instrumentType);
        if (speaker != null)
        {
            speaker.Stop();
        }
    }

    /// <summary>
    /// Воспроизводит все треки через подключенные динамики
    /// </summary>
    public void PlayAllTracks()
    {
        if (TrackManager.I == null)
        {
            Debug.LogError("SpeakerPlaybackManager: TrackManager не найден!");
            return;
        }

        List<InstrumentType> recordedInstruments = TrackManager.I.GetRecordedInstruments();
        
        foreach (var instrumentType in recordedInstruments)
        {
            PlayTrack(instrumentType);
        }
    }

    /// <summary>
    /// Останавливает все треки
    /// </summary>
    public void StopAllTracks()
    {
        if (speakers == null) return;

        foreach (var speaker in speakers)
        {
            if (speaker != null)
            {
                speaker.Stop();
            }
        }
    }

    /// <summary>
    /// Получает динамик для инструмента
    /// </summary>
    private Speaker GetSpeakerForInstrument(InstrumentType instrumentType)
    {
        // Проверяем кэш
        if (instrumentSpeakerMap.ContainsKey(instrumentType))
        {
            Speaker cached = instrumentSpeakerMap[instrumentType];
            if (cached != null && cached.IsInstrumentConnected(GetInstrumentByIdentity(instrumentType)))
            {
                return cached;
            }
            else
            {
                // Удаляем из кэша, если больше не подключен
                instrumentSpeakerMap.Remove(instrumentType);
            }
        }

        // Ищем инструмент в сцене
        InstrumentIdentity instrument = GetInstrumentByIdentity(instrumentType);
        if (instrument == null)
        {
            Debug.LogWarning($"SpeakerPlaybackManager: Инструмент {instrumentType} не найден в сцене!");
            return null;
        }

        // Ищем подключенный динамик через ConnectionManager
        if (ConnectionManager.I != null)
        {
            Speaker speaker = ConnectionManager.I.GetConnectedSpeaker(instrument);
            if (speaker != null)
            {
                // Кэшируем
                instrumentSpeakerMap[instrumentType] = speaker;
                return speaker;
            }
        }

        // Если ConnectionManager не используется, ищем первый доступный динамик
        if (speakers != null && speakers.Length > 0)
        {
            Speaker firstSpeaker = speakers[0];
            instrumentSpeakerMap[instrumentType] = firstSpeaker;
            return firstSpeaker;
        }

        return null;
    }

    /// <summary>
    /// Получает InstrumentIdentity по типу инструмента
    /// </summary>
    private InstrumentIdentity GetInstrumentByIdentity(InstrumentType instrumentType)
    {
        InstrumentIdentity[] allInstruments = FindObjectsOfType<InstrumentIdentity>();
        foreach (var instrument in allInstruments)
        {
            if (instrument.type == instrumentType)
            {
                return instrument;
            }
        }
        return null;
    }

    /// <summary>
    /// Проверяет, подключены ли все инструменты к динамикам
    /// </summary>
    public bool AreAllInstrumentsConnected()
    {
        if (TrackManager.I == null) return false;

        List<InstrumentType> recordedInstruments = TrackManager.I.GetRecordedInstruments();
        
        foreach (var instrumentType in recordedInstruments)
        {
            InstrumentIdentity instrument = GetInstrumentByIdentity(instrumentType);
            if (instrument == null) continue;

            if (ConnectionManager.I == null || !ConnectionManager.I.IsInstrumentConnected(instrument))
            {
                return false;
            }
        }
        return true;
    }
}
