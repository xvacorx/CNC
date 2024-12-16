using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

public class FirebaseDataManager : MonoBehaviour
{
    public TMP_InputField projectNameInput;
    public TMP_Text statusText;
    public Transform tableContent;
    public GameObject rowPrefab;

    private DatabaseReference databaseReference;
    public GCodeCollector collector;
    public GCodeInterpreter interpreter;

    void Start()
    {
        // Inicializa Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
                statusText.text = "Firebase inicializado correctamente.";
            }
            else
            {
                statusText.text = "Error al inicializar Firebase: " + task.Result;
            }
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

        string gCodeData = CollectGCodeList();
        if (string.IsNullOrEmpty(gCodeData))
        {
            statusText.text = "No hay datos de GCode para subir.";
            return;
        }

        // Guarda en Firebase un objeto JSON
        var projectData = new Dictionary<string, object>();
        projectData["gcode"] = gCodeData;

        // Sube los datos a Firebase
        databaseReference.Child("GCodeProjects").Child(projectName).SetValueAsync(projectData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                statusText.text = "Proyecto subido exitosamente.";
            }
            else
            {
                statusText.text = "Error al subir el proyecto.";
                Debug.LogError("Error al subir el GCode: " + task.Exception);
            }
        });
    }

    public void DownloadGCode()
    {
        string projectName = projectNameInput.text;
        if (string.IsNullOrEmpty(projectName))
        {
            statusText.text = "El nombre del proyecto no puede estar vacío.";
            return;
        }

        // Descarga el proyecto de Firebase
        databaseReference.Child("GCodeProjects").Child(projectName).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                if (task.Result.Exists)
                {
                    string gCodeData = task.Result.Child("gcode").Value.ToString();
                    if (string.IsNullOrEmpty(gCodeData))
                    {
                        statusText.text = $"El proyecto '{projectName}' no tiene datos de GCode.";
                        return;
                    }

                    LoadGCodeToTable(gCodeData);
                    statusText.text = $"El proyecto '{projectName}' se cargó correctamente.";
                }
                else
                {
                    statusText.text = $"El proyecto '{projectName}' no existe.";
                }
            }
            else
            {
                statusText.text = "Error al descargar el proyecto.";
                Debug.LogError("Error al descargar el GCode: " + task.Exception);
            }
        });
    }

    private string CollectGCodeList()
    {
        List<string> gCodeLines = new List<string>();

        foreach (Transform child in tableContent)
        {
            TMP_Text[] rowFields = child.GetComponentsInChildren<TMP_Text>();

            string gLine = "";
            foreach (TMP_Text field in rowFields)
            {
                if (!string.IsNullOrEmpty(field.text))
                {
                    gLine += field.text + " ";
                }
            }

            if (!string.IsNullOrWhiteSpace(gLine))
            {
                gCodeLines.Add(gLine.Trim());
            }
        }

        return string.Join("\\n", gCodeLines);
    }

    private void LoadGCodeToTable(string gCodeData)
    {
        // Limpia la tabla
        foreach (Transform child in tableContent)
        {
            Destroy(child.gameObject);
        }

        string[] gCodeLines = gCodeData.Split(new[] { "\\n" }, System.StringSplitOptions.None);

        foreach (string line in gCodeLines)
        {
            GameObject newRow = Instantiate(rowPrefab, tableContent);
            TMP_Text[] rowFields = newRow.GetComponentsInChildren<TMP_Text>();

            for (int i = 0; i < rowFields.Length; i++)
            {
                rowFields[i].text = string.Empty;
            }

            // Divide los valores GCode por espacio (e.g., "G1 X10 Y20 Z30")
            string[] commands = line.Split(' ');
            foreach (string command in commands)
            {
                if (string.IsNullOrWhiteSpace(command))
                    continue;

                if (command.StartsWith("G"))
                    rowFields[0].text = "G" + command.Substring(1); // G
                else if (command.StartsWith("X"))
                    rowFields[1].text = "X" + command.Substring(1); // X
                else if (command.StartsWith("Y"))
                    rowFields[2].text = "Y" + command.Substring(1); // Y
                else if (command.StartsWith("Z"))
                    rowFields[3].text = "Z" + command.Substring(1); // Z
                else if (command.StartsWith("R"))
                    rowFields[4].text = "R" + command.Substring(1); // R
                else if (command.StartsWith("F"))
                    rowFields[5].text = "F" + command.Substring(1); // F
            }
        }
        collector.SendGCodeToInterpreter(interpreter);
    }
}