using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// arma el juego de puntos en la escena de la plaza
// se usa desde el menu: Plaza > Armar juego de puntos
public static class ArmadorPlaza
{
    const string NombreCapaPiso = "Piso";
    const string RutaMoneda = "Assets/PrefabsPlaza/Moneda.prefab";
    const string RutaSpriteMoneda = "Assets/Art/coin.png";
    const string RutaSonidoMoneda = "Assets/SonidosPlaza/moneda.wav";
    const string RutaSpriteCubito = "Assets/ArteBoliviano/cubito_gorrito.png";
    const string RutaSpriteCaja = "Assets/ArteBoliviano/caja_madera.png";
    const string RutaMusicaHuayno = "Assets/SonidosPlaza/huayno_plaza.wav";
    const float AlturaDelCubito = 2f;
    const float EscalaDelCubito = 2f;
    const int PuntosMeta = 160;

    [MenuItem("Plaza/Armar juego de puntos")]
    public static void Armar()
    {
        GameObject cholita = GameObject.Find("Cholita Paceña");
        if (cholita == null)
        {
            Debug.LogError("no encontre a la cholita, abri la escena PlazaBoliviana primero");
            return;
        }

        AsegurarCapaPiso();
        int capaPiso = LayerMask.NameToLayer(NombreCapaPiso);

        // la cholita es la referencia fija del suelo de la plaza
        float suelo = LineaDeSuelo(cholita);

        // la cholita queda como parte del decorado, el jugador es el cubito
        DesarmarCholita();
        PrepararCubito(capaPiso, suelo);
        CrearPiso(suelo, capaPiso);
        CrearBordes(suelo, capaPiso);
        CrearCajas(suelo, capaPiso);
        ColocarMonedas(suelo);
        CrearPuntaje();
        AjustarMeta();
        CrearMusica();

        EditorSceneManager.MarkAllScenesDirty();
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("plaza lista, fijate que la cholita salte con w y junte las monedas");
    }

    [MenuItem("Plaza/Limpiar armado")]
    public static void Limpiar()
    {
        foreach (MonedaPlaza moneda in Object.FindObjectsByType<MonedaPlaza>(FindObjectsInactive.Exclude))
        {
            Object.DestroyImmediate(moneda.gameObject);
        }

        Destruir("Piso de la plaza");
        Destruir("Borde izquierdo");
        Destruir("Borde derecho");
        Destruir("PlazaUI");
        Destruir("Cajas de la plaza");
        Destruir("Cubito");
        Destruir("MusicaPlaza");

        DesarmarCholita();

        EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("plaza limpia");
    }

    static void PrepararCubito(int capaPiso, float suelo)
    {
        GameObject cubito = GameObject.Find("Cubito");

        if (cubito == null)
        {
            cubito = new GameObject("Cubito");
            var sprite = cubito.AddComponent<SpriteRenderer>();
            sprite.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(RutaSpriteCubito);
            cubito.transform.position = new Vector3(-3f, 0f, 0f);
        }

        SpriteRenderer render = cubito.GetComponent<SpriteRenderer>();
        render.sortingOrder = 50;

        // que se vea bien, parado justo sobre el suelo
        cubito.transform.localScale = Vector3.one * EscalaDelCubito;
        cubito.transform.position = new Vector3(
            cubito.transform.position.x,
            suelo + AlturaDelCubito * EscalaDelCubito / 2f,
            0f);

        var datos = render.sprite;

        float ancho = datos.rect.width / datos.pixelsPerUnit;
        float alto = datos.rect.height / datos.pixelsPerUnit;
        var pivote = new Vector2(datos.pivot.x / datos.pixelsPerUnit, datos.pivot.y / datos.pixelsPerUnit);

        var caja = cubito.GetComponent<BoxCollider2D>();
        if (caja == null) caja = cubito.AddComponent<BoxCollider2D>();
        caja.size = new Vector2(ancho * 0.8f, alto * 0.9f);
        caja.offset = new Vector2(pivote.x - ancho / 2f, pivote.y - alto / 2f);

        var cuerpo = cubito.GetComponent<Rigidbody2D>();
        if (cuerpo == null) cuerpo = cubito.AddComponent<Rigidbody2D>();
        cuerpo.freezeRotation = true;
        cuerpo.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var movimiento = cubito.GetComponent<MovimientoPlaza>();
        if (movimiento == null) movimiento = cubito.AddComponent<MovimientoPlaza>();
        movimiento.velocidad = 4f;
        movimiento.fuerzaSalto = 7f;
        movimiento.capaSuelo = LayerMask.GetMask(NombreCapaPiso);

        Transform pies = cubito.transform.Find("Pies");
        if (pies == null)
        {
            pies = new GameObject("Pies").transform;
            pies.SetParent(cubito.transform, false);
        }
        pies.localPosition = new Vector3(caja.offset.x, caja.offset.y - caja.size.y / 2f, 0f);
        movimiento.pies = pies;
    }

