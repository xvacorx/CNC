using UnityEngine;
using System.Collections.Generic;

public class GCodeInterpreter : MonoBehaviour
{
    public LineRenderer lineRenderer;
    private List<Vector3> points = new List<Vector3>();
    private Vector3 currentPosition = Vector3.zero;

    void Start()
    {
        lineRenderer.positionCount = 0; // Inicializar con 0 puntos
        string gCode = "G00 X0 Y0\nG01 X10 Y10\nG01 X20 Y5\nG02 X25 Y10 I5 J5"; // Ejemplo de G-code
        ParseGCode(gCode);
    }

    // Función para parsear G-code
    void ParseGCode(string code)
    {
        string[] lines = code.Split('\n');
        foreach (string line in lines)
        {
            string[] commands = line.Split(' ');
            foreach (string command in commands)
            {
                if (command.StartsWith("G"))
                {
                    string gCommand = command.Substring(0, 3); // Obtener el código G
                    float x = currentPosition.x;
                    float y = currentPosition.y;

                    foreach (string param in commands)
                    {
                        if (param.StartsWith("X"))
                            x = float.Parse(param.Substring(1));
                        if (param.StartsWith("Y"))
                            y = float.Parse(param.Substring(1));
                    }

                    // Ejecutar el movimiento basado en el comando
                    if (gCommand == "G00") // Movimiento rápido
                    {
                        MoveTo(new Vector3(x, y, 0));
                    }
                    else if (gCommand == "G01") // Movimiento lineal
                    {
                        MoveTo(new Vector3(x, y, 0));
                    }
                    else if (gCommand == "G02") // Movimiento en arco horario
                    {
                        // Necesitarás una función especial para calcular el arco
                    }
                }
            }
        }
    }

    // Función para mover y dibujar la línea
    void MoveTo(Vector3 newPosition)
    {
        points.Add(newPosition);
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPosition(points.Count - 1, newPosition);
        currentPosition = newPosition;
    }
}
