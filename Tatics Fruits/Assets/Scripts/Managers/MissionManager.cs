using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public class MissionManager : MonoBehaviour
    {
        public static MissionManager Instance;
        public List<Mission> _activeMissions = new List<Mission>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void Start()
        {
            GenerateMissions();
        }

        void GenerateMissions()
        {
            _activeMissions.Clear();
            _activeMissions.Add(new Mission("Marque 5000 pontos", "Faça 5000 pontos na partida", MissionType.ScorePoints, 5000, 100, "Gold"));
            _activeMissions.Add(new Mission("Jogue 3 partidas", "Complete 3 partidas", MissionType.PlayXMatches, 3, 1, "NewCard"));

            foreach (var mission in _activeMissions)
                mission.OnMissionCompleted += HandleMissionCompleted;
        }

        void HandleMissionCompleted(Mission mission)
        {
            Debug.Log($"Missão concluída: {mission._missionName}! Recompensa: {mission._rewardAmount} {mission._rewardType}");
        }

        public void UpdateMissionProgress(MissionType type, int amount)
        {
            foreach (var mission in _activeMissions)
            {
                if (mission._type == type)
                    mission.UpdateProgress(amount);
            }
        }
    }
}