using UnityEngine;
using Valve.VR.InteractionSystem;

/// <summary>
/// Компонент для идентификации струны гитары
/// Добавляется на каждый GameObject струны
/// </summary>
public class GuitarString : MonoBehaviour
{
    [Header("String Settings")]
    [Tooltip("Индекс струны: 0 = E (струна 6, нижняя, толстая), 1 = A (струна 5), 2 = D (струна 4), 3 = G (струна 3), 4 = B (струна 2), 5 = E (струна 1, верхняя, тонкая)")]
    public int stringIndex = 0;

    [Header("Visual Feedback")]
    public Color normalColor = Color.white;
    public Color hitColor = Color.yellow;
    public Renderer stringRenderer;
    public Material stringMaterial;

    private GuitarSoundManager soundManager;
    private Interactable interactable;

    void Start()
    {
        // Находим GuitarSoundManager на родительском объекте (гитаре)
        soundManager = GetComponentInParent<GuitarSoundManager>();
        
        if (soundManager == null)
        {
            Debug.LogWarning($"GuitarString {stringIndex} не найден GuitarSoundManager на родителе!");
        }

        // Добавляем Interactable для лучшего обнаружения в SteamVR (опционально)
        interactable = GetComponent<Interactable>();
        if (interactable == null)
        {
            interactable = gameObject.AddComponent<Interactable>();
            interactable.hideHandOnAttach = false; // Не скрываем руку
        }

        // Настраиваем визуальную обратную связь
        SetupVisualFeedback();
    }

    // Альтернативный метод через SteamVR Hand hover события
    private void OnHandHoverBegin(Hand hand)
    {
        if (hand != null)
        {
            try
            {
                // Проверяем, что Hand активен и отслеживается
                if (!hand.isActive || (hand.trackedObject == null && !hand.currentAttachedObjectInfo.HasValue))
                {
                    OnStringHit(0.5f);
                    Debug.Log($"[GuitarString] Hand hover begin: hand not tracked, using default velocity");
                    return;
                }

                Vector3 handVelocity = hand.GetTrackedObjectVelocity();
                float velocity = handVelocity.magnitude;
                
                if (velocity >= 0.01f)
                {
                    float normalizedVelocity = Mathf.Clamp01(velocity / 5f);
                    OnStringHit(normalizedVelocity);
                    Debug.Log($"[GuitarString] Hand hover begin: velocity={velocity:F2}");
                }
                else
                {
                    OnStringHit(0.1f);
                    Debug.Log($"[GuitarString] Hand hover begin: low velocity={velocity:F2}, using min volume");
                }
            }
            catch (System.InvalidOperationException)
            {
                OnStringHit(0.5f);
                Debug.Log($"[GuitarString] Hand not ready, using default velocity");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[GuitarString] Error getting hand velocity: {e.Message}, using default");
                OnStringHit(0.5f);
            }
        }
    }

    /// <summary>
    /// Вызывается когда контроллер касается струны
    /// </summary>
    public void OnStringHit(float velocity = 1f)
    {
        if (soundManager != null)
        {
            soundManager.PlayString(stringIndex, velocity);
        }
        else
        {
            Debug.LogWarning($"GuitarString {stringIndex}: SoundManager не найден!");
        }

        // Визуальная обратная связь
        ShowHitFeedback();
    }

    /// <summary>
    /// Настраивает визуальную обратную связь
    /// </summary>
    private void SetupVisualFeedback()
    {
        if (stringRenderer == null)
        {
            stringRenderer = GetComponent<Renderer>();
        }

        if (stringRenderer != null && stringMaterial == null)
        {
            // Создаем копию материала для изменения цвета
            stringMaterial = new Material(stringRenderer.material);
            stringRenderer.material = stringMaterial;
            stringMaterial.color = normalColor;
        }
    }

    /// <summary>
    /// Показывает визуальную обратную связь при ударе
    /// </summary>
    private void ShowHitFeedback()
    {
        if (stringMaterial != null)
        {
            stringMaterial.color = hitColor;
            // Возвращаем цвет обратно через короткое время
            Invoke(nameof(ResetColor), 0.1f);
        }
    }

    /// <summary>
    /// Сбрасывает цвет струны
    /// </summary>
    private void ResetColor()
    {
        if (stringMaterial != null)
        {
            stringMaterial.color = normalColor;
        }
    }

    void OnDrawGizmos()
    {
        // Визуализация в редакторе
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.01f);
    }
}
