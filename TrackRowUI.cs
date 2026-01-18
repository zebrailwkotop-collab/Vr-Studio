using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI строка для одного трека в микшере
/// Управляет громкостью, панорамой, воспроизведением для одного инструмента
/// </summary>
public class TrackRowUI : MonoBehaviour
{
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

    [Header("UI Elements - Recording")]
    public Image recordingIndicator;

    [Header("UI Elements - Selection")]
    public VRButton selectInstrumentButton;
    public InstrumentIdentity instrumentIdentity;

    [Header("Settings")]
    [Tooltip("Шаг изменения Pan при нажатии кнопки (в процентах)")]
    public float panStepPercent = 5f;
    
    [Tooltip("Минимальное значение Pan (в процентах)")]
    public float minPanPercent = 5f;
    
    [Tooltip("Максимальное значение Pan (в процентах)")]
    public float maxPanPercent = 100f;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color recordingColor = Color.red;

    private InstrumentType instrumentType;
    private float currentPanPercent = 50f; // Начальное значение: 50% (центр)
    private bool isPlaying = false;

    /// <summary>
    /// Инициализирует строку трека
    /// </summary>
    public void Initialize(InstrumentType type)
    {
        instrumentType = type;

        // Устанавливаем название
        if (trackNameText != null)
        {
            trackNameText.text = type.ToString();
        }

        // Настраиваем слайдер громкости
        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = 0.8f; // Значение по умолчанию
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            UpdateVolumeText(0.8f);
        }

        // Настраиваем кнопку Pan
        if (panButton != null)
        {
            panButton.OnButtonPressed.AddListener(OnPanButtonPressed);
        }

        // Инициализируем Pan
        currentPanPercent = 50f; // Центр (50%)
        UpdatePanDisplay();

        // Настраиваем кнопки Play/Stop
        if (playButton != null)
        {
            playButton.OnButtonPressed.AddListener(OnPlayPressed);
        }

        if (stopButton != null)
        {
            stopButton.OnButtonPressed.AddListener(OnStopPressed);
        }

        // Настраиваем кнопку выбора инструмента
        if (selectInstrumentButton != null)
        {
            selectInstrumentButton.OnButtonPressed.AddListener(OnSelectInstrumentPressed);
        }

        // Инициализируем индикаторы
        if (recordingIndicator != null)
        {
            recordingIndicator.color = normalColor;
            recordingIndicator.gameObject.SetActive(false);
        }

