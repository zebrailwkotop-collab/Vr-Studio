using UnityEngine;

/// <summary>
/// Управляет звуками бас-гитары при взаимодействии игрока
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class BassSoundManager : MonoBehaviour
{
    [Header("Bass Strings")]
    public AudioClip[] stringSounds = new AudioClip[4]; // 4 струны: [0]E(4), [1]A(3), [2]D(2), [3]G(1)
    
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
    /// Воспроизводит звук струны
    /// </summary>
    /// <param name="stringIndex">Индекс струны (0-3)</param>
    /// <param name="velocity">Сила удара (0-1)</param>
    public void PlayString(int stringIndex, float velocity = 1f)
    {
        if (stringIndex < 0 || stringIndex >= stringSounds.Length)
        {
            Debug.LogWarning($"Invalid string index: {stringIndex}");
            return;
        }

        if (stringSounds[stringIndex] == null)
        {
            Debug.LogWarning($"No sound assigned for string {stringIndex}");
            return;
        }

        // Нормализуем velocity и применяем к громкости
        float normalizedVelocity = Mathf.Clamp01(velocity);
        if (normalizedVelocity < minVelocity) return;

        audioSource.volume = normalizedVelocity * maxVolume;
        audioSource.PlayOneShot(stringSounds[stringIndex]);
        
        Debug.Log($"Playing bass string {stringIndex} with velocity {normalizedVelocity:F2}");
    }

    /// <summary>
    /// Воспроизводит случайную струну (для тестирования)
    /// </summary>
    public void PlayRandomString()
    {
        int randomString = Random.Range(0, stringSounds.Length);
        PlayString(randomString, 1f);
    }
}
