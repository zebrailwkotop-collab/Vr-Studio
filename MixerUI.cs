using UnityEngine;
using UnityEngine.UI;
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

    [Header("Master Controls")]
    public Slider masterVolumeSlider;
    public TextMeshProUGUI masterVolumeText;

    [Header("Global Controls")]
    public VRButton playAllButton;
    public VRButton stopAllButton;

    void Start()
    {
        InitializeTrackRows();
        InitializeMasterControls();
        InitializeGlobalControls();
    }

    void Update()
    {
        // Обновляем индикаторы записи
        UpdateRecordingIndicators();
        
        // Обновляем состояние воспроизведения
        UpdatePlaybackStates();
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
    /// Инициализирует мастер-контролы
    /// </summary>
    private void InitializeMasterControls()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.minValue = 0f;
            masterVolumeSlider.maxValue = 1f;
            masterVolumeSlider.value = 1f;
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            UpdateMasterVolumeText(1f);
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
    /// Обработчик изменения мастер-громкости
    /// </summary>
    private void OnMasterVolumeChanged(float value)
    {
        if (MixerController.I != null)
        {
            MixerController.I.SetMasterVolume(value);
        }
        UpdateMasterVolumeText(value);
    }

    /// <summary>
    /// Обновляет текст мастер-громкости
    /// </summary>
    private void UpdateMasterVolumeText(float value)
    {
        if (masterVolumeText != null)
        {
            masterVolumeText.text = $"Master: {(value * 100f):F0}%";
        }
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
