using System.Collections.Generic;
using UnityEngine;

// Pantalla 1: Construcción estructural (Lemming logic + Terreno Exacto + Colapso Físico + UI 25 piezas)
public class PuenteRapido : MonoBehaviour
{
    enum Fase { Construccion, Prueba, Victoria, Derrota }
    Fase fase;

    struct TablonData
    {
        public GameObject obj;
        public SpriteRenderer renderer;
        public BoxCollider2D col;
        public Rigidbody2D rb;
        public HingeJoint2D jointA;
        public HingeJoint2D jointB;
        public int nodoA;
        public int nodoB;
        public bool esCalzada;
    }

    const int MAX_TABLONES = 25; // Límite ampliado para permitir estructuras complejas
    const int TOTAL_NODOS = 14;
    const float DISTANCIA_MAX_TABLON = 2.75f;
    const float FUERZA_RUPTURA = 185f;

    readonly List<TablonData> tablones = new List<TablonData>();
    readonly List<Vector2Int> enlaces = new List<Vector2Int>();
    readonly Vector2[] puntos = new Vector2[TOTAL_NODOS];
    readonly bool[] nodoFijo = new bool[TOTAL_NODOS];
    readonly Rigidbody2D[] cuerposNodos = new Rigidbody2D[TOTAL_NODOS];
    readonly List<SpriteRenderer> indicadores = new List<SpriteRenderer>();
    readonly List<SpriteRenderer> aros = new List<SpriteRenderer>();

    Sprite cuadrado, circulo;
    SpriteRenderer exploradora;
    Sprite[] poses = new Sprite[8];
    SpriteRenderer fondo;
    Camera camara;
    Rigidbody2D personaje;
    CapsuleCollider2D colPersonaje;
    Transform figura;
    int seleccionado = -1, intentos;
    int cursorTeclado;
    bool usandoTeclado;
    bool puenteColapso;
    float tiempo;
    string mensaje = "Une los anclajes y triangula arriba para soportar el peso.";
    GUIStyle titulo, texto, pequeno, numero, boton;
    GUIStyle cartel, rotulo, subtitulo;
    Texture2D medallon, maderaCartel;
    MusicaSelva musica;
    SonidosSelva sonidos;
    readonly Color crema = new Color(.95f, .90f, .72f);
    readonly Color oro = new Color(.96f, .69f, .27f);
    readonly Color cianCercha = new Color(.35f, .88f, .95f);
    PhysicsMaterial2D materialFriccion;

