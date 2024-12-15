using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GCodeInputManager : MonoBehaviour
{
    public TMP_InputField gInput;
    public TMP_InputField xInput;
    public TMP_InputField yInput;
    public TMP_InputField zInput;
    public TMP_InputField rInput;
    public TMP_InputField fInput;
    public GameObject rowPrefab;
    public Transform content;
    public TMP_Text warningText;
    public GCodeInterpreter trajectoryMesh;

    private TMP_InputField[] inputFields;
    private int currentFieldIndex = 0;
    public RadiusRangeCalculator rangeCalculator;
    public FirebaseDataManager firebaseDataManager;
    public GCodeCollector codeCollector;

    void Start()
    {
        inputFields = new TMP_InputField[] { gInput, xInput, yInput, zInput, rInput, fInput };
        inputFields[0].Select();
    }

    void Update()
    {
        HandleTabNavigation();
        HandleSubmit();
    }

    private void HandleTabNavigation()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            currentFieldIndex = (currentFieldIndex + 1) % inputFields.Length;
            inputFields[currentFieldIndex].Select();
        }
    }

    private void HandleSubmit()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            AddGCodeRow();
        }
    }

    public void AddGCodeRow()
    {
        string invalidField = ValidateInputs(out int gCode, out float xValue, out float yValue, out float zValue, out float rValue, out float fValue);
        if (invalidField != null)
        {
            Debug.LogError($"El campo {invalidField} no es válido. Revisa los datos de entrada.");
            warningText.text = ($"El campo {invalidField} no es válido. Revisa los datos de entrada.");
            return;
        }

        GameObject newRow = Instantiate(rowPrefab, content);
        TMP_Text[] rowFields = newRow.GetComponentsInChildren<TMP_Text>();

        rowFields[0].text = $"G{gCode}";
        rowFields[1].text = gCode == 0 || gCode == 1 || gCode == 2 || gCode == 3 ? $"{xValue:F0}" : "";
        rowFields[2].text = gCode == 0 || gCode == 1 || gCode == 2 || gCode == 3 ? $"{yValue:F0}" : "";
        rowFields[3].text = gCode == 0 || gCode == 1 || gCode == 2 || gCode == 3 ? $"{zValue:F0}" : "";
        rowFields[4].text = gCode == 2 || gCode == 3 ? $"{rValue:F2}" : "";
        rowFields[5].text = gCode == 1 || gCode == 2 || gCode == 3 ? $"{fValue:F0}" : "";

        codeCollector.SendGCodeToInterpreter(trajectoryMesh);
        ClearInputs();
    }

    private string ValidateInputs(out int gCode, out float xValue, out float yValue, out float zValue, out float rValue, out float fValue)
    {
        gCode = 0;
        xValue = yValue = zValue = rValue = fValue = 0;

        // Validar G
        if (string.IsNullOrEmpty(gInput.text) || !int.TryParse(gInput.text, out gCode) || (gCode != 0 && gCode != 1 && gCode != 2 && gCode != 3))
        {
            warningText.text = "El comando G debe ser 0, 1, 2 o 3.";
            return "G";
        }

        // Validar X, Y, Z (obligatorios para todos los comandos G0, G1, G2, G3)
        if (!float.TryParse(xInput.text, out xValue))
        {
            warningText.text = "El valor de X es obligatorio y debe ser un número válido.";
            return "X";
        }
        if (!float.TryParse(yInput.text, out yValue))
        {
            warningText.text = "El valor de Y es obligatorio y debe ser un número válido.";
            return "Y";
        }
        if (!float.TryParse(zInput.text, out zValue))
        {
            warningText.text = "El valor de Z es obligatorio y debe ser un número válido.";
            return "Z";
        }

        // Validar R y F según el comando G
        if (gCode == 2 || gCode == 3)
        {
            // Validar R
            if (string.IsNullOrEmpty(rInput.text) || !float.TryParse(rInput.text, out rValue))
            {
                warningText.text = "El valor de R es obligatorio para G2/G3 y debe ser un número válido.";
                return "R";
            }

            // Validar que R esté dentro del rango definido por RadiusRangeCalculator
            if (rValue < rangeCalculator.minR || rValue > rangeCalculator.maxR)
            {
                warningText.text = $"El valor de R debe estar entre {rangeCalculator.minR} y {rangeCalculator.maxR}.";
                return "R";
            }

            // Validar F
            if (string.IsNullOrEmpty(fInput.text) || !float.TryParse(fInput.text, out fValue))
            {
                warningText.text = "El valor de F es obligatorio para G2/G3 y debe ser un número válido.";
                return "F";
            }
        }
        else if (gCode == 1)
        {
            // Validar F para G1
            if (string.IsNullOrEmpty(fInput.text) || !float.TryParse(fInput.text, out fValue))
            {
                warningText.text = "El valor de F es obligatorio para G1 y debe ser un número válido.";
                return "F";
            }
        }
        else
        {
            // Ignorar R y F para G0
            if (!string.IsNullOrEmpty(fInput.text) || !string.IsNullOrEmpty(rInput.text))
            {
                warningText.text = "Los valores de R y F se desestiman para G0.";
            }
        }

        // Todos los valores son válidos
        warningText.text = "";
        return null;
    }

    private void ClearInputs()
    {
        foreach (var field in inputFields)
        {
            field.text = "";
        }
        warningText.text = "";
        inputFields[0].Select();
    }

    public void RemoveLastRow()
    {
        if (content.childCount > 0)
        {
            Transform lastRow = content.GetChild(content.childCount - 1);
            Destroy(lastRow.gameObject);
            codeCollector.SendGCodeToInterpreter(trajectoryMesh);
        }
        else
        {
            Debug.LogWarning("No hay filas para eliminar.");
            warningText.text = ("No hay filas para eliminar.");
        }
    }
}
