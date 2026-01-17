using UnityEngine;

/// <summary>
/// Типы зон ударной установки
/// </summary>
public enum DrumZoneType
{
    Kick,      // Бочка
    Snare,     // Малый барабан
    HiHat,     // Хай-хэт
    Crash,     // Крэш тарелка
    Ride,      // Райд тарелка
    Tom1,      // Том 1
    Tom2,      // Том 2
    FloorTom   // Напольный том
}

/// <summary>
/// Управляет звуками ударной установки
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class DrumsSoundManager : MonoBehaviour
{
    [Header("Drum Sounds")]
    public AudioClip kickSound;
    public AudioClip snareSound;
    public AudioClip hiHatSound;
    public AudioClip crashSound;
    public AudioClip rideSound;
    public AudioClip tom1Sound;
    public AudioClip tom2Sound;
    public AudioClip floorTomSound;

    [Header("Settings")]
    public float minVelocity = 0.1f;
    public float maxVolume = 1f;
    
    private AudioSource audioSource;
    private InstrumentIdentity identity;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        identity = GetComponent<InstrumentIdentity>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D звук
    }

    /// <summary>
    /// Воспроизводит звук ударной установки
    /// </summary>
    /// <param name="zoneType">Тип зоны</param>
    /// <param name="velocity">Сила удара (0-1)</param>
    public void PlayDrum(DrumZoneType zoneType, float velocity = 1f)
    {
        AudioClip clip = GetClipForZone(zoneType);
        
        if (clip == null)
        {
            Debug.LogWarning($"No sound assigned for {zoneType}");
            return;
        }

        // Нормализуем velocity и применяем к громкости
        float normalizedVelocity = Mathf.Clamp01(velocity);
        if (normalizedVelocity < minVelocity) return;

        audioSource.volume = normalizedVelocity * maxVolume;
        audioSource.PlayOneShot(clip);
        
        Debug.Log($"Playing {zoneType} with velocity {normalizedVelocity:F2}");
    }

    /// <summary>
    /// Получает AudioClip для типа зоны
    /// </summary>
    private AudioClip GetClipForZone(DrumZoneType zoneType)
    {
        switch (zoneType)
        {
            case DrumZoneType.Kick: return kickSound;
            case DrumZoneType.Snare: return snareSound;
            case DrumZoneType.HiHat: return hiHatSound;
            case DrumZoneType.Crash: return crashSound;
            case DrumZoneType.Ride: return rideSound;
            case DrumZoneType.Tom1: return tom1Sound;
            case DrumZoneType.Tom2: return tom2Sound;
            case DrumZoneType.FloorTom: return floorTomSound;
            default: return null;
        }
    }

    /// <summary>
    /// Воспроизводит случайный звук (для тестирования)
    /// </summary>
    public void PlayRandomDrum()
    {
        DrumZoneType[] zones = (DrumZoneType[])System.Enum.GetValues(typeof(DrumZoneType));
        DrumZoneType randomZone = zones[Random.Range(0, zones.Length)];
        PlayDrum(randomZone, 1f);
    }
}
