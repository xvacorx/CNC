using UnityEngine;

public class CameraAdjuster : MonoBehaviour
{
    public Camera targetCamera; // Cámara que se ajustará
    public Transform drawingBounds; // GameObject que contiene el dibujo

    public float padding = 1.2f; // Margen adicional para que el dibujo no quede justo al borde

    void Start()
    {
        AdjustCameraToFitDrawing(); // Ajusta la cámara al inicio
    }

    public void AdjustCameraToFitDrawing()
    {
        if (drawingBounds == null || targetCamera == null)
        {
            Debug.LogWarning("Faltan referencias: asegúrate de asignar la cámara y el objeto de dibujo.");
            return;
        }

        Bounds bounds = CalculateBounds(drawingBounds);
        Vector3 center = bounds.center;

        // Resetear las rotaciones de la cámara
        targetCamera.transform.rotation = Quaternion.identity;

        // Calcular la posición en Z para que el dibujo entre en la vista
        float frustumHeight = bounds.size.y * padding;
        float distance = frustumHeight / (2f * Mathf.Tan(Mathf.Deg2Rad * targetCamera.fieldOfView * 0.5f));

        // Mover la cámara a la posición correcta
        targetCamera.transform.position = new Vector3(center.x, center.y, center.z - distance);
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
