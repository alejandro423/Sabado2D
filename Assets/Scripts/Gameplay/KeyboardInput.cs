using UnityEngine;
using UnityEngine.InputSystem;

namespace Platformer
{
    /// <summary>
    /// Reads the keyboard and pushes it into <see cref="PlayerController"/>.
    /// Left/right: A-D or arrow keys. Jump: Space, W or Up.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class KeyboardInput : MonoBehaviour
    {
        private PlayerController controller;

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                controller.MoveInput = 0f;
                return;
            }

            float horizontal = 0f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            {
                horizontal -= 1f;
            }

            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            {
                horizontal += 1f;
            }

            controller.MoveInput = horizontal;

            bool jump = keyboard.spaceKey.wasPressedThisFrame
                        || keyboard.wKey.wasPressedThisFrame
                        || keyboard.upArrowKey.wasPressedThisFrame;
            if (jump)
            {
                controller.QueueJump();
            }
        }
    }
}
