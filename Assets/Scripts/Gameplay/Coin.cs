using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Collectible coin: adds score when the player touches it, then disappears.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Coin : MonoBehaviour
    {
        [SerializeField] private int value = 1;

        public int Value => value;

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddCoins(value);
            }

            Destroy(gameObject);
        }
    }
}
