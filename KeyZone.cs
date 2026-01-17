using UnityEngine;
using Valve.VR.InteractionSystem;

/// <summary>
/// Компонент для идентификации клавиши
/// Добавляется на каждый GameObject клавиши
/// Поддерживает белые клавиши (C, D, E, F, G, A, B) с несколькими октавами
/// </summary>
public class KeyZone : MonoBehaviour
{
    [Header("Key Settings")]
    [Tooltip("Индекс кнопки: 0 = C4, 1 = D4, 2 = E4, 3 = F4, 4 = G4, 5 = A4, 6 = B4, 7 = C5, 8 = D5, и т.д.")]
    public int buttonIndex = 0;
    
    [Header("Alternative: Note + Octave")]
    [Tooltip("Использовать нотацию нота+октава вместо индекса кнопки")]
    public bool useNoteAndOctave = false;
    
    [Tooltip("Индекс ноты (0-6: C, D, E, F, G, A, B)")]
    [Range(0, 6)]
    public int noteIndex = 0;
    
    [Tooltip("Октава (4 = средняя)")]
    [Range(2, 9)]
    public int octave = 4;
    
    [Header("Middle Keys Only")]
    [Tooltip("Работают только средние кнопки (E, F, G - индексы 2-4 в первой октаве)")]
    public bool onlyMiddleKeys = false;
    
    [Tooltip("Минимальный индекс средней клавиши (E = 2)")]
    public int minMiddleKeyIndex = 2;
    
    [Tooltip("Максимальный индекс средней клавиши (G = 4)")]
    public int maxMiddleKeyIndex = 4;

    [Header("Visual Feedback")]
    public Color normalColor = Color.white;
    public Color pressedColor = Color.yellow;
    public Renderer keyRenderer;
    public Material keyMaterial;

    private KeysSoundManager soundManager;
    private bool isPressed = false;
    private KeyAnimation keyAnimation;
    private Interactable interactable;
    
    private readonly string[] whiteKeyNames = { "C", "D", "E", "F", "G", "A", "B" };

    void Start()
    {
        // Находим KeysSoundManager на родительском объекте (клавиатуре)
        soundManager = GetComponentInParent<KeysSoundManager>();
        
        if (soundManager == null)
        {
            Debug.LogWarning($"KeyZone {buttonIndex} не найден KeysSoundManager на родителе!");
        }

        // Если используется нотация нота+октава, вычисляем buttonIndex
        if (useNoteAndOctave)
        {
            KeysSoundManager manager = GetComponentInParent<KeysSoundManager>();
            if (manager != null)
            {
                buttonIndex = manager.GetButtonIndex(noteIndex, octave);
            }
        }

        // Получаем KeyAnimation для анимации
        keyAnimation = GetComponent<KeyAnimation>();
        
        // Если KeyAnimation нет, можно создать автоматически
        if (keyAnimation == null)
        {
            keyAnimation = gameObject.AddComponent<KeyAnimation>();
        }
        
        // Добавляем Interactable для VR взаимодействия
        interactable = GetComponent<Interactable>();
        if (interactable == null)
        {
            interactable = gameObject.AddComponent<Interactable>();
            interactable.hideHandOnAttach = false; // Не скрываем руку
        }

        // Настраиваем визуальную обратную связь
        SetupVisualFeedback();
        
        // Визуальная индикация средних кнопок
        if (IsMiddleKey())
        {
            // Можно изменить цвет для средних кнопок
            // normalColor = Color.cyan; // Раскомментируйте для голубого цвета
        }
    }

    /// <summary>
    /// Вызывается когда контроллер касается клавиши
    /// </summary>
    public void OnKeyPressed(float velocity = 1f)
    {
        // Проверяем, является ли клавиша средней (если включено ограничение)
        if (onlyMiddleKeys && !IsMiddleKey())
        {
            int noteInOctave = buttonIndex % 7;
            Debug.Log($"KeyZone {buttonIndex} ({GetNoteName()}): Не является средней кнопкой, игнорируем нажатие");
            return;
        }
        
        // Предотвращаем повторное нажатие, если клавиша уже нажата
        if (isPressed) return;

        isPressed = true;

        if (soundManager != null)
        {
            soundManager.PlayKeyByButtonIndex(buttonIndex, velocity);
        }
        else
        {
            Debug.LogWarning($"KeyZone {buttonIndex}: SoundManager не найден!");
        }

        // Визуальная обратная связь
        ShowPressFeedback();
        
        // Анимация продавливания
        if (keyAnimation != null)
        {
            keyAnimation.OnKeyPressed();
        }
    }
    
