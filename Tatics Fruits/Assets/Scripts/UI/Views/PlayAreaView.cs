using TMPro;
using UnityEngine;

namespace New_GameplayCore.Views
{
    public class PlayAreaView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI playText;

        public void ShowMessage(string message)
        {
            playText.text = message;
        }
    }
}