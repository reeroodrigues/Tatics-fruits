using System;
using System.Collections.Generic;

namespace New_GameplayCore.Services
{
    [Serializable]
    public class LevelProgressData
    {
        public int currentIndex = 0;
        public int unlockedMaxIndex = 0;
        public Dictionary<string, int> best = new();
        public Dictionary<string, int> stars = new();
    }
}