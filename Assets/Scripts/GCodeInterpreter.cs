using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GCodeInterpreter : MonoBehaviour
{
    public LineRenderer rapidLineRenderer;  // Para movimientos G0 (rápidos)
    private LineRenderer cutLineRenderer;  // Para movimientos G1, G2, G3
    private List<Vector3> rapidMovePoints = new List<Vector3>();
    private List<Vector3> cutMovePoints = new List<Vector3>();

    private Stack<GCodeCommand> commandHistory = new Stack<GCodeCommand>();  // Pila de comandos
    private float currentLineWidth;

    void Awake()
    {
        cutLineRenderer = GetComponent<LineRenderer>();
        cutLineRenderer.startWidth = 0.1f;
        cutLineRenderer.endWidth = 0.1f;
        cutLineRenderer.positionCount = 0;

        if (rapidLineRenderer != null)
        {
            rapidLineRenderer.startWidth = 0.1f;
            rapidLineRenderer.endWidth = 0.1f;
            rapidLineRenderer.positionCount = 0;
        }
    }

    public void Initialize(float lineWidth)
    {
        currentLineWidth = lineWidth;
        cutMovePoints.Clear();
        cutLineRenderer.positionCount = 0;

        if (rapidLineRenderer != null)
        {
            rapidMovePoints.Clear();
            rapidLineRenderer.positionCount = 0;
        }
    }

    public void AddGCodeCommand(int gCode, Vector3 point, float radius = 0)
    {
        if (gCode == 0)
        {
            // G0: Movimiento rápido
            rapidMovePoints.Add(point);
            UpdateLineRenderer(rapidLineRenderer, rapidMovePoints);
            commandHistory.Push(new GCodeCommand(gCode, rapidMovePoints, radius)); // Guardar comando
        }
        else if (gCode == 1)
        {
            // G1: Movimiento lineal
            cutMovePoints.Add(point);
            UpdateLineRenderer(cutLineRenderer, cutMovePoints);
            commandHistory.Push(new GCodeCommand(gCode, cutMovePoints, radius)); // Guardar comando
        }
        else if (gCode == 2 || gCode == 3)
        {
            // G2/G3: Movimiento en arco
            AddArc(gCode, cutMovePoints[cutMovePoints.Count - 1], point, radius);
            UpdateLineRenderer(cutLineRenderer, cutMovePoints);
            commandHistory.Push(new GCodeCommand(gCode, cutMovePoints, radius)); // Guardar comando
        }
    }

    private void AddArc(int gCode, Vector3 start, Vector3 end, float radius)
    {
        // Determinar el centro del arco
        Vector3 midPoint = (start + end) / 2;
        Vector3 dir = (end - start).normalized;
        
        Vector3 normal = Vector3.Cross(dir, Vector3.forward).normalized;
        if (gCode == 3) // Anti-horario
            normal = -normal;

        float halfChord = Vector3.Distance(start, midPoint);
        float height = Mathf.Sqrt(Mathf.Abs(radius * radius - halfChord * halfChord));

        Vector3 center = midPoint + normal * height;

        // Calcular ángulo de inicio y fin
        Vector3 startToCenter = start - center;
        Vector3 endToCenter = end - center;

        float startAngle = Mathf.Atan2(startToCenter.y, startToCenter.x);
        float endAngle = Mathf.Atan2(endToCenter.y, endToCenter.x);

        if (gCode == 2 && startAngle < endAngle) startAngle += 2 * Mathf.PI;
        if (gCode == 3 && startAngle > endAngle) endAngle += 2 * Mathf.PI;

        // Generar puntos
        int numSegments = 20;
        for (int i = 0; i <= numSegments; i++)
        {
            float t = i / (float)numSegments;
            float angle = Mathf.Lerp(startAngle, endAngle, t);

            float x = center.x + radius * Mathf.Cos(angle);
            float y = center.y + radius * Mathf.Sin(angle);

            cutMovePoints.Add(new Vector3(x, y, start.z));  // Mantener Z constante
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

    public void UndoLastCommand()
    {
        if (commandHistory.Count > 0)
        {
            GCodeCommand lastCommand = commandHistory.Pop();
            
            // Según el tipo de GCode, eliminar los puntos asociados a ese comando
            if (lastCommand.gCode == 0)
            {
                // Eliminar solo el último punto del movimiento rápido
                if (rapidMovePoints.Count > 0)
                {
                    rapidMovePoints.RemoveAt(rapidMovePoints.Count - 1);
                    UpdateLineRenderer(rapidLineRenderer, rapidMovePoints);  // Actualizar solo el LineRenderer de G0
                }
            }
            else if (lastCommand.gCode == 1)
            {
                // Eliminar solo el último punto del movimiento lineal
                if (cutMovePoints.Count > 0)
                {
                    cutMovePoints.RemoveAt(cutMovePoints.Count - 1);
                    UpdateLineRenderer(cutLineRenderer, cutMovePoints);  // Actualizar solo el LineRenderer de G1
                }
            }
            else if (lastCommand.gCode == 2 || lastCommand.gCode == 3)
            {
                // Eliminar solo los puntos del arco
                int numPointsToRemove = lastCommand.points.Count;
                for (int i = 0; i < numPointsToRemove; i++)
                {
                    cutMovePoints.RemoveAt(cutMovePoints.Count - 1);
                }
                UpdateLineRenderer(cutLineRenderer, cutMovePoints);  // Actualizar solo el LineRenderer de G2/G3
            }
        }
    }

    // Función para habilitar/deshabilitar la visibilidad de los movimientos rápidos
    public void ToggleRapidMoves()
    {
        if (rapidLineRenderer != null)
        {
            rapidLineRenderer.enabled = !rapidLineRenderer.enabled;
        }
    }

    // Limpiar todo
    public void ClearAll()
    {
        rapidMovePoints.Clear();
        cutMovePoints.Clear();

        rapidLineRenderer.positionCount = 0;
        cutLineRenderer.positionCount = 0;
        commandHistory.Clear();  // Limpiar la pila de comandos
    }

    // Clase interna para almacenar los comandos GCode
    private class GCodeCommand
    {
        public int gCode;
        public List<Vector3> points;
        public float radius;

        public GCodeCommand(int gCode, List<Vector3> points, float radius)
        {
            this.gCode = gCode;
            this.points = new List<Vector3>(points);
            this.radius = radius;
        }
    }
}
