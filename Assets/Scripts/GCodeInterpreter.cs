using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GCodeInterpreter : MonoBehaviour
{
    public LineRenderer rapidLineRenderer;
    private LineRenderer cutLineRenderer;

    private List<Vector3> rapidMovePoints = new List<Vector3>();
    private List<Vector3> cutMovePoints = new List<Vector3>();

    private Material rapidMaterial;
    private Material cutMaterial;

    private bool isLastMoveCut = true; 

    void Awake()
    {
        cutLineRenderer = GetComponent<LineRenderer>();

        rapidMaterial = new Material(Shader.Find("Unlit/Color"));
        rapidMaterial.color = Color.yellow;
        cutMaterial = new Material(Shader.Find("Unlit/Color"));
        cutMaterial.color = Color.red;

        if (rapidLineRenderer != null)
        {
            rapidLineRenderer.material = rapidMaterial;
            rapidLineRenderer.positionCount = 0;
        }

        cutLineRenderer.material = cutMaterial;
        cutLineRenderer.positionCount = 0;
    }

    public void Initialize(float lineWidth)
    {
        rapidMovePoints.Clear();
        cutMovePoints.Clear();

        if (rapidLineRenderer != null)
        {
            rapidLineRenderer.positionCount = 0;
            rapidLineRenderer.startWidth = lineWidth;
            rapidLineRenderer.endWidth = lineWidth;
        }

        cutLineRenderer.positionCount = 0;
        cutLineRenderer.startWidth = lineWidth;
        cutLineRenderer.endWidth = lineWidth;
    }

    public void AddGCodeCommand(int gCode, Vector3 point, float radius = 0)
    {
        point.z = -point.z;

        if (gCode == 0)
        {
            // G0: Movimiento rápido
            if (isLastMoveCut)
            {
                rapidMovePoints.Clear();
                rapidMovePoints.Add(cutMovePoints[^1]); // Continuidad desde el último punto de G1
            }
            rapidMovePoints.Add(point);
            UpdateLineRenderer(rapidLineRenderer, rapidMovePoints);
            isLastMoveCut = false;
        }
        else if (gCode == 1 || gCode == 2 || gCode == 3)
        {
            // G1/G2/G3: Movimientos de corte
            if (!isLastMoveCut)
            {
                cutMovePoints.Clear();
                cutMovePoints.Add(rapidMovePoints[^1]); // Continuidad desde el último punto de G0
            }

            if (gCode == 1)
            {
                // Movimiento lineal
                cutMovePoints.Add(point);
            }
            else
            {
                // Movimiento en arco (G2/G3)
                AddArc(gCode, cutMovePoints[^1], point, radius);
            }

            UpdateLineRenderer(cutLineRenderer, cutMovePoints);
            isLastMoveCut = true;
        }
    }

    private void AddArc(int gCode, Vector3 start, Vector3 end, float radius)
    {
        Vector3 midPoint = (start + end) / 2;
        Vector3 dir = (end - start).normalized;

        Vector3 normal = Vector3.Cross(dir, Vector3.forward).normalized;
        if (gCode == 3) // Anti-horario
            normal = -normal;

        float halfChord = Vector3.Distance(start, midPoint);
        float height = Mathf.Sqrt(Mathf.Abs(radius * radius - halfChord * halfChord));

        Vector3 center = midPoint + normal * height;

        Vector3 startToCenter = start - center;
        Vector3 endToCenter = end - center;

        float startAngle = Mathf.Atan2(startToCenter.y, startToCenter.x);
        float endAngle = Mathf.Atan2(endToCenter.y, endToCenter.x);

        if (gCode == 2 && startAngle < endAngle) startAngle += 2 * Mathf.PI;
        if (gCode == 3 && startAngle > endAngle) endAngle += 2 * Mathf.PI;

        int numSegments = 20;
        for (int i = 0; i <= numSegments; i++)
        {
            float t = i / (float)numSegments;
            float angle = Mathf.Lerp(startAngle, endAngle, t);

            float x = center.x + radius * Mathf.Cos(angle);
            float y = center.y + radius * Mathf.Sin(angle);

            cutMovePoints.Add(new Vector3(x, y, start.z));
        }
    }

    private void UpdateLineRenderer(LineRenderer lineRenderer, List<Vector3> points)
    {
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = points.Count;
            for (int i = 0; i < points.Count; i++)
            {
                lineRenderer.SetPosition(i, points[i]);
            }
        }
    }

    public void UndoLastMovement()
    {
        if (isLastMoveCut)
        {
            // Eliminar el último punto de G1/G2/G3
            if (cutMovePoints.Count > 1)
            {
                cutMovePoints.RemoveAt(cutMovePoints.Count - 1);
                UpdateLineRenderer(cutLineRenderer, cutMovePoints);
            }
        }
        else
        {
            // Eliminar el último punto de G0
            if (rapidMovePoints.Count > 1)
            {
                rapidMovePoints.RemoveAt(rapidMovePoints.Count - 1);
                UpdateLineRenderer(rapidLineRenderer, rapidMovePoints);
            }
        }
    }

    public void ToggleRapidMoves()
    {
        if (rapidLineRenderer != null)
        {
            rapidLineRenderer.enabled = !rapidLineRenderer.enabled;
        }
    }

    public void ClearAll()
    {
        rapidMovePoints.Clear();
        cutMovePoints.Clear();

        if (rapidLineRenderer != null) rapidLineRenderer.positionCount = 0;
        cutLineRenderer.positionCount = 0;
    }
}
