using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// arma el nivel largo de plataformas en la plaza
// se usa desde el menu: Plaza > Armar juego de puntos
public static class ArmadorPlaza
{
    const string NombreCapaPiso = "Piso";

    const string RutaMoneda = "Assets/PrefabsPlaza/Moneda.prefab";
    const string RutaSpriteMoneda = "Assets/Art/coin.png";
    const string RutaSonidoMoneda = "Assets/SonidosPlaza/moneda.wav";
    const string RutaMusicaHuayno = "Assets/SonidosPlaza/huayno_plaza.wav";

    const string RutaSpriteCubito = "Assets/ArteBoliviano/cubito_gorrito.png";
    const string RutaSpritePlataforma = "Assets/ArteBoliviano/plataforma_madera.png";
    const string RutaFondo = "Assets/ArteBoliviano/fondo_plaza.png";
    const string RutaFondoLateral = "Assets/ArteBoliviano/fondo_plaza_lateral.png";

    // el nivel son tres pantallas pegadas una al lado de la otra
    const float AnchoSegmento = 19.2f;
    const int Segmentos = 3;
    const float BordeMundo = AnchoSegmento * Segmentos / 2f;

    // la linea del piso es la misma donde estan parados los vecinos
    const float SueloY = -3.28f;

    const float AlturaDelCubito = 2f;
    const float EscalaDelCubito = 0.45f;
    const float EscalaDeVecino = 0.262f;

    const int PuntosMeta = 240;

    [MenuItem("Plaza/Armar juego de puntos")]
    public static void Armar()
    {
        if (GameObject.Find("Cholita Paceña") == null)
        {
            Debug.LogError("no encontre a la cholita, abri la escena PlazaBoliviana primero");
            return;
        }

        AsegurarCapaPiso();
        int capaPiso = LayerMask.NameToLayer(NombreCapaPiso);

        DesarmarCholita();
        AjustarVecinos();
        GameObject cubito = PrepararCubito(capaPiso);
        PrepararCamara(cubito);
        CrearFondos();
        CrearPiso(capaPiso);
        CrearBordes(capaPiso);
        CrearPlataformas(capaPiso);
        ColocarMonedas();
        CrearPuntaje();
        AjustarMeta();
        CrearMusica();

        EditorSceneManager.MarkAllScenesDirty();
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("plaza lista, el nivel mide " + (AnchoSegmento * Segmentos) + " de largo");
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
        Destruir("Plataformas de la plaza");
        Destruir("Cajas de la plaza");
        Destruir("Cubito");
        Destruir("MusicaPlaza");
        Destruir("Fondo izquierdo");
        Destruir("Fondo derecho");

        DesarmarCholita();

        var camara = GameObject.Find("Main Camera");
        if (camara != null) Quitar<CamaraPlaza>(camara);

        EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("plaza limpia");
    }

    static void AjustarVecinos()
    {
        var vecinos = new[]
        {
            ("Cholita Paceña", "Assets/ArteBoliviano/cholita.png"),
            ("Campesino", "Assets/ArteBoliviano/campesino.png"),
            ("Vendedora de Salteñas", "Assets/ArteBoliviano/vendedora.png"),
            ("Charanguista", "Assets/ArteBoliviano/charanguista.png"),
        };

        foreach (var (nombre, ruta) in vecinos)
        {
            var vecino = GameObject.Find(nombre);
            if (vecino == null) continue;

            var render = vecino.GetComponent<SpriteRenderer>();
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
            if (render == null || sprite == null) continue;

            render.sprite = sprite;
            render.sortingOrder = 30;
            vecino.transform.localScale = Vector3.one * EscalaDeVecino;
            PararEnElSuelo(vecino, render);
        }
    }

    static GameObject PrepararCubito(int capaPiso)
    {
        GameObject cubito = GameObject.Find("Cubito");

        if (cubito == null)
        {
            cubito = new GameObject("Cubito");
            cubito.AddComponent<SpriteRenderer>();
        }

        SpriteRenderer render = cubito.GetComponent<SpriteRenderer>();
        render.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(RutaSpriteCubito);
        render.sortingOrder = 50;

        cubito.transform.localScale = Vector3.one * EscalaDelCubito;
        cubito.transform.position = new Vector3(-24f, 0f, 0f);
        PararEnElSuelo(cubito, render);

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

        return cubito;
    }

    static void PrepararCamara(GameObject cubito)
    {
        var camara = GameObject.Find("Main Camera");
        if (camara == null) return;

        var script = camara.GetComponent<CamaraPlaza>();
        if (script == null) script = camara.AddComponent<CamaraPlaza>();

        script.objetivo = cubito.transform;
        script.limiteIzquierdo = -(BordeMundo - 9.6f);
        script.limiteDerecho = BordeMundo - 9.6f;
        script.alturaFija = 0f;
    }

    static void PararEnElSuelo(GameObject objeto, SpriteRenderer render)
    {
        objeto.transform.position = new Vector3(objeto.transform.position.x, SueloY, 0f);
        float diferencia = SueloY - render.bounds.min.y;
        objeto.transform.position += new Vector3(0f, diferencia, 0f);
    }

    static void CrearFondos()
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(RutaFondo);
        if (sprite == null) return;

        var centro = GameObject.Find("Fondo");
        if (centro != null)
        {
            var render = centro.GetComponent<SpriteRenderer>();
            if (render != null) render.sprite = sprite;
        }