        // Применяем начальные настройки к микшеру
        if (MixerController.I != null)
        {
            MixerController.I.SetVolume(instrumentType, volumeSlider.value);
            ApplyPanToMixer();
        }
    }

    /// <summary>
    /// Обработчик изменения громкости
    /// </summary>
    private void OnVolumeChanged(float value)
    {
        if (MixerController.I != null)
        {
            MixerController.I.SetVolume(instrumentType, value);
        }
        UpdateVolumeText(value);
    }

    /// <summary>
    /// Обработчик кнопки Pan
    /// При нажатии увеличивает Pan на 5%, при достижении 100% сбрасывается до 5%
    /// </summary>
    private void OnPanButtonPressed()
    {
        // Увеличиваем Pan на шаг
        currentPanPercent += panStepPercent;

        // Если достигли максимума, сбрасываем до минимума
        if (currentPanPercent > maxPanPercent)
        {
            currentPanPercent = minPanPercent;
        }

        // Применяем изменения
        UpdatePanDisplay();
        ApplyPanToMixer();
    }

    /// <summary>
    /// Обновляет отображение Pan в UI
    /// </summary>
    private void UpdatePanDisplay()
    {
        if (panText != null)
        {
            panText.text = $"{currentPanPercent:F0}%";
        }
    }

    /// <summary>
    /// Применяет значение Pan к микшеру
    /// Конвертирует проценты (0-100) в диапазон Pan (-1.0 до 1.0)
    /// </summary>
    private void ApplyPanToMixer()
    {
        if (MixerController.I == null) return;

        // Конвертируем проценты в диапазон -1.0 до 1.0
        // 0% = -1.0 (полностью влево)
        // 50% = 0.0 (центр)
        // 100% = 1.0 (полностью вправо)
        float panValue = (currentPanPercent / 100f) * 2f - 1f;
        panValue = Mathf.Clamp(panValue, -1f, 1f);

        MixerController.I.SetPan(instrumentType, panValue);
        Debug.Log($"[TrackRowUI] {instrumentType} Pan set to {currentPanPercent:F0}% (panValue: {panValue:F2})");
    }

    /// <summary>
    /// Обработчик кнопки Play
    /// Воспроизводит записанный трек для этого инструмента
    /// </summary>
    private void OnPlayPressed()
    {
        if (TrackManager.I == null)
        {
            Debug.LogWarning($"[TrackRowUI] TrackManager не найден!");
            return;
        }

        if (!TrackManager.I.HasTrack(instrumentType))
        {
            Debug.LogWarning($"[TrackRowUI] Нет записанного трека для {instrumentType}");
            return;
        }

        // Если уже играет, останавливаем
        if (isPlaying)
        {
            StopPlayback();
        }
        else
        {
            StartPlayback();
        }
    }

    /// <summary>
    /// Обработчик кнопки Stop
    /// Останавливает воспроизведение трека
    /// </summary>
    private void OnStopPressed()
    {
        StopPlayback();
    }

    /// <summary>
    /// Начинает воспроизведение трека
    /// </summary>
    private void StartPlayback()
    {
        if (TrackManager.I == null) return;

        // Используем SpeakerPlaybackManager если доступен, иначе TrackManager
        if (SpeakerPlaybackManager.I != null)
        {
            SpeakerPlaybackManager.I.PlayTrack(instrumentType);
        }
        else
        {
            TrackManager.I.PlayTrack(instrumentType);
        }

        isPlaying = true;
        Debug.Log($"[TrackRowUI] Started playing {instrumentType}");
    }

    /// <summary>
    /// Останавливает воспроизведение трека
    /// </summary>
    private void StopPlayback()
    {
        if (TrackManager.I == null) return;

        // Останавливаем через оба менеджера для надежности
        if (SpeakerPlaybackManager.I != null)
        {
            SpeakerPlaybackManager.I.StopTrack(instrumentType);
        }
        
        if (TrackManager.I != null)
        {
            TrackManager.I.StopTrack(instrumentType);
        }

        isPlaying = false;
        Debug.Log($"[TrackRowUI] Stopped {instrumentType}");
    }

    /// <summary>
    /// Обработчик кнопки выбора инструмента
    /// </summary>
    private void OnSelectInstrumentPressed()
    {
        InstrumentIdentity instrument = instrumentIdentity != null ? instrumentIdentity : FindInstrumentIdentity();
        if (instrument == null)
        {
            Debug.LogWarning($"[TrackRowUI] InstrumentIdentity не найден для {instrumentType}");
            return;
        }

        if (InstrumentSelector.I != null)
        {
            InstrumentSelector.I.Select(instrument);
        }
    }

    /// <summary>
    /// Обновляет текст громкости
    /// </summary>
    private void UpdateVolumeText(float value)
    {
        if (volumeText != null)
        {
            volumeText.text = $"{(value * 100f):F0}%";
        }
    }

    /// <summary>
    /// Обновляет индикатор записи
    /// </summary>
    public void UpdateRecordingIndicator()
    {
        if (recordingIndicator == null) return;

        // Проверяем, идет ли запись для этого инструмента
        bool isRecording = false;
        
        RecordController recordController = FindObjectOfType<RecordController>();
        if (recordController != null && recordController.IsRecording)
        {
            // Проверяем, записывается ли именно этот инструмент
            if (InstrumentSelector.I != null && InstrumentSelector.I.HasSelection)
            {
                isRecording = InstrumentSelector.I.Current.type == instrumentType;
            }
        }
        
        recordingIndicator.gameObject.SetActive(isRecording);
        if (isRecording)
        {
            // Мигающий эффект
            float alpha = Mathf.PingPong(Time.time * 2f, 1f);
            Color color = recordingColor;
            color.a = alpha;
            recordingIndicator.color = color;
        }
    }

    /// <summary>
    /// Обновляет состояние воспроизведения (вызывается извне)
    /// </summary>
    public void UpdatePlaybackState()
    {
        if (TrackManager.I != null)
        {
            isPlaying = TrackManager.I.IsTrackPlaying(instrumentType);
        }
    }

    /// <summary>
    /// Устанавливает значение Pan в процентах (для внешнего использования)
    /// </summary>
    public void SetPanPercent(float percent)
    {
        currentPanPercent = Mathf.Clamp(percent, minPanPercent, maxPanPercent);
        UpdatePanDisplay();
        ApplyPanToMixer();
    }

    /// <summary>
    /// Получает текущее значение Pan в процентах
    /// </summary>
    public float GetPanPercent()
    {
        return currentPanPercent;
    }

    /// <summary>
    /// Находит InstrumentIdentity по типу инструмента
    /// </summary>
    private InstrumentIdentity FindInstrumentIdentity()
    {
        InstrumentIdentity[] allInstruments = FindObjectsOfType<InstrumentIdentity>();
        foreach (var instrument in allInstruments)
        {
            if (instrument.type == instrumentType)
            {
                return instrument;
            }
        }
        return null;
    }
}
