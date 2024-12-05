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
        if (!float.TryParse(xInput.text, out float xValue) ||
            !float.TryParse(yInput.text, out float yValue) ||
            !float.TryParse(zInput.text, out float zValue) ||
            !float.TryParse(rInput.text, out float rValue) ||
            !float.TryParse(fInput.text, out float fValue) ||
            string.IsNullOrEmpty(gInput.text) || !int.TryParse(gInput.text, out int gValue))
        {
            Debug.LogError("Uno o más valores no son válidos.");
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