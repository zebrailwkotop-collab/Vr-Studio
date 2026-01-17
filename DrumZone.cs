using UnityEngine;
using Valve.VR.InteractionSystem;

/// <summary>
/// Компонент для идентификации зоны ударной установки
/// Добавляется на каждый GameObject зоны (Kick, Snare, HiHat, и т.д.)
/// </summary>
public class DrumZone : MonoBehaviour
{
    [Header("Zone Settings")]
    [Tooltip("Тип зоны ударной установки")]
    public DrumZoneType zoneType = DrumZoneType.Kick;

    [Header("Visual Feedback")]
    public Color normalColor = Color.white;
    public Color hitColor = Color.yellow;
    public Renderer zoneRenderer;
    public Material zoneMaterial;

    private DrumsSoundManager soundManager;
    private Interactable interactable;

    void Start()
    {
        // Находим DrumsSoundManager на родительском объекте (ударной установке)
        soundManager = GetComponentInParent<DrumsSoundManager>();
        
        if (soundManager == null)
        {
            Debug.LogWarning($"DrumZone {zoneType} не найден DrumsSoundManager на родителе!");
        }

        // Добавляем Interactable для лучшего обнаружения в SteamVR
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
                    // Hand не отслеживается, используем фиксированную скорость
                    OnZoneHit(0.5f);
                    Debug.Log($"[DrumZone] Hand hover begin: {zoneType}, hand not tracked, using default velocity");
                    return;
                }

                Vector3 handVelocity = hand.GetTrackedObjectVelocity();
                float velocity = handVelocity.magnitude;
                
                if (velocity >= 0.01f)
                {
                    float normalizedVelocity = Mathf.Clamp01(velocity / 5f);
                    OnZoneHit(normalizedVelocity);
                    Debug.Log($"[DrumZone] Hand hover begin: {zoneType}, velocity={velocity:F2}");
                }
                else
                {
                    OnZoneHit(0.1f);
                    Debug.Log($"[DrumZone] Hand hover begin: {zoneType}, low velocity={velocity:F2}, using min volume");
                }
            }
            catch (System.InvalidOperationException)
            {
                // Hand не отслеживается или не готов, используем фиксированную скорость
                OnZoneHit(0.5f);
                Debug.Log($"[DrumZone] Hand not ready, using default velocity for {zoneType}");
            }
            catch (System.Exception e)
            {
                // Если не удалось получить скорость, используем фиксированную
                Debug.LogWarning($"[DrumZone] Error getting hand velocity: {e.Message}, using default");
                OnZoneHit(0.5f);
            }
        }
    }

    /// <summary>
    /// Вызывается когда контроллер касается зоны
    /// </summary>
    public void OnZoneHit(float velocity = 1f)
    {
        if (soundManager != null)
        {
            soundManager.PlayDrum(zoneType, velocity);
        }
        else
        {
            Debug.LogWarning($"DrumZone {zoneType}: SoundManager не найден!");
        }

        // Визуальная обратная связь
        ShowHitFeedback();
    }

    /// <summary>
    /// Настраивает визуальную обратную связь
    /// </summary>
    private void SetupVisualFeedback()
    {
        if (zoneRenderer == null)
        {
            zoneRenderer = GetComponent<Renderer>();
        }

        if (zoneRenderer != null && zoneMaterial == null)
        {
            // Создаем копию материала для изменения цвета
            zoneMaterial = new Material(zoneRenderer.material);
            zoneRenderer.material = zoneMaterial;
            zoneMaterial.color = normalColor;
        }
    }

    /// <summary>
    /// Показывает визуальную обратную связь при ударе
    /// </summary>
    private void ShowHitFeedback()
    {
        if (zoneMaterial != null)
        {
            zoneMaterial.color = hitColor;
            // Возвращаем цвет обратно через короткое время
            Invoke(nameof(ResetColor), 0.1f);
        }
    }

    /// <summary>
    /// Сбрасывает цвет зоны
    /// </summary>
    private void ResetColor()
    {
        if (zoneMaterial != null)
        {
            zoneMaterial.color = normalColor;
        }
    }

    void OnDrawGizmos()
    {
        // Визуализация в редакторе
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.05f);
        
        // Показываем тип зоны
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.1f, zoneType.ToString());
        #endif
    }
}
