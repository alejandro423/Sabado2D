using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PrepararPelotaSimple
{
    [MenuItem("FLUXLAB/Mejorar escena de pelota")]
    public static void Preparar()
    {
        if (Application.isPlaying) { Debug.LogWarning("Deten Play antes de preparar la escena."); return; }
        int grupo = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Mejorar escena de pelota");
        Crear("Suelo", new Vector3(0,-3,0), new Vector3(16,0.5f,1), new Color(0.12f,0.75f,0.58f));
        Crear("ParedIzquierda", new Vector3(-8,-0.25f,0), new Vector3(0.5f,6,1), new Color(0.12f,0.75f,0.58f));
        Crear("ParedDerecha", new Vector3(8,-0.25f,0), new Vector3(0.5f,6,1), new Color(0.12f,0.75f,0.58f));
        Crear("Techo", new Vector3(0,2.75f,0), new Vector3(16.5f,0.5f,1), new Color(0.12f,0.75f,0.58f));
        var rampa = Crear("Rampa", new Vector3(-3.7f,-0.8f,0), new Vector3(8,0.6f,1), new Color(1,0.74f,0.15f));
        rampa.transform.rotation = Quaternion.Euler(0,0,-20);
        for (int i=0; i<3; i++)
        {
            var caja = Crear("Caja"+(i+1), i==2 ? new Vector3(3.6f,0.55f,0) : new Vector3(2.45f+i*2.3f,-1.65f,0), new Vector3(2.2f,2.2f,1),
                i==2 ? new Color(1,0.44f,0.55f) : new Color(1,0.55f,0.22f));
            if (!caja.GetComponent<Rigidbody2D>()) Undo.AddComponent<Rigidbody2D>(caja);
        }
        var pelota = GameObject.Find("Pelota");
        if (!pelota) { pelota = new GameObject("Pelota"); Undo.RegisterCreatedObjectUndo(pelota,"Crear pelota"); }
        Undo.RegisterFullObjectHierarchyUndo(pelota,"Configurar pelota");
        pelota.transform.position = new Vector3(-6,1,0);
        pelota.transform.localScale = new Vector3(0.8f,0.8f,1);
        // Unity incluye Circle como sprite integrado del editor.
        var circulo = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        var renderer = pelota.GetComponent<SpriteRenderer>() ?? Undo.AddComponent<SpriteRenderer>(pelota);
        renderer.sprite = circulo;
        renderer.drawMode = SpriteDrawMode.Simple;
        renderer.sortingOrder = 2;
        renderer.color = new Color(0.65f,0.3f,1);
        renderer.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        // Ajusta el sprite integrado a un diámetro de una unidad antes de escalarlo.
        if (circulo) pelota.transform.localScale = new Vector3(0.8f/circulo.bounds.size.x,0.8f/circulo.bounds.size.y,1);
        foreach (var collider in pelota.GetComponents<Collider2D>()) Undo.DestroyObjectImmediate(collider);
        var esfera = Undo.AddComponent<CircleCollider2D>(pelota);
        esfera.radius = circulo ? circulo.bounds.size.x/2 : 0.5f;
        var cuerpo = pelota.GetComponent<Rigidbody2D>() ?? Undo.AddComponent<Rigidbody2D>(pelota);
        cuerpo.bodyType = RigidbodyType2D.Dynamic;
        cuerpo.gravityScale = 1;
        cuerpo.mass = 1;
        cuerpo.constraints = RigidbodyConstraints2D.None;
        cuerpo.linearDamping = 0.5f;
        cuerpo.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        if (!pelota.GetComponent<PelotaControlesSimple>()) Undo.AddComponent<PelotaControlesSimple>(pelota);
        if (Camera.main)
        {
            Undo.RecordObject(Camera.main,"Color de fondo");
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = new Color(0.055f,0.09f,0.18f);
            Camera.main.orthographic = true;
            Camera.main.orthographicSize = 5;
            Undo.RecordObject(Camera.main.transform,"Encuadrar escenario");
            Camera.main.transform.position = new Vector3(0,0,-10);
            Camera.main.transform.rotation = Quaternion.identity;
            if (!Camera.main.GetComponent<PelotaEncuadreSimple>()) Undo.AddComponent<PelotaEncuadreSimple>(Camera.main.gameObject);
        }
        Undo.CollapseUndoOperations(grupo);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = pelota;
        Debug.Log("Escena preparada. Guarda con Ctrl+S y pulsa Play. WASD, Espacio y Shift.");
    }

    private static GameObject Crear(string nombre, Vector3 posicion, Vector3 escala, Color color)
    {
        var objeto = GameObject.Find(nombre);
        if (!objeto) { objeto = new GameObject(nombre); Undo.RegisterCreatedObjectUndo(objeto,"Crear "+nombre); }
        Undo.RegisterFullObjectHierarchyUndo(objeto,"Configurar "+nombre);
        objeto.transform.position = posicion;
        objeto.transform.rotation = Quaternion.identity;
        var renderer = objeto.GetComponent<SpriteRenderer>() ?? Undo.AddComponent<SpriteRenderer>(objeto);
        renderer.sprite = CuadradoSolido();
        renderer.drawMode = SpriteDrawMode.Simple;
        var dimensiones = renderer.sprite.bounds.size;
        objeto.transform.localScale = new Vector3(escala.x/dimensiones.x, escala.y/dimensiones.y, 1);
        renderer.color = color;
        renderer.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        var collider = objeto.GetComponent<BoxCollider2D>() ?? Undo.AddComponent<BoxCollider2D>(objeto);
        collider.autoTiling = false;
        collider.offset = Vector2.zero;
        collider.size = new Vector2(dimensiones.x,dimensiones.y);
        collider.isTrigger = false;
        return objeto;
    }

    private static Sprite CuadradoSolido()
    {
        const string carpeta = "Assets/Art/PelotaSimple";
        const string ruta = carpeta + "/CuadradoSolido.png";
        var existente = AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
        if (existente) return existente;
        System.IO.Directory.CreateDirectory(carpeta);
        var textura = new Texture2D(64,64,TextureFormat.RGBA32,false);
        var pixeles = new Color32[64*64];
        for (int i=0; i<pixeles.Length; i++) pixeles[i] = new Color32(255,255,255,255);
        textura.SetPixels32(pixeles);
        textura.Apply();
        System.IO.File.WriteAllBytes(ruta,textura.EncodeToPNG());
        Object.DestroyImmediate(textura);
        AssetDatabase.ImportAsset(ruta,ImportAssetOptions.ForceSynchronousImport);
        var importador = (TextureImporter)AssetImporter.GetAtPath(ruta);
        importador.textureType = TextureImporterType.Sprite;
        importador.spriteImportMode = SpriteImportMode.Single;
        importador.spritePixelsPerUnit = 64;
        importador.filterMode = FilterMode.Point;
        importador.textureCompression = TextureImporterCompression.Uncompressed;
        importador.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
    }
}
