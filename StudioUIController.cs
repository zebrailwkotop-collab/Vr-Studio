using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Центральный контроллер UI для микшера и записи.
/// Управляет всеми UI-элементами, треками и записью.
/// </summary>
public class StudioUIController : MonoBehaviour
{
    [System.Serializable]
    public class TrackRow
    {
        public InstrumentType instrumentType;

        [Header("UI Elements - Volume")]
        public TextMeshProUGUI trackNameText;
        public Slider volumeSlider;
        public TextMeshProUGUI volumeText;

        [Header("UI Elements - Pan")]
        public VRButton panButton;
        public TextMeshProUGUI panText;

        [Header("UI Elements - Playback")]
        public VRButton playButton;
        public VRButton stopButton;

        [Header("UI Elements - Selection")]
        public VRButton selectInstrumentButton;

        [Header("UI Elements - Recording")]
        public Image recordingIndicator;

        [Header("Settings")]
        public float panStepPercent = 5f;
        public float minPanPercent = 5f;
        public float maxPanPercent = 100f;

        [Header("Colors")]
        public Color normalColor = Color.white;
        public Color recordingColor = Color.red;

        [HideInInspector]
        public float currentPanPercent = 50f;
        [HideInInspector]
        public bool isPlaying = false;
    }

    [Header("Track Rows")]
    public TrackRow[] trackRows;

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

    private void Start()
    {
        recordController = FindObjectOfType<RecordController>();
        InitializeTrackRows();
        InitializeGlobalControls();
        InitializeRecordingControls();
        UpdateSelectedInstrumentText();
        UpdateCountdownDisplay(countdownSeconds);
    }

    private void Update()
    {
        UpdateSelectedInstrumentText();
        UpdateRecordingIndicators();
        UpdatePlaybackStates();
    }

    private void InitializeTrackRows()
    {
        if (trackRows == null) return;

        foreach (TrackRow row in trackRows)
        {
            if (row == null) continue;

            if (row.trackNameText != null)
            {
                row.trackNameText.text = row.instrumentType.ToString();
                row.trackNameText.color = row.normalColor;
            }

            if (row.volumeSlider != null)
            {
                row.volumeSlider.minValue = 0f;
                row.volumeSlider.maxValue = 1f;
                row.volumeSlider.value = 0.8f;
                row.volumeSlider.onValueChanged.AddListener(value => OnVolumeChanged(row, value));
                UpdateVolumeText(row, 0.8f);
            }

            if (row.panButton != null)
            {
                row.panButton.OnButtonPressed.AddListener(() => OnPanButtonPressed(row));
            }

            row.currentPanPercent = 50f;
            UpdatePanDisplay(row);

            if (row.playButton != null)
            {
                row.playButton.OnButtonPressed.AddListener(() => OnPlayPressed(row));
            }

            if (row.stopButton != null)
            {
                row.stopButton.OnButtonPressed.AddListener(() => OnStopPressed(row));
            }

            if (row.selectInstrumentButton != null)
            {
                row.selectInstrumentButton.OnButtonPressed.AddListener(() => OnSelectInstrumentPressed(row));
            }

            if (row.recordingIndicator != null)
            {
                row.recordingIndicator.color = row.normalColor;
                row.recordingIndicator.gameObject.SetActive(false);
            }

            if (MixerController.I != null && row.volumeSlider != null)
            {
                MixerController.I.SetVolume(row.instrumentType, row.volumeSlider.value);
                ApplyPanToMixer(row);
            }
        }
    }

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
        EnsureRecordController();
        if (recordController == null)
        {
            Debug.LogWarning("[StudioUIController] RecordController не найден.");
            return;
        }

        if (recordController.IsRecording || isCountingDown)
        {
            Debug.LogWarning("[StudioUIController] Запись уже идет или идет отсчет.");
            return;
        }

