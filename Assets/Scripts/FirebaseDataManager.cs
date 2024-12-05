using System.Collections.Generic;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using TMPro;

public class FirebaseDataManager : MonoBehaviour
{
    private DatabaseReference databaseReference;

    public TMP_InputField gInput;
    public TMP_InputField xInput;
    public TMP_InputField yInput;
    public TMP_InputField zInput;
    public TMP_InputField rInput;
    public TMP_InputField fInput;

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            FirebaseApp firebaseApp = FirebaseApp.DefaultInstance;
            databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
            Debug.Log("Firebase inicializado correctamente.");
        });
    }

    public void SaveDataToFirebase()
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

        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "G", gValue },
            { "X", xValue },
            { "Y", yValue },
            { "Z", zValue },
            { "R", rValue },
            { "F", fValue }
        };

        string entryId = databaseReference.Child("GCodeEntries").Push().Key;

        databaseReference.Child("GCodeEntries").Child(entryId).SetValueAsync(data).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Datos guardados exitosamente en Firebase.");
            }
            else
            {
                Debug.LogError("Error al guardar los datos: " + task.Exception);
            }
        });
    }
}