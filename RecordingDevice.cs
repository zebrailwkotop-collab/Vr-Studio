using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Компонент записывающего устройства (компьютер/микшерный пульт)
/// Добавляется на GameObject компьютера/записывающего устройства
/// </summary>
public class RecordingDevice : MonoBehaviour
{
    [Header("Device Settings")]
    [Tooltip("Название устройства (для отображения в UI)")]
    public string deviceName = "Recording Device";

    [Tooltip("Подключенные микрофоны (через провода)")]
    public MicrophoneRecorder[] connectedMicrophones = new MicrophoneRecorder[0];

    [Tooltip("Провода, подключенные к этому устройству")]
    public Cable[] connectedCables = new Cable[0];

    [Header("Visual Feedback")]
    [Tooltip("Индикатор подключения")]
    public GameObject connectionIndicator;

    void Start()
    {
        UpdateConnectionIndicator();
    }

    /// <summary>
    /// Проверяет, подключен ли микрофон к этому устройству
    /// </summary>
    public bool IsMicrophoneConnected(MicrophoneRecorder microphone)
    {
        if (connectedMicrophones == null) return false;
        
        foreach (var connected in connectedMicrophones)
        {
            if (connected == microphone)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Добавляет подключение микрофона
    /// </summary>
    public void ConnectMicrophone(MicrophoneRecorder microphone, Cable cable = null)
    {
        if (microphone == null) return;

        // Проверяем, не подключен ли уже
        if (IsMicrophoneConnected(microphone)) return;

        // Добавляем в массив
        System.Array.Resize(ref connectedMicrophones, connectedMicrophones.Length + 1);
        connectedMicrophones[connectedMicrophones.Length - 1] = microphone;

        // Подключаем микрофон к записывающему устройству
        microphone.ConnectToRecorder(cable);

        // Добавляем провод, если есть
        if (cable != null)
        {
            System.Array.Resize(ref connectedCables, connectedCables.Length + 1);
            connectedCables[connectedCables.Length - 1] = cable;
            cable.destinationRecorder = this;
        }

        UpdateConnectionIndicator();
    }

    /// <summary>
    /// Отключает микрофон
    /// </summary>
    public void DisconnectMicrophone(MicrophoneRecorder microphone)
    {
        if (connectedMicrophones == null || connectedMicrophones.Length == 0) return;

        // Удаляем из массива
        var list = new System.Collections.Generic.List<MicrophoneRecorder>(connectedMicrophones);
        if (list.Remove(microphone))
        {
            connectedMicrophones = list.ToArray();
        }

        // Отключаем микрофон
        microphone.DisconnectFromRecorder();

        // Удаляем связанные провода
        var cableList = new System.Collections.Generic.List<Cable>(connectedCables);
        for (int i = cableList.Count - 1; i >= 0; i--)
        {
            if (cableList[i] != null && cableList[i].sourceMicrophone == microphone)
            {
                cableList[i].DestroyCable();
                cableList.RemoveAt(i);
            }
        }
        connectedCables = cableList.ToArray();

        UpdateConnectionIndicator();
    }

    /// <summary>
    /// Обновляет индикатор подключения
    /// </summary>
    private void UpdateConnectionIndicator()
    {
        if (connectionIndicator != null)
        {
            bool hasConnections = connectedMicrophones != null && connectedMicrophones.Length > 0;
            connectionIndicator.SetActive(hasConnections);
        }
    }

    void OnDrawGizmos()
    {
        // Визуализация в редакторе
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.2f);
        
        // Показываем подключения
        if (connectedMicrophones != null)
        {
            Gizmos.color = Color.cyan;
            foreach (var microphone in connectedMicrophones)
            {
                if (microphone != null)
                {
                    Gizmos.DrawLine(transform.position, microphone.transform.position);
                }
            }
        }
    }
}
