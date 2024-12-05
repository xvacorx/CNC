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
    public TMP_InputField lineWidthInput;
    public GameObject rowPrefab;
    public Transform content;

    public GCodeInterpreter trajectoryMesh;

    private TMP_InputField[] inputFields;
    private int currentFieldIndex = 0;

    public FirebaseDataManager firebaseDataManager;

    void Start()
    {
        inputFields = new TMP_InputField[] { gInput, xInput, yInput, zInput, rInput, fInput };
        inputFields[0].Select();

        if (float.TryParse(lineWidthInput.text, out float initialWidth))
        {
            trajectoryMesh.Initialize(initialWidth);
        }
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
        float rValue = 0;
        float fValue = 0;
        if (!float.TryParse(xInput.text, out float xValue) ||
            !float.TryParse(yInput.text, out float yValue) ||
            !float.TryParse(zInput.text, out float zValue) ||
            (!string.IsNullOrEmpty(rInput.text) && !float.TryParse(rInput.text, out rValue)) ||
            (!string.IsNullOrEmpty(fInput.text) && !float.TryParse(fInput.text, out fValue)))
        {
            Debug.LogError("Uno o más valores no son válidos.");
            return;
        }

        string gValue = gInput.text;
        if (string.IsNullOrEmpty(gValue) || !int.TryParse(gValue, out int gCode))
        {
            Debug.LogError("El comando G es obligatorio y debe ser un número válido.");
            return;
        }

        GameObject newRow = Instantiate(rowPrefab, content);
        TMP_Text[] rowFields = newRow.GetComponentsInChildren<TMP_Text>();

        rowFields[0].text = $"G{gCode}";
        rowFields[1].text = $"X{xValue:F2}";
        rowFields[2].text = $"Y{yValue:F2}";
        rowFields[3].text = $"Z{zValue:F2}";
        rowFields[4].text = (gCode == 2 || gCode == 3) && !string.IsNullOrEmpty(rInput.text) ? $"R{rValue:F2}" : "";
        rowFields[5].text = !string.IsNullOrEmpty(fInput.text) ? $"F{fValue:F2}" : "";

        trajectoryMesh.AddGCodeCommand(gCode, new Vector3(xValue, yValue, zValue), rValue);

        firebaseDataManager.SaveDataToFirebase();

        ClearInputs();
    }

    private void ClearInputs()
    {
        foreach (var field in inputFields)
        {
            field.text = "";
        }

        inputFields[0].Select();
    }

    public void RemoveLastRow()
    {
        if (content.childCount > 0)
        {
            Transform lastRow = content.GetChild(content.childCount - 1);
            Destroy(lastRow.gameObject);
            trajectoryMesh.UndoLastMovement();
        }
        else
        {
            Debug.LogWarning("No hay filas para eliminar.");
        }
    }
}
