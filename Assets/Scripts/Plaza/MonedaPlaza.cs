using UnityEngine;

// moneda de la plaza, al tocarla suma puntos y desaparece
public class MonedaPlaza : MonoBehaviour
{
    public int valor = 10;
    public float velocidadGiro = 90f;

    void Update()
    {
        transform.Rotate(0f, 0f, velocidadGiro * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (PuntosPlaza.Instancia == null) return;
        PuntosPlaza.Instancia.Sumar(valor);
        Destroy(gameObject);
    }
}
