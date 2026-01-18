using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Система записи, связанная с UI микшера.
/// Отвечает за отсчет и запуск/остановку записи.
/// </summary>
public class RecordSystem : MonoBehaviour
{
    public float defaultCountdownSeconds = 5f;

    public bool IsCountingDown { get; private set; }

    public event Action<float> CountdownUpdated;
    public event Action CountdownFinished;
    public event Action RecordingStarted;
    public event Action RecordingStopped;

    private RecordController recordController;
    private Coroutine countdownRoutine;

    private void Awake()
    {
        recordController = FindObjectOfType<RecordController>();
    }

    public void SetRecordController(RecordController controller)
    {
        recordController = controller;
    }

    public void StartRecordingWithCountdown(float seconds)
    {
        EnsureRecordController();
        if (recordController == null)
        {
            Debug.LogWarning("[RecordSystem] RecordController не найден.");
            return;
        }

        if (recordController.IsRecording || IsCountingDown)
        {
            Debug.LogWarning("[RecordSystem] Запись уже идет или идет отсчет.");
            return;
        }

        if (InstrumentSelector.I == null || !InstrumentSelector.I.HasSelection)
        {
            Debug.LogWarning("[RecordSystem] Нет выбранного инструмента для записи.");
            return;
        }

        float countdown = Mathf.Max(0f, seconds);
        countdownRoutine = StartCoroutine(CountdownAndRecord(countdown));
    }

    public void StartRecordingWithDefaultCountdown()
    {
        StartRecordingWithCountdown(defaultCountdownSeconds);
    }

    public void StopRecording()
    {
        StopCountdown();

        EnsureRecordController();
        if (recordController == null)
        {
            Debug.LogWarning("[RecordSystem] RecordController не найден.");
            return;
        }

        if (!recordController.IsRecording)
        {
            Debug.LogWarning("[RecordSystem] Запись не идет.");
            return;
        }

        recordController.StopRecording();
        RecordingStopped?.Invoke();
    }

    public void StopCountdown()
    {
        if (countdownRoutine != null)
        {
            StopCoroutine(countdownRoutine);
            countdownRoutine = null;
        }

        if (IsCountingDown)
        {
            IsCountingDown = false;
        }
    }

    private IEnumerator CountdownAndRecord(float seconds)
    {
        IsCountingDown = true;
        float remaining = seconds;

        while (remaining > 0f)
        {
            CountdownUpdated?.Invoke(remaining);
            remaining -= Time.deltaTime;
            yield return null;
        }

        CountdownUpdated?.Invoke(0f);
        CountdownFinished?.Invoke();

        if (recordController != null)
        {
            recordController.countdownSeconds = 0f;
            recordController.StartRecordingSelected();
            RecordingStarted?.Invoke();
        }

        IsCountingDown = false;
        countdownRoutine = null;
    }

    private void EnsureRecordController()
    {
        if (recordController == null)
        {
            recordController = FindObjectOfType<RecordController>();
        }
    }
}
