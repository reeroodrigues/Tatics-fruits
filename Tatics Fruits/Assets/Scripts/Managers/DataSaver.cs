using System;
using System.Collections;
using Firebase.Database;
using Google.MiniJSON;
using UnityEngine;

[Serializable]
public class dataToSave
{
    public string userName;
    public int totalCoins;
    public int crrLevel;
    public int highScore;
}
public class DataSaver : MonoBehaviour
{
    public dataToSave dts;
    public string userId;
    private DatabaseReference dbRef;

    private void Awake()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void SaveDataFn()
    {
        var json = JsonUtility.ToJson(dts);
        dbRef.Child("users").Child(userId).SetRawJsonValueAsync(json);
    }

    public void LoadDataFn()
    {
        StartCoroutine(LoadDataEnum());
    }

    IEnumerator LoadDataEnum()
    {
        var serverData = dbRef.Child("users").Child(userId).GetValueAsync();
        yield return new WaitUntil(predicate: () => serverData.IsCompleted);
        
        Debug.Log("Process is Completed!");
        
        DataSnapshot snapshot = serverData.Result;
        var jsonData = snapshot.GetRawJsonValue();

        if (jsonData != null)
        {
            Debug.Log("Server data found!");
            dts = JsonUtility.FromJson<dataToSave>(jsonData);
        }
        else
        {
            Debug.Log("No data found!");
        }
    }
}
