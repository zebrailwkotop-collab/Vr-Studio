using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Компонент провода для визуального отображения подключений
/// Создается динамически при подключении оборудования
/// </summary>
public class Cable : MonoBehaviour
{
    [Header("Cable Settings")]
    [Tooltip("Источник (откуда идет провод)")]
    public Transform source;

    [Tooltip("Назначение (куда идет провод)")]
    public Transform destination;

    [Tooltip("Инструмент-источник (если есть)")]
    public InstrumentIdentity sourceInstrument;

    [Tooltip("Динамик-назначение (если есть)")]
    public Speaker destinationSpeaker;

    [Tooltip("Микрофон-источник (если есть)")]
    public MicrophoneRecorder sourceMicrophone;

    [Tooltip("Записывающее устройство-назначение (если есть)")]
    public RecordingDevice destinationRecorder;

    [Header("Visual Settings")]
    [Tooltip("Материал провода")]
    public Material cableMaterial;

    [Tooltip("Толщина провода")]
    public float cableThickness = 0.01f;

    [Tooltip("Количество сегментов для плавности")]
    public int segments = 20;

    private LineRenderer lineRenderer;
    private List<Vector3> cablePoints = new List<Vector3>();

    void Awake()
    {
        // Создаем LineRenderer для визуализации провода
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        SetupLineRenderer();
    }

    void Start()
    {
        UpdateCable();
    }

    void Update()
    {
        // Обновляем позицию провода каждый кадр
        if (source != null && destination != null)
        {
            UpdateCable();
        }
    }

    /// <summary>
    /// Настраивает LineRenderer
    /// </summary>
    private void SetupLineRenderer()
    {
        lineRenderer.material = cableMaterial != null ? cableMaterial : CreateDefaultMaterial();
        lineRenderer.startWidth = cableThickness;
        lineRenderer.endWidth = cableThickness;
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = segments;
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
    }

    /// <summary>
    /// Создает материал по умолчанию
    /// </summary>
    private Material CreateDefaultMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.black;
        return mat;
    }

    /// <summary>
    /// Обновляет визуализацию провода
    /// </summary>
    private void UpdateCable()
    {
        if (source == null || destination == null) return;

        Vector3 startPos = source.position;
        Vector3 endPos = destination.position;

        // Вычисляем промежуточные точки для плавной кривой (провод провисает)
        cablePoints.Clear();
        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            Vector3 point = Vector3.Lerp(startPos, endPos, t);
            
            // Добавляем провисание (синусоида)
            float sag = Mathf.Sin(t * Mathf.PI) * 0.1f;
            point.y -= sag;
            
            cablePoints.Add(point);
        }

        // Устанавливаем точки в LineRenderer
        lineRenderer.positionCount = cablePoints.Count;
        for (int i = 0; i < cablePoints.Count; i++)
        {
            lineRenderer.SetPosition(i, cablePoints[i]);
        }
    }

    /// <summary>
    /// Устанавливает источник и назначение
    /// </summary>
    public void SetConnection(Transform source, Transform destination)
    {
        this.source = source;
        this.destination = destination;
        UpdateCable();
    }

    /// <summary>
    /// Удаляет провод
    /// </summary>
    public void DestroyCable()
    {
        Destroy(gameObject);
    }

    void OnDrawGizmos()
    {
        if (source != null && destination != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(source.position, destination.position);
        }
    }
}
