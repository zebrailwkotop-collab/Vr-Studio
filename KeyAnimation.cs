using UnityEngine;

/// <summary>
/// Анимация продавливания клавиши при нажатии
/// Добавляется на каждый GameObject клавиши вместе с KeyZone
/// </summary>
[RequireComponent(typeof(KeyZone))]
public class KeyAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Расстояние, на которое клавиша продавливается (в метрах)")]
    public float pressDistance = 0.01f; // 1 см
    
    [Tooltip("Скорость продавливания")]
    public float pressSpeed = 0.1f;
    
    [Tooltip("Скорость возврата")]
    public float releaseSpeed = 0.15f;

    [Header("Rotation (опционально)")]
    [Tooltip("Угол наклона клавиши при нажатии (в градусах)")]
    public float pressRotation = 2f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool isPressed = false;
    private float currentPressAmount = 0f; // 0 = не нажата, 1 = полностью нажата
    private KeyZone keyZone;

    void Start()
    {
        // Сохраняем начальную позицию и поворот
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
        
        // Получаем KeyZone для отслеживания нажатий
        keyZone = GetComponent<KeyZone>();
    }

    void Update()
    {
        // Плавная анимация продавливания/возврата
        if (isPressed)
        {
            // Продавливаем клавишу
            currentPressAmount = Mathf.MoveTowards(currentPressAmount, 1f, pressSpeed * Time.deltaTime);
        }
        else
        {
            // Возвращаем клавишу
            currentPressAmount = Mathf.MoveTowards(currentPressAmount, 0f, releaseSpeed * Time.deltaTime);
        }

        // Применяем анимацию
        ApplyAnimation();
    }

    /// <summary>
    /// Применяет анимацию продавливания
    /// </summary>
    private void ApplyAnimation()
    {
        // Вычисляем смещение
        Vector3 offset = Vector3.down * (pressDistance * currentPressAmount);
        transform.localPosition = initialPosition + offset;

        // Применяем поворот (если нужен)
        if (pressRotation > 0f)
        {
            float rotationAmount = pressRotation * currentPressAmount;
            transform.localRotation = initialRotation * Quaternion.Euler(rotationAmount, 0f, 0f);
        }
    }

    /// <summary>
    /// Вызывается когда клавиша нажата
    /// </summary>
    public void OnKeyPressed()
    {
        isPressed = true;
    }

    /// <summary>
    /// Вызывается когда клавиша отпущена
    /// </summary>
    public void OnKeyReleased()
    {
        isPressed = false;
    }

    /// <summary>
    /// Сбрасывает анимацию в начальное состояние
    /// </summary>
    public void ResetAnimation()
    {
        isPressed = false;
        currentPressAmount = 0f;
        transform.localPosition = initialPosition;
        transform.localRotation = initialRotation;
    }

    void OnDrawGizmosSelected()
    {
        // Визуализация в редакторе
        Gizmos.color = Color.cyan;
        Vector3 pressedPosition = transform.position + Vector3.down * pressDistance;
        Gizmos.DrawLine(transform.position, pressedPosition);
        Gizmos.DrawWireSphere(pressedPosition, 0.005f);
    }
}
