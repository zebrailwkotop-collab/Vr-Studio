using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Менеджер для управления подключениями оборудования проводами
/// Singleton для глобального доступа
/// </summary>
public class ConnectionManager : MonoBehaviour
{
    public static ConnectionManager I { get; private set; }

    [Header("Cable Prefab")]
    [Tooltip("Префаб провода для создания подключений")]
    public GameObject cablePrefab;

    [Header("Cable Settings")]
    [Tooltip("Материал провода по умолчанию")]
    public Material defaultCableMaterial;

    [Tooltip("Толщина провода")]
    public float cableThickness = 0.01f;

    [Header("Connection Points")]
    [Tooltip("Точки подключения на инструментах (по умолчанию используется transform.position)")]
    public bool useCustomConnectionPoints = false;

    private List<Cable> allCables = new List<Cable>();

    void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        I = this;
    }

    /// <summary>
    /// Подключает инструмент к динамику через провод
    /// </summary>
    public Cable ConnectInstrumentToSpeaker(InstrumentIdentity instrument, Speaker speaker)
    {
        if (instrument == null || speaker == null)
        {
            Debug.LogError("ConnectionManager: Нельзя подключить null объекты!");
            return null;
        }

        // Проверяем, не подключен ли уже
        if (speaker.IsInstrumentConnected(instrument))
        {
            Debug.LogWarning($"Инструмент {instrument.type} уже подключен к динамику {speaker.speakerName}");
            return null;
        }

        // Создаем провод
        Cable cable = CreateCable(instrument.transform, speaker.transform);
        if (cable == null) return null;

        // Настраиваем провод
        cable.sourceInstrument = instrument;
        cable.destinationSpeaker = speaker;

        // Подключаем
        speaker.ConnectInstrument(instrument, cable);
        allCables.Add(cable);

        Debug.Log($"Подключен {instrument.type} к динамику {speaker.speakerName}");

        return cable;
    }

    /// <summary>
    /// Подключает микрофон к записывающему устройству через провод
    /// </summary>
    public Cable ConnectMicrophoneToRecorder(MicrophoneRecorder microphone, RecordingDevice recorder)
    {
        if (microphone == null || recorder == null)
        {
            Debug.LogError("ConnectionManager: Нельзя подключить null объекты!");
            return null;
        }

        // Проверяем, не подключен ли уже
        if (recorder.IsMicrophoneConnected(microphone))
        {
            Debug.LogWarning($"Микрофон {microphone.microphoneName} уже подключен к {recorder.deviceName}");
            return null;
        }

        // Создаем провод
        Cable cable = CreateCable(microphone.transform, recorder.transform);
        if (cable == null) return null;

        // Настраиваем провод
        cable.sourceMicrophone = microphone;
        cable.destinationRecorder = recorder;

        // Подключаем
        recorder.ConnectMicrophone(microphone, cable);
        allCables.Add(cable);

        Debug.Log($"Подключен микрофон {microphone.microphoneName} к {recorder.deviceName}");

        return cable;
    }

    /// <summary>
    /// Отключает инструмент от динамика
    /// </summary>
    public void DisconnectInstrumentFromSpeaker(InstrumentIdentity instrument, Speaker speaker)
    {
        if (instrument == null || speaker == null) return;

        speaker.DisconnectInstrument(instrument);

        // Удаляем провода
        for (int i = allCables.Count - 1; i >= 0; i--)
        {
            if (allCables[i] != null && 
                allCables[i].sourceInstrument == instrument && 
                allCables[i].destinationSpeaker == speaker)
            {
                allCables[i].DestroyCable();
                allCables.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Отключает микрофон от записывающего устройства
    /// </summary>
    public void DisconnectMicrophoneFromRecorder(MicrophoneRecorder microphone, RecordingDevice recorder)
    {
        if (microphone == null || recorder == null) return;

        recorder.DisconnectMicrophone(microphone);

        // Провода удаляются автоматически в DisconnectMicrophone
    }

    /// <summary>
    /// Создает провод между двумя точками
    /// </summary>
    private Cable CreateCable(Transform source, Transform destination)
    {
        GameObject cableObject;
        
        if (cablePrefab != null)
        {
            cableObject = Instantiate(cablePrefab);
        }
        else
        {
            // Создаем простой GameObject с Cable компонентом
            cableObject = new GameObject("Cable");
            cableObject.AddComponent<Cable>();
        }

        Cable cable = cableObject.GetComponent<Cable>();
        if (cable == null)
        {
            cable = cableObject.AddComponent<Cable>();
        }

        // Настраиваем провод
        cable.SetConnection(source, destination);
        cable.cableMaterial = defaultCableMaterial;
        cable.cableThickness = cableThickness;

        return cable;
    }

    /// <summary>
    /// Получает все провода
    /// </summary>
    public List<Cable> GetAllCables()
    {
        return new List<Cable>(allCables);
    }

    /// <summary>
    /// Удаляет все провода
    /// </summary>
    public void ClearAllCables()
    {
        foreach (var cable in allCables)
        {
            if (cable != null)
            {
                cable.DestroyCable();
            }
        }
        allCables.Clear();
    }

    /// <summary>
    /// Проверяет, подключен ли инструмент к какому-либо динамику
    /// </summary>
    public bool IsInstrumentConnected(InstrumentIdentity instrument)
    {
        if (instrument == null) return false;

        // Ищем все динамики в сцене
        Speaker[] speakers = FindObjectsOfType<Speaker>();
        foreach (var speaker in speakers)
        {
            if (speaker.IsInstrumentConnected(instrument))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Получает динамик, к которому подключен инструмент
    /// </summary>
    public Speaker GetConnectedSpeaker(InstrumentIdentity instrument)
    {
        if (instrument == null) return null;

        Speaker[] speakers = FindObjectsOfType<Speaker>();
        foreach (var speaker in speakers)
        {
            if (speaker.IsInstrumentConnected(instrument))
            {
                return speaker;
            }
        }
        return null;
    }
}
