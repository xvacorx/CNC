using UnityEngine;

public class OrbitalCameraController : MonoBehaviour
{
    [Header("Objeto de enfoque")]
    public Transform target;

    [Header("Control de órbita")]
    public float rotationSpeed = 100f;
    public float minYAngle = 10f;
    public float maxYAngle = 80f;

    [Header("Control de zoom")]
    public float zoomSpeed = 10f;
    public float minZoomDistance = 2f;
    public float maxZoomDistance = 20f;

    [Header("Control de ajustes automáticos")]
    public Camera targetCamera;
    public Transform drawingBounds;
    public float padding = 1.2f;

    private float currentDistance;
    private float currentYaw = 0f;
    private float currentPitch = 45f;

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
