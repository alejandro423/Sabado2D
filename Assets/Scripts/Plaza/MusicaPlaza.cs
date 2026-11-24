using UnityEngine;

// musica de fondo de la plaza, suena en bucle desde que arranca el juego
public class MusicaPlaza : MonoBehaviour
{
    void Awake()
    {
        var fuente = GetComponent<AudioSource>();
        fuente.loop = true;
        fuente.Play();
    }
}
