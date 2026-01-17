using UnityEngine;
using Valve.VR.InteractionSystem;

/// <summary>
/// Компонент для идентификации струны бас-гитары
/// Аналогичен GuitarString, но для 4 струн
/// </summary>
public class BassString : MonoBehaviour
{
    [Header("String Settings")]
    [Tooltip("Индекс струны: 0 = E (струна 4, толстая), 1 = A (струна 3), 2 = D (струна 2), 3 = G (струна 1, тонкая)")]
    public int stringIndex = 0;

    [Header("Visual Feedback")]
    public Color normalColor = Color.white;
    public Color hitColor = Color.yellow;
    public Renderer stringRenderer;
    public Material stringMaterial;

    private BassSoundManager soundManager;
    private Interactable interactable;

    void Start()
    {
        soundManager = GetComponentInParent<BassSoundManager>();
        
        if (soundManager == null)
        {
            Debug.LogWarning($"BassString {stringIndex} не найден BassSoundManager на родителе!");
        }

        // Добавляем Interactable для лучшего обнаружения в SteamVR
        interactable = GetComponent<Interactable>();
        if (interactable == null)
        {
            interactable = gameObject.AddComponent<Interactable>();
            interactable.hideHandOnAttach = false; // Не скрываем руку
        }

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
                    Debug.Log($"[BassString] Hand hover begin: hand not tracked, using default velocity");
                    return;
                }

                Vector3 handVelocity = hand.GetTrackedObjectVelocity();
                float velocity = handVelocity.magnitude;
                
                if (velocity >= 0.01f)
                {
                    float normalizedVelocity = Mathf.Clamp01(velocity / 5f);
                    OnStringHit(normalizedVelocity);
                    Debug.Log($"[BassString] Hand hover begin: velocity={velocity:F2}");
                }
                else
                {
                    OnStringHit(0.1f);
                    Debug.Log($"[BassString] Hand hover begin: low velocity={velocity:F2}, using min volume");
                }
            }
            catch (System.InvalidOperationException)
            {
                OnStringHit(0.5f);
                Debug.Log($"[BassString] Hand not ready, using default velocity");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[BassString] Error getting hand velocity: {e.Message}, using default");
                OnStringHit(0.5f);
            }
        }
    }

    public void OnStringHit(float velocity = 1f)
    {
        if (soundManager != null)
        {
            soundManager.PlayString(stringIndex, velocity);
        }
        else
        {
            Debug.LogWarning($"BassString {stringIndex}: SoundManager не найден!");
        }

        ShowHitFeedback();
    }

    private void SetupVisualFeedback()
    {
        if (stringRenderer == null)
        {
            stringRenderer = GetComponent<Renderer>();
        }

        if (stringRenderer != null && stringMaterial == null)
        {
            stringMaterial = new Material(stringRenderer.material);
            stringRenderer.material = stringMaterial;
            stringMaterial.color = normalColor;
        }
    }

    private void ShowHitFeedback()
    {
        if (stringMaterial != null)
        {
            stringMaterial.color = hitColor;
            Invoke(nameof(ResetColor), 0.1f);
        }
    }

    private void ResetColor()
    {
        if (stringMaterial != null)
        {
            stringMaterial.color = normalColor;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.01f);
    }
}
