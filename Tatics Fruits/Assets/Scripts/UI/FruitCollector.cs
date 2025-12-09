using MoreMountains.Feedbacks;
using UnityEngine;

namespace UI
{
    public class FruitCollector : MonoBehaviour
    {
        public MMF_Player collectFeedback;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if(other.CompareTag("Fruit"))
                collectFeedback?.PlayFeedbacks();
        }
    }
}
