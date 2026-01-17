using UnityEngine;

/// <summary>
/// Главный контроллер синтезатора
/// Управляет настройками и ограничениями клавиш
/// </summary>
[RequireComponent(typeof(KeysSoundManager))]
public class SynthesizerController : MonoBehaviour
{
    [Header("Middle Keys Range")]
    [Tooltip("Работают только средние кнопки (E, F, G - индексы 2-4 в октаве)")]
    public bool onlyMiddleKeys = false;
    
    [Tooltip("Минимальный индекс средней клавиши в октаве (E = 2)")]
    [Range(0, 6)]
    public int minMiddleKeyIndex = 2;
    
    [Tooltip("Максимальный индекс средней клавиши в октаве (G = 4)")]
    [Range(0, 6)]
    public int maxMiddleKeyIndex = 4;
    
    [Header("Settings")]
    [Tooltip("Автоматически обновляет настройки всех KeyZone при изменении")]
    public bool autoUpdateKeyZones = true;
    
    private KeysSoundManager soundManager;
    private KeyZone[] keyZones;
    
    void Awake()
    {
        soundManager = GetComponent<KeysSoundManager>();
        
        // Находим все KeyZone на дочерних объектах
        keyZones = GetComponentsInChildren<KeyZone>();
    }
    
    void Start()
    {
        // Обновляем настройки всех клавиш
        if (autoUpdateKeyZones)
        {
            UpdateAllKeyZones();
        }
    }
    
    /// <summary>
    /// Проверяет, является ли клавиша средней (по индексу ноты в октаве 0-6)
    /// </summary>
    public bool IsMiddleKey(int noteIndexInOctave)
    {
        if (!onlyMiddleKeys) return true; // Если ограничение выключено, все клавиши работают
        
        // noteIndexInOctave должен быть в диапазоне 0-6 (C, D, E, F, G, A, B)
        noteIndexInOctave = noteIndexInOctave % 7; // Убеждаемся, что в диапазоне 0-6
        return noteIndexInOctave >= minMiddleKeyIndex && noteIndexInOctave <= maxMiddleKeyIndex;
    }
    
    /// <summary>
    /// Проверяет, является ли клавиша средней (по buttonIndex)
    /// </summary>
    public bool IsMiddleKeyByButtonIndex(int buttonIndex)
    {
        if (!onlyMiddleKeys) return true;
        
        int noteIndexInOctave = buttonIndex % 7; // Нота в октаве (0-6)
        return IsMiddleKey(noteIndexInOctave);
    }
    
    /// <summary>
    /// Обновляет настройки всех KeyZone
    /// </summary>
    public void UpdateAllKeyZones()
    {
        if (keyZones == null || keyZones.Length == 0)
        {
            keyZones = GetComponentsInChildren<KeyZone>();
        }
        
        foreach (KeyZone keyZone in keyZones)
        {
            if (keyZone != null)
            {
                keyZone.onlyMiddleKeys = onlyMiddleKeys;
                keyZone.minMiddleKeyIndex = minMiddleKeyIndex;
                keyZone.maxMiddleKeyIndex = maxMiddleKeyIndex;
            }
        }
        
        Debug.Log($"[SynthesizerController] Обновлены настройки {keyZones.Length} клавиш. Средние кнопки: {minMiddleKeyIndex}-{maxMiddleKeyIndex}");
    }
    
    /// <summary>
    /// Получает список активных (средних) клавиш
    /// </summary>
    public string[] GetActiveKeys()
    {
        string[] noteNames = { "C", "D", "E", "F", "G", "A", "B" }; // Только белые клавиши
        System.Collections.Generic.List<string> activeKeys = new System.Collections.Generic.List<string>();
        
        for (int i = minMiddleKeyIndex; i <= maxMiddleKeyIndex; i++)
        {
            if (i >= 0 && i < noteNames.Length)
            {
                activeKeys.Add(noteNames[i]);
            }
        }
        
        return activeKeys.ToArray();
    }
    
    /// <summary>
    /// Включает/выключает ограничение средних кнопок
    /// </summary>
    public void SetOnlyMiddleKeys(bool enabled)
    {
        onlyMiddleKeys = enabled;
        UpdateAllKeyZones();
    }
    
    /// <summary>
    /// Устанавливает диапазон средних кнопок (индексы в октаве 0-6)
    /// </summary>
    public void SetMiddleKeysRange(int min, int max)
    {
        minMiddleKeyIndex = Mathf.Clamp(min, 0, 6);
        maxMiddleKeyIndex = Mathf.Clamp(max, 0, 6);
        
        // Убеждаемся, что min <= max
        if (minMiddleKeyIndex > maxMiddleKeyIndex)
        {
            int temp = minMiddleKeyIndex;
            minMiddleKeyIndex = maxMiddleKeyIndex;
            maxMiddleKeyIndex = temp;
        }
        
        UpdateAllKeyZones();
    }
    
    void OnValidate()
    {
        // В редакторе автоматически обновляем настройки при изменении
        if (Application.isPlaying && autoUpdateKeyZones)
        {
            UpdateAllKeyZones();
        }
        
        // Проверяем корректность диапазона
        if (minMiddleKeyIndex > maxMiddleKeyIndex)
        {
            int temp = minMiddleKeyIndex;
            minMiddleKeyIndex = maxMiddleKeyIndex;
            maxMiddleKeyIndex = temp;
        }
    }
}

