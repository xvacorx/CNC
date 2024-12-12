using System.Collections.Generic;
using UnityEngine;

public class GCodeInterpreter : MonoBehaviour
{
    public GameObject drawingObject;  // El objeto que se usa para el dibujo, como un LineRenderer.
    private List<Vector3> currentPoints = new List<Vector3>();  // Lista de puntos actuales.

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
            // Aquí parseamos las coordenadas del GCode (suponiendo que ya están en formato X Y Z).
            Vector3 point = ParsePoint(line);  // Este método debe extraer las coordenadas del GCode.

            // Llamar a AddGCodeCommand para agregar el punto al dibujo.
            AddGCodeCommand(point);
        }
    }

    private Vector3 ParsePoint(string line)
    {
        // Este método debe parsear las coordenadas X, Y, Z de la línea de GCode.
        // Suponemos que la línea contiene 'X', 'Y', 'Z' en algún formato.

        float x = 0, y = 0, z = 0;

        // Buscar las coordenadas X, Y, Z en la línea (puedes ajustarlo según el formato exacto del GCode).
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

        return new Vector3(x, y, z);  // Devolver las coordenadas como un Vector3.
    }
}
