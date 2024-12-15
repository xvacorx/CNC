using System.Collections.Generic;
using UnityEngine;

public class GCodeInterpreter : MonoBehaviour
{
    public GameObject lineRendererPrefab;  // Prefab del GameObject con un LineRenderer.
    public Material cuttingMaterial;      // Material para cortes (G1, G2, G3).
    public Material rapidMaterial;        // Material para movimientos rápidos (G0).

    private GameObject currentLineObject; // LineRenderer actual para el segmento.
    private LineRenderer currentLineRenderer; // LineRenderer activo.
    private List<Vector3> currentPoints = new List<Vector3>(); // Puntos del segmento actual.

    private bool isRapidMove = false; // Indica si el movimiento actual es G0.
    private Vector3 lastPoint = Vector3.zero; // Último punto procesado.

    public void AddGCodeCommand(Vector3 point, bool isRapid)
    {
        // Si el tipo de movimiento cambia (de G0 a G1/G2/G3 o viceversa), iniciamos un nuevo segmento.
        if (isRapid != isRapidMove)
        {
            FinalizeCurrentSegment();
            StartNewSegment(isRapid);
        }

        // Añadimos el punto al segmento actual.
        currentPoints.Add(point);
        currentLineRenderer.positionCount = currentPoints.Count;
        currentLineRenderer.SetPositions(currentPoints.ToArray());

        lastPoint = point;
    }

    private void StartNewSegment(bool isRapid)
    {
        // Crear un nuevo GameObject para el segmento.
        currentLineObject = Instantiate(lineRendererPrefab, transform);
        currentLineRenderer = currentLineObject.GetComponent<LineRenderer>();

        // Configurar el material según el tipo de movimiento.
        currentLineRenderer.material = isRapid ? rapidMaterial : cuttingMaterial;

        // Configurar visibilidad de las líneas.
        currentLineRenderer.startWidth = isRapid ? 0.05f : 0.1f; // Ancho distinto para G0 y G1/G2/G3.
        currentLineRenderer.endWidth = isRapid ? 0.05f : 0.1f;

        currentPoints.Clear(); // Limpiar los puntos del nuevo segmento.
        isRapidMove = isRapid; // Actualizar el estado actual.
    }

    private void FinalizeCurrentSegment()
    {
        if (currentLineRenderer != null)
        {
            // Finalizar el segmento actual: puedes añadir lógica adicional aquí si es necesario.
            currentLineRenderer.positionCount = currentPoints.Count;
            currentLineRenderer.SetPositions(currentPoints.ToArray());
        }
    }

    public void LoadGCodeCommands(List<string> gCodeLines)
    {
        // Limpiar cualquier segmento previo.
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Reiniciar el estado.
        currentLineObject = null;
        currentLineRenderer = null;
        currentPoints.Clear();
        isRapidMove = false;
        lastPoint = Vector3.zero;

        // Procesar las líneas de GCode.
        foreach (var line in gCodeLines)
        {
            ProcessGCodeLine(line);
        }

        // Finalizar el último segmento.
        FinalizeCurrentSegment();
    }

    private void ProcessGCodeLine(string line)
    {
        if (line.StartsWith("G0")) // Movimiento rápido
        {
            Vector3 point = ParsePoint(line);
            AddGCodeCommand(point, true);
        }
        else if (line.StartsWith("G1") || line.StartsWith("G2") || line.StartsWith("G3")) // Movimiento de corte
        {
            Vector3 point = ParsePoint(line);
            AddGCodeCommand(point, false);
        }
    }

    private Vector3 ParsePoint(string line)
    {
        // Coordenadas X, Y, Z de la línea de GCode.
        float x = lastPoint.x, y = lastPoint.y, z = lastPoint.z;

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
}
