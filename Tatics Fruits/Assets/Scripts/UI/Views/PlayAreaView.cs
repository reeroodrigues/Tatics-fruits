using TMPro;
using UnityEngine;

namespace UI.Views
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