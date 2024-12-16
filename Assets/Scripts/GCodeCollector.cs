using System.Collections.Generic;
using UnityEngine;

public class GCodeCollector : MonoBehaviour
{
    public Transform tableParent;
    public GCodeInputManager inputs;

    /// <summary>
    /// Devuelve la lista de comandos GCode procesados.
    /// </summary>
    public List<string> CollectGCodeList()
    {
        List<string> gCodeLines = new List<string>();

        foreach (Transform row in tableParent)
        {
            if (row.childCount == 6)
            {
                string g = row.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text;
                string x = row.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text;
                string y = row.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text;
                string z = row.GetChild(3).GetComponent<TMPro.TextMeshProUGUI>().text;
                string r = row.GetChild(4).GetComponent<TMPro.TextMeshProUGUI>().text;
                string f = row.GetChild(5).GetComponent<TMPro.TextMeshProUGUI>().text;

                string gCodeLine = $"{g}";

                if (!string.IsNullOrEmpty(x)) gCodeLine += $" {x}";
                if (!string.IsNullOrEmpty(y)) gCodeLine += $" {y}";
                if (!string.IsNullOrEmpty(z)) gCodeLine += $" {z}";
                if (!string.IsNullOrEmpty(r)) gCodeLine += $" {r}";
                if (!string.IsNullOrEmpty(f)) gCodeLine += $" {f}";

                gCodeLines.Add(gCodeLine);
            }
            else
            {
                Debug.LogWarning($"Row {row.name} no tiene exactamente 6 hijos. Ignorado.");
            }
        }

        return gCodeLines; // Devuelve la lista de GCode procesada
    }

    /// <summary>
    /// Envía el GCode al intérprete para su carga.
    /// </summary>
    public void SendGCodeToInterpreter(GCodeInterpreter interpreter)
    {
        List<string> gCodeLines = CollectGCodeList(); // Genera la lista de GCode
        foreach (var line in gCodeLines)
        {
            Debug.Log($"GCode Line: {line}");
        }
        interpreter.LoadGCodeCommands(gCodeLines); // Llama al método LoadGCodeCommands del intérprete
    }
}
