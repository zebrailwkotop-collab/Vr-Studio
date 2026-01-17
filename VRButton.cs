using UnityEngine;
using UnityEngine.Events;
using Valve.VR;

/// <summary>
/// Простая кнопка для VR взаимодействия
/// </summary>
public class VRButton : MonoBehaviour
{
    [Header("Button Settings")]
    public UnityEvent OnButtonPressed;
    public UnityEvent OnButtonHoverEnter;
    public UnityEvent OnButtonHoverExit;

    [Header("Visual Feedback")]
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;
    public Color pressedColor = Color.red;
    public float pressDistance = 0.01f; // Расстояние нажатия

    private Renderer buttonRenderer;
    private Material buttonMaterial;
    private bool isHovered = false;
    private bool isPressed = false;
    private Vector3 initialPosition;

    void Start()
    {
        buttonRenderer = GetComponent<Renderer>();
        if (buttonRenderer != null)
        {
            buttonMaterial = buttonRenderer.material;
            if (buttonMaterial != null)
            {
                buttonMaterial.color = normalColor;
            }
        }

        initialPosition = transform.localPosition;
    }

    /// <summary>
    /// Вызывается при наведении на кнопку
    /// </summary>
    public void OnHoverEnter()
    {
        isHovered = true;
        if (buttonMaterial != null)
        {
            buttonMaterial.color = hoverColor;
        }
        OnButtonHoverEnter?.Invoke();
    }

    /// <summary>
    /// Вызывается при уходе с кнопки
    /// </summary>
    public void OnHoverExit()
    {
        isHovered = false;
        if (buttonMaterial != null)
        {
            buttonMaterial.color = normalColor;
        }
        OnButtonHoverExit?.Invoke();
    }

    /// <summary>
    /// Вызывается при нажатии на кнопку
    /// </summary>
    public void OnPress()
    {
        if (!isPressed)
        {
            isPressed = true;
            if (buttonMaterial != null)
            {
                buttonMaterial.color = pressedColor;
            }

            // Визуальная анимация нажатия
            transform.localPosition = initialPosition - transform.forward * pressDistance;

            OnButtonPressed?.Invoke();
        }
    }

    /// <summary>
    /// Вызывается при отпускании кнопки
    /// </summary>
    public void OnRelease()
    {
        if (isPressed)
        {
            isPressed = false;
            transform.localPosition = initialPosition;

            if (buttonMaterial != null)
            {
                buttonMaterial.color = isHovered ? hoverColor : normalColor;
            }
        }
    }

    /// <summary>
    /// Программное нажатие кнопки
    /// </summary>
    public void Press()
    {
        OnPress();
        // Автоматически отпускаем через короткое время
        Invoke(nameof(OnRelease), 0.1f);
    }
}
