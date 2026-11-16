using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovimientoNave : MonoBehaviour
{
    public float velocidad = 5f;
    public float impulso = 15f;
    private Rigidbody rb;
    private Vector3 direccion, ultimaDireccion = Vector3.up, direccionImpulso;
    private float tiempoImpulso;
    void Awake() { rb = GetComponent<Rigidbody>(); }
    void Update()
    {
        float x = (Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0);
        float y = (Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0);
        direccion = new Vector3(x, y, 0).normalized;
        if (direccion != Vector3.zero) ultimaDireccion = direccion;
        if (Input.GetKeyDown(KeyCode.E)) { direccionImpulso = ultimaDireccion; tiempoImpulso = 0.3f; }
    }
    void FixedUpdate()
    {
        Vector3 objetivo = tiempoImpulso > 0 ? direccionImpulso * impulso : direccion * velocidad;
        tiempoImpulso -= Time.fixedDeltaTime;
        rb.AddForce(objetivo - rb.GetPointVelocity(rb.worldCenterOfMass), ForceMode.VelocityChange);
    }
}
