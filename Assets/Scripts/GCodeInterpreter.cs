using System.Collections.Generic;
using UnityEngine;

public class GCodeInterpreter : MonoBehaviour
{
    public GameObject drawingObject;  // El objeto que se usa para el dibujo, como un LineRenderer.
    public RadiusRangeCalculator rangeCalculator;
    private List<Vector3> currentPoints = new List<Vector3>();  // Lista de puntos actuales.
    private Vector3 lastPoint;  // Último punto conocido (necesario para G2/G3).

    public void AddGCodeCommand(Vector3 point)
    {
        // Agregar el punto a la lista de puntos
        currentPoints.Add(point);

        // Si estás utilizando un LineRenderer, actualizamos el contador de puntos
        LineRenderer lineRenderer = drawingObject.GetComponent<LineRenderer>();
        lineRenderer.positionCount = currentPoints.Count;
        lineRenderer.SetPositions(currentPoints.ToArray());
    }

    public void LoadGCodeCommands(List<string> gCodeLines)
    {
        // Limpiar cualquier dibujo previo antes de empezar a redibujar.
        currentPoints.Clear();  // Limpiar la lista de puntos.

        // Si usas un LineRenderer, reseteamos el contador de posiciones.
        LineRenderer lineRenderer = drawingObject.GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;  // Eliminar líneas previas.

        // Iterar por cada línea de GCode y procesarla.
        foreach (var line in gCodeLines)
        {
            ProcessGCodeLine(line);
        }
    }

    private void ProcessGCodeLine(string line)
    {
        if (line.StartsWith("G1")) // Línea recta
        {
            Vector3 point = ParsePoint(line);
            AddGCodeCommand(point);
            rangeCalculator.UpdateLastPoint(point);
            lastPoint = point;
        }
        else if (line.StartsWith("G2") || line.StartsWith("G3")) // Arco
        {
            Vector3 point = ParsePoint(line);  // Obtener el punto de destino (final)
            Vector3 arcCenter = CalculateArcCenter(lastPoint, point, line);  // Calcular centro del arco
            float radius = Vector3.Distance(lastPoint, arcCenter);  // Radio calculado

            float startAngle = Mathf.Atan2(lastPoint.y - arcCenter.y, lastPoint.x - arcCenter.x);
            float endAngle = Mathf.Atan2(point.y - arcCenter.y, point.x - arcCenter.x);

            if (line.StartsWith("G3"))  // Arco en sentido antihorario
            {
                endAngle = startAngle - (endAngle - startAngle);  // Revertir la dirección
            }

            DrawArc(arcCenter, radius, startAngle, endAngle);  // Dibujar el arco
            rangeCalculator.UpdateLastPoint(point);
            lastPoint = point;
        }
    }

    private Vector3 ParsePoint(string line)
    {
        // Este método debe parsear las coordenadas X, Y, Z de la línea de GCode.
        float x = 0, y = 0, z = 0;

        // Buscar las coordenadas X, Y, Z en la línea
        string[] parts = line.Split(' ');

        foreach (var part in parts)
        {
            if (part.StartsWith("X"))
            {
                x = float.Parse(part.Substring(1));
            }
            else if (part.StartsWith("Y"))
            {
                y = float.Parse(part.Substring(1));
            }
            else if (part.StartsWith("Z"))
            {
                z = float.Parse(part.Substring(1));
            }
        }

        return new Vector3(x, y, z);
    }

    private Vector3 CalculateArcCenter(Vector3 startPoint, Vector3 endPoint, string line)
    {
        // El centro del arco (I, J, K) se calcula a partir de la posición de los puntos de inicio y fin
        float r = 0;

        // Buscar R en la línea (especificado en GCode).
        string[] parts = line.Split(' ');
        foreach (var part in parts)
        {
            if (part.StartsWith("R"))
            {
                r = float.Parse(part.Substring(1));
            }
        }

        if (r == 0)
        {
            Debug.LogError("Radio no especificado para el arco.");
            return Vector3.zero;
        }

        // Cálculo del centro del arco (asumiendo un arco circular en el plano XY)
        Vector3 direction = (endPoint - startPoint).normalized;
        float d = Vector3.Distance(startPoint, endPoint) / 2;
        float h = Mathf.Sqrt(r * r - d * d);  // Altura del triángulo formado

        // Determinar la dirección perpendicular al vector de la línea
        Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0);  // Perpendicular en 2D (XY)

        // El centro del arco es el punto medio de la línea con el desplazamiento hacia la perpendicular
        Vector3 center = (startPoint + endPoint) / 2 + perpendicular * h;

        return center;
    }

    private void DrawArc(Vector3 center, float radius, float startAngle, float endAngle)
    {
        // Dibujar el arco utilizando segmentos de línea.
        int segmentCount = 100;
        for (int i = 0; i <= segmentCount; i++)
        {
            float angle = Mathf.Lerp(startAngle, endAngle, i / (float)segmentCount);
            Vector3 point = new Vector3(center.x + Mathf.Cos(angle) * radius, center.y + Mathf.Sin(angle) * radius, 0);
            rangeCalculator.UpdateLastPoint(point);
            AddGCodeCommand(point);
        }
    }
}
