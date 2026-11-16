using UnityEngine;

// Reproduce los archivos WAV de efectos con una fuente independiente de la música.
[DisallowMultipleComponent]
public class SonidosSelva : MonoBehaviour
{
    [Range(0f,1f)] public float volumen=.38f;
    AudioSource fuente;
    AudioClip clic, seleccion, madera, cancelar, error;

    void Awake()
    {
        fuente=gameObject.AddComponent<AudioSource>();
        fuente.playOnAwake=false; fuente.spatialBlend=0; fuente.loop=false;
        clic=Cargar("ClickBoton");
        seleccion=Cargar("SeleccionAnclaje");
        madera=Cargar("ColocarMadera");
        cancelar=Cargar("Cancelar");
        error=Cargar("UnionInvalida");
    }

    AudioClip Cargar(string nombre)
    {
        var clip=Resources.Load<AudioClip>("Sonido/Efectos/"+nombre);
        if(!clip)Debug.LogWarning("Falta el sonido: "+nombre);
        return clip;
    }

    void Tocar(AudioClip clip) { if(fuente && clip)fuente.PlayOneShot(clip,volumen); }
    public void Boton() { Tocar(clic); }
    public void Seleccionar() { Tocar(seleccion); }
    public void Colocar() { Tocar(madera); }
    public void Cancelar() { Tocar(cancelar); }
    public void Error() { Tocar(error); }

}
