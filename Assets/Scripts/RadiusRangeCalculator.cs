using UnityEngine;
using TMPro;

public class RadiusRangeCalculator : MonoBehaviour
{
    public TMP_InputField gInput;
    public TMP_InputField xInput;
    public TMP_InputField yInput;
    public TMP_Text rRangeText; // Texto para mostrar el rango

    void Update()
    {
        UpdateRadiusRange();
    }

    private void UpdateRadiusRange()
    {
        // Validar si los inputs tienen valores válidos
        if (!int.TryParse(gInput.text, out int gValue) || 
            !float.TryParse(xInput.text, out float xValue) || 
            !float.TryParse(yInput.text, out float yValue))
        {
            rRangeText.text = "Rango no disponible";
            return;
        }

        // Calcular el rango para G02 o G03 (arcos)
        if (gValue == 2 || gValue == 3)
        {
            float minR = Vector2.Distance(Vector2.zero, new Vector2(xValue, yValue)) / 2;
            float maxR = minR * 2; // Ejemplo: establecer un máximo arbitrario

            rRangeText.text = $"Rango R: {minR:F2} - {maxR:F2}";
        }
        else
        {
            rRangeText.text = "R no requerido para este G";
        }
    }
}
