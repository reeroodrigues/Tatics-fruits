using System;

namespace New_GameplayCore.GameState
{
    public class ComboTracker : IComboTracker
    {
        private readonly LevelConfigSO _cfg;
        private float _timer;
        private bool _active;
        
        public int CurrentCombo { get; private set; }
        public float RemainingWindowsMs  => _timer;
        public event Action<int> OnComboChanged;
        
        public ComboTracker(LevelConfigSO cfg) => _cfg = cfg;

        public void RegisterPair()
        {
            if (_active && _timer > 0)
                CurrentCombo++;
            else
                CurrentCombo = -1;

            _timer = _cfg.comboWindows;
            _active = true;
            OnComboChanged?.Invoke(CurrentCombo);
        }

        public void Tick(float deltaMs)
        {
            if(!_active)
                return;
            
            _timer -= deltaMs;
            if (_timer <= 0)
            {
                _active = false;
                CurrentCombo = 0;
                OnComboChanged?.Invoke(CurrentCombo);
            }
        }

        public void Reset()
        {
            _active = false;
            _timer = 0;
            CurrentCombo = 0;
            OnComboChanged?.Invoke(CurrentCombo);
        }
        
    }
}