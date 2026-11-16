using UnityEngine;
using UnityEngine.InputSystem;

namespace Grupo5Lunes
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class EmpujarPelota : MonoBehaviour
    {
        [SerializeField] private float velocidad = 3.5f;
        private Rigidbody2D cuerpo;
        private float direccion;

        private void Awake() => cuerpo = GetComponent<Rigidbody2D>();

        private void Update()
        {
            var teclado = Keyboard.current;
            direccion = 0;
            if (teclado == null) return;
            if (teclado.aKey.isPressed || teclado.leftArrowKey.isPressed) direccion -= 1;
            if (teclado.dKey.isPressed || teclado.rightArrowKey.isPressed) direccion += 1;
        }

        private void FixedUpdate()
        {
            cuerpo.linearVelocity = new Vector2(direccion * velocidad, cuerpo.linearVelocity.y);
        }

        private void OnDisable()
        {
            direccion = 0;
            if (cuerpo != null) cuerpo.linearVelocity = Vector2.zero;
        }
    }
}