        if (InstrumentSelector.I == null || !InstrumentSelector.I.HasSelection)
        {
            Debug.LogWarning("[StudioUIController] Нет выбранного инструмента для записи.");
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

        EnsureRecordController();
        if (recordController == null)
        {
            Debug.LogWarning("[StudioUIController] RecordController не найден.");
            return;
        }

        if (!recordController.IsRecording)
        {
            Debug.LogWarning("[StudioUIController] Запись не идет.");
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

    private void UpdateRecordingIndicators()
    {
        if (trackRows == null) return;

        foreach (TrackRow row in trackRows)
        {
            if (row == null || row.recordingIndicator == null) continue;

            bool isRecording = false;
            if (recordController != null && recordController.IsRecording)
            {
                if (InstrumentSelector.I != null && InstrumentSelector.I.HasSelection)
                {
                    isRecording = InstrumentSelector.I.Current.type == row.instrumentType;
                }
            }

            row.recordingIndicator.gameObject.SetActive(isRecording);
            if (isRecording)
            {
                float alpha = Mathf.PingPong(Time.time * 2f, 1f);
                Color color = row.recordingColor;
                color.a = alpha;
                row.recordingIndicator.color = color;
            }
        }
    }

    private void UpdatePlaybackStates()
    {
        if (trackRows == null || TrackManager.I == null) return;

        foreach (TrackRow row in trackRows)
        {
            if (row == null) continue;
            row.isPlaying = TrackManager.I.IsTrackPlaying(row.instrumentType);
        }
    }

    private void OnVolumeChanged(TrackRow row, float value)
    {
        if (MixerController.I != null)
        {
            MixerController.I.SetVolume(row.instrumentType, value);
        }
        UpdateVolumeText(row, value);
    }

    private void UpdateVolumeText(TrackRow row, float value)
    {
        if (row.volumeText != null)
        {
            row.volumeText.text = $"{(value * 100f):F0}%";
        }
    }

    private void OnPanButtonPressed(TrackRow row)
    {
        row.currentPanPercent += row.panStepPercent;

        if (row.currentPanPercent > row.maxPanPercent)
        {
            row.currentPanPercent = row.minPanPercent;
        }

        UpdatePanDisplay(row);
        ApplyPanToMixer(row);
    }

    private void UpdatePanDisplay(TrackRow row)
    {
        if (row.panText != null)
        {
            row.panText.text = $"{row.currentPanPercent:F0}%";
        }
    }

    private void ApplyPanToMixer(TrackRow row)
    {
        if (MixerController.I == null) return;

        float panValue = (row.currentPanPercent / 100f) * 2f - 1f;
        panValue = Mathf.Clamp(panValue, -1f, 1f);
        MixerController.I.SetPan(row.instrumentType, panValue);
    }

    private void OnPlayPressed(TrackRow row)
    {
        if (TrackManager.I == null)
        {
            Debug.LogWarning("[StudioUIController] TrackManager не найден!");
            return;
        }

        if (!TrackManager.I.HasTrack(row.instrumentType))
        {
            Debug.LogWarning($"[StudioUIController] Нет записанного трека для {row.instrumentType}");
            return;
        }

        if (row.isPlaying)
        {
            StopPlayback(row);
        }
        else
        {
            StartPlayback(row);
        }
    }

    private void OnStopPressed(TrackRow row)
    {
        StopPlayback(row);
    }

    private void StartPlayback(TrackRow row)
    {
        if (TrackManager.I == null) return;

        if (SpeakerPlaybackManager.I != null)
        {
            SpeakerPlaybackManager.I.PlayTrack(row.instrumentType);
        }
        else
        {
            TrackManager.I.PlayTrack(row.instrumentType);
        }

        row.isPlaying = true;
    }

    private void StopPlayback(TrackRow row)
    {
        if (TrackManager.I == null) return;

        if (SpeakerPlaybackManager.I != null)
        {
            SpeakerPlaybackManager.I.StopTrack(row.instrumentType);
        }

        TrackManager.I.StopTrack(row.instrumentType);
        row.isPlaying = false;
    }

    private void OnSelectInstrumentPressed(TrackRow row)
    {
        InstrumentIdentity instrument = FindInstrumentIdentity(row.instrumentType);
        if (instrument == null)
        {
            Debug.LogWarning($"[StudioUIController] InstrumentIdentity не найден для {row.instrumentType}");
            return;
        }

        if (InstrumentSelector.I != null)
        {
            InstrumentSelector.I.Select(instrument);
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

    private void EnsureRecordController()
    {
        if (recordController == null)
        {
            recordController = FindObjectOfType<RecordController>();
        }
    }

    private void OnPlayAllPressed()
    {
        if (SpeakerPlaybackManager.I != null)
        {
            if (SpeakerPlaybackManager.I.AreAllInstrumentsConnected())
            {
                SpeakerPlaybackManager.I.PlayAllTracks();
                return;
            }

            if (TrackManager.I != null)
            {
                TrackManager.I.PlayAllTracks();
                return;
            }
        }

        if (TrackManager.I != null)
        {
            TrackManager.I.PlayAllTracks();
            return;
        }

        Debug.LogError("[StudioUIController] TrackManager не найден!");
    }

    private void OnStopAllPressed()
    {
        bool stopped = false;

        if (SpeakerPlaybackManager.I != null)
        {
            SpeakerPlaybackManager.I.StopAllTracks();
            stopped = true;
        }

        if (TrackManager.I != null)
        {
            TrackManager.I.StopAllTracks();
            stopped = true;
        }

        if (!stopped)
        {
            Debug.LogError("[StudioUIController] Не удалось остановить треки - менеджеры не найдены!");
        }
    }
}
