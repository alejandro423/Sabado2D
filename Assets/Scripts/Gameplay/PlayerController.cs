using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Horizontal movement and jumping for the platformer player.
    /// Input is pushed in by <see cref="KeyboardInput"/> so the controller can be driven by tests.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float jumpSpeed = 10f;
        [SerializeField] private float fallResetY = -8f;

        private Rigidbody2D body;
        private BoxCollider2D bodyCollider;
        private Vector3 spawnPoint;
        private bool jumpQueued;

        public float MoveInput { get; set; }

        public bool IsGrounded { get; private set; }

        public bool JumpQueued => jumpQueued;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<BoxCollider2D>();
            spawnPoint = transform.position;
        }

        private void FixedUpdate()
        {
            IsGrounded = CheckGrounded();

            Vector2 velocity = body.linearVelocity;
            body.linearVelocity = new Vector2(Mathf.Clamp(MoveInput, -1f, 1f) * moveSpeed, velocity.y);

            if (jumpQueued && IsGrounded)
            {
                body.linearVelocity = new Vector2(body.linearVelocity.x, jumpSpeed);
            }

            jumpQueued = false;

            if (transform.position.y < fallResetY)
            {
                Respawn();
            }
        }

        public void QueueJump()
        {
            jumpQueued = true;
        }

        private bool CheckGrounded()
        {
            Bounds bounds = bodyCollider.bounds;
            Vector2 feet = new Vector2(bounds.center.x, bounds.min.y + 0.02f);
            Vector2 size = new Vector2(bounds.size.x * 0.9f, 0.08f);

            Collider2D[] hits = Physics2D.OverlapBoxAll(feet, size, 0f);
            foreach (Collider2D hit in hits)
            {
                if (hit != bodyCollider && !hit.isTrigger && hit.attachedRigidbody != body)
                {
                    return true;
                }
            }

            return false;
        }

        private void Respawn()
        {
            body.position = spawnPoint;
            body.linearVelocity = Vector2.zero;
        }
    }
}
