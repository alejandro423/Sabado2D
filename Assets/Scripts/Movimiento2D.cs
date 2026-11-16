using UnityEngine;

public class Movimiento2D : MonoBehaviour
{
    public float velocidad = 5f;
    public float impulso = 10f;

    private Rigidbody2D rb;
    private Vector2 movimiento;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        movimiento.x = Input.GetAxisRaw("Horizontal");
        movimiento.y = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.Y))
        {
            transform.Rotate(0, 0, 90);
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position +
            movimiento.normalized * velocidad *
            Time.fixedDeltaTime);
    }
}