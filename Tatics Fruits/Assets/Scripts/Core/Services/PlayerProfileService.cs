using System.Collections.Generic;
using UnityEngine.VFX;

namespace New_GameplayCore.Services
{
    public class PlayerProfileService
    {
        private const string FILE = "player_profile.json";
        private PlayerProfileData _data;

        public PlayerProfileData Data => _data;

        public void Load()
        {
            if (!JsonDataService.TryLoad(FILE, out _data))
            {
                _data = new PlayerProfileData();
                Save();
            }
        }
        
        public void Save() => JsonDataService.Save(FILE, _data);

        public void AddGold(int amount)
        {
            _data.gold += amount;
            if (_data.gold < 0)
                _data.gold = 0;
            Save();
        }

        public void SetLevel(int index, bool completed = false)
        {
            _data.currentLevelIndex = index;
            if (completed && index >= _data.highestLevelUnlocked)
                _data.highestLevelUnlocked = index + 1;
            Save();
        }

        public void SetSettings(bool music, bool sfx, bool vfx, string lang)
        {
            _data.musicOn = music;
            _data.sfxOn = sfx;
            _data.vfxOn = vfx;
            _data.language = lang;
            Save();
        }

        public void UpdateDailyLogin(string todayKey)
        {
            if (_data.daily.login.lastClaimDayKey != todayKey)
            {
                _data.daily.login.lastClaimDayKey = todayKey;
                Save();
            }
        }

        public void UpdateDailyMissions(List<DailyMissionState> missions)
        {
            _data.daily.missions = missions;
            Save();
        }

        public void RegisterBestScore(string levelId, int score)
        {
            if(string.IsNullOrEmpty(levelId))
                return;
            
            if(!_data.BestScores.ContainsKey(levelId))
                _data.BestScores[levelId] = score;
            else if(score > _data.BestScores[levelId])
                _data.BestScores[levelId] = score;

            Save();
        }

        public int GetBestScore(string levelId)
        {
            if(string.IsNullOrEmpty(levelId))
                return 0;
            
            return _data.BestScores.TryGetValue(levelId, out int best) ? best : 0;
        }
    }
}