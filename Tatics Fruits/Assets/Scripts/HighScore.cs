using UnityEngine;
using Core.SaveSystem;

namespace DefaultNamespace
{
    public class HighScore : MonoBehaviour
    {
        private HighScoreData _data;

        private void Awake()
        {
            _data = SaveManager.Instance.Load<HighScoreData>();
        }

        public int GetHighScore()
        {
            if (_data == null)
            {
                _data = SaveManager.Instance.Load<HighScoreData>();
            }
            return _data.score;
        }

        public void TrySetHighScore(int newScore)
        {
            int currentHighScore = GetHighScore();
            if (newScore > currentHighScore)
            {
                _data.score = newScore;
                SaveManager.Instance.Save(_data);
                Debug.Log($"Novo High Score Salvo: {newScore}");
            }
        }
    }
}