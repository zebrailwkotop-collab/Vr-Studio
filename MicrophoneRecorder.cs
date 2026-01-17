using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Компонент микрофона для записи звука
/// Добавляется на GameObject микрофона в сцене
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class MicrophoneRecorder : MonoBehaviour
{
    [Header("Microphone Settings")]
    [Tooltip("Название микрофона (для отображения в UI)")]
    public string microphoneName = "Microphone";

    [Tooltip("Инструмент, который записывает этот микрофон")]
    public InstrumentIdentity targetInstrument;

    [Header("Connection")]
    [Tooltip("Подключен ли микрофон к компьютеру/записывающему устройству")]
    public bool isConnectedToRecorder = false;

    [Tooltip("Провод, подключенный к этому микрофону")]
    public Cable connectedCable;

    [Header("Audio Settings")]
    [Tooltip("AudioMixerGroup для этого микрофона")]
    public AudioMixerGroup inputMixerGroup;

    private AudioSource audioSource;
    private AudioSourceRecorder recorder;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Настройка AudioSource
        audioSource.outputAudioMixerGroup = inputMixerGroup;
        audioSource.spatialBlend = 1f; // 3D звук
        audioSource.playOnAwake = false;
        audioSource.volume = 1f;

        // Получаем AudioSourceRecorder с инструмента
        if (targetInstrument != null && targetInstrument.source != null)
        {
            recorder = targetInstrument.source.GetComponent<AudioSourceRecorder>();
        }
    }

    /// <summary>
    /// Проверяет, готов ли микрофон к записи
    /// </summary>
    public bool IsReadyToRecord()
    {
        if (!isConnectedToRecorder)
        {
            Debug.LogWarning($"Microphone {microphoneName}: Не подключен к записывающему устройству!");
            return false;
        }

        if (targetInstrument == null)
        {
            Debug.LogWarning($"Microphone {microphoneName}: Не назначен целевой инструмент!");
            return false;
        }

        if (recorder == null)
        {
            Debug.LogWarning($"Microphone {microphoneName}: AudioSourceRecorder не найден на инструменте!");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Получает AudioSourceRecorder целевого инструмента
    /// </summary>
    public AudioSourceRecorder GetRecorder()
    {
        return recorder;
    }

    /// <summary>
    /// Подключает микрофон к записывающему устройству
    /// </summary>
    public void ConnectToRecorder(Cable cable = null)
    {
        isConnectedToRecorder = true;
        if (cable != null)
        {
            connectedCable = cable;
        }
    }

    /// <summary>
    /// Отключает микрофон от записывающего устройства
    /// </summary>
    public void DisconnectFromRecorder()
    {
        isConnectedToRecorder = false;
        connectedCable = null;
    }

    void OnDrawGizmos()
    {
        // Визуализация в редакторе
        Gizmos.color = isConnectedToRecorder ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.08f);
        
        // Показываем подключение к инструменту
        if (targetInstrument != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, targetInstrument.transform.position);
        }
    }
}
