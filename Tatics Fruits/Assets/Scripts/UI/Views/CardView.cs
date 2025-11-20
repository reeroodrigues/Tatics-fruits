using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace New_GameplayCore.Views
{
    public class CardView : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI rightValueText;
        [SerializeField] private TextMeshProUGUI leftValueText;

        private CardInstance _data;
        private System.Action<CardInstance> _onSelected;
        public CardInstance GetCardData() => _data;

        public void Initialize(CardInstance data, System.Action<CardInstance> onSelected)
        {
            _data = data;
            _onSelected = onSelected;
            icon.sprite = data.Type.sprite;
            rightValueText.text = data.Value.ToString();
            leftValueText.text = data.Value.ToString();
        }

        public void OnClick()
        {
            _onSelected?.Invoke(_data);
        }
    }
}