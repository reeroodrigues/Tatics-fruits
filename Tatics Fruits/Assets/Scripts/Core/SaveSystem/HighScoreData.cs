using System;
using UnityEngine;
using Core.SaveSystem;

[Serializable]
public class HighScoreData : ISaveData
{
    public int score;

    public string GetFileName() => "highscore.json";

    public void OnBeforeSave()
    {
    }

    public void OnAfterLoad()
    {
        score = Mathf.Max(0, score);
    }
}
