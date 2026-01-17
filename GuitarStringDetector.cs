using UnityEngine;
using Valve.VR.InteractionSystem;

/// <summary>
/// Определяет взаимодействие контроллера со струнами гитары
/// Добавляется на контроллер или на гитару
/// </summary>
public class GuitarStringDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Минимальная скорость движения для воспроизведения звука")]
    public float minVelocity = 0.01f; // Снижено для более чувствительного обнаружения (было 0.1f)
    
    [Tooltip("Максимальная скорость для нормализации")]
    public float maxVelocity = 5f;

    [Tooltip("Время между ударами (cooldown)")]
    public float hitCooldown = 0.1f;

    [Header("Debug")]
    public bool showDebug = true;

    private Hand hand;
    private Vector3 lastPosition;
    private float lastHitTime;
    private MonoBehaviour lastHitObject; // Универсальный для всех типов объектов
    private KeyZone currentPressedKey; // Отслеживаем текущую нажатую клавишу


    void FixedUpdate()
    {
        // Обновляем позицию для расчета скорости в FixedUpdate для более точного расчета
        // Это обеспечит более стабильный расчет скорости
        Vector3 currentPos = transform.position;
        if (Vector3.Distance(currentPos, lastPosition) > 0.001f)
        {
            lastPosition = currentPos;
        }
    }

    /// <summary>
    /// Вызывается когда контроллер входит в триггер струны/зоны/клавиши
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        // ВСЕГДА выводим лог, даже если showDebug = false, для диагностики
        Debug.Log($"[GuitarStringDetector] OnTriggerEnter на {gameObject.name}: {other.name} (tag: {other.tag}, layer: {other.gameObject.layer})");
        
        if (showDebug)
        {
            Debug.Log($"[GuitarStringDetector] Детали: позиция контроллера={transform.position}, позиция объекта={other.transform.position}");
        }

        // Проверяем гитару/бас
        GuitarString guitarString = other.GetComponent<GuitarString>();
        if (guitarString != null)
        {
            if (showDebug) Debug.Log($"[GuitarStringDetector] Found GuitarString on {other.name}");
            HandleGuitarString(guitarString);
            return;
        }

        BassString bassString = other.GetComponent<BassString>();
        if (bassString != null)
        {
            if (showDebug) Debug.Log($"[GuitarStringDetector] Found BassString on {other.name}, index: {bassString.stringIndex}");
            HandleBassString(bassString);
            return;
        }

        // Проверяем ударные
        DrumZone drumZone = other.GetComponent<DrumZone>();
        if (drumZone != null)
        {
            if (showDebug) Debug.Log($"[GuitarStringDetector] Found DrumZone on {other.name}, type: {drumZone.zoneType}");
            HandleDrumZone(drumZone);
            return;
        }

        // Проверяем клавиши
        KeyZone keyZone = other.GetComponent<KeyZone>();
        if (keyZone != null)
        {
            if (showDebug) Debug.Log($"[GuitarStringDetector] Found KeyZone on {other.name}");
            HandleKeyZone(keyZone);
            return;
        }

        if (showDebug)
        {
            Debug.LogWarning($"[GuitarStringDetector] OnTriggerEnter: {other.name} не содержит известных компонентов (GuitarString, BassString, DrumZone, KeyZone)");
        }
    }

    /// <summary>
    /// Обрабатывает касание струны гитары
    /// </summary>
    private void HandleGuitarString(GuitarString guitarString)
    {
        // Проверяем cooldown
        if (Time.time - lastHitTime < hitCooldown && lastHitObject == guitarString)
        {
            return;
        }

        float velocity = CalculateVelocity();
        if (velocity >= minVelocity)
        {
            float normalizedVelocity = Mathf.Clamp01(velocity / maxVelocity);
            guitarString.OnStringHit(normalizedVelocity);
            
            lastHitTime = Time.time;
            lastHitObject = guitarString;

            if (showDebug)
            {
                Debug.Log($"Hit guitar string {guitarString.stringIndex} with velocity {normalizedVelocity:F2}");
            }

            TriggerHapticFeedback(normalizedVelocity);
        }
    }

    /// <summary>
    /// Обрабатывает касание струны баса
    /// </summary>
    private void HandleBassString(BassString bassString)
    {
        if (Time.time - lastHitTime < hitCooldown)
        {
            if (showDebug)
            {
                Debug.Log($"[Bass] Cooldown active: {Time.time - lastHitTime:F2}s < {hitCooldown:F2}s");
            }
            return;
        }

        float velocity = CalculateVelocity();
        if (showDebug)
        {
            Debug.Log($"[Bass] Velocity: {velocity:F2}, minVelocity: {minVelocity:F2}, will play: {velocity >= minVelocity}");
        }

        if (velocity >= minVelocity)
        {
            float normalizedVelocity = Mathf.Clamp01(velocity / maxVelocity);
            bassString.OnStringHit(normalizedVelocity);
            
            lastHitTime = Time.time;
            lastHitObject = bassString;

            if (showDebug)
            {
                Debug.Log($"✓ Hit bass string {bassString.stringIndex} with velocity {normalizedVelocity:F2}");
            }

            TriggerHapticFeedback(normalizedVelocity);
        }
        else
        {
            if (showDebug)
            {
                Debug.LogWarning($"[Bass] Velocity too low: {velocity:F2} < {minVelocity:F2}. Try moving controller faster!");
            }
        }
    }

    /// <summary>
    /// Обрабатывает касание зоны ударных
    /// </summary>
    private void HandleDrumZone(DrumZone drumZone)
    {
        if (Time.time - lastHitTime < hitCooldown)
        {
            if (showDebug)
            {
                Debug.Log($"[Drum] Cooldown active: {Time.time - lastHitTime:F2}s < {hitCooldown:F2}s");
            }
            return;
        }

        float velocity = CalculateVelocity();
        if (showDebug)
        {
            Debug.Log($"[Drum] Velocity: {velocity:F2}, minVelocity: {minVelocity:F2}, will play: {velocity >= minVelocity}");
        }

        if (velocity >= minVelocity)
        {
            float normalizedVelocity = Mathf.Clamp01(velocity / maxVelocity);
            drumZone.OnZoneHit(normalizedVelocity);
            
            lastHitTime = Time.time;
            lastHitObject = drumZone;

            if (showDebug)
            {
                Debug.Log($"✓ Hit drum zone {drumZone.zoneType} with velocity {normalizedVelocity:F2}");
            }

            TriggerHapticFeedback(normalizedVelocity);
        }
        else
        {
            if (showDebug)
            {
                Debug.LogWarning($"[Drum] Velocity too low: {velocity:F2} < {minVelocity:F2}. Try moving controller faster!");
            }
        }
    }

    /// <summary>
    /// Обрабатывает касание клавиши
    /// </summary>
    private void HandleKeyZone(KeyZone keyZone)
    {
        // Если уже нажата другая клавиша, отпускаем её
        if (currentPressedKey != null && currentPressedKey != keyZone)
        {
            currentPressedKey.OnKeyReleased();
        }

        if (Time.time - lastHitTime < hitCooldown && currentPressedKey == keyZone)
        {
            return;
        }

        float velocity = CalculateVelocity();
        if (velocity >= minVelocity)
        {
            float normalizedVelocity = Mathf.Clamp01(velocity / maxVelocity);
            keyZone.OnKeyPressed(normalizedVelocity);
            
            currentPressedKey = keyZone; // Запоминаем нажатую клавишу
            lastHitTime = Time.time;

            if (showDebug)
            {
                Debug.Log($"Pressed key {keyZone.GetNoteName()} (index {keyZone.noteIndex}) with velocity {normalizedVelocity:F2}");
            }

            TriggerHapticFeedback(normalizedVelocity);
        }
    }

    /// <summary>
    /// Вызывается когда контроллер находится в триггере струны
    /// (для непрерывного взаимодействия, например, слайд по струнам)
    /// </summary>
    void OnTriggerStay(Collider other)
    {
        // Можно добавить логику для слайда по струнам
        // Пока оставляем пустым, используем только OnTriggerEnter
    }

    /// <summary>
    /// Вызывается когда контроллер выходит из триггера
    /// Используется для отпускания клавиш
    /// </summary>
    void OnTriggerExit(Collider other)
    {
        // Проверяем клавиши - отпускаем если контроллер вышел из триггера
        KeyZone keyZone = other.GetComponent<KeyZone>();
        if (keyZone != null && currentPressedKey == keyZone)
        {
            keyZone.OnKeyReleased();
            currentPressedKey = null;

            if (showDebug)
            {
                Debug.Log($"Released key {keyZone.GetNoteName()} (index {keyZone.noteIndex})");
            }
        }
    }

    /// <summary>
    /// Рассчитывает скорость движения контроллера
    /// </summary>
    private float CalculateVelocity()
    {
        Vector3 currentPosition = transform.position;
        Vector3 deltaPosition = currentPosition - lastPosition;
        
        // Используем FixedDeltaTime для более стабильного расчета
        float deltaTime = Time.fixedDeltaTime > 0 ? Time.fixedDeltaTime : Time.deltaTime;
        float velocity = deltaPosition.magnitude / deltaTime;
        
        // Если скорость очень маленькая, пробуем альтернативный метод через Rigidbody
        if (velocity < minVelocity)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null && rb.velocity.magnitude > velocity)
            {
                velocity = rb.velocity.magnitude;
                if (showDebug)
                {
                    Debug.Log($"Using Rigidbody velocity: {velocity:F2} m/s (deltaPosition method: {deltaPosition.magnitude / deltaTime:F2})");
                }
            }
        }
        
        if (showDebug)
        {
            Debug.Log($"Velocity calculated: {velocity:F2} m/s (delta: {deltaPosition.magnitude:F4}m, time: {deltaTime:F4}s, minVelocity: {minVelocity:F2})");
        }
        
        return velocity;
    }

    /// <summary>
    /// Вызывает тактильную обратную связь (вибрацию)
    /// </summary>
    private void TriggerHapticFeedback(float intensity)
    {
        if (hand != null)
        {
            // SteamVR haptic feedback
            float duration = 0.05f; // 50ms
            float frequency = 100f; // Hz
            hand.TriggerHapticPulse(duration, frequency, intensity);
        }
    }

    /// <summary>
    /// Альтернативный метод через коллизию (если не используется триггер)
    /// </summary>
    void OnCollisionEnter(Collision collision)
    {
        if (showDebug)
        {
            Debug.Log($"[GuitarStringDetector] OnCollisionEnter: {collision.gameObject.name}");
        }

        // Проверяем все типы инструментов
        GuitarString guitarString = collision.gameObject.GetComponent<GuitarString>();
        if (guitarString != null)
        {
            float velocity = collision.relativeVelocity.magnitude;
            float normalizedVelocity = Mathf.Clamp01(velocity / maxVelocity);
            
            if (normalizedVelocity >= minVelocity)
            {
                guitarString.OnStringHit(normalizedVelocity);
                TriggerHapticFeedback(normalizedVelocity);
            }
            return;
        }

        BassString bassString = collision.gameObject.GetComponent<BassString>();
        if (bassString != null)
        {
            float velocity = collision.relativeVelocity.magnitude;
            float normalizedVelocity = Mathf.Clamp01(velocity / maxVelocity);
            
            if (normalizedVelocity >= minVelocity)
            {
                bassString.OnStringHit(normalizedVelocity);
                TriggerHapticFeedback(normalizedVelocity);
            }
            return;
        }

        DrumZone drumZone = collision.gameObject.GetComponent<DrumZone>();
        if (drumZone != null)
        {
            float velocity = collision.relativeVelocity.magnitude;
            float normalizedVelocity = Mathf.Clamp01(velocity / maxVelocity);
            
            if (normalizedVelocity >= minVelocity)
            {
                drumZone.OnZoneHit(normalizedVelocity);
                TriggerHapticFeedback(normalizedVelocity);
            }
            return;
        }
    }

    /// <summary>
    /// Проверяет, есть ли коллайдер-триггер на этом объекте
    /// Если нет - выводит предупреждение
    /// </summary>
    void Start()
    {
        // Получаем компонент Hand (SteamVR)
        hand = GetComponentInParent<Hand>();
        
        if (hand == null)
        {
            hand = GetComponent<Hand>();
        }

        if (hand == null)
        {
            Debug.LogWarning("GuitarStringDetector: Hand компонент не найден!");
        }

        // Проверяем наличие коллайдера-триггера
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            // Проверяем дочерние объекты
            col = GetComponentInChildren<Collider>();
        }

        if (col == null)
        {
            Debug.LogError($"GuitarStringDetector на {gameObject.name}: НЕТ КОЛЛАЙДЕРА! Добавьте Collider с isTrigger = true. Используйте Tools > Fix Sound System Issues > Настроить VR контроллеры");
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning($"GuitarStringDetector на {gameObject.name}: Коллайдер не является триггером! Установите isTrigger = true. Используйте Tools > Fix Sound System Issues > Настроить VR контроллеры");
        }
        else
        {
            if (showDebug)
            {
                Debug.Log($"GuitarStringDetector на {gameObject.name}: Коллайдер настроен правильно (isTrigger = true)");
            }
        }

        lastPosition = transform.position;
    }
}
