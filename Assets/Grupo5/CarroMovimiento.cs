using UnityEngine;
using UnityEngine.InputSystem;

public class CarroMovimiento : MonoBehaviour
{
    [SerializeField] private float forwardSpeed = 2.5f;
    [SerializeField] private float steeringSpeed = 4.5f;
    [SerializeField] private float roadHalfWidth = 3.8f;
    [SerializeField] private float vehicleHalfWidth = 0.36f;
    [SerializeField] private float finishY = 7.2f;

    private void Update()
    {
        Vector3 position = transform.position;
        if (position.y >= finishY)
            return;

        float horizontalInput = 0f;
        Keyboard keyboard = Keyboard.current;

        if (keyboard != null)
        {
            if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed)
                horizontalInput -= 1f;
            if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed)
                horizontalInput += 1f;
        }

        position.x = Mathf.Clamp(
            position.x + horizontalInput * steeringSpeed * Time.deltaTime,
            -roadHalfWidth + vehicleHalfWidth,
            roadHalfWidth - vehicleHalfWidth);
        position.y = Mathf.Min(position.y + forwardSpeed * Time.deltaTime, finishY);
        transform.position = position;
    }
}