    void Start()
    {
        materialFriccion = new PhysicsMaterial2D("FriccionPuente")
        {
            friction = 0.95f,
            bounciness = 0.0f
        };

        cuadrado = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * .5f, 1);
        var textura = new Texture2D(64, 64);
        var pixeles = new Color[4096];
        for (int y = 0; y < 64; y++)
            for (int x = 0; x < 64; x++)
                pixeles[y * 64 + x] = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f)) < 31 ? Color.white : Color.clear;
        textura.SetPixels(pixeles);
        textura.Apply();
        circulo = Sprite.Create(textura, new Rect(0, 0, 64, 64), Vector2.one * .5f, 64);

        camara = Camera.main;
        if (!camara) camara = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
        camara.orthographic = true;
        camara.transform.position = new Vector3(0, 1, -10);
        camara.backgroundColor = new Color(.50f, .66f, .57f);

        CrearEscenario();
        ConfigurarNodosEstructurales();
        CrearPersonaje();

        musica = GetComponent<MusicaSelva>();
        if (!musica) musica = gameObject.AddComponent<MusicaSelva>();
        sonidos = GetComponent<SonidosSelva>();
        if (!sonidos) sonidos = gameObject.AddComponent<SonidosSelva>();
    }

    void ConfigurarNodosEstructurales()
    {
        for (int i = 0; i < 6; i++)
        {
            puntos[i] = new Vector2(-5f + i * 2f, 0f);
            nodoFijo[i] = (i == 0 || i == 5);
        }

        for (int i = 0; i < 5; i++)
        {
            puntos[6 + i] = new Vector2(-4f + i * 2f, 1.65f);
            nodoFijo[6 + i] = true;
        }

        for (int i = 0; i < 3; i++)
        {
            puntos[11 + i] = new Vector2(-2f + i * 2f, 3.15f);
            nodoFijo[11 + i] = true;
        }

        for (int i = 0; i < TOTAL_NODOS; i++)
        {
            if (i < 6)
            {
                Arte("Anclaje ilustrado " + (i + 1), "Anclaje", puntos[i] + Vector2.down * .12f, new Vector2(.42f, .53f), 7);
                var aro = Pieza("Aro de anclaje " + (i + 1), puntos[i], Vector2.one * .34f, new Color(.22f, .23f, .17f), 8, true);
                aros.Add(aro);
            }
            else
            {
                aros.Add(null);
            }

            // Nodos superiores al 100% de opacidad y mayor escala visual para nitidez
            Color colorBase = i < 6 ? oro : cianCercha;
            var ind = Pieza("Anclaje " + (i + 1), puntos[i], Vector2.one * (i < 6 ? .21f : .20f), colorBase, 9, true);
            indicadores.Add(ind);

            var rbNodo = ind.gameObject.AddComponent<Rigidbody2D>();
            rbNodo.bodyType = RigidbodyType2D.Kinematic;
            rbNodo.mass = 0.6f;
            rbNodo.linearDamping = 0.8f;
            rbNodo.angularDamping = 1.5f;
            cuerposNodos[i] = rbNodo;
        }
    }

    string RutaArte(string archivo)
    {
        if (archivo == "Fondo") return "Imagenes/Fondo";
        if (archivo.StartsWith("Exploradora")) return "Sprites/Personaje/" + archivo;
        return "Sprites/Entorno/" + archivo;
    }

    Sprite CargarArte(string ruta)
    {
        var importado = Resources.Load<Sprite>(ruta);
        if (importado) return importado;
        var textura = Resources.Load<Texture2D>(ruta);
        if (!textura) { Debug.LogError("Falta el asset: " + ruta); return cuadrado; }
        return Sprite.Create(textura, new Rect(0, 0, textura.width, textura.height), Vector2.one * .5f, 100, 0, SpriteMeshType.FullRect);
    }

    SpriteRenderer Pieza(string nombre, Vector2 posicion, Vector2 escala, Color color, int orden = 0, bool redonda = false)
    {
        var objeto = new GameObject(nombre);
        objeto.transform.SetParent(transform);
        objeto.transform.position = posicion;
        objeto.transform.localScale = escala;
        var renderer = objeto.AddComponent<SpriteRenderer>();
        renderer.sprite = redonda ? circulo : cuadrado;
        renderer.color = color;
        renderer.sortingOrder = orden;
        return renderer;
    }

    SpriteRenderer Arte(string nombre, string archivo, Vector2 posicion, Vector2 tamano, int orden)
    {
        var sprite = CargarArte(RutaArte(archivo));
        var objeto = new GameObject(nombre);
        objeto.transform.SetParent(transform);
        objeto.transform.position = posicion;
        var r = objeto.AddComponent<SpriteRenderer>();
        r.sprite = sprite;
        var material = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default") ?? Shader.Find("Sprites/Default"));
        material.mainTexture = sprite.texture;
        r.sharedMaterial = material;
        r.sortingOrder = orden;
        objeto.transform.localScale = new Vector3(tamano.x / sprite.bounds.size.x, tamano.y / sprite.bounds.size.y, 1);
        return r;
    }

    void CrearEscenario()
    {
        fondo = Arte("Selva y cascadas", "Fondo", new Vector2(0, 1), new Vector2(24, 13.5f), -40);
        Arte("Orilla izquierda", "OrillaIzquierda", new Vector2(-8.3f, -1.65f), new Vector2(6.6f, 6), 2);
        Arte("Orilla derecha", "OrillaDerecha", new Vector2(8.3f, -1.65f), new Vector2(6.6f, 6), 2);

        GameObject terrenoIzq = new GameObject("TerrenoIzquierdo");
        terrenoIzq.transform.SetParent(transform);
        var colIzq = terrenoIzq.AddComponent<EdgeCollider2D>();
        colIzq.sharedMaterial = materialFriccion;
        colIzq.points = new Vector2[] {
            new Vector2(-12f, -0.08f),
            new Vector2(-5f, -0.08f),
            new Vector2(-4.2f, -1.8f),
            new Vector2(-4.2f, -6f)
        };

        GameObject terrenoDer = new GameObject("TerrenoDerecho");
        terrenoDer.transform.SetParent(transform);
        var colDer = terrenoDer.AddComponent<EdgeCollider2D>();
        colDer.sharedMaterial = materialFriccion;
        colDer.points = new Vector2[] {
            new Vector2(4.2f, -6f),
            new Vector2(4.2f, -1.8f),
            new Vector2(5f, -0.08f),
            new Vector2(12f, -0.08f)
        };

        Arte("Portal de las ruinas", "Portal", new Vector2(7.8f, 1.7f), new Vector2(3.5f, 3.5f), 4);
        Arte("Arbol de entrada", "Arbol", new Vector2(-9.25f, 2.7f), new Vector2(4.6f, 5.9f), 3);
        Arte("Plantas izquierda", "Plantas", new Vector2(-9.4f, -.1f), new Vector2(2.4f, 2.5f), 15);
        Arte("Plantas derecha", "Plantas", new Vector2(10, -.1f), new Vector2(2.4f, 2.5f), 15);
    }

    void CrearPersonaje()
    {
        var raiz = new GameObject("Exploradora");
        raiz.transform.SetParent(transform);
        raiz.transform.position = new Vector2(-7, .65f);

        personaje = raiz.AddComponent<Rigidbody2D>();
        personaje.freezeRotation = true;
        personaje.mass = 2.8f;
        personaje.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        personaje.bodyType = RigidbodyType2D.Kinematic;

        colPersonaje = raiz.AddComponent<CapsuleCollider2D>();
        colPersonaje.size = new Vector2(.46f, 1.1f);
        colPersonaje.sharedMaterial = materialFriccion;

        figura = new GameObject("Aspecto").transform;
        figura.SetParent(raiz.transform, false);
        for (int i = 0; i < 8; i++) poses[i] = CargarArte("Sprites/Personaje/Exploradora" + i);
        exploradora = Arte("Exploradora ilustrada", "Exploradora4", Vector2.zero, new Vector2(.85f, 1.55f), 12);
        exploradora.transform.SetParent(figura, false);
        exploradora.transform.localPosition = new Vector2(0, .225f);
    }

    void Update()
    {
        ActualizarIndicadores();
        camara.orthographicSize = Mathf.Max(5.65f, 10.3f / camara.aspect);
        if (fondo)
        {
            float escala = Mathf.Max(camara.orthographicSize * 2 * camara.aspect / fondo.sprite.bounds.size.x, camara.orthographicSize * 2 / fondo.sprite.bounds.size.y);
            fondo.transform.localScale = Vector3.one * escala;
        }
        if (exploradora)
        {
            int pose = fase == Fase.Prueba ? (personaje.position.y < .3f ? 6 : (int)(Time.time * 7) % 4) : fase == Fase.Victoria ? 7 : fase == Fase.Derrota ? 6 : 4;
            exploradora.sprite = poses[pose];
            exploradora.sharedMaterial.mainTexture = poses[pose].texture;
            float escala = 1.55f / exploradora.sprite.bounds.size.y;
            exploradora.transform.localScale = new Vector3(escala, escala, 1);
        }
    }

    void CapturarClic()
    {
        if (!camara || fase != Fase.Construccion || Event.current.type != EventType.MouseDown) return;
        if (Event.current.button == 1)
        {
            CancelarSeleccion();
            return;
        }
        if (Event.current.button != 0) return;

        Vector2 raton = Event.current.mousePosition;
        Vector2 cursor = camara.ScreenToWorldPoint(new Vector3(raton.x, Screen.height - raton.y, 0));

        int mejorIndice = -1;
        float mejorDist = 0.48f;
        for (int i = 0; i < puntos.Length; i++)
        {
            float d = Vector2.Distance(cursor, puntos[i]);
            if (d < mejorDist)
            {
                mejorDist = d;
                mejorIndice = i;
            }
        }

        if (mejorIndice >= 0)
        {
            usandoTeclado = false;
            cursorTeclado = mejorIndice;
            Elegir(mejorIndice);
        }
    }

    void ActualizarIndicadores()
    {
        for (int i = 0; i < indicadores.Count; i++)
        {
            if (aros[i]) aros[i].transform.position = indicadores[i].transform.position;

            bool enfocado = usandoTeclado && fase == Fase.Construccion && i == cursorTeclado;
            bool vecinoValido = fase == Fase.Construccion && seleccionado >= 0 && i != seleccionado &&
                                Vector2.Distance(puntos[seleccionado], puntos[i]) <= DISTANCIA_MAX_TABLON;

            Color colorBase = i < 6 ? oro : cianCercha;
            indicadores[i].color = i == seleccionado ? Color.white :
                                 enfocado ? new Color(.25f, .95f, 1f, 1f) :
                                 vecinoValido ? new Color(.55f, 1f, .55f, 1f) : colorBase;

            float tamBase = i < 6 ? .21f : .20f;
            indicadores[i].transform.localScale = Vector3.one * ((enfocado || i == seleccionado) ? .29f : tamBase);
        }
    }

    void CancelarSeleccion()
    {
        if (seleccionado >= 0) sonidos?.Cancelar();
        seleccionado = -1;
    }

    void Deshacer()
    {
        if (fase != Fase.Construccion || tablones.Count == 0) return;
        int ultimo = tablones.Count - 1;
        Destroy(tablones[ultimo].obj);
        tablones.RemoveAt(ultimo);
        enlaces.RemoveAt(ultimo);
        seleccionado = -1;
        mensaje = "Última pieza retirada.";
    }

    void CapturarTeclado()
    {
        var evento = Event.current;
        if (evento.type != EventType.KeyDown || evento.control || evento.alt || evento.command) return;
        KeyCode tecla = evento.keyCode;
        bool numero = tecla >= KeyCode.Alpha1 && tecla <= KeyCode.Alpha6;
        bool direccion = tecla == KeyCode.LeftArrow || tecla == KeyCode.RightArrow || tecla == KeyCode.UpArrow || tecla == KeyCode.DownArrow ||
                         tecla == KeyCode.A || tecla == KeyCode.D || tecla == KeyCode.W || tecla == KeyCode.S;
        bool aceptar = tecla == KeyCode.Space;
        bool probar = tecla == KeyCode.Return || tecla == KeyCode.KeypadEnter;
        bool deshacer = tecla == KeyCode.Z || tecla == KeyCode.Backspace;
        bool reiniciar = tecla == KeyCode.R;
        bool limpiar = tecla == KeyCode.C;
        bool cancelar = tecla == KeyCode.Escape;
        bool audio = tecla == KeyCode.M;
        if (!(numero || direccion || aceptar || probar || deshacer || reiniciar || limpiar || cancelar || audio)) return;
        evento.Use();
        GUI.FocusControl(null);

        if (audio) { if (musica) { sonidos?.Boton(); musica.Alternar(); } return; }
        if (fase == Fase.Prueba) return;
        if (limpiar) { sonidos?.Boton(); Reiniciar(true); return; }
        if (reiniciar) { sonidos?.Boton(); Reiniciar(false); return; }
        if (probar) { sonidos?.Boton(); if (fase == Fase.Construccion) Probar(); else Reiniciar(false); return; }
        if (fase != Fase.Construccion) return;
        if (cancelar) { CancelarSeleccion(); return; }
        if (deshacer) { if (tablones.Count > 0) { sonidos?.Boton(); Deshacer(); } return; }

        usandoTeclado = true;
        if (direccion)
        {
            Vector2 dir = (tecla == KeyCode.LeftArrow || tecla == KeyCode.A) ? Vector2.left :
                          (tecla == KeyCode.RightArrow || tecla == KeyCode.D) ? Vector2.right :
                          (tecla == KeyCode.UpArrow || tecla == KeyCode.W) ? Vector2.up : Vector2.down;
            int siguiente = BuscarNodoEnDireccion(cursorTeclado, dir);
            if (siguiente != cursorTeclado) sonidos?.Boton();
            cursorTeclado = siguiente;
        }
        if (numero) { cursorTeclado = (int)tecla - (int)KeyCode.Alpha1; Elegir(cursorTeclado); }
        else if (aceptar) Elegir(cursorTeclado);
        ActualizarIndicadores();
    }

    int BuscarNodoEnDireccion(int origen, Vector2 dir)
    {
        int mejor = origen;
        float mejorPuntaje = float.MaxValue;
        for (int i = 0; i < puntos.Length; i++)
        {
            if (i == origen) continue;
            Vector2 delta = puntos[i] - puntos[origen];
            float proyeccion = Vector2.Dot(delta.normalized, dir);
            if (proyeccion > 0.45f)
            {
                float puntaje = delta.magnitude / proyeccion;
                if (puntaje < mejorPuntaje)
                {
                    mejorPuntaje = puntaje;
                    mejor = i;
                }
            }
        }
        return mejor;
    }

    void Elegir(int indice)
    {
        if (seleccionado < 0)
        {
            sonidos?.Seleccionar();
            seleccionado = indice;
            mensaje = "Elige un nodo iluminado en verde para conectar.";
            return;
        }
        if (indice == seleccionado)
        {
            sonidos?.Cancelar();
            seleccionado = -1;
            return;
        }

        float distancia = Vector2.Distance(puntos[seleccionado], puntos[indice]);
        if (distancia > DISTANCIA_MAX_TABLON)
        {
            sonidos?.Error();
            mensaje = "Demasiado lejos: selecciona un nodo vecino más cercano.";
            seleccionado = indice;
            return;
        }

        int a = Mathf.Min(indice, seleccionado);
        int b = Mathf.Max(indice, seleccionado);
        Vector2Int par = new Vector2Int(a, b);
        if (enlaces.Contains(par))
        {
            sonidos?.Error();
            mensaje = "Ya existe una pieza entre esos dos nodos.";
            seleccionado = -1;
            return;
        }
        if (tablones.Count >= MAX_TABLONES)
        {
            sonidos?.Error();
            mensaje = "Alcanzaste el límite de " + MAX_TABLONES + " piezas.";
            seleccionado = -1;
            return;
        }

        CrearPiezaPuente(a, b);
        enlaces.Add(par);
        seleccionado = -1;
        sonidos?.Colocar();

        bool calzadaLista = CalzadaCompleta();
        bool tieneTriangulacion = tablones.Count >= 7;
        if (calzadaLista && !tieneTriangulacion)
            mensaje = "Calzada lista, pero sin soportes arriba podría colapsar por el peso.";
        else if (calzadaLista)
            mensaje = "¡Estructura triangular reforzada! Pulsa PROBAR para cruzar.";
        else
            mensaje = "Pieza colocada. Une ambas orillas y refuerza con los nodos superiores.";
    }

    void CrearPiezaPuente(int a, int b)
    {
        Vector2 posA = puntos[a];
        Vector2 posB = puntos[b];
        Vector2 centro = (posA + posB) * 0.5f;
        Vector2 dir = posB - posA;
        float largo = dir.magnitude;
        float angulo = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        bool esCalzada = (a < 6 && b < 6);
        float grosor = esCalzada ? 0.36f : 0.22f;
        Vector2 offsetVisual = esCalzada ? Vector2.down * 0.11f : Vector2.zero;

        var pieza = Arte("Pieza_" + a + "_" + b, "Tablon", centro + offsetVisual, new Vector2(largo + 0.14f, grosor), esCalzada ? 6 : 5);
        pieza.transform.rotation = Quaternion.Euler(0, 0, angulo);
        if (!esCalzada)
        {
            pieza.color = new Color(0.84f, 0.76f, 0.64f);
        }

        var col = pieza.gameObject.AddComponent<BoxCollider2D>();
        col.size = new Vector2((largo * 0.96f) / pieza.transform.localScale.x, (grosor * 0.65f) / pieza.transform.localScale.y);
        col.sharedMaterial = materialFriccion;

        if (!esCalzada)
        {
            Physics2D.IgnoreCollision(colPersonaje, col);
        }

        var rb = pieza.gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.mass = esCalzada ? 1.1f : 0.65f;
        rb.angularDamping = 2f;

        var jointA = pieza.gameObject.AddComponent<HingeJoint2D>();
        jointA.connectedBody = cuerposNodos[a];
        jointA.autoConfigureConnectedAnchor = false;
        jointA.anchor = pieza.transform.InverseTransformPoint(posA);
        jointA.connectedAnchor = Vector2.zero;
        jointA.breakForce = FUERZA_RUPTURA;

        var jointB = pieza.gameObject.AddComponent<HingeJoint2D>();
        jointB.connectedBody = cuerposNodos[b];
        jointB.autoConfigureConnectedAnchor = false;
        jointB.anchor = pieza.transform.InverseTransformPoint(posB);
        jointB.connectedAnchor = Vector2.zero;
        jointB.breakForce = FUERZA_RUPTURA;

        tablones.Add(new TablonData
        {
            obj = pieza.gameObject,
            renderer = pieza,
            col = col,
            rb = rb,
            jointA = jointA,
            jointB = jointB,
            nodoA = a,
            nodoB = b,
            esCalzada = esCalzada
        });
    }

    bool CalzadaCompleta()
    {
        for (int i = 0; i < 5; i++)
        {
            if (!enlaces.Contains(new Vector2Int(i, i + 1))) return false;
        }
        return true;
    }

    void FixedUpdate()
    {
        if (fase != Fase.Prueba) return;
        tiempo += Time.fixedDeltaTime;

        for (int i = 0; i < tablones.Count; i++)
        {
            var t = tablones[i];
            if (!t.obj) continue;

            if (t.jointA == null || t.jointB == null)
            {
                if (!puenteColapso)
                {
                    puenteColapso = true;
                    sonidos?.Error();
                }
                t.renderer.color = new Color(0.95f, 0.25f, 0.2f);
                continue;
            }

            float fuerzaA = t.jointA.GetReactionForce(Time.fixedDeltaTime).magnitude;
            float fuerzaB = t.jointB.GetReactionForce(Time.fixedDeltaTime).magnitude;
            float estres = Mathf.Clamp01(Mathf.Max(fuerzaA, fuerzaB) / FUERZA_RUPTURA);

            Color colorSano = t.esCalzada ? Color.white : new Color(0.84f, 0.76f, 0.64f);
            t.renderer.color = Color.Lerp(colorSano, new Color(1f, 0.22f, 0.18f), estres * 1.15f);
        }

        personaje.linearVelocity = new Vector2(1.8f, personaje.linearVelocity.y);
        figura.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(tiempo * 12) * 3);

        if (personaje.position.y < -3.7f)
        {
            Terminar(false, puenteColapso ? "¡El puente colapsó! Triangula con los nodos superiores." : "Falta camino: la exploradora cayó al abismo.");
        }
        else if (tiempo > 18f)
        {
            Terminar(false, "Se agotó el tiempo. Asegúrate de conectar bien ambas orillas.");
        }
        else if (personaje.position.x > 7.4f && personaje.position.y > 0f)
        {
            Terminar(true, "¡Llegaste a las ruinas! La estructura resistió el peso.");
        }
    }

    void Probar()
    {
        fase = Fase.Prueba;
        seleccionado = -1;
        puenteColapso = false;
        intentos++;
        tiempo = 0;

        for (int i = 0; i < TOTAL_NODOS; i++)
        {
            indicadores[i].color = i < 6 ? oro : cianCercha;
            if (!nodoFijo[i])
            {
                cuerposNodos[i].bodyType = RigidbodyType2D.Dynamic;
                cuerposNodos[i].WakeUp();
            }
        }

        for (int i = 0; i < tablones.Count; i++)
        {
            tablones[i].rb.bodyType = RigidbodyType2D.Dynamic;
            tablones[i].rb.WakeUp();
        }

        personaje.bodyType = RigidbodyType2D.Dynamic;
        personaje.WakeUp();
        mensaje = "Probando resistencia del puente bajo el peso…";
    }

    void Terminar(bool gano, string msgFinal)
    {
        fase = gano ? Fase.Victoria : Fase.Derrota;
        if (!gano && musica) musica.AlPerder();

        personaje.linearVelocity = Vector2.zero;
        figura.localRotation = Quaternion.identity;
        mensaje = msgFinal;
    }

    void Reiniciar(bool borrar)
    {
        if (musica) musica.AlReintentar();
        fase = Fase.Construccion;
        seleccionado = -1;
        cursorTeclado = 0;
        puenteColapso = false;
        tiempo = 0;

        personaje.bodyType = RigidbodyType2D.Kinematic;
        personaje.linearVelocity = Vector2.zero;
        personaje.position = new Vector2(-7, .65f);
        figura.localRotation = Quaternion.identity;

        for (int i = 0; i < TOTAL_NODOS; i++)
        {
            cuerposNodos[i].bodyType = RigidbodyType2D.Kinematic;
            cuerposNodos[i].linearVelocity = Vector2.zero;
            cuerposNodos[i].angularVelocity = 0f;
            indicadores[i].transform.position = puntos[i];
            if (aros[i] != null) aros[i].transform.position = puntos[i];
            indicadores[i].color = i < 6 ? oro : cianCercha;
        }

        var enlacesGuardados = new List<Vector2Int>(enlaces);
        foreach (var t in tablones)
        {
            if (t.obj) Destroy(t.obj);
        }
        tablones.Clear();
        enlaces.Clear();

        if (!borrar)
        {
            foreach (var par in enlacesGuardados)
            {
                CrearPiezaPuente(par.x, par.y);
                enlaces.Add(par);
            }
        }

        mensaje = "Une los anclajes y triangula arriba para soportar el peso.";
    }

    void Estilos()
    {
        if (titulo != null) return;
        titulo = new GUIStyle(GUI.skin.label) { fontSize = 38, fontStyle = FontStyle.Bold };
        titulo.normal.textColor = crema;
        texto = new GUIStyle(GUI.skin.label) { fontSize = 24, wordWrap = true };
        texto.normal.textColor = crema;
        pequeno = new GUIStyle(texto) { fontSize = 17 };
        pequeno.normal.textColor = crema;
        numero = new GUIStyle(titulo) { fontSize = 50 };
        boton = new GUIStyle(GUI.skin.button) { fontSize = 21, fontStyle = FontStyle.Bold };
        boton.normal.textColor = crema;
        boton.hover.textColor = Color.white;
        boton.active.textColor = oro;
        var madera = Resources.Load<Texture2D>("Sprites/Entorno/Tablon");
        boton.normal.background = madera;
        boton.hover.background = madera;
        boton.active.background = madera;
        boton.border = new RectOffset(48, 48, 12, 12);
        var hoja = Resources.Load<Texture2D>("Imagenes/InterfazOriginal");
        var panel = RecortarInterfaz(hoja, new RectInt(25, 24, 770, 290));
        medallon = RecortarInterfaz(hoja, new RectInt(985, 0, 405, 350));
        maderaCartel = RecortarInterfaz(hoja, new RectInt(240, 130, 320, 55));
        cartel = new GUIStyle { border = new RectOffset(105, 105, 48, 48) };
        cartel.normal.background = panel ? panel : madera;
        rotulo = new GUIStyle(pequeno) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 20 };
        rotulo.normal.textColor = oro;
        subtitulo = new GUIStyle(pequeno) { alignment = TextAnchor.MiddleCenter, fontSize = 18 };
        subtitulo.normal.textColor = crema;
        titulo.font = Font.CreateDynamicFontFromOSFont("Georgia", 38);
        titulo.alignment = TextAnchor.MiddleCenter;
        titulo.normal.textColor = new Color(1, .91f, .60f);
        numero.alignment = TextAnchor.MiddleCenter;
    }

    Texture2D RecortarInterfaz(Texture2D hoja, RectInt zona)
    {
        if (!hoja || !hoja.isReadable) return null;
        var pixeles = hoja.GetPixels(zona.x, zona.y, zona.width, zona.height);
        for (int i = 0; i < pixeles.Length; i++) pixeles[i].a = Mathf.InverseLerp(.68f, .97f, pixeles[i].a);
        var textura = new Texture2D(zona.width, zona.height, TextureFormat.RGBA32, false);
        textura.SetPixels(pixeles);
        textura.Apply();
        textura.wrapMode = TextureWrapMode.Clamp;
        return textura;
    }

    void Cartel(Rect rect)
    {
        var anterior = GUI.color;
        GUI.color = new Color(0, 0, 0, .42f);
        GUI.Box(new Rect(rect.x + 3, rect.y + 6, rect.width, rect.height), GUIContent.none, cartel);
        GUI.color = anterior;
        GUI.Box(rect, GUIContent.none, cartel);
        var interior = new Rect(rect.x + 24, rect.y + 20, rect.width - 48, rect.height - 40);
        Caja(interior, new Color(.24f, .105f, .035f));
        if (maderaCartel) GUI.DrawTexture(interior, maderaCartel, ScaleMode.StretchToFill);
        Caja(interior, new Color(.035f, .018f, .009f, .64f));
        Color borde = new Color(.81f, .58f, .20f);
        Caja(new Rect(interior.x, interior.y, interior.width, 2), borde);
        Caja(new Rect(interior.x, interior.yMax - 2, interior.width, 2), borde);
        Caja(new Rect(interior.x, interior.y, 2, interior.height), borde);
        Caja(new Rect(interior.xMax - 2, interior.y, 2, interior.height), borde);
    }

    void Etiqueta(Rect rect, string contenido, GUIStyle estilo)
    {
        var ajustado = new GUIStyle(estilo) { clipping = TextClipping.Clip, padding = new RectOffset(0, 0, 0, 0) };
        var valor = new GUIContent(contenido);
        while (ajustado.fontSize > 12 &&
            (ajustado.CalcHeight(valor, rect.width) > rect.height ||
             (!ajustado.wordWrap && ajustado.CalcSize(valor).x > rect.width))) ajustado.fontSize--;
        GUI.Label(rect, valor, ajustado);
    }

    void TituloTallado(Rect rect, string etiqueta)
    {
        var anterior = GUI.color;
        GUI.color = new Color(.12f, .07f, .02f, .9f);
        Etiqueta(new Rect(rect.x + 2, rect.y + 3, rect.width - 2, rect.height - 3), etiqueta, titulo);
        GUI.color = anterior;
        Etiqueta(rect, etiqueta, titulo);
    }

    void Caja(Rect rect, Color color)
    {
        Color anterior = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = anterior;
    }

    bool Boton(Rect rect, string etiqueta, Color color)
    {
        Color anterior = GUI.backgroundColor;
        GUI.backgroundColor = new Color(.42f, .32f, .23f);
        bool pulso = GUI.Button(rect, etiqueta, boton);
        GUI.backgroundColor = anterior;
        if (pulso) sonidos?.Boton();
        return pulso;
    }

    void OnGUI()
    {
        Estilos();
        CapturarTeclado();
        CapturarClic();
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width / 1440f, Screen.height / 900f, 1));

        if (musica && Boton(new Rect(880, 54, 160, 52), musica.Silenciada ? "MÚSICA: OFF" : "MÚSICA: ON", crema)) musica.Alternar();
        Cartel(new Rect(22, 4, 850, 150));
        if (medallon) GUI.DrawTexture(new Rect(42, 30, 90, 90), medallon, ScaleMode.ScaleToFit);
        TituloTallado(new Rect(130, 37, 690, 55), "EL PUENTE DE LA SELVA");
        Etiqueta(new Rect(130, 94, 690, 30), "F L U X L A B   /   CONSTRUYE · PRUEBA · CRUZA", subtitulo);

        Cartel(new Rect(1050, 4, 370, 150));
        Etiqueta(new Rect(1080, 40, 310, 35), "01 / RUINAS PERDIDAS", rotulo);
        Etiqueta(new Rect(1080, 82, 310, 48), "UNA AVENTURA ENTRE CASCADAS", subtitulo);

        Cartel(new Rect(24, 170, 300, 230));
        Etiqueta(new Rect(60, 205, 230, 32), "TUS MATERIALES", rotulo);
        int restantes = MAX_TABLONES - tablones.Count;
        Etiqueta(new Rect(60, 242, 230, 66), restantes.ToString("00") + " / " + MAX_TABLONES.ToString("00"), numero);
        Etiqueta(new Rect(60, 312, 230, 32), "piezas disponibles", subtitulo);

        // Distribución dinámica de hasta 25 maderitas en el cartel de TUS MATERIALES
        for (int i = 0; i < MAX_TABLONES; i++)
        {
            float posX = 55 + (i % 12) * 18f;
            float posY = 345 + (i / 12) * 12f;
            Caja(new Rect(posX, posY, 12f, 6), i < restantes ? oro : new Color(.25f, .31f, .25f));
        }

        Cartel(new Rect(1060, 170, 360, 175));
        Etiqueta(new Rect(1090, 208, 300, 34), fase == Fase.Construccion ? "PREPARA EL CAMINO" : fase == Fase.Prueba ? "¡EN MARCHA!" : "FIN DEL RECORRIDO", rotulo);
        Etiqueta(new Rect(1090, 251, 300, 32), "INTENTOS  " + intentos.ToString("00"), subtitulo);

        Cartel(new Rect(22, 728, 1398, 170));
        Etiqueta(new Rect(70, 768, 755, 65), mensaje, texto);
        Etiqueta(new Rect(70, 831, 755, 50), "CLIC o FLECHAS: nodo - ESPACIO: unir - ENTER: probar\nZ: deshacer - R: reintentar - C: limpiar - ESC: cancelar - M: musica", pequeno);

        GUI.enabled = fase == Fase.Construccion && tablones.Count > 0;
        if (Boton(new Rect(865, 779, 170, 72), "DESHACER", crema)) Deshacer();
        GUI.enabled = fase != Fase.Prueba;
        if (Boton(new Rect(1045, 779, 150, 72), "LIMPIAR", crema)) Reiniciar(true);
        GUI.enabled = fase != Fase.Prueba;
        if (Boton(new Rect(1205, 779, 175, 72), fase == Fase.Construccion ? "PROBAR  →" : "REINTENTAR", oro))
        {
            if (fase == Fase.Construccion) Probar();
            else Reiniciar(false);
        }
        GUI.enabled = true;

        if (fase == Fase.Victoria || fase == Fase.Derrota)
        {
            Cartel(new Rect(330, 255, 780, 280));
            if (medallon) GUI.DrawTexture(new Rect(675, 224, 90, 90), medallon, ScaleMode.ScaleToFit);
            TituloTallado(new Rect(365, 320, 710, 60), fase == Fase.Victoria ? "¡CRUCE COMPLETADO!" : "¡INTÉNTALO OTRA VEZ!");
            Etiqueta(new Rect(405, 395, 630, 85), mensaje, texto);
        }
        GUI.matrix = Matrix4x4.identity;
    }
}


