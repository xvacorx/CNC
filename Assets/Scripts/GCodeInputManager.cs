using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GCodeInputManager : MonoBehaviour
{
    // Referencias a los input fields y otros objetos
    public TMP_InputField gInput;
    public TMP_InputField xInput;
    public TMP_InputField yInput;
    public TMP_InputField zInput;
    public TMP_InputField rInput;  // Campo de radio
    public TMP_InputField fInput;
    public GameObject rowPrefab;
    public Transform content;

    // Referencia al GCodeInterpreter
    public GCodeInterpreter gCodeInterpreter;

    private TMP_InputField[] inputFields;  // Array para los campos de entrada
    private int currentFieldIndex = 0; // Para gestionar la navegación con TAB

    void Start()
    {
        // Inicializar el array de InputFields y seleccionar el primer campo
        inputFields = new TMP_InputField[] { gInput, xInput, yInput, zInput, rInput, fInput };
        inputFields[0].Select(); // Seleccionamos el primer campo al inicio
    }

    void Update()
    {
        HandleTabNavigation(); // Navegación entre campos con TAB
        HandleSubmit(); // Enviar datos con ENTER
    }

    private void HandleTabNavigation()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) // Al presionar TAB
        {
            currentFieldIndex = (currentFieldIndex + 1) % inputFields.Length; // Navegación cíclica
            inputFields[currentFieldIndex].Select();
        }
    }

    private void HandleSubmit()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) // Al presionar ENTER
        {
            AddGCodeRow();
        }
    }

public void AddGCodeRow()
{
    // Declarar rValue fuera del bloque
    float rValue = 0f;
    float fValue = 0f;
    // Captura de valores de los input fields como float
    if (!float.TryParse(xInput.text, out float xValue) || 
        !float.TryParse(yInput.text, out float yValue) || 
        !float.TryParse(zInput.text, out float zValue) || 
        (fInput.text != "" && !float.TryParse(fInput.text, out fValue)) ||  // Validar solo si F no está vacío
        (rInput.text != "" && !float.TryParse(rInput.text, out rValue)))  // Asignar rValue solo si rInput no está vacío
    {
        Debug.LogError("Uno o más valores no son válidos.");
        return;
    }

    string gValue = gInput.text;
    if (string.IsNullOrEmpty(gValue))
    {
        Debug.LogError("El comando G es obligatorio.");
        return;
    }

    int gCode = int.Parse(gValue);

    // Validaciones según el comando G
    if (gCode == 0 && (string.IsNullOrEmpty(xInput.text) || string.IsNullOrEmpty(yInput.text) || string.IsNullOrEmpty(zInput.text)))
    {
        Debug.LogError("G00 requiere X, Y y Z.");
        return;
    }
    else if ((gCode == 1 || gCode == 2 || gCode == 3) && 
             (string.IsNullOrEmpty(xInput.text) || string.IsNullOrEmpty(yInput.text) || string.IsNullOrEmpty(zInput.text) || string.IsNullOrEmpty(fInput.text)))
    {
        Debug.LogError($"G{gCode} requiere X, Y, Z y F.");
        return;
    }

    if ((gCode == 2 || gCode == 3) && string.IsNullOrEmpty(rInput.text))
    {
        Debug.LogError($"G{gCode} requiere un valor para R (radio).");
        return;
    }

    // Crear e instanciar la fila en la tabla usando los valores como float
    GameObject newRow = Instantiate(rowPrefab, content);
    TMP_Text[] rowFields = newRow.GetComponentsInChildren<TMP_Text>();

    rowFields[0].text = $"G{gCode}";
    rowFields[1].text = $"X{xValue:F0}";  // Formatear para mostrar sin decimales
    rowFields[2].text = $"Y{yValue:F0}";
    rowFields[3].text = $"Z{zValue:F0}";

    // Asignar R solo si el GCode es 2 o 3 y rInput no está vacío
    string rText = (gCode == 2 || gCode == 3) && !string.IsNullOrEmpty(rInput.text) ? $"R{rValue:F0}" : "";
    rowFields[4].text = rText;

    rowFields[5].text = fInput.text != "" ? $"F{fValue:F0}" : "";  // Mostrar F solo si se ingresó

    // Llamada al GCodeInterpreter para procesar la nueva fila con valores como float
    gCodeInterpreter.AddNewGCodeRow(gCode, xValue, yValue, rValue, fValue);

    ClearInputs(); // Limpiar campos después de agregar la fila
}


    private void ClearInputs()
    {
        foreach (var field in inputFields)
        {
            field.text = ""; // Limpiar cada campo de entrada
        }

        inputFields[0].Select(); // Regresar al primer campo después de enviar
    }
}