using System;
using UnityEngine;

namespace DefaultNamespace
{
    [Obsolete("This class is deprecated. Use LevelProgressService and PlayerProfileService instead for persistent game state.")]
    public static class GameSession_DEPRECATED
    {
        [Obsolete("Use LevelProgressService instead")]
        public static int _phaseNumber;
        
        [Obsolete("Use LevelProgressService instead")]
        public static int _targetScore;
        
        [Obsolete("Use LevelProgressService instead")]
        public static int _totalTime;
        
        [Obsolete("Use LevelProgressService instead")]
        public static string _objectiveDescription;
        
        [Obsolete("Use PlayerProfileService instead")]
        public static int _currentLevel = 1;
    }
}