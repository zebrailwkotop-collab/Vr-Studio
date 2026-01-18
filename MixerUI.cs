using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// UI контроллер для микшера
/// Управляет всеми треками и глобальными функциями
/// </summary>
public class MixerUI : MonoBehaviour
{
    [Header("Track Rows")]
    public TrackRowUI guitarRow;
    public TrackRowUI bassRow;
    public TrackRowUI drumsRow;
    public TrackRowUI keysRow;

    [Header("Global Controls")]
    public VRButton playAllButton;
    public VRButton stopAllButton;

    [Header("Recording Controls")]
    public TextMeshProUGUI selectedInstrumentText;
    public TextMeshProUGUI countdownText;
    public VRButton startRecordingButton;
    public VRButton stopRecordingButton;
    public VRButton addTimeButton;
    public VRButton subtractTimeButton;

    [Header("Recording Settings")]
    public float countdownSeconds = 5f;
    public float minCountdownSeconds = 1f;
    public float maxCountdownSeconds = 30f;
    public float countdownStepSeconds = 1f;

    private RecordController recordController;
    private Coroutine countdownRoutine;
    private bool isCountingDown = false;

    void Start()
    {
        recordController = FindObjectOfType<RecordController>();
        InitializeTrackRows();
        InitializeGlobalControls();
        InitializeRecordingControls();
        UpdateSelectedInstrumentText();
        UpdateCountdownDisplay(countdownSeconds);
    }

    void Update()
    {
        // Обновляем индикаторы записи
        UpdateRecordingIndicators();
        
        // Обновляем состояние воспроизведения
        UpdatePlaybackStates();

        UpdateSelectedInstrumentText();
    }

    /// <summary>
    /// Инициализирует строки треков
    /// </summary>
    private void InitializeTrackRows()
    {
        if (guitarRow != null)
        {
            guitarRow.Initialize(InstrumentType.Guitar);
        }
        if (bassRow != null)
        {
            bassRow.Initialize(InstrumentType.Bass);
        }
        if (drumsRow != null)
        {
            drumsRow.Initialize(InstrumentType.Drums);
        }
        if (keysRow != null)
        {
            keysRow.Initialize(InstrumentType.Keys);
        }
    }

    /// <summary>
    /// Инициализирует глобальные контролы
    /// </summary>
    private void InitializeGlobalControls()
    {
        if (playAllButton != null)
        {
            playAllButton.OnButtonPressed.AddListener(OnPlayAllPressed);
        }

        if (stopAllButton != null)
        {
            stopAllButton.OnButtonPressed.AddListener(OnStopAllPressed);
        }
    }

    /// <summary>
    /// Инициализирует контролы записи
    /// </summary>
    private void InitializeRecordingControls()
    {
        if (startRecordingButton != null)
        {
            startRecordingButton.OnButtonPressed.AddListener(OnStartRecordingPressed);
        }

        if (stopRecordingButton != null)
        {
            stopRecordingButton.OnButtonPressed.AddListener(OnStopRecordingPressed);
        }

        if (addTimeButton != null)
        {
            addTimeButton.OnButtonPressed.AddListener(OnAddTimePressed);
        }

        if (subtractTimeButton != null)
        {
            subtractTimeButton.OnButtonPressed.AddListener(OnSubtractTimePressed);
        }

        UpdateCountdownDisplay(countdownSeconds);
    }

    private void UpdateSelectedInstrumentText()
    {
        if (selectedInstrumentText == null) return;

        if (InstrumentSelector.I != null && InstrumentSelector.I.HasSelection)
        {
            selectedInstrumentText.text = $"Selected: {InstrumentSelector.I.Current.type}";
        }
        else
        {
            selectedInstrumentText.text = "Selected: None";
        }
    }

    private void UpdateCountdownDisplay(float seconds)
    {
        if (countdownText == null) return;

        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        countdownText.text = $"{minutes:00}:{secs:00}";
    }

    private void OnAddTimePressed()
    {
        if (isCountingDown) return;

        countdownSeconds = Mathf.Min(maxCountdownSeconds, countdownSeconds + countdownStepSeconds);
        UpdateCountdownDisplay(countdownSeconds);
    }

    private void OnSubtractTimePressed()
    {
        if (isCountingDown) return;

        countdownSeconds = Mathf.Max(minCountdownSeconds, countdownSeconds - countdownStepSeconds);
        UpdateCountdownDisplay(countdownSeconds);
    }

    private void OnStartRecordingPressed()
    {
        if (recordController == null)
        {
            recordController = FindObjectOfType<RecordController>();
        }

        if (recordController == null)
        {
            Debug.LogWarning("[MixerUI] RecordController не найден.");
            return;
        }

        if (recordController.IsRecording || isCountingDown)
        {
            Debug.LogWarning("[MixerUI] Запись уже идет или идет отсчет.");
            return;
        }

        if (InstrumentSelector.I == null || !InstrumentSelector.I.HasSelection)
        {
            Debug.LogWarning("[MixerUI] Нет выбранного инструмента для записи.");
            return;
        }

        countdownRoutine = StartCoroutine(StartCountdownAndRecord());
    }

    private void OnStopRecordingPressed()
    {
        if (countdownRoutine != null)
        {
            StopCoroutine(countdownRoutine);
            countdownRoutine = null;
            isCountingDown = false;
            UpdateCountdownDisplay(countdownSeconds);
        }

        if (recordController == null)
        {
            recordController = FindObjectOfType<RecordController>();
        }

        if (recordController == null)
        {
            Debug.LogWarning("[MixerUI] RecordController не найден.");
            return;
        }

        if (!recordController.IsRecording)
        {
            Debug.LogWarning("[MixerUI] Запись не идет.");
            return;
        }

        recordController.StopRecording();
    }

    private IEnumerator StartCountdownAndRecord()
    {
        isCountingDown = true;
        float remaining = Mathf.Clamp(countdownSeconds, minCountdownSeconds, maxCountdownSeconds);

        while (remaining > 0f)
        {
            UpdateCountdownDisplay(remaining);
            remaining -= Time.deltaTime;
            yield return null;
        }

        UpdateCountdownDisplay(0f);

        if (recordController != null)
        {
            recordController.countdownSeconds = 0f;
            recordController.StartRecordingSelected();
        }

        isCountingDown = false;
        countdownRoutine = null;
    }

    /// <summary>
    /// Обработчик кнопки Play All
    /// Воспроизводит все записанные треки одновременно
    /// </summary>
    private void OnPlayAllPressed()
    {
        Debug.Log("[MixerUI] Play All pressed");

        // Используем SpeakerPlaybackManager если доступен, иначе TrackManager
        if (SpeakerPlaybackManager.I != null)
        {
            // Проверяем подключения перед воспроизведением
            if (SpeakerPlaybackManager.I.AreAllInstrumentsConnected())
            {
                SpeakerPlaybackManager.I.PlayAllTracks();
                Debug.Log("[MixerUI] Playing all tracks via SpeakerPlaybackManager");
            }
            else
            {
                Debug.LogWarning("[MixerUI] Не все инструменты подключены к динамикам! Используется TrackManager.");
                if (TrackManager.I != null)
                {
                    TrackManager.I.PlayAllTracks();
                    Debug.Log("[MixerUI] Playing all tracks via TrackManager");
                }
            }
        }
        else if (TrackManager.I != null)
        {
            TrackManager.I.PlayAllTracks();
            Debug.Log("[MixerUI] Playing all tracks via TrackManager");
        }
        else
        {
            Debug.LogError("[MixerUI] TrackManager не найден!");
        }
    }

    /// <summary>
    /// Обработчик кнопки Stop All
    /// Останавливает все воспроизводимые треки
    /// </summary>
    private void OnStopAllPressed()
    {
        Debug.Log("[MixerUI] Stop All pressed");

        // Останавливаем через оба менеджера для надежности
        bool stopped = false;

        if (SpeakerPlaybackManager.I != null)
        {
            SpeakerPlaybackManager.I.StopAllTracks();
            stopped = true;
            Debug.Log("[MixerUI] Stopped all tracks via SpeakerPlaybackManager");
        }
        
        if (TrackManager.I != null)
        {
            TrackManager.I.StopAllTracks();
            stopped = true;
            Debug.Log("[MixerUI] Stopped all tracks via TrackManager");
        }

        if (!stopped)
        {
            Debug.LogError("[MixerUI] Не удалось остановить треки - менеджеры не найдены!");
        }
    }

    /// <summary>
    /// Обновляет индикаторы записи для всех треков
    /// </summary>
    private void UpdateRecordingIndicators()
    {
        if (guitarRow != null) guitarRow.UpdateRecordingIndicator();
        if (bassRow != null) bassRow.UpdateRecordingIndicator();
        if (drumsRow != null) drumsRow.UpdateRecordingIndicator();
        if (keysRow != null) keysRow.UpdateRecordingIndicator();
    }

    /// <summary>
    /// Обновляет состояние воспроизведения для всех треков
    /// </summary>
    private void UpdatePlaybackStates()
    {
        if (guitarRow != null) guitarRow.UpdatePlaybackState();
        if (bassRow != null) bassRow.UpdatePlaybackState();
        if (drumsRow != null) drumsRow.UpdatePlaybackState();
        if (keysRow != null) keysRow.UpdatePlaybackState();
    }
}
