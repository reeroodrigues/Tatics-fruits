using System;
using System.IO;
using UnityEngine;

public static class JsonDataService
{
    private static string PathFor(string fileName)
        => Path.Combine(Application.persistentDataPath, fileName);
    
    public static void Save<T>(string fileName, T data)
    {
        try
        {
            var json = JsonUtility.ToJson(data, prettyPrint: true);
            File.WriteAllText(PathFor(fileName), json);

#if UNITY_EDITOR
            Debug.Log($"[Save] {PathFor(fileName)}\n{json}");
#endif
        }
        catch (Exception e)
        {
            Debug.LogError($"[JsonDataService] Falha ao salvar {fileName}: {e.Message}");
        }
    }
    
    public static bool TryLoad<T>(string fileName, out T data) where T : new()
    {
        var path = PathFor(fileName);

        if (File.Exists(path))
        {
            try
            {
                var json = File.ReadAllText(path);
                data = JsonUtility.FromJson<T>(json);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[JsonDataService] Erro ao ler {fileName}: {e.Message}");
            }
        }

        data = new T();
        return false;
    }
    
    public static T Load<T>(string fileName) where T : new()
    {
        if (TryLoad(fileName, out T data))
            return data;

        return new T();
    }
    
    public static void Delete(string fileName)
    {
        var path = PathFor(fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
#if UNITY_EDITOR
            Debug.Log($"[Delete] {path}");
#endif
        }
    }
}
