using UnityEngine;

public class CameraAdjuster : MonoBehaviour
{
    public Camera targetCamera;
    public Transform drawingBounds;

    public float padding = 1.2f;

    void Start()
    {
        AdjustCameraToFitDrawing();
    }

    public void AdjustCameraToFitDrawing()
    {

        Bounds bounds = CalculateBounds(drawingBounds);
        Vector3 center = bounds.center;

        targetCamera.transform.rotation = Quaternion.identity;

        float frustumHeight = bounds.size.y * padding;
        float distance = frustumHeight / (2f * Mathf.Tan(Mathf.Deg2Rad * targetCamera.fieldOfView * 0.5f));

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
