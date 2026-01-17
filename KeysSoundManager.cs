using UnityEngine;

/// <summary>
/// Управляет звуками клавишных инструментов
/// Поддерживает белые клавиши (C, D, E, F, G, A, B) с несколькими октавами
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class KeysSoundManager : MonoBehaviour
{
    [Header("Piano Keys - White Keys Only")]
    [Tooltip("Звуки для белых клавиш: C, D, E, F, G, A, B (7 звуков)")]
    public AudioClip[] whiteKeySounds = new AudioClip[7]; // 7 белых клавиш
    
    [Header("Settings")]
    [Tooltip("Базовая октава для первых 7 кнопок")]
    public int baseOctave = 4; // Средняя октава
    
    [Tooltip("Количество октав (сколько раз повторяется последовательность)")]
    public int numberOfOctaves = 2; // 2 октавы = 14 кнопок
    
    [Tooltip("Использовать изменение pitch для разных октав")]
    public bool usePitchShift = true;
    
    public float minVelocity = 0.1f;
    public float maxVolume = 1f;
    
    private AudioSource audioSource;
    private InstrumentIdentity identity;
    
    // Маппинг белых клавиш в 12-нотную систему (для совместимости)
    // C=0, D=2, E=4, F=5, G=7, A=9, B=11
    private readonly int[] whiteKeyToSemitone = { 0, 2, 4, 5, 7, 9, 11 };
    private readonly string[] whiteKeyNames = { "C", "D", "E", "F", "G", "A", "B" };

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
    /// Воспроизводит звук клавиши по индексу кнопки
    /// </summary>
    /// <param name="buttonIndex">Индекс кнопки (0 = C октава 4, 7 = C октава 5, и т.д.)</param>
    /// <param name="velocity">Сила нажатия (0-1)</param>
    public void PlayKeyByButtonIndex(int buttonIndex, float velocity = 1f)
    {
        if (buttonIndex < 0)
        {
            Debug.LogWarning($"Invalid button index: {buttonIndex}");
            return;
        }

        // Определяем ноту (0-6) и октаву
        int noteIndex = buttonIndex % 7; // 0-6: C, D, E, F, G, A, B
        int octaveOffset = buttonIndex / 7; // Сколько октав выше базовой
        int currentOctave = baseOctave + octaveOffset;

        PlayKey(noteIndex, currentOctave, velocity);
    }

    /// <summary>
    /// Воспроизводит звук клавиши
    /// </summary>
    /// <param name="noteIndex">Индекс ноты (0-6: C, D, E, F, G, A, B)</param>
    /// <param name="octave">Октава (4 = средняя)</param>
    /// <param name="velocity">Сила нажатия (0-1)</param>
    public void PlayKey(int noteIndex, int octave, float velocity = 1f)
    {
        if (noteIndex < 0 || noteIndex >= whiteKeySounds.Length)
        {
            Debug.LogWarning($"Invalid note index: {noteIndex} (must be 0-6)");
            return;
        }

        if (whiteKeySounds[noteIndex] == null)
        {
            Debug.LogWarning($"No sound assigned for note {whiteKeyNames[noteIndex]}");
            return;
        }

        // Нормализуем velocity и применяем к громкости
        float normalizedVelocity = Mathf.Clamp01(velocity);
        if (normalizedVelocity < minVelocity) return;

        audioSource.volume = normalizedVelocity * maxVolume;

        // Вычисляем pitch для октавы
        if (usePitchShift && octave != baseOctave)
        {
            int octaveDifference = octave - baseOctave;
            float pitchMultiplier = Mathf.Pow(2f, octaveDifference); // Удваиваем частоту на каждую октаву
            audioSource.pitch = pitchMultiplier;
        }
        else
        {
            audioSource.pitch = 1f;
        }

        audioSource.PlayOneShot(whiteKeySounds[noteIndex]);
        
        Debug.Log($"Playing key {whiteKeyNames[noteIndex]}{octave} (button {GetButtonIndex(noteIndex, octave)}, pitch: {audioSource.pitch:F2}) with velocity {normalizedVelocity:F2}");
    }

    /// <summary>
    /// Старый метод для совместимости (использует 12-нотную систему)
    /// </summary>
    [System.Obsolete("Use PlayKeyByButtonIndex or PlayKey instead")]
    public void PlayKey(int noteIndex, float velocity = 1f)
    {
        // Конвертируем из 12-нотной системы в белые клавиши
        // C=0, C#=1, D=2, D#=3, E=4, F=5, F#=6, G=7, G#=8, A=9, A#=10, B=11
        // Белые: C=0, D=2, E=4, F=5, G=7, A=9, B=11
        
        int whiteKeyIndex = -1;
        for (int i = 0; i < whiteKeyToSemitone.Length; i++)
        {
            if (whiteKeyToSemitone[i] == noteIndex)
            {
                whiteKeyIndex = i;
                break;
            }
        }

        if (whiteKeyIndex >= 0)
        {
            PlayKey(whiteKeyIndex, baseOctave, velocity);
        }
        else
        {
            Debug.LogWarning($"Note index {noteIndex} is not a white key. Use only white keys: C(0), D(2), E(4), F(5), G(7), A(9), B(11)");
        }
    }

    /// <summary>
    /// Воспроизводит ноту по имени (C, D, E, F, G, A, B)
    /// </summary>
    public void PlayNoteByName(string noteName, int octave, float velocity = 1f)
    {
        for (int i = 0; i < whiteKeyNames.Length; i++)
        {
            if (whiteKeyNames[i].Equals(noteName, System.StringComparison.OrdinalIgnoreCase))
            {
                PlayKey(i, octave, velocity);
                return;
            }
        }
        
        Debug.LogWarning($"Note '{noteName}' not found. Use only white keys: C, D, E, F, G, A, B");
    }

    /// <summary>
    /// Получает индекс кнопки по ноте и октаве
    /// </summary>
    public int GetButtonIndex(int noteIndex, int octave)
    {
        int octaveOffset = octave - baseOctave;
        return noteIndex + (octaveOffset * 7);
    }

    /// <summary>
    /// Получает название ноты по индексу кнопки
    /// </summary>
    public string GetNoteNameByButtonIndex(int buttonIndex)
    {
        int noteIndex = buttonIndex % 7;
        int octaveOffset = buttonIndex / 7;
        int currentOctave = baseOctave + octaveOffset;
        
        return $"{whiteKeyNames[noteIndex]}{currentOctave}";
    }

    /// <summary>
    /// Получает количество доступных кнопок
    /// </summary>
    public int GetTotalButtonCount()
    {
        return 7 * numberOfOctaves;
    }
}
