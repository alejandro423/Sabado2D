using UnityEngine;
using UnityEngine.UI;

// lleva el puntaje y lo muestra en pantalla
public class PuntosPlaza : MonoBehaviour
{
    public static PuntosPlaza Instancia { get; private set; }

    [Header("reglas del juego")]
    public int puntosMeta = 100;

    private int puntos;
    private bool gano;
    private Text texto;

    void Awake()
    {
        Instancia = this;
        texto = GetComponent<Text>();
        ActualizarTexto();
    }

    void OnDestroy()
    {
        if (Instancia == this) Instancia = null;
    }

    public void Sumar(int cantidad)
    {
        puntos += cantidad;
        ActualizarTexto();
    }

    void ActualizarTexto()
    {
        if (texto == null) return;

        if (!gano && puntos >= puntosMeta)
        {
            gano = true;
            texto.text = "ganaste, juntaste " + puntos + " puntos";
            texto.color = new Color(0.1f, 0.5f, 0.1f);
        }
        else
        {
            texto.text = "puntos: " + puntos + " / " + puntosMeta;
        }
    }
}