    static float LineaDeSuelo(GameObject cholita)
    {
        // los pies de la cholita son la referencia fija del suelo de la plaza
        var sprite = cholita.GetComponent<SpriteRenderer>();
        if (sprite != null) return sprite.bounds.min.y;

        return 0f;
    }

    static void DesarmarCholita()
    {
        GameObject cholita = GameObject.Find("Cholita Paceña");
        if (cholita == null) return;

        Quitar<MovimientoPlaza>(cholita);
        Quitar<BoxCollider2D>(cholita);
        Quitar<Rigidbody2D>(cholita);
        Transform pies = cholita.transform.Find("Pies");
        if (pies != null) Object.DestroyImmediate(pies.gameObject);
    }

    static void CrearPiso(float suelo, int capaPiso)
    {
        GameObject piso = GameObject.Find("Piso de la plaza");
        if (piso == null) piso = new GameObject("Piso de la plaza");

        piso.layer = capaPiso;
        var caja = piso.GetComponent<BoxCollider2D>();
        if (caja == null) caja = piso.AddComponent<BoxCollider2D>();
        caja.size = new Vector2(40f, 0.5f);
        piso.transform.position = new Vector3(0f, suelo - 0.25f, 0f);
    }

    static void CrearBordes(float suelo, int capaPiso)
    {
        CrearBorde("Borde izquierdo", -20.25f, suelo, capaPiso);
        CrearBorde("Borde derecho", 20.25f, suelo, capaPiso);
    }

    static void CrearBorde(string nombre, float x, float suelo, int capaPiso)
    {
        GameObject borde = GameObject.Find(nombre);
        if (borde == null) borde = new GameObject(nombre);

        borde.layer = capaPiso;
        var caja = borde.GetComponent<BoxCollider2D>();
        if (caja == null) caja = borde.AddComponent<BoxCollider2D>();
        caja.size = new Vector2(0.5f, 10f);
        borde.transform.position = new Vector3(x, suelo + 5f, 0f);
    }

    static void CrearCajas(float suelo, int capaPiso)
    {
        GameObject padre = GameObject.Find("Cajas de la plaza");
        if (padre == null) padre = new GameObject("Cajas de la plaza");

        // se rehacen cada vez para que queden siempre bien puestas
        for (int i = padre.transform.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(padre.transform.GetChild(i).gameObject);
        }

        float[] posiciones = { -7f, -3.5f, 0f, 3.5f, 7f };
        for (int i = 0; i < posiciones.Length; i++)
        {
            var caja = new GameObject("Caja " + (i + 1));
            caja.transform.SetParent(padre.transform, false);

            var sprite = caja.AddComponent<SpriteRenderer>();
            sprite.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(RutaSpriteCaja);
            sprite.sortingOrder = 3;

            var colision = caja.AddComponent<BoxCollider2D>();
            colision.size = new Vector2(1f, 1f);

            caja.layer = capaPiso;
            caja.transform.localScale = Vector3.one * 2f;
            caja.transform.position = new Vector3(posiciones[i], suelo + 1f, 0f);
        }
    }

    static void ColocarMonedas(float suelo)
    {
        foreach (MonedaPlaza moneda in Object.FindObjectsByType<MonedaPlaza>(FindObjectsInactive.Exclude))
        {
            Object.DestroyImmediate(moneda.gameObject);
        }

        GameObject prefab = CargarOCrearPrefabMoneda();

        int indice = 0;

        // fila baja, en los huecos entre las cajas
        foreach (float x in new float[] { -5.25f, -1.75f, 1.75f, 5.25f })
        {
            ColocarMoneda(prefab, x, suelo + 0.9f, ++indice);
        }

        // fila sobre las cajas
        foreach (float x in new float[] { -7f, -3.5f, 0f, 3.5f, 7f })
        {
            ColocarMoneda(prefab, x, suelo + 2.7f, ++indice);
        }

        // fila alta, se llega saltando desde las cajas
        for (int i = 0; i < 7; i++)
        {
            ColocarMoneda(prefab, -6f + i * 2f, suelo + 4.5f, ++indice);
        }
    }

