using UnityEngine;

public class OrbitalCameraController : MonoBehaviour
{
    [Header("Objeto de enfoque")]
    public Transform target; // Objeto alrededor del cual la camara orbitará

    [Header("Control de órbita")]
    public float rotationSpeed = 100f; // Velocidad de rotación
    public float minYAngle = 10f; // Ángulo Y
    public float maxYAngle = 80f; // Ángulo Y

    [Header("Control de zoom")]
    public float zoomSpeed = 10f; // Velocidad de zoom
    public float minZoomDistance = 2f; // Distancia mínima al objeto
    public float maxZoomDistance = 20f; // Distancia máxima al objeto

    [Header("Control de ajustes automáticos")]
    public Camera targetCamera; // Cámara que será ajustada
    public Transform drawingBounds; // define el área del dibujo
    public float padding = 1.2f; // Margen adicional

    private float currentDistance; // Distancia de cámara
    private float currentYaw = 0f; // Rotación horizontal
    private float currentPitch = 45f; // Rotación vertical

    void Start()
    {
        currentDistance = Mathf.Clamp(Vector3.Distance(transform.position, target.position), minZoomDistance, maxZoomDistance);
        AdjustCameraToFitDrawing(); // Inicializar cámara
    }

    void Update()
    {
        HandleOrbit();
        HandleZoom();
    }

    private void HandleOrbit()
    {
        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            float mouseY = -Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

            currentYaw += mouseX;
            currentPitch = Mathf.Clamp(currentPitch + mouseY, minYAngle, maxYAngle);
        }

        Vector3 direction = new Vector3(0, 0, -currentDistance);
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);

        transform.position = target.position + rotation * direction;
        transform.LookAt(target);
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (targetCamera.orthographic)
        {
            targetCamera.orthographicSize -= scroll * zoomSpeed;
            targetCamera.orthographicSize = Mathf.Clamp(targetCamera.orthographicSize, minZoomDistance, maxZoomDistance);
        }
        else
        {
            currentDistance -= scroll * zoomSpeed;
            currentDistance = Mathf.Clamp(currentDistance, minZoomDistance, maxZoomDistance);
        }
    }

    public void ToggleView()
    {
        targetCamera.orthographic = !targetCamera.orthographic;
        AdjustCameraToFitDrawing(); // Ajustar cámara
    }

    public void AdjustCameraToFitDrawing()
    {
        if (drawingBounds == null || targetCamera == null)
        {
            Debug.LogWarning("Debe asignar 'drawingBounds' y 'targetCamera' en el Inspector.");
            return;
        }

        Bounds bounds = CalculateBounds(drawingBounds);
        Vector3 center = bounds.center;

        // Ajustar el transform del target al centro del dibujo
        target.position = center;

        if (targetCamera.orthographic)
        {
            targetCamera.orthographicSize = Mathf.Max(bounds.size.x, bounds.size.y) * padding * 0.5f;
            currentDistance = maxZoomDistance; // Mantener una distancia maxima
        }
        else
        {
            float frustumHeight = bounds.size.y * padding;
            currentDistance = frustumHeight / (2f * Mathf.Tan(Mathf.Deg2Rad * targetCamera.fieldOfView * 0.5f));
        }

        currentPitch = 0f;
        currentYaw = 0f;
        HandleOrbit(); // Actualizar posición
    }


    private Bounds CalculateBounds(Transform root)
    {
        Bounds bounds = new Bounds(root.position, Vector3.zero);
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }

        return bounds;
    }
}
