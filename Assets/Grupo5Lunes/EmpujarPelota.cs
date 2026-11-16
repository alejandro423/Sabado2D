using UnityEngine;
using UnityEngine.InputSystem;

namespace Grupo5Lunes
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class EmpujarPelota : MonoBehaviour
    {
        [SerializeField] private float velocidad = 3.5f;
        [SerializeField] private float velocidadSalto = 6f;
        private readonly ContactPoint2D[] contactos = new ContactPoint2D[8];
        private Rigidbody2D cuerpo;
        private float direccion;
        private bool saltoSolicitado;

        private void Awake() => cuerpo = GetComponent<Rigidbody2D>();

        private void Update()
        {
            var teclado = Keyboard.current;
            direccion = 0;
            if (teclado == null) return;
            if (teclado.aKey.isPressed || teclado.leftArrowKey.isPressed) direccion -= 1;
            if (teclado.dKey.isPressed || teclado.rightArrowKey.isPressed) direccion += 1;
            if (teclado.spaceKey.wasPressedThisFrame) saltoSolicitado = true;
        }

        private void FixedUpdate()
        {
            cuerpo.linearVelocity = new Vector2(direccion * velocidad, cuerpo.linearVelocity.y);
            if (saltoSolicitado && cuerpo.linearVelocity.y <= 0.05f && EstaApoyado())
                cuerpo.linearVelocity = new Vector2(cuerpo.linearVelocity.x, velocidadSalto);
            saltoSolicitado = false;
        }

        private bool EstaApoyado()
        {
            int cantidad = cuerpo.GetContacts(contactos);
            for (int i = 0; i < cantidad; i++)
            {
                // Solo cuentan superficies bajo los pies, no paredes laterales.
                if (contactos[i].normal.y > 0.5f) return true;
            }
            return false;
        }

        private void OnDisable()
        {
            direccion = 0;
            saltoSolicitado = false;
            if (cuerpo != null) cuerpo.linearVelocity = Vector2.zero;
        }
    }
}
