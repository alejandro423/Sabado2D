using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

[InitializeOnLoad]
public static class CrearEscena
{
    const string Ruta = "Assets/EscenaEspacial.unity";
    static CrearEscena() { EditorApplication.delayCall += Preparar; }
    static void Preparar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (!File.Exists(Ruta)) { Crear(); return; }
        var activa = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (string.IsNullOrEmpty(activa.path) && !activa.isDirty)
            EditorSceneManager.OpenScene(Ruta);
    }
    [MenuItem("Proyecto/Abrir escena espacial")]
    public static void Abrir()
    {
        if (!File.Exists(Ruta)) { Crear(); return; }
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            EditorSceneManager.OpenScene(Ruta);
    }
    static Material MaterialDe(string nombre, Color color, Texture2D textura = null)
    {
        string ruta = "Assets/" + nombre + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (m == null) { m = new Material(Shader.Find("Standard")); AssetDatabase.CreateAsset(m, ruta); }
        m.color = color; m.mainTexture = textura; EditorUtility.SetDirty(m); return m;
    }
    static GameObject Cubo(string nombre, Vector3 pos, Vector3 escala, Material m)
    {
        var o = GameObject.CreatePrimitive(PrimitiveType.Cube); o.name = nombre; o.transform.position = pos; o.transform.localScale = escala; o.GetComponent<Renderer>().sharedMaterial = m; return o;
    }
    [MenuItem("Proyecto/Crear escena espacial")]
    public static void Crear()
    {
        var escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var textura = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/TexturaNave.png");
        var azul = MaterialDe("MaterialNave", Color.white, textura);
        var gris = MaterialDe("MaterialAsteroide", new Color(.3f,.35f,.45f));
        var estrella = MaterialDe("MaterialEstrella", Color.white);
        var fondo = MaterialDe("MaterialFondo", new Color(.025f,.04f,.09f));
        var nave = new GameObject("Nave");
        var rb = nave.AddComponent<Rigidbody>(); rb.useGravity = false; rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation; rb.interpolation = RigidbodyInterpolation.Interpolate; rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        nave.AddComponent<MovimientoNave>();
        var cuerpo = Cubo("Cuerpo texturizado", Vector3.zero, new Vector3(.65f,1.25f,.32f), azul); cuerpo.transform.SetParent(nave.transform);
        for(int i=-1;i<=1;i+=2) { var ala = Cubo("Ala", new Vector3(i*.48f,-.23f,0), new Vector3(.48f,.75f,.24f), azul); ala.transform.rotation = Quaternion.Euler(0,0,i*25); ala.transform.SetParent(nave.transform); }
        var cam = new GameObject("Main Camera"); cam.tag="MainCamera"; var c=cam.AddComponent<Camera>(); cam.transform.position=new Vector3(0,0,-15); c.orthographic=true; c.orthographicSize=Mathf.Max(5.5f, 9f / c.aspect); cam.AddComponent<AudioListener>(); c.clearFlags=CameraClearFlags.SolidColor; c.backgroundColor=new Color(.025f,.04f,.09f);
        var luz=new GameObject("Luz"); var l=luz.AddComponent<Light>(); l.type=LightType.Directional; l.intensity=1.3f; luz.transform.rotation=Quaternion.Euler(25,-25,0);
        var baseFondo=Cubo("Fondo",new Vector3(0,0,4),new Vector3(24,13,.1f),fondo); Object.DestroyImmediate(baseFondo.GetComponent<Collider>());
        var rng=new System.Random(42);
        for(int i=0;i<75;i++) { float x=(float)rng.NextDouble()*19-9.5f; float y=(float)rng.NextDouble()*10-5; float s=.025f+(float)rng.NextDouble()*.045f; var e=Cubo("Estrella",new Vector3(x,y,3),Vector3.one*s,estrella); Object.DestroyImmediate(e.GetComponent<Collider>()); }
        Cubo("Obstaculo 1",new Vector3(-4,2,0),new Vector3(.9f,.9f,.7f),gris);
        Cubo("Obstaculo 2",new Vector3(4,-2,0),new Vector3(1.1f,1.1f,.7f),gris);
        Cubo("Limite superior",new Vector3(0,5.3f,0),new Vector3(19,.2f,1),gris);
        Cubo("Limite inferior",new Vector3(0,-5.3f,0),new Vector3(19,.2f,1),gris);
        Cubo("Limite izquierdo",new Vector3(-8.6f,0,0),new Vector3(.2f,11,1),gris);
        Cubo("Limite derecho",new Vector3(8.6f,0,0),new Vector3(.2f,11,1),gris);
        EditorSceneManager.SaveScene(escena,Ruta); EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Ruta,true)}; AssetDatabase.SaveAssets(); Selection.activeGameObject=nave;
    }
}
