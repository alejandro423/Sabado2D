using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Level exit: touching it with the player wins the run.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Goal : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.Win();
            }
        }
    }
}
