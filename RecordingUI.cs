using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI контроллер для панели записи
/// </summary>
public class RecordingUI : MonoBehaviour
{
    [Header("Recording Controls")]
    public TextMeshProUGUI selectedInstrumentText;
    public TextMeshProUGUI recordingStatusText;
    public TextMeshProUGUI recordingTimerText;
    
    public VRButton startRecordingButton;
    public VRButton stopRecordingButton;
    public VRButton playbackButton;

    [Header("Status Colors")]
    public Color readyColor = Color.green;
    public Color recordingColor = Color.red;
    public Color playingColor = Color.blue;

    private RecordController recordController;
    private bool isRecording = false;
    private bool isPlaying = false;
    private float recordingTime = 0f;

    void Start()
    {
        recordController = FindObjectOfType<RecordController>();
        
        InitializeButtons();
        UpdateUI();
    }

    void Update()
    {
        // Синхронизируем статус записи с RecordController
        if (recordController != null)
        {
            bool wasRecording = isRecording;
            isRecording = recordController.IsRecording;
            
            if (isRecording && !wasRecording)
            {
                recordingTime = 0f;
            }
        }

        // Обновляем таймер записи
        if (isRecording)
        {
            recordingTime += Time.deltaTime;
            UpdateRecordingTimer();
        }

        // Обновляем статус
        UpdateStatus();
    }

    /// <summary>
    /// Инициализирует кнопки
    /// </summary>
    private void InitializeButtons()
    {
        if (startRecordingButton != null)
        {
            startRecordingButton.OnButtonPressed.AddListener(OnStartRecording);
        }

        if (stopRecordingButton != null)
        {
            stopRecordingButton.OnButtonPressed.AddListener(OnStopRecording);
        }

        if (playbackButton != null)
        {
            playbackButton.OnButtonPressed.AddListener(OnPlayback);
        }
    }

    /// <summary>
    /// Обработчик кнопки Start Recording
    /// </summary>
    private void OnStartRecording()
    {
        if (recordController != null && InstrumentSelector.I != null && InstrumentSelector.I.HasSelection)
        {
            recordController.StartRecordingSelected();
            isRecording = true;
            recordingTime = 0f;
            UpdateUI();
        }
        else
        {
            Debug.LogWarning("Нет выбранного инструмента для записи!");
        }
    }

    /// <summary>
    /// Обработчик кнопки Stop Recording
    /// </summary>
    private void OnStopRecording()
    {
        if (recordController != null && recordController.IsRecording)
        {
            recordController.StopRecording();
            isRecording = false;
            UpdateUI();
        }
    }

    /// <summary>
    /// Обработчик кнопки Playback
    /// </summary>
    private void OnPlayback()
    {
        if (InstrumentSelector.I != null && InstrumentSelector.I.HasSelection)
        {
            InstrumentType type = InstrumentSelector.I.Current.type;
            
            if (TrackManager.I != null && TrackManager.I.HasTrack(type))
            {
                if (TrackManager.I.IsTrackPlaying(type))
                {
                    TrackManager.I.StopTrack(type);
                    isPlaying = false;
                }
                else
                {
                    TrackManager.I.PlayTrack(type);
                    isPlaying = true;
                }
                UpdateUI();
            }
            else
            {
                Debug.LogWarning($"Нет записанного трека для {type}");
            }
        }
    }

    /// <summary>
    /// Обновляет UI
    /// </summary>
    private void UpdateUI()
    {
        UpdateSelectedInstrument();
        UpdateStatus();
        UpdateButtons();
    }

    /// <summary>
    /// Обновляет текст выбранного инструмента
    /// </summary>
    private void UpdateSelectedInstrument()
    {
        if (selectedInstrumentText != null)
        {
            if (InstrumentSelector.I != null && InstrumentSelector.I.HasSelection)
            {
                selectedInstrumentText.text = $"Selected: {InstrumentSelector.I.Current.type}";
            }
            else
            {
                selectedInstrumentText.text = "Selected: None";
            }
        }
    }

    /// <summary>
    /// Обновляет статус записи
    /// </summary>
    private void UpdateStatus()
    {
        if (recordingStatusText != null)
        {
            if (isRecording)
            {
                recordingStatusText.text = "Status: Recording";
                recordingStatusText.color = recordingColor;
            }
            else if (isPlaying)
            {
                recordingStatusText.text = "Status: Playing";
                recordingStatusText.color = playingColor;
            }
            else
            {
                recordingStatusText.text = "Status: Ready";
                recordingStatusText.color = readyColor;
            }
        }
    }

    /// <summary>
    /// Обновляет таймер записи
    /// </summary>
    private void UpdateRecordingTimer()
    {
        if (recordingTimerText != null)
        {
            int minutes = Mathf.FloorToInt(recordingTime / 60f);
            int seconds = Mathf.FloorToInt(recordingTime % 60f);
            recordingTimerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    /// <summary>
    /// Обновляет состояние кнопок
    /// </summary>
    private void UpdateButtons()
    {
        if (startRecordingButton != null)
        {
            startRecordingButton.gameObject.SetActive(!isRecording);
        }

        if (stopRecordingButton != null)
        {
            stopRecordingButton.gameObject.SetActive(isRecording);
        }
    }

    /// <summary>
    /// Устанавливает статус записи (вызывается извне)
    /// </summary>
    public void SetRecordingStatus(bool recording)
    {
        isRecording = recording;
        if (!recording)
        {
            recordingTime = 0f;
        }
        UpdateUI();
    }

    /// <summary>
    /// Устанавливает статус воспроизведения (вызывается извне)
    /// </summary>
    public void SetPlayingStatus(bool playing)
    {
        isPlaying = playing;
        UpdateUI();
    }
}
