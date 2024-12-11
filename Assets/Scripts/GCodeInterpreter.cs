using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GCodeInterpreter : MonoBehaviour
{
    public LineRenderer rapidLineRenderer;
    private LineRenderer cutLineRenderer;

    private List<MoveSegment> rapidMoves = new List<MoveSegment>();
    private List<MoveSegment> cutMoves = new List<MoveSegment>();

    private Material rapidMaterial;
    private Material cutMaterial;

    private bool isLastMoveCut = true;

    private class MoveSegment
    {
        public int GCode;
        public List<Vector3> Points = new List<Vector3>();
    }

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
        rapidMoves.Clear();
        cutMoves.Clear();

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
        MoveSegment segment = new MoveSegment { GCode = gCode };

        if (gCode == 0)
        {
            if (isLastMoveCut && cutMoves.Count > 0)
            {
                segment.Points.Add(cutMoves[^1].Points[^1]);
            }
            segment.Points.Add(point);
            rapidMoves.Add(segment);
            UpdateLineRenderer(rapidLineRenderer, GetAllPoints(rapidMoves));
            isLastMoveCut = false;
        }
        else if (gCode == 1 || gCode == 2 || gCode == 3)
        {
            if (!isLastMoveCut && rapidMoves.Count > 0)
            {
                segment.Points.Add(rapidMoves[^1].Points[^1]);
            }
            if (gCode == 1)
            {
                segment.Points.Add(point);
            }
            else
            {
                AddArc(gCode, segment.Points[^1], point, radius, segment.Points);
            }
            cutMoves.Add(segment);
            UpdateLineRenderer(cutLineRenderer, GetAllPoints(cutMoves));
            isLastMoveCut = true;
        }
    }

    private void AddArc(int gCode, Vector3 start, Vector3 end, float radius, List<Vector3> points)
    {
        Vector3 midPoint = (start + end) / 2;
        Vector3 dir = (end - start).normalized;

        Vector3 normal = Vector3.Cross(dir, Vector3.forward).normalized;
        if (gCode == 3)
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

            points.Add(new Vector3(x, y, start.z));
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

    private List<Vector3> GetAllPoints(List<MoveSegment> segments)
    {
        List<Vector3> points = new List<Vector3>();
        foreach (var segment in segments)
        {
            points.AddRange(segment.Points);
        }
        return points;
    }

    public void UndoLastMovement()
    {
        if (isLastMoveCut && cutMoves.Count > 0)
        {
            // Eliminar el último segmento de corte
            cutMoves.RemoveAt(cutMoves.Count - 1);

            // Actualizar el LineRenderer con las líneas restantes
            UpdateLineRenderer(cutLineRenderer, GetAllPoints(cutMoves));
        }
        else if (!isLastMoveCut && rapidMoves.Count > 0)
        {
            // Eliminar el último segmento de movimiento rápido
            rapidMoves.RemoveAt(rapidMoves.Count - 1);

            // Actualizar el LineRenderer con las líneas restantes
            UpdateLineRenderer(rapidLineRenderer, GetAllPoints(rapidMoves));
        }
    }


    public void ToggleLineVisibility()
    {
        if (rapidLineRenderer != null)
            rapidLineRenderer.enabled = !rapidLineRenderer.enabled;

        cutLineRenderer.enabled = !cutLineRenderer.enabled;
    }
}
