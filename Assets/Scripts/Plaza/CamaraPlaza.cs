using UnityEngine;

// sigue al cubito por el nivel largo, con un poco de adelanto
// los limites evitan que la camara muestre el borde del mundo
public class CamaraPlaza : MonoBehaviour
{
    public Transform objetivo;
    public float suavizado = 3f;
    public float adelanto = 1.5f;
    public float limiteIzquierdo = -19.2f;
    public float limiteDerecho = 19.2f;
    public float alturaFija = 0f;

    private float velocidad;

    void LateUpdate()
    {
        if (objetivo == null) return;

        var cuerpo = objetivo.GetComponent<Rigidbody2D>();
        float direccion = cuerpo != null ? Mathf.Sign(cuerpo.linearVelocity.x) : 0f;

        float xDeseada = Mathf.Clamp(
            objetivo.position.x + direccion * adelanto,
            limiteIzquierdo,
            limiteDerecho);

        float x = Mathf.SmoothDamp(transform.position.x, xDeseada, ref velocidad, 1f / Mathf.Max(0.01f, suavizado));

        transform.position = new Vector3(x, alturaFija, transform.position.z);
    }
}
