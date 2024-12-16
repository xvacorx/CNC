using UnityEngine;
using TMPro;
using System.Xml.Schema;
using System.Collections;

public class RadiusRangeCalculator : MonoBehaviour
{
    public TMP_InputField gInput;
    public TMP_InputField xInput;
    public TMP_InputField yInput;
    public TMP_Text rRangeText;
    [HideInInspector] public float minR;
    [HideInInspector] public float maxR;
    private Vector2 lastPoint;
    private void Start()
    {
        lastPoint = Vector2.zero;
    }
    void Update()
    {
        UpdateRadiusRange();
    }

    private void UpdateRadiusRange()
    {
        if (!int.TryParse(gInput.text, out int gValue) ||
            !float.TryParse(xInput.text, out float xValue) ||
            !float.TryParse(yInput.text, out float yValue))
        {
            rRangeText.text = "Rango no disponible";
            return;
        }

        if (gValue == 2 || gValue == 3)
        {
            minR = Vector2.Distance(lastPoint, new Vector2(xValue, yValue)) / 2;
            maxR = minR * 2;

            rRangeText.text = $"Rango R: {minR:F2} - {maxR:F2}";
        }
        else
        {
            rRangeText.text = "R no requerido para este G";
        }
    }
    public void UpdateLastPoint(Vector2 point)
    {
        lastPoint = point;
    }
}