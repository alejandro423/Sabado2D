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

        float alturaPies = PrepararJugadora(cholita, capaPiso);
        CrearPiso(alturaPies, capaPiso);
        CrearBordes(alturaPies, capaPiso);
        ColocarMonedas(alturaPies);
        CrearPuntaje();

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

        GameObject cholita = GameObject.Find("Cholita Paceña");
        if (cholita != null)
        {
            Quitar<MovimientoPlaza>(cholita);
            Quitar<BoxCollider2D>(cholita);
            Quitar<Rigidbody2D>(cholita);
            Transform pies = cholita.transform.Find("Pies");
            if (pies != null) Object.DestroyImmediate(pies.gameObject);
        }

        EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("plaza limpia");
    }

    static float PrepararJugadora(GameObject cholita, int capaPiso)
    {
        SpriteRenderer sprite = cholita.GetComponent<SpriteRenderer>();
        var datos = sprite.sprite;

        float ancho = datos.rect.width / datos.pixelsPerUnit;
        float alto = datos.rect.height / datos.pixelsPerUnit;
        var pivote = new Vector2(datos.pivot.x / datos.pixelsPerUnit, datos.pivot.y / datos.pixelsPerUnit);

        var caja = cholita.GetComponent<BoxCollider2D>();
        if (caja == null) caja = cholita.AddComponent<BoxCollider2D>();
        caja.size = new Vector2(ancho * 0.8f, alto * 0.9f);
        caja.offset = new Vector2(pivote.x - ancho / 2f, pivote.y - alto / 2f);

        var cuerpo = cholita.GetComponent<Rigidbody2D>();
        if (cuerpo == null) cuerpo = cholita.AddComponent<Rigidbody2D>();
        cuerpo.freezeRotation = true;
        cuerpo.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var movimiento = cholita.GetComponent<MovimientoPlaza>();
        if (movimiento == null) movimiento = cholita.AddComponent<MovimientoPlaza>();
        movimiento.velocidad = 4f;
        movimiento.fuerzaSalto = 7f;
        movimiento.capaSuelo = LayerMask.GetMask(NombreCapaPiso);

        Transform pies = cholita.transform.Find("Pies");
        if (pies == null)
        {
            pies = new GameObject("Pies").transform;
            pies.SetParent(cholita.transform, false);
        }
        pies.localPosition = new Vector3(caja.offset.x, caja.offset.y - caja.size.y / 2f, 0f);
        movimiento.pies = pies;

        return cholita.transform.position.y - caja.offset.y + caja.size.y / 2f - caja.size.y;
    }

    static void CrearPiso(float alturaPies, int capaPiso)
    {
        GameObject piso = GameObject.Find("Piso de la plaza");
        if (piso == null) piso = new GameObject("Piso de la plaza");

        piso.layer = capaPiso;
        var caja = piso.GetComponent<BoxCollider2D>();
        if (caja == null) caja = piso.AddComponent<BoxCollider2D>();
        caja.size = new Vector2(40f, 0.5f);
        piso.transform.position = new Vector3(0f, alturaPies - 0.25f, 0f);
    }

    static void CrearBordes(float alturaPies, int capaPiso)
    {
        CrearBorde("Borde izquierdo", -20.25f, alturaPies, capaPiso);
        CrearBorde("Borde derecho", 20.25f, alturaPies, capaPiso);
    }

    static void CrearBorde(string nombre, float x, float alturaPies, int capaPiso)
    {
        GameObject borde = GameObject.Find(nombre);
        if (borde == null) borde = new GameObject(nombre);

        borde.layer = capaPiso;
        var caja = borde.GetComponent<BoxCollider2D>();
        if (caja == null) caja = borde.AddComponent<BoxCollider2D>();
        caja.size = new Vector2(0.5f, 10f);
        borde.transform.position = new Vector3(x, alturaPies + 5f, 0f);
    }

    static void ColocarMonedas(float alturaPies)
    {
        foreach (MonedaPlaza moneda in Object.FindObjectsByType<MonedaPlaza>(FindObjectsInactive.Exclude))
        {
            Object.DestroyImmediate(moneda.gameObject);
        }

        GameObject prefab = CargarOCrearPrefabMoneda();

        int indice = 0;
        for (int fila = 0; fila < 2; fila++)
        {
            for (int i = 0; i < 5; i++)
            {
                var moneda = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                float x = -6f + i * 3f + (fila == 1 ? 1.5f : 0f);
                float y = alturaPies + (fila == 0 ? 0.7f : 1.9f);
                moneda.transform.position = new Vector3(x, y, 0f);
                moneda.name = "Moneda " + (++indice);
            }
        }
    }

    static GameObject CargarOCrearPrefabMoneda()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RutaMoneda);
        if (prefab != null) return prefab;

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
        return prefab;
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
