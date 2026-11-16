using UnityEngine;

[DisallowMultipleComponent]
public class MusicaSelva : MonoBehaviour
{
    [Range(0f,1f)] public float volumen = .22f;
    AudioSource fuente;
    AudioClip ambiente, derrota;
    float posicionAmbiente, duracionEntrada=2;
    bool enDerrota;
    float entrada;
    public bool Silenciada { get; private set; }
    const string Preferencia="FLUXLAB.MusicaSilenciada";

    void Awake()
    {
        fuente=gameObject.AddComponent<AudioSource>();
        fuente.playOnAwake=false;
        ambiente=Resources.Load<AudioClip>("Sonido/Musica/SelvaAmbiente");
        derrota=Resources.Load<AudioClip>("Sonido/Musica/Derrota");
        fuente.clip=ambiente;
        fuente.loop=true;
        fuente.spatialBlend=0;
        fuente.volume=0;
        Silenciada=PlayerPrefs.GetInt(Preferencia,0)==1;
        fuente.mute=Silenciada;
        if(fuente.clip)fuente.Play();
        else Debug.LogWarning("No se encontró la música de la selva.");
    }

    void Update()
    {
        entrada=Mathf.Min(1,entrada+Time.unscaledDeltaTime/duracionEntrada);
        fuente.volume=volumen*entrada;
    }

    public void Alternar()
    {
        Silenciada=!Silenciada;
        fuente.mute=Silenciada;
        PlayerPrefs.SetInt(Preferencia,Silenciada?1:0);
        PlayerPrefs.Save();
    }

    public void AlPerder()
    {
        if(enDerrota)return;
        posicionAmbiente=fuente.time;
        enDerrota=true;
        fuente.Stop();fuente.clip=derrota;fuente.loop=false;
        entrada=0;duracionEntrada=.15f;fuente.volume=0;
        fuente.Play();
    }

    public void AlReintentar()
    {
        if(!enDerrota)return;
        enDerrota=false;
        fuente.Stop();fuente.clip=ambiente;fuente.loop=true;
        entrada=0;duracionEntrada=.8f;fuente.volume=0;
        if(ambiente){fuente.time=Mathf.Clamp(posicionAmbiente,0,Mathf.Max(0,ambiente.length-.01f));fuente.Play();}
    }

}
