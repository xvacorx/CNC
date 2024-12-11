using System.Collections.Generic;
using UnityEngine;

public class GCodeCollector : MonoBehaviour
{
    public Transform tableParent;

    public string CollectGCode()
    {
        List<string> gCodeLines = new List<string>();

        foreach (Transform row in tableParent)
        {
            if (row.childCount == 6)
            {
                string g = row.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text;
                string x = RoundToInteger(row.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text);
                string y = RoundToInteger(row.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text);
                string z = RoundToInteger(row.GetChild(3).GetComponent<TMPro.TextMeshProUGUI>().text);
                string r = RoundToInteger(row.GetChild(4).GetComponent<TMPro.TextMeshProUGUI>().text);
                string f = RoundToInteger(row.GetChild(5).GetComponent<TMPro.TextMeshProUGUI>().text);

                string gCodeLine = $"{g} X{x} Y{y} Z{z} R{r} F{f}";
                gCodeLines.Add(gCodeLine);
            }
            else
            {
                Debug.LogWarning($"Row {row.name} no tiene exactamente 6 hijos. Ignorado.");
            }
        }

        return string.Join("\\n", gCodeLines);
    }

    private string RoundToInteger(string value)
    {
        if (int.TryParse(value, out int numericValue))
        {
            return numericValue.ToString();
        }

        return value;
    }

    public void PrintGCode()
    {
        string gCodeText = CollectGCode();
        Debug.Log(gCodeText);
    }
}