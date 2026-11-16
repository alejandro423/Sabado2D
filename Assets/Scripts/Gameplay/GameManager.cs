using UnityEngine;
using UnityEngine.UI;

namespace Platformer
{
    /// <summary>
    /// Holds the run state (score) and keeps the HUD in sync.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Text scoreText;

        private int score;

        public static GameManager Instance { get; private set; }

        public int Score => score;

        public Text ScoreText
        {
            get => scoreText;
            set
            {
                scoreText = value;
                RefreshHud();
            }
        }

        private void Awake()
        {
            Instance = this;
            RefreshHud();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void AddCoins(int amount)
        {
            score += amount;
            RefreshHud();
        }

        private void RefreshHud()
        {
            if (scoreText != null)
            {
                scoreText.text = "Monedas: " + score;
            }
        }
    }
}
