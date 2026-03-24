using TMPro;
using UnityEngine;

namespace _Game.Scripts.Core
{
    public class StatCardController : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text hitText;
        [SerializeField] private TMP_Text attackText;
        [SerializeField] private TMP_Text bestComboText;
        [SerializeField] private TMP_Text currentComboText;
        [SerializeField] private TMP_Text timeText;

        private void Awake()
        {
            StatisticController stats = StatisticController.current;

            scoreText.text = stats.score.ToString();
            hitText.text = stats.hit.ToString();
            attackText.text = stats.attack.ToString();
            bestComboText.text = stats.bestCombo.ToString();
            currentComboText.text = stats.currentCombo.ToString();
            timeText.text = Time.timeSinceLevelLoad.ToString("F0");
        }
    }
}
