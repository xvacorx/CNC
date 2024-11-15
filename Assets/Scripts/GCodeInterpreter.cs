using UnityEngine;
using System.Collections.Generic;

public class GCodeInterpreter : MonoBehaviour
{
    public LineRenderer lineRenderer;
    private List<Vector3> points = new List<Vector3>();
    private Vector3 currentPosition = Vector3.zero;

    // Lista de filas de la tabla
    public List<GCodeRow> gCodeRows = new List<GCodeRow>();

    void Start()
    {
        lineRenderer.positionCount = 0;
    }

    // Método que se llama cuando se agrega una fila nueva
    public void AddNewGCodeRow(int gCode, float x, float y, float r, float f)
    {
        // Crear nueva fila y agregarla a la lista
        GCodeRow newRow = new GCodeRow(gCode, x, y, r, f);
        gCodeRows.Add(newRow);

        // Procesar solo esta fila y dibujar la línea correspondiente
        ProcessGCode(newRow);
    }

    void ProcessGCode(GCodeRow row)
    {
        int gCode = row.gCode;
        float x = row.x;
        float y = row.y;
        float r = row.r;
        float f = row.f;

        if (gCode == 0 || gCode == 1) // Movimiento rápido o lineal
        {
            MoveTo(new Vector3(x, y, 0));
        }
        else if (gCode == 2 || gCode == 3) // Arco horario o antihorario
        {
            bool clockwise = gCode == 2;
            DrawArc(new Vector3(x, y, 0), r, clockwise);
        }
    }

    void MoveTo(Vector3 newPosition)
    {
        points.Add(newPosition);
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPosition(points.Count - 1, newPosition);
        currentPosition = newPosition;
    }

    void DrawArc(Vector3 targetPosition, float radius, bool clockwise)
    {
        Vector3 center = CalculateArcCenter(currentPosition, targetPosition, radius, clockwise);

        float startAngle = Mathf.Atan2(currentPosition.y - center.y, currentPosition.x - center.x);
        float endAngle = Mathf.Atan2(targetPosition.y - center.y, targetPosition.x - center.x);

        float angleStep = clockwise ? -1 : 1;
        float angle = startAngle;

        while (Mathf.Abs(Mathf.DeltaAngle(Mathf.Rad2Deg * angle, Mathf.Rad2Deg * endAngle)) > 0.1f)
        {
            angle += angleStep * Mathf.Deg2Rad;
            Vector3 arcPoint = new Vector3(
                center.x + radius * Mathf.Cos(angle),
                center.y + radius * Mathf.Sin(angle),
                0
            );
            MoveTo(arcPoint);
        }

        MoveTo(targetPosition);
    }

    Vector3 CalculateArcCenter(Vector3 start, Vector3 end, float radius, bool clockwise)
    {
        Vector3 midpoint = (start + end) / 2;
        Vector3 dir = (end - start).normalized;
        float dist = Vector3.Distance(start, end) / 2;

        float height = Mathf.Sqrt(Mathf.Abs(radius * radius - dist * dist));
        Vector3 perpendicular = clockwise ? new Vector3(-dir.y, dir.x) : new Vector3(dir.y, -dir.x);

        return midpoint + perpendicular * height;
    }
}

[System.Serializable]
public class GCodeRow
{
    public int gCode;
    public float x;
    public float y;
    public float r;
    public float f;

    // Constructor para crear una fila de GCode
    public GCodeRow(int gCode, float x, float y, float r, float f)
    {
        this.gCode = gCode;
        this.x = x;
        this.y = y;
        this.r = r;
        this.f = f;
    }
}