    static void ColocarMoneda(GameObject prefab, float x, float y, int indice)
    {
        var moneda = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        moneda.transform.position = new Vector3(x, y, 0f);
        moneda.name = "Moneda " + indice;
    }

    static void AjustarMeta()
    {
        foreach (PuntosPlaza puntaje in Object.FindObjectsByType<PuntosPlaza>(FindObjectsInactive.Exclude))
        {
            puntaje.puntosMeta = PuntosMeta;
        }
    }

    static void CrearMusica()
    {
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(RutaMusicaHuayno);
        if (clip == null)
        {
            Debug.Log("todavia no hay musica del huayno, cuando exista el wav la agrego aca");
            return;
        }

        if (GameObject.Find("MusicaPlaza") != null) return;

        var musica = new GameObject("MusicaPlaza");
        var fuente = musica.AddComponent<AudioSource>();
        fuente.clip = clip;
        fuente.loop = true;
        musica.AddComponent<MusicaPlaza>();
    }

    static GameObject CargarOCrearPrefabMoneda()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RutaMoneda);

        if (prefab == null)
        {
            if (AssetDatabase.IsValidFolder("Assets/PrefabsPlaza") == false)
            {
                AssetDatabase.CreateFolder("Assets", "PrefabsPlaza");
            }

            var temp = new GameObject("Moneda");
            var sprite = temp.AddComponent<SpriteRenderer>();
            sprite.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(RutaSpriteMoneda);

            var caja = temp.AddComponent<BoxCollider2D>();
            caja.isTrigger = true;
            var datos = sprite.sprite;
            caja.size = new Vector2(datos.rect.width, datos.rect.height) / datos.pixelsPerUnit;

            temp.AddComponent<MonedaPlaza>();

            prefab = PrefabUtility.SaveAsPrefabAsset(temp, RutaMoneda);
            Object.DestroyImmediate(temp);
        }

        AsegurarSonidoDeMoneda(prefab);
        return prefab;
    }

    static void AsegurarSonidoDeMoneda(GameObject prefab)
    {
        var script = prefab.GetComponent<MonedaPlaza>();
        if (script == null || script.sonido != null) return;

        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(RutaSonidoMoneda);
        if (clip == null) return;

        script.sonido = clip;
        PrefabUtility.SavePrefabAsset(prefab);
    }

    static void CrearPuntaje()
    {
        if (GameObject.Find("PlazaUI") != null) return;

        var ui = new GameObject("PlazaUI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var lienzo = ui.GetComponent<Canvas>();
        lienzo.renderMode = RenderMode.ScreenSpaceOverlay;

        var escala = ui.GetComponent<CanvasScaler>();
        escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escala.referenceResolution = new Vector2(1920, 1080);

        var textoGO = new GameObject("TextoPuntos", typeof(Text));
        textoGO.transform.SetParent(ui.transform, false);

        var texto = textoGO.GetComponent<Text>();
        texto.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        texto.fontSize = 36;
        texto.color = new Color(0.15f, 0.1f, 0.05f);
        texto.alignment = TextAnchor.UpperLeft;

        var rect = textoGO.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(24f, -18f);
        rect.sizeDelta = new Vector2(800f, 60f);

        textoGO.AddComponent<PuntosPlaza>();
    }

    static void AsegurarCapaPiso()
    {
        if (LayerMask.NameToLayer(NombreCapaPiso) >= 0) return;

        var admin = new SerializedObject(AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/TagManager.asset"));
        var capas = admin.FindProperty("layers");
        for (int i = 8; i < capas.arraySize; i++)
        {
            var slot = capas.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(slot.stringValue))
            {
                slot.stringValue = NombreCapaPiso;
                admin.ApplyModifiedProperties();
                return;
            }
        }
    }

    static void Destruir(string nombre)
    {
        GameObject objeto = GameObject.Find(nombre);
        if (objeto != null) Object.DestroyImmediate(objeto);
    }

    static void Quitar<T>(GameObject objetivo) where T : Component
    {
        T componente = objetivo.GetComponent<T>();
        if (componente != null) Object.DestroyImmediate(componente);
    }
}