    /// <summary>
    /// Альтернативный метод через SteamVR Hand hover события
    /// </summary>
    private void OnHandHoverBegin(Hand hand)
    {
        if (hand != null)
        {
            try
            {
                // Проверяем, что Hand активен и отслеживается
                if (!hand.isActive || (hand.trackedObject == null && !hand.currentAttachedObjectInfo.HasValue))
                {
                    OnKeyPressed(0.5f);
                    Debug.Log($"[KeyZone] Hand hover begin: {GetNoteName()}, hand not tracked, using default velocity");
                    return;
                }

                Vector3 handVelocity = hand.GetTrackedObjectVelocity();
                float velocity = handVelocity.magnitude;
                
                if (velocity >= 0.01f)
                {
                    float normalizedVelocity = Mathf.Clamp01(velocity / 5f);
                    OnKeyPressed(normalizedVelocity);
                    Debug.Log($"[KeyZone] Hand hover begin: {GetNoteName()}, velocity={velocity:F2}");
                }
                else
                {
                    OnKeyPressed(0.1f);
                    Debug.Log($"[KeyZone] Hand hover begin: {GetNoteName()}, low velocity={velocity:F2}, using min volume");
                }
            }
            catch (System.InvalidOperationException)
            {
                OnKeyPressed(0.5f);
                Debug.Log($"[KeyZone] Hand not ready, using default velocity");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[KeyZone] Error getting hand velocity: {e.Message}, using default");
                OnKeyPressed(0.5f);
            }
        }
    }
    
    /// <summary>
    /// Проверяет, является ли клавиша средней
    /// </summary>
    public bool IsMiddleKey()
    {
        if (!onlyMiddleKeys) return true; // Если ограничение выключено, все клавиши работают
        
        int noteInOctave = buttonIndex % 7; // Нота в октаве (0-6)
        return noteInOctave >= minMiddleKeyIndex && noteInOctave <= maxMiddleKeyIndex;
    }
    
    /// <summary>
    /// Получает название ноты по индексу кнопки
    /// </summary>
    public string GetNoteName()
    {
        if (soundManager != null)
        {
            return soundManager.GetNoteNameByButtonIndex(buttonIndex);
        }
        
        // Fallback если soundManager не найден
        int noteInOctave = buttonIndex % 7;
        int octaveOffset = buttonIndex / 7;
        int currentOctave = 4 + octaveOffset; // Предполагаем базовую октаву 4
        
        if (noteInOctave >= 0 && noteInOctave < whiteKeyNames.Length)
        {
            return $"{whiteKeyNames[noteInOctave]}{currentOctave}";
        }
        return "?";
    }

    /// <summary>
    /// Вызывается когда контроллер отпускает клавишу
    /// </summary>
    public void OnKeyReleased()
    {
        isPressed = false;
        ResetColor();
        
        // Анимация возврата
        if (keyAnimation != null)
        {
            keyAnimation.OnKeyReleased();
        }
    }

    /// <summary>
    /// Настраивает визуальную обратную связь
    /// </summary>
    private void SetupVisualFeedback()
    {
        if (keyRenderer == null)
        {
            keyRenderer = GetComponent<Renderer>();
        }

        if (keyRenderer != null && keyMaterial == null)
        {
            // Создаем копию материала для изменения цвета
            keyMaterial = new Material(keyRenderer.material);
            keyRenderer.material = keyMaterial;
            keyMaterial.color = normalColor;
        }
    }

    /// <summary>
    /// Показывает визуальную обратную связь при нажатии
    /// </summary>
    private void ShowPressFeedback()
    {
        if (keyMaterial != null)
        {
            isPressed = true;
            keyMaterial.color = pressedColor;
        }
    }

    /// <summary>
    /// Сбрасывает цвет клавиши
    /// </summary>
    private void ResetColor()
    {
        if (keyMaterial != null)
        {
            keyMaterial.color = normalColor;
        }
    }

    void OnDrawGizmos()
    {
        // Визуализация в редакторе
        // Цвет зависит от того, является ли клавиша средней
        if (onlyMiddleKeys)
        {
            Gizmos.color = IsMiddleKey() ? Color.green : Color.red;
        }
        else
        {
            Gizmos.color = Color.magenta;
        }
        Gizmos.DrawWireCube(transform.position, transform.localScale);
        
        // Показываем название ноты
        #if UNITY_EDITOR
        string label = GetNoteName();
        if (onlyMiddleKeys)
        {
            label += IsMiddleKey() ? " (Active)" : " (Disabled)";
        }
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.1f, label);
        #endif
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Альтернативный метод через триггер
        if (other.CompareTag("Controller") || other.GetComponent<Hand>() != null)
        {
            Hand hand = other.GetComponent<Hand>();
            if (hand != null)
            {
                OnHandHoverBegin(hand);
            }
            else
            {
                OnKeyPressed(0.5f);
            }
        }
    }
}
