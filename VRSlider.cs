using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

/// <summary>
/// VR-слайдер для взаимодействия через контроллеры SteamVR
/// </summary>
[RequireComponent(typeof(Slider))]
public class VRSlider : MonoBehaviour
{
    [Header("VR Settings")]
    [Tooltip("Расстояние взаимодействия с контроллером")]
    public float interactionDistance = 0.1f;
    
    [Tooltip("Использовать лазерный указатель для взаимодействия")]
    public bool useLaserPointer = true;

    [Header("Visual Feedback")]
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;
    public Color activeColor = Color.green;

    private Slider slider;
    private Image handleImage;
    private Image fillImage;
    private bool isHovered = false;
    private bool isDragging = false;
    private Hand currentHand;

    void Start()
    {
        slider = GetComponent<Slider>();
        
        // Находим изображения для визуальной обратной связи
        if (slider.handleRect != null)
        {
            handleImage = slider.handleRect.GetComponent<Image>();
        }
        
        if (slider.fillRect != null)
        {
            fillImage = slider.fillRect.GetComponent<Image>();
        }

        // Добавляем Interactable для SteamVR взаимодействия
        SetupInteractable();
    }

    /// <summary>
    /// Настраивает Interactable для SteamVR
    /// </summary>
    private void SetupInteractable()
    {
        Interactable interactable = GetComponent<Interactable>();
        if (interactable == null)
        {
            interactable = gameObject.AddComponent<Interactable>();
        }
    }

    void Update()
    {
        // Проверяем взаимодействие через Interactable
        Interactable interactable = GetComponent<Interactable>();
        if (interactable != null)
        {
            // Проверяем, есть ли рука, наводящаяся на слайдер
            bool wasHovered = isHovered;
            isHovered = interactable.isHovering && interactable.hoveringHand != null;

            if (isHovered && !wasHovered)
            {
                // Начало наведения
                OnHandHoverBegin();
            }
            else if (!isHovered && wasHovered)
            {
                // Конец наведения
                OnHandHoverEnd();
            }

            if (isHovered)
            {
                // Обновление при наведении
                OnHandHoverUpdate(interactable.hoveringHand);
            }
        }
    }

    /// <summary>
    /// Вызывается когда рука начинает наводиться на слайдер
    /// </summary>
    private void OnHandHoverBegin()
    {
        UpdateVisualFeedback();
    }

    /// <summary>
    /// Вызывается когда рука уходит со слайдера
    /// </summary>
    private void OnHandHoverEnd()
    {
        isDragging = false;
        currentHand = null;
        UpdateVisualFeedback();
    }

    /// <summary>
    /// Вызывается когда рука обновляет позицию над слайдером
    /// </summary>
    private void OnHandHoverUpdate(Hand hand)
    {
        currentHand = hand;

        // Если нажата кнопка захвата, начинаем перетаскивание
        if (hand.GetGrabStarting() != GrabTypes.None)
        {
            isDragging = true;
            UpdateVisualFeedback();
        }

        // Если отпущена кнопка, прекращаем перетаскивание
        if (hand.GetGrabEnding() != GrabTypes.None)
        {
            isDragging = false;
            UpdateVisualFeedback();
        }

        // Если перетаскиваем, обновляем значение слайдера
        if (isDragging)
        {
            UpdateSliderValue(hand);
        }
    }

    /// <summary>
    /// Обновляет значение слайдера на основе позиции контроллера
    /// </summary>
    private void UpdateSliderValue(Hand hand)
    {
        if (slider == null) return;

        // Получаем позицию контроллера в локальных координатах слайдера
        Vector3 localPoint = transform.InverseTransformPoint(hand.transform.position);
        
        // Определяем направление слайдера (горизонтальный или вертикальный)
        RectTransform sliderRect = slider.GetComponent<RectTransform>();
        bool isHorizontal = slider.direction == Slider.Direction.LeftToRight || 
                           slider.direction == Slider.Direction.RightToLeft;

        float normalizedValue;
        
        if (isHorizontal)
        {
            // Горизонтальный слайдер
            float minX = sliderRect.rect.xMin;
            float maxX = sliderRect.rect.xMax;
            normalizedValue = Mathf.InverseLerp(minX, maxX, localPoint.x);
        }
        else
        {
            // Вертикальный слайдер
            float minY = sliderRect.rect.yMin;
            float maxY = sliderRect.rect.yMax;
            normalizedValue = Mathf.InverseLerp(minY, maxY, localPoint.y);
        }

        // Учитываем направление слайдера
        if (slider.direction == Slider.Direction.RightToLeft || 
            slider.direction == Slider.Direction.TopToBottom)
        {
            normalizedValue = 1f - normalizedValue;
        }

        // Устанавливаем значение
        slider.value = Mathf.Lerp(slider.minValue, slider.maxValue, normalizedValue);
    }

    /// <summary>
    /// Обновляет визуальную обратную связь
    /// </summary>
    private void UpdateVisualFeedback()
    {
        Color targetColor = normalColor;
        
        if (isDragging)
        {
            targetColor = activeColor;
        }
        else if (isHovered)
        {
            targetColor = hoverColor;
        }

        if (handleImage != null)
        {
            handleImage.color = targetColor;
        }
    }

    /// <summary>
    /// Альтернативный метод через Raycast (для лазерного указателя)
    /// </summary>
    public void OnRaycastHit(Vector3 hitPoint)
    {
        if (!useLaserPointer) return;

        // Конвертируем точку попадания в значение слайдера
        RectTransform sliderRect = slider.GetComponent<RectTransform>();
        Vector3 localPoint = sliderRect.InverseTransformPoint(hitPoint);
        
        bool isHorizontal = slider.direction == Slider.Direction.LeftToRight || 
                           slider.direction == Slider.Direction.RightToLeft;

        float normalizedValue;
        
        if (isHorizontal)
        {
            float minX = sliderRect.rect.xMin;
            float maxX = sliderRect.rect.xMax;
            normalizedValue = Mathf.InverseLerp(minX, maxX, localPoint.x);
        }
        else
        {
            float minY = sliderRect.rect.yMin;
            float maxY = sliderRect.rect.yMax;
            normalizedValue = Mathf.InverseLerp(minY, maxY, localPoint.y);
        }

        if (slider.direction == Slider.Direction.RightToLeft || 
            slider.direction == Slider.Direction.TopToBottom)
        {
            normalizedValue = 1f - normalizedValue;
        }

        slider.value = Mathf.Lerp(slider.minValue, slider.maxValue, normalizedValue);
    }
}
