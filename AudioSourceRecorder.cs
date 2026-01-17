using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioSourceRecorder : MonoBehaviour
{
    public bool IsRecording { get; private set; }
    private List<float> samples = new List<float>(48000 * 10);
    private readonly object lockObject = new object();

    private int channels = 2;
    private int sampleRate;

    void Awake()
    {
        sampleRate = AudioSettings.outputSampleRate;
    }

    // Это сигнал именно этого AudioSource
    void OnAudioFilterRead(float[] data, int channels)
    {
        this.channels = channels;
        if (!IsRecording) return;

        lock (lockObject)
        {
            for (int i = 0; i < data.Length; i++)
                samples.Add(data[i]);
        }
    }

    public void StartRecording()
    {
        lock (lockObject)
        {
            samples.Clear();
            IsRecording = true;
        }
    }

    public AudioClip StopAndSaveClip(string clipName)
    {
        float[] copy;
        int channels;
        int rate;

        lock (lockObject)
        {
            IsRecording = false;
            copy = samples.ToArray();
            channels = this.channels;
            rate = sampleRate;
        }

        if (copy.Length == 0) return null;

        int lengthSamples = copy.Length / channels;
        var clip = AudioClip.Create(clipName, lengthSamples, channels, rate, false);
        clip.SetData(copy, 0);
        return clip;
    }
}
