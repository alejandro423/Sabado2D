using UnityEngine;

// mueve a la cholita por la plaza con las flechas o a/d
// saltar con w, espacio o flecha arriba
public class MovimientoPlaza : MonoBehaviour
{
    [Header("ajustes de movimiento")]
    public float velocidad = 4f;
    public float fuerzaSalto = 7f;

    [Header(" chequeo de suelo")]
    public Transform pies;
    public float radioSuelo = 0.08f;
    public LayerMask capaSuelo;

    private Rigidbody2D cuerpo;
    private bool enSuelo;

    void Awake()
    {
        cuerpo = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        Mover();
        Saltar();
    }

    void Mover()
    {
        float direccion = Input.GetAxisRaw("Horizontal");
        cuerpo.linearVelocity = new Vector2(direccion * velocidad, cuerpo.linearVelocity.y);

        // que mire hacia donde camina
        if (direccion != 0f)
        {
            Vector3 escala = transform.localScale;
            escala.x = Mathf.Abs(escala.x) * Mathf.Sign(direccion);
            transform.localScale = escala;
        }
    }

    void Saltar()
    {
        enSuelo = pies != null && Physics2D.OverlapCircle(pies.position, radioSuelo, capaSuelo);

        bool quiereSaltar = Input.GetKey(KeyCode.W)
                         || Input.GetKey(KeyCode.UpArrow)
                         || Input.GetKey(KeyCode.Space);

        if (quiereSaltar && enSuelo)
        {
            cuerpo.linearVelocity = new Vector2(cuerpo.linearVelocity.x, fuerzaSalto);
        }
    }

    // para ver en el editor donde chequea el suelo
    void OnDrawGizmosSelected()
    {
        if (pies == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(pies.position, radioSuelo);
    }
}