        // los laterales usan la plaza sin sol ni montaña, asi queda uno solo en el centro
        var lateral = AssetDatabase.LoadAssetAtPath<Sprite>(RutaFondoLateral);
        if (lateral != null)
        {
            CrearFondoLateral("Fondo izquierdo", -AnchoSegmento, lateral, true);
            CrearFondoLateral("Fondo derecho", AnchoSegmento, lateral, true);
        }
    }

    static void CrearFondoLateral(string nombre, float x, Sprite sprite, bool espejado)
    {
        var fondo = GameObject.Find(nombre);
        if (fondo == null) fondo = new GameObject(nombre);

        var render = fondo.GetComponent<SpriteRenderer>();
        if (render == null) render = fondo.AddComponent<SpriteRenderer>();
        render.sprite = sprite;
        render.sortingOrder = -100;

        var escala = fondo.transform.localScale;
        fondo.transform.localScale = new Vector3(espejado ? -Mathf.Abs(escala.x) : Mathf.Abs(escala.x), escala.y, escala.z);
        fondo.transform.position = new Vector3(x, 0f, 1f);
    }

    static void CrearPiso(int capaPiso)
    {
        GameObject piso = GameObject.Find("Piso de la plaza");
        if (piso == null) piso = new GameObject("Piso de la plaza");

        piso.layer = capaPiso;
        var caja = piso.GetComponent<BoxCollider2D>();
        if (caja == null) caja = piso.AddComponent<BoxCollider2D>();
        caja.size = new Vector2(BordeMundo * 2f + 2f, 0.5f);
        piso.transform.position = new Vector3(0f, SueloY - 0.25f, 0f);
    }

    static void CrearBordes(int capaPiso)
    {
        CrearBorde("Borde izquierdo", -BordeMundo - 0.25f, capaPiso);
        CrearBorde("Borde derecho", BordeMundo + 0.25f, capaPiso);
    }

    static void CrearBorde(string nombre, float x, int capaPiso)
    {
        GameObject borde = GameObject.Find(nombre);
        if (borde == null) borde = new GameObject(nombre);

        borde.layer = capaPiso;
        var caja = borde.GetComponent<BoxCollider2D>();
        if (caja == null) caja = borde.AddComponent<BoxCollider2D>();
        caja.size = new Vector2(0.5f, 10f);
        borde.transform.position = new Vector3(x, SueloY + 5f, 0f);
    }

    static void CrearPlataformas(int capaPiso)
    {
        GameObject padre = GameObject.Find("Plataformas de la plaza");
        if (padre == null) padre = new GameObject("Plataformas de la plaza");

        for (int i = padre.transform.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(padre.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < Segmentos; i++)
        {
            float centro = (i - 1) * AnchoSegmento;

            CrearPlataforma(padre, capaPiso, "Plataforma baja " + (i + 1), centro - 7f, 3.4f, SueloY + 1.6f);
            CrearPlataforma(padre, capaPiso, "Plataforma alta " + (i + 1), centro - 4.2f, 3.4f, SueloY + 2.9f);
        }
    }

    static void CrearPlataforma(GameObject padre, int capaPiso, string nombre, float x, float ancho, float techoY)
    {
        var plataforma = new GameObject(nombre);
        plataforma.transform.SetParent(padre.transform, false);
        plataforma.layer = capaPiso;

        var sprite = plataforma.AddComponent<SpriteRenderer>();
        sprite.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(RutaSpritePlataforma);
        sprite.sortingOrder = 3;

        var colision = plataforma.AddComponent<BoxCollider2D>();
        colision.size = new Vector2(0.96f, 0.48f);

        plataforma.transform.localScale = new Vector3(ancho / 0.96f, 1f, 1f);
        plataforma.transform.position = new Vector3(x, techoY - 0.24f, 0f);
    }

    static void ColocarMonedas()
    {
        foreach (MonedaPlaza moneda in Object.FindObjectsByType<MonedaPlaza>(FindObjectsInactive.Exclude))
        {
            Object.DestroyImmediate(moneda.gameObject);
        }

        GameObject prefab = CargarOCrearPrefabMoneda();
        int indice = 0;

        for (int i = 0; i < Segmentos; i++)
        {
            float centro = (i - 1) * AnchoSegmento;

            // en el piso, a la altura del cubito
            ColocarMoneda(prefab, centro + 3.3f, SueloY + 0.6f, ++indice);
            ColocarMoneda(prefab, centro + 6.5f, SueloY + 0.6f, ++indice);

            // sobre la plataforma baja
            foreach (float x in new float[] { centro - 7.8f, centro - 7f, centro - 6.2f })
            {
                ColocarMoneda(prefab, x, SueloY + 2.4f, ++indice);
            }

            // sobre la plataforma alta
            foreach (float x in new float[] { centro - 5f, centro - 4.2f, centro - 3.4f })
            {
                ColocarMoneda(prefab, x, SueloY + 3.7f, ++indice);
            }
        }
    }

    static void ColocarMoneda(GameObject prefab, float x, float y, int indice)
    {
        var moneda = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        moneda.transform.position = new Vector3(x, y, 0f);
        moneda.name = "Moneda " + indice;

        var render = moneda.GetComponent<SpriteRenderer>();
        if (render != null) render.sortingOrder = 60;
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
