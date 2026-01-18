using UnityEngine;
using Valve.VR;

/// <summary>
/// Обрабатывает ввод (SteamVR/клавиатура) для управления записью.
/// </summary>
public class RecordingInputController : MonoBehaviour
{
    [Header("References")]
    public RecordController recordController;
    public RecordSystem recordSystem;

    [Header("SteamVR Actions")]
    public SteamVR_Action_Boolean startRecordingAction;
    public SteamVR_Action_Boolean stopRecordingAction;
    public SteamVR_Action_Boolean togglePlaybackAction;
    public SteamVR_Input_Sources inputSource = SteamVR_Input_Sources.Any;

    private void Reset()
    {
        recordController = FindObjectOfType<RecordController>();
        recordSystem = FindObjectOfType<RecordSystem>();
    }

    private void Update()
    {
        if (recordController == null)
        {
            recordController = FindObjectOfType<RecordController>();
        }

        if (recordSystem == null)
        {
            recordSystem = FindObjectOfType<RecordSystem>();
        }

        if (IsStartRecordingPressed())
        {
            StartRecording();
        }

        if (IsStopRecordingPressed())
        {
            StopRecording();
        }

        if (IsTogglePlaybackPressed())
        {
            TogglePlayback();
        }
    }

    private bool IsStartRecordingPressed()
    {
        return startRecordingAction != null && startRecordingAction.GetStateDown(inputSource);
    }

    private bool IsStopRecordingPressed()
    {
        return stopRecordingAction != null && stopRecordingAction.GetStateDown(inputSource);
    }

    private bool IsTogglePlaybackPressed()
    {
        return togglePlaybackAction != null && togglePlaybackAction.GetStateDown(inputSource);
    }

    private void StartRecording()
    {
        if (recordSystem != null)
        {
            recordSystem.StartRecordingWithDefaultCountdown();
            return;
        }

        if (recordController == null)
        {
            Debug.LogWarning("RecordController не найден.");
            return;
        }

        if (recordController.IsRecording)
        {
            Debug.LogWarning("Запись уже идет.");
            return;
        }

        if (InstrumentSelector.I == null || !InstrumentSelector.I.HasSelection)
        {
            Debug.LogWarning("Нет выбранного инструмента для записи.");
            return;
        }

        recordController.StartRecordingSelected();
    }

    private void StopRecording()
    {
        if (recordSystem != null)
        {
            recordSystem.StopRecording();
            return;
        }

        if (recordController == null)
        {
            Debug.LogWarning("RecordController не найден.");
            return;
        }

        if (!recordController.IsRecording)
        {
            Debug.LogWarning("Запись не идет.");
            return;
        }

        recordController.StopRecording();
    }

    private void TogglePlayback()
    {
        if (InstrumentSelector.I == null || !InstrumentSelector.I.HasSelection)
        {
            Debug.LogWarning("Нет выбранного инструмента для воспроизведения.");
            return;
        }

        InstrumentType type = InstrumentSelector.I.Current.type;
        if (TrackManager.I == null || !TrackManager.I.HasTrack(type))
        {
            Debug.LogWarning($"Нет записанного трека для {type}.");
            return;
        }

        if (TrackManager.I.IsTrackPlaying(type))
        {
            TrackManager.I.StopTrack(type);
        }
        else
        {
            TrackManager.I.PlayTrack(type);
        }
    }
}
