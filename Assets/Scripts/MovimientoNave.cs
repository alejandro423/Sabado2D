using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody))]
public class MovimientoNave : MonoBehaviour
{
    public float velocidad = 5f;
    public float impulso = 15f;
    private Rigidbody rb;
    private Vector3 direccion, ultimaDireccion = Vector3.up, direccionImpulso;
    private float tiempoImpulso;

    void Awake() { rb = GetComponent<Rigidbody>(); }

    // Funciona con el Input Manager viejo y con el Input System nuevo (Unity 6)
    static bool Presionada(char t)
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        var k = Keyboard.current; if (k == null) return false;
        switch (t) { case 'W': return k.wKey.isPressed; case 'A': return k.aKey.isPressed; case 'S': return k.sKey.isPressed; case 'D': return k.dKey.isPressed; case 'E': return k.eKey.isPressed; }
        return false;
#else
        switch (t) { case 'W': return Input.GetKey(KeyCode.W); case 'A': return Input.GetKey(KeyCode.A); case 'S': return Input.GetKey(KeyCode.S); case 'D': return Input.GetKey(KeyCode.D); case 'E': return Input.GetKey(KeyCode.E); }
        return false;
#endif
    }

    static bool PresionoE()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }

    void Update()
    {
        float x = (Presionada('D') ? 1 : 0) - (Presionada('A') ? 1 : 0);
        float y = (Presionada('W') ? 1 : 0) - (Presionada('S') ? 1 : 0);
        direccion = new Vector3(x, y, 0).normalized;
        if (direccion != Vector3.zero) ultimaDireccion = direccion;
        if (PresionoE()) { direccionImpulso = ultimaDireccion; tiempoImpulso = 0.3f; }
    }

    void FixedUpdate()
    {
        Vector3 objetivo = tiempoImpulso > 0 ? direccionImpulso * impulso : direccion * velocidad;
        tiempoImpulso -= Time.fixedDeltaTime;
        rb.AddForce(objetivo - rb.GetPointVelocity(rb.worldCenterOfMass), ForceMode.VelocityChange);
    }
}
