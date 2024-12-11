using System.Collections.Generic;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using TMPro;

public class FirebaseDataManager : MonoBehaviour
{
    private DatabaseReference databaseReference;

    public TMP_InputField projectNameInput; // Input para el nombre del proyecto
    public TMP_Text statusText; // TMP para mostrar los mensajes al usuario
    public Transform tableContent; // Contenedor que tiene las filas (Rows) de la tabla

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            FirebaseApp firebaseApp = FirebaseApp.DefaultInstance;
            databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
            Debug.Log("Firebase inicializado correctamente.");
        });
    }

    public void UploadGCode()
    {
        string projectName = projectNameInput.text;

        if (string.IsNullOrEmpty(projectName))
        {
            statusText.text = "El nombre del proyecto no puede estar vacío.";
            return;
        }

        // Verificar si ya existe un proyecto con el mismo nombre
        databaseReference.Child("GCodeProjects").Child(projectName).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                if (task.Result.Exists)
                {
                    statusText.text = $"El proyecto '{projectName}' ya existe.";
                }
                else
                {
                    // Recolectar el GCode de la tabla
                    string gCodeData = GenerateGCode();
                    if (string.IsNullOrEmpty(gCodeData))
                    {
                        statusText.text = "No hay datos en la tabla para subir.";
                        return;
                    }

                    // Subir el GCode a Firebase
                    Dictionary<string, object> data = new Dictionary<string, object>
                    {
                        { "gcode", gCodeData }
                    };

                    databaseReference.Child("GCodeProjects").Child(projectName).SetValueAsync(data).ContinueWithOnMainThread(uploadTask =>
                    {
                        if (uploadTask.IsCompleted)
                        {
                            statusText.text = $"El proyecto '{projectName}' se subió correctamente.";
                        }
                        else
                        {
                            statusText.text = "Error al subir el proyecto.";
                            Debug.LogError("Error al subir el GCode: " + uploadTask.Exception);
                        }
                    });
                }
            }
            else
            {
                statusText.text = "Error al verificar la existencia del proyecto.";
                Debug.LogError("Error al verificar el proyecto: " + task.Exception);
            }
        });
    }

    private string GenerateGCode()
    {
        if (tableContent.childCount == 0)
        {
            return null;
        }

        string gCodeData = "";
        foreach (Transform row in tableContent)
        {
            TMP_Text[] rowFields = row.GetComponentsInChildren<TMP_Text>();
            string line = "";

            foreach (TMP_Text field in rowFields)
            {
                if (!string.IsNullOrEmpty(field.text))
                {
                    line += ProcessField(field.text) + " ";
                }
            }

            gCodeData += line.TrimEnd() + "\\n"; // Agregar salto de línea
        }

        return gCodeData.TrimEnd(); // Eliminar el último salto de línea
    }

    private string ProcessField(string fieldText)
    {
        // Extraer solo la parte numérica y redondear a un entero
        if (float.TryParse(fieldText.Substring(1), out float numericValue)) // Ignorar la primera letra (G, X, Y, Z, etc.)
        {
            return fieldText[0] + Mathf.RoundToInt(numericValue).ToString(); // Concatenar la letra con el número redondeado
        }

        return fieldText; // Si no es un número, devolver el texto original
    }
}
