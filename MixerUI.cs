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

    [Header("Instrument Selection Buttons")]
    public VRButton selectGuitarButton;
    public VRButton selectBassButton;
    public VRButton selectDrumsButton;
    public VRButton selectKeysButton;
    public InstrumentIdentity guitarInstrument;
    public InstrumentIdentity bassInstrument;
    public InstrumentIdentity drumsInstrument;
    public InstrumentIdentity keysInstrument;

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

    public RecordSystem recordSystem;

    private RecordController recordController;

    void Start()
    {
        recordController = FindObjectOfType<RecordController>();
        if (recordSystem == null)
        {
            recordSystem = FindObjectOfType<RecordSystem>();
        }

        if (recordSystem != null)
        {
            recordSystem.SetRecordController(recordController);
            recordSystem.defaultCountdownSeconds = countdownSeconds;
        }

        InitializeTrackRows();
        InitializeSelectionButtons();
        InitializeGlobalControls();
        InitializeRecordingControls();
        UpdateSelectedInstrumentText();
        UpdateCountdownDisplay(countdownSeconds);

        if (stopRecordingButton != null)
        {
            stopRecordingButton.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        // Обновляем индикаторы записи
        UpdateRecordingIndicators();
        
        // Обновляем состояние воспроизведения
        UpdatePlaybackStates();

        UpdateSelectedInstrumentText();
    }

    private void OnEnable()
    {
        if (recordSystem != null)
        {
            recordSystem.CountdownUpdated += UpdateCountdownDisplay;
        }
    }

    private void OnDisable()
    {
        if (recordSystem != null)
        {
            recordSystem.CountdownUpdated -= UpdateCountdownDisplay;
        }
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

    private void InitializeSelectionButtons()
    {
        if (selectGuitarButton != null)
        {
            selectGuitarButton.OnButtonPressed.AddListener(() => SelectInstrument(guitarInstrument, InstrumentType.Guitar));
        }

        if (selectBassButton != null)
        {
            selectBassButton.OnButtonPressed.AddListener(() => SelectInstrument(bassInstrument, InstrumentType.Bass));
        }

        if (selectDrumsButton != null)
        {
            selectDrumsButton.OnButtonPressed.AddListener(() => SelectInstrument(drumsInstrument, InstrumentType.Drums));
        }

        if (selectKeysButton != null)
        {
            selectKeysButton.OnButtonPressed.AddListener(() => SelectInstrument(keysInstrument, InstrumentType.Keys));
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

    private void SelectInstrument(InstrumentIdentity instrument, InstrumentType fallbackType)
    {
        InstrumentIdentity resolved = instrument != null ? instrument : FindInstrumentIdentity(fallbackType);
        if (resolved == null)
        {
            Debug.LogWarning($"[MixerUI] InstrumentIdentity не найден для {fallbackType}");
            return;
        }

        if (InstrumentSelector.I != null)
        {
            InstrumentSelector.I.Select(resolved);
        }
    }

    private InstrumentIdentity FindInstrumentIdentity(InstrumentType type)
    {
        InstrumentIdentity[] allInstruments = FindObjectsOfType<InstrumentIdentity>();
        foreach (var instrument in allInstruments)
        {
            if (instrument.type == type)
            {
                return instrument;
            }
        }
        return null;
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
        if (recordSystem != null && recordSystem.IsCountingDown) return;

        countdownSeconds = Mathf.Min(maxCountdownSeconds, countdownSeconds + countdownStepSeconds);
        UpdateCountdownDisplay(countdownSeconds);
        if (recordSystem != null)
        {
            recordSystem.defaultCountdownSeconds = countdownSeconds;
        }
    }

    private void OnSubtractTimePressed()
    {
        if (recordSystem != null && recordSystem.IsCountingDown) return;

        countdownSeconds = Mathf.Max(minCountdownSeconds, countdownSeconds - countdownStepSeconds);
        UpdateCountdownDisplay(countdownSeconds);
        if (recordSystem != null)
        {
            recordSystem.defaultCountdownSeconds = countdownSeconds;
        }
    }

    private void OnStartRecordingPressed()
    {
        if (recordSystem == null)
        {
            Debug.LogWarning("[MixerUI] RecordSystem не найден.");
            return;
        }

        recordSystem.defaultCountdownSeconds = countdownSeconds;
        recordSystem.StartRecordingWithCountdown(countdownSeconds);
    }

    private void OnStopRecordingPressed()
    {
        if (recordSystem == null)
        {
            Debug.LogWarning("[MixerUI] RecordSystem не найден.");
            return;
        }

        recordSystem.StopRecording();
        UpdateCountdownDisplay(countdownSeconds);
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
