using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardEntryView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI posLabel;
    [SerializeField] private Image avatar;
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private TextMeshProUGUI scoreLabel;
    [SerializeField] private TextMeshProUGUI timeLabel;
    [SerializeField] private Image background;
    
    [Header("Colors")]
    [SerializeField] private Color top1 = new Color(1f, 0.85f, 0.3f);
    [SerializeField] private Color top2 = new Color(0.85f, 0.9f, 1f);
    [SerializeField] private Color top3 = new Color(1f, 0.8f, 0.6f);
    [SerializeField] private Color regular = Color.white;
    [SerializeField] private Color currentPlayer = new Color(0.85f, 1f, 0.85f);

    public void Bind(LeaderboardEntry e, bool isCurrent = false)
    {
        if (posLabel)
            posLabel.text = e.rank.ToString();
        if (nameLabel)
            nameLabel.text = e.playerName;
        if (scoreLabel)
            scoreLabel.text = e.score.ToString();
        if (timeLabel)
            timeLabel.text = e.timeSeconds > 0 ? FormatTime(e.timeSeconds): "";

        if (background)
        {
            if(isCurrent)
                background.color = currentPlayer;
            else if (e.rank == 1)
                background.color = top1;
            else if (e.rank == 2)
                background.color = top2;
            else if (e.rank == 3)
                background.color = top3;
            else background.color = regular;
        }
    }

    private string FormatTime(float s)
    {
        int m = Mathf.FloorToInt(s / 60f);
        int ss = Mathf.FloorToInt(s % 60f);
        return $"{m:00}:{ss:00}";
    }
}