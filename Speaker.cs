using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Компонент динамика для воспроизведения звука
/// Добавляется на GameObject динамика в сцене
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class Speaker : MonoBehaviour
{
    [Header("Speaker Settings")]
    [Tooltip("Название динамика (для отображения в UI)")]
    public string speakerName = "Speaker";

    [Tooltip("Максимальная громкость динамика")]
    [Range(0f, 1f)]
    public float maxVolume = 1f;

    [Header("Audio Settings")]
    [Tooltip("AudioMixerGroup для этого динамика")]
    public AudioMixerGroup outputMixerGroup;

    [Tooltip("3D звук (true) или 2D (false)")]
    public bool spatialSound = true;

    [Header("Connection")]
    [Tooltip("Подключенные инструменты (через провода)")]
    public InstrumentIdentity[] connectedInstruments = new InstrumentIdentity[0];

    [Tooltip("Провода, подключенные к этому динамику")]
    public Cable[] connectedCables = new Cable[0];

    private AudioSource audioSource;
    private bool isPlaying = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Настройка AudioSource
        audioSource.outputAudioMixerGroup = outputMixerGroup;
        audioSource.spatialBlend = spatialSound ? 1f : 0f;
        audioSource.playOnAwake = false;
        audioSource.volume = maxVolume;
    }

    /// <summary>
    /// Воспроизводит AudioClip через динамик
    /// </summary>
    public void PlayClip(AudioClip clip, float volume = 1f)
    {
        if (clip == null)
        {
            Debug.LogWarning($"Speaker {speakerName}: Попытка воспроизвести null AudioClip");
            return;
        }

        audioSource.clip = clip;
        audioSource.volume = maxVolume * volume;
        audioSource.Play();
        isPlaying = true;
    }

    /// <summary>
    /// Останавливает воспроизведение
    /// </summary>
    public void Stop()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
        isPlaying = false;
    }

    /// <summary>
    /// Проверяет, воспроизводится ли звук
    /// </summary>
    public bool IsPlaying()
    {
        return isPlaying && audioSource != null && audioSource.isPlaying;
    }

    /// <summary>
    /// Устанавливает громкость
    /// </summary>
    public void SetVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = Mathf.Clamp01(volume) * maxVolume;
        }
    }

    /// <summary>
    /// Получает текущую громкость
    /// </summary>
    public float GetVolume()
    {
        return audioSource != null ? audioSource.volume / maxVolume : 0f;
    }

    /// <summary>
    /// Проверяет, подключен ли инструмент к этому динамику
    /// </summary>
    public bool IsInstrumentConnected(InstrumentIdentity instrument)
    {
        if (connectedInstruments == null) return false;
        
        foreach (var connected in connectedInstruments)
        {
            if (connected == instrument)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Добавляет подключение инструмента
    /// </summary>
    public void ConnectInstrument(InstrumentIdentity instrument, Cable cable = null)
    {
        if (instrument == null) return;

        // Проверяем, не подключен ли уже
        if (IsInstrumentConnected(instrument)) return;

        // Добавляем в массив
        System.Array.Resize(ref connectedInstruments, connectedInstruments.Length + 1);
        connectedInstruments[connectedInstruments.Length - 1] = instrument;

        // Добавляем провод, если есть
        if (cable != null)
        {
            System.Array.Resize(ref connectedCables, connectedCables.Length + 1);
            connectedCables[connectedCables.Length - 1] = cable;
        }
    }

    /// <summary>
    /// Отключает инструмент
    /// </summary>
    public void DisconnectInstrument(InstrumentIdentity instrument)
    {
        if (connectedInstruments == null || connectedInstruments.Length == 0) return;

        // Удаляем из массива
        var list = new System.Collections.Generic.List<InstrumentIdentity>(connectedInstruments);
        if (list.Remove(instrument))
        {
            connectedInstruments = list.ToArray();
        }

        // Удаляем связанные провода
        var cableList = new System.Collections.Generic.List<Cable>(connectedCables);
        for (int i = cableList.Count - 1; i >= 0; i--)
        {
            if (cableList[i] != null && cableList[i].sourceInstrument == instrument)
            {
                cableList.RemoveAt(i);
            }
        }
        connectedCables = cableList.ToArray();
    }

    void OnDrawGizmos()
    {
        // Визуализация в редакторе
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
        
        // Показываем подключения
        if (connectedInstruments != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var instrument in connectedInstruments)
            {
                if (instrument != null)
                {
                    Gizmos.DrawLine(transform.position, instrument.transform.position);
                }
            }
        }
    }
}
