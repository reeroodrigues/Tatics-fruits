using MoreMountains.Feedbacks;
using UnityEngine;

namespace Core.Services
{
    public class PairMatchFeedback : MonoBehaviour
    {
        [Header("Match Feedbacks")] 
        [SerializeField] private MMF_Player successFeedback;
        [SerializeField] private MMF_Player comboFeedback;
        [SerializeField] private int comboThreshold = 3;

        public void PlayMatchFeedback(int comboCount)
        {
            if (comboCount >= comboThreshold && comboFeedback != null)
            {
                comboFeedback.PlayFeedbacks();
            }
            else if(successFeedback != null)
            {
                successFeedback.PlayFeedbacks();
            }
        }
    }
}