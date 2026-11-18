using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PelotaControlesSimple : MonoBehaviour
{
    public float fuerza = 18f;
    public float salto = 7f;
    public float impulso = 7f;
    private Rigidbody2D cuerpo;
    private Vector2 movimiento;
    private bool saltar, acelerar;
    private float siguienteImpulso;
    private float direccion = 1f;
    private Vector3 inicio;

    void Awake()
    {
        cuerpo = GetComponent<Rigidbody2D>();
        inicio = transform.position;
    }

    void Update()
    {
        var teclado = Keyboard.current;
        if (teclado == null) return;
        movimiento = new Vector2((teclado.dKey.isPressed ? 1 : 0) - (teclado.aKey.isPressed ? 1 : 0),
            (teclado.wKey.isPressed ? 1 : 0) - (teclado.sKey.isPressed ? 1 : 0)).normalized;
        if (movimiento.x != 0) direccion = Mathf.Sign(movimiento.x);
        saltar |= teclado.spaceKey.wasPressedThisFrame;
        acelerar |= teclado.leftShiftKey.wasPressedThisFrame || teclado.rightShiftKey.wasPressedThisFrame;
        if (transform.position.y < -8f)
        {
            cuerpo.position = inicio;
            cuerpo.linearVelocity = Vector2.zero;
            cuerpo.angularVelocity = 0;
        }
    }

    void FixedUpdate()
    {
        cuerpo.AddForce(movimiento * fuerza);
        // Se comprueba el suelo sin detectar el collider de la propia pelota.
        bool enSuelo = false;
        foreach (var hit in Physics2D.RaycastAll(cuerpo.position, Vector2.down, 0.52f))
            if (hit.collider != null && hit.rigidbody != cuerpo && !hit.collider.isTrigger) enSuelo = true;
        if (saltar && enSuelo) cuerpo.AddForce(Vector2.up * salto, ForceMode2D.Impulse);
        if (acelerar && Time.time >= siguienteImpulso)
        {
            cuerpo.AddForce(Vector2.right * direccion * impulso, ForceMode2D.Impulse);
            siguienteImpulso = Time.time + 1f;
        }
        saltar = acelerar = false;
        cuerpo.linearVelocity = Vector2.ClampMagnitude(cuerpo.linearVelocity, 12f);
    }

}

