using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Platformer
{
    /// <summary>
    /// Holds the run state (score, win) and keeps the HUD in sync.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Text scoreText;
        [SerializeField] private Text winText;

        private int score;

        public static GameManager Instance { get; private set; }

        public int Score => score;

        public bool HasWon { get; private set; }

        public Text ScoreText
        {
            get => scoreText;
            set
            {
                scoreText = value;
                RefreshHud();
            }
        }

        public Text WinText
        {
            get => winText;
            set => winText = value;
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

        private void Update()
        {
            bool restartPressed = Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
            if (HasWon && restartPressed)
            {
                Restart();
            }
        }

        public void AddCoins(int amount)
        {
            score += amount;
            RefreshHud();
        }

        public void Win()
        {
            if (HasWon)
            {
                return;
            }

            HasWon = true;

            if (winText != null)
            {
                winText.text = "Ganaste! Monedas: " + score + "\nR para reiniciar";
                winText.gameObject.SetActive(true);
            }
        }

        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
