using UnityEngine;

// moneda de la plaza, al tocarla suma puntos, suena y desaparece
public class MonedaPlaza : MonoBehaviour
{
    public int valor = 10;
    public float velocidadGiro = 90f;
    public AudioClip sonido;

    void Update()
    {
        transform.Rotate(0f, 0f, velocidadGiro * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (sonido != null)
        {
            AudioSource.PlayClipAtPoint(sonido, transform.position);
        }

        if (PuntosPlaza.Instancia == null) return;
        PuntosPlaza.Instancia.Sumar(valor);
        Destroy(gameObject);
    }
}
