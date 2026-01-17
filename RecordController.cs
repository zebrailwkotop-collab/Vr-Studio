using System.Collections;
using UnityEngine;

public class RecordController : MonoBehaviour
{
    public float countdownSeconds = 5f;
    public float maxRecordSeconds = 10f;
    private Coroutine recordingRoutine;
    private AudioSourceRecorder currentRecorder;
    private InstrumentIdentity currentInstrument;
    public bool IsRecording { get; private set; }

    public void StartRecordingSelected()
    {
        if (IsRecording)
        {
            Debug.LogWarning("Запись уже идет!");
            return;
        }

        if (InstrumentSelector.I.HasSelection)
        {
            var instrument = InstrumentSelector.I.Current;
            
            // Проверяем подключение микрофона
            if (!CheckMicrophoneConnection(instrument))
            {
                Debug.LogError($"Нельзя записывать {instrument.type}: микрофон не подключен к записывающему устройству!");
                return;
            }
            
            var recorder = instrument.GetComponent<AudioSourceRecorder>();
            if (recorder != null)
            {
                currentInstrument = instrument;
                currentRecorder = recorder;
                recordingRoutine = StartCoroutine(RecordFlow(instrument, recorder));
            }
            else
            {
                Debug.LogError($"AudioSourceRecorder не найден на инструменте {instrument.type}!");
            }
        }
        else
        {
            Debug.LogWarning("Инструмент не выбран!");
        }
    }

    /// <summary>
    /// Проверяет, подключен ли микрофон к записывающему устройству
    /// </summary>
    private bool CheckMicrophoneConnection(InstrumentIdentity instrument)
    {
        // Ищем микрофон для этого инструмента
        MicrophoneRecorder[] microphones = FindObjectsOfType<MicrophoneRecorder>();
        foreach (var mic in microphones)
        {
            if (mic.targetInstrument == instrument)
            {
                return mic.IsReadyToRecord();
            }
        }
        
        // Если микрофон не найден, разрешаем запись (для обратной совместимости)
        Debug.LogWarning($"Микрофон для {instrument.type} не найден. Запись разрешена, но рекомендуется подключить микрофон.");
        return true;
    }

    public void StopRecording()
    {
        if (!IsRecording)
        {
            Debug.LogWarning("Запись не идет!");
            return;
        }

        if (recordingRoutine != null)
        {
            StopCoroutine(recordingRoutine);
            recordingRoutine = null;
        }

        if (currentRecorder != null && currentInstrument != null)
        {
            var clip = currentRecorder.StopAndSaveClip(currentInstrument.type.ToString() + "_Take");
            if (clip != null && TrackManager.I != null)
            {
                TrackManager.I.SetTake(currentInstrument.type, clip);
            }
            
            IsRecording = false;
            currentRecorder = null;
            currentInstrument = null;
            
            Debug.Log("Запись остановлена вручную");
        }
    }

    private IEnumerator RecordFlow(InstrumentIdentity instrument, AudioSourceRecorder recorder)
    {
        // Отсчёт 5 секунд
        float t = countdownSeconds;
        while (t > 0f)
        {
            t -= Time.deltaTime;
            yield return null;
        }

        // Запуск записи
        IsRecording = true;
        recorder.StartRecording();

        // Записываем до maxRecordSeconds или до ручной остановки
        float elapsed = 0f;
        while (elapsed < maxRecordSeconds && IsRecording)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Завершаем запись только если она еще идет
        if (IsRecording)
        {
            var clip = recorder.StopAndSaveClip(instrument.type.ToString() + "_Take");
            if (clip != null && TrackManager.I != null)
            {
                TrackManager.I.SetTake(instrument.type, clip);
            }
            IsRecording = false;
            currentRecorder = null;
            currentInstrument = null;
        }
    }
}
