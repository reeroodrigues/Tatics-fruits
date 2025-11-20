using UnityEngine;

public enum MissionEventType { WinLevel = 1 }

[CreateAssetMenu(menuName = "Game/Daily Mission", fileName = "DailyMission_")]
public class DailyMissionSo : ScriptableObject
{
    [Header("Identity")]
    public string id;
    public MissionEventType eventType = MissionEventType.WinLevel;

    [Header("Design")]
    public int target = 1;
    public int rewardGold = 50;
    public int levelParam = 0;

    [Header("Localization")]
    [Tooltip("Chave no arquivo de idiomas. Ex.: mission_win_level")]
    public string descriptionKey;

    [TextArea]
    [Tooltip("Fallback caso a chave não exista. Ex.: \"Vença o nível {0}\"")]
    public string descriptionTemplate = "Vença o nível {0}";
}