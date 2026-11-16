using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Una sola pantalla. Los elementos visuales y las colisiones se crean al iniciar.
public class PuenteRapido : MonoBehaviour
{
    enum Fase { Construccion, Prueba, Victoria, Derrota }
    Fase fase;
    readonly List<GameObject> tablones = new List<GameObject>();
    readonly List<int> enlaces = new List<int>();
    readonly Vector2[] puntos = new Vector2[6];
    readonly List<SpriteRenderer> indicadores = new List<SpriteRenderer>();
    Sprite cuadrado, circulo;
    SpriteRenderer exploradora;
    Sprite[] poses = new Sprite[8];
    SpriteRenderer fondo;
    Camera camara;
    Rigidbody2D personaje;
    Transform figura;
    int seleccionado = -1, intentos;
    int cursorTeclado;
    bool usandoTeclado;
    float tiempo;
    string mensaje = "Selecciona dos anclajes vecinos para colocar un tablón.";
    GUIStyle titulo, texto, pequeno, numero, boton;
    GUIStyle cartel, rotulo, subtitulo;
    Texture2D medallon, maderaCartel;
    MusicaSelva musica;
    SonidosSelva sonidos;
    readonly Color crema = new Color(.95f,.90f,.72f);
    readonly Color oro = new Color(.96f,.69f,.27f);
    readonly Color verde = new Color(.10f,.20f,.17f);
    // --- Controles de prueba ---
    bool saltoSolicitado;
    // --- Viento (destruye tablón) ---
    float tiempoVientoProximo = 6f;
    bool vientoAvisoActivo;
    int indiceTablon = -1;
    float tiempoAlertaViento;

    void Start()
    {
        cuadrado = Sprite.Create(Texture2D.whiteTexture, new Rect(0,0,1,1), Vector2.one*.5f, 1);
        var textura = new Texture2D(64,64);
        var pixeles = new Color[4096];
        for (int y=0;y<64;y++) for(int x=0;x<64;x++)
            pixeles[y*64+x] = Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f))<31 ? Color.white : Color.clear;
        textura.SetPixels(pixeles); textura.Apply();
        circulo = Sprite.Create(textura,new Rect(0,0,64,64),Vector2.one*.5f,64);
        camara = Camera.main;
        if (!camara) camara = new GameObject("Main Camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();
        camara.orthographic = true;
        camara.transform.position = new Vector3(0,1,-10);
        camara.backgroundColor = new Color(.50f,.66f,.57f);
        CrearEscenario();
        for(int i=0;i<6;i++)
        {
            puntos[i] = new Vector2(-5+i*2,0);
            Arte("Anclaje ilustrado "+(i+1),"Anclaje",puntos[i]+Vector2.down*.12f,new Vector2(.42f,.53f),7);
            Pieza("Aro de anclaje",puntos[i],Vector2.one*.34f,new Color(.22f,.23f,.17f),8,true);
            indicadores.Add(Pieza("Anclaje "+(i+1),puntos[i],Vector2.one*.21f,oro,9,true));
        }
        CrearPersonaje();
        musica=GetComponent<MusicaSelva>();
        if(!musica)musica=gameObject.AddComponent<MusicaSelva>();
        sonidos=GetComponent<SonidosSelva>();
        if(!sonidos)sonidos=gameObject.AddComponent<SonidosSelva>();
    }

    string RutaArte(string archivo)
    {
        if(archivo=="Fondo")return "Imagenes/Fondo";
        if(archivo.StartsWith("Exploradora"))return "Sprites/Personaje/"+archivo;
        return "Sprites/Entorno/"+archivo;
    }

    Sprite CargarArte(string ruta)
    {
        var importado=Resources.Load<Sprite>(ruta);
        if(importado) return importado;
        var textura=Resources.Load<Texture2D>(ruta);
        if(!textura) { Debug.LogError("Falta el asset: "+ruta); return cuadrado; }
        return Sprite.Create(textura,new Rect(0,0,textura.width,textura.height),Vector2.one*.5f,100,0,SpriteMeshType.FullRect);
    }

    SpriteRenderer Pieza(string nombre, Vector2 posicion, Vector2 escala, Color color, int orden=0, bool redonda=false)
    {
        var objeto = new GameObject(nombre);
        objeto.transform.SetParent(transform);
        objeto.transform.position=posicion;
        objeto.transform.localScale=escala;
        var renderer=objeto.AddComponent<SpriteRenderer>();
        renderer.sprite=redonda?circulo:cuadrado; renderer.color=color; renderer.sortingOrder=orden;
        return renderer;
    }

    SpriteRenderer Arte(string nombre, string archivo, Vector2 posicion, Vector2 tamano, int orden)
    {
        var sprite = CargarArte(RutaArte(archivo));
        var objeto = new GameObject(nombre); objeto.transform.SetParent(transform);
        objeto.transform.position = posicion;
        var r = objeto.AddComponent<SpriteRenderer>(); r.sprite = sprite;
        // Cada textura conserva su propio material: no reutilizar el fondo en otros sprites.
        var material = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default") ?? Shader.Find("Sprites/Default"));
        material.mainTexture = sprite.texture;
        r.sharedMaterial = material; r.sortingOrder = orden;
        objeto.transform.localScale = new Vector3(tamano.x / sprite.bounds.size.x, tamano.y / sprite.bounds.size.y, 1);
        return r;
    }

    void CrearEscenario()
    {
        fondo = Arte("Selva y cascadas", "Fondo", new Vector2(0,1), new Vector2(24,13.5f), -40);
        Arte("Orilla izquierda", "OrillaIzquierda", new Vector2(-8.3f,-1.65f), new Vector2(6.6f,6), 2);
        Arte("Orilla derecha", "OrillaDerecha", new Vector2(8.3f,-1.65f), new Vector2(6.6f,6), 2);
        for (int lado=-1; lado<=1; lado+=2)
        {
            var suelo = new GameObject("Suelo " + lado); suelo.transform.SetParent(transform);
            suelo.transform.position = new Vector2(lado*8.5f,-.28f);
            suelo.AddComponent<BoxCollider2D>().size = new Vector2(7,.4f);
        }
        Arte("Portal de las ruinas", "Portal", new Vector2(7.8f,1.7f), new Vector2(3.5f,3.5f), 4);
        Arte("Arbol de entrada", "Arbol", new Vector2(-9.25f,2.7f), new Vector2(4.6f,5.9f), 3);
        Arte("Plantas izquierda", "Plantas", new Vector2(-9.4f,-.1f), new Vector2(2.4f,2.5f), 15);
        Arte("Plantas derecha", "Plantas", new Vector2(10,-.1f), new Vector2(2.4f,2.5f), 15);
    }

    void CrearPersonaje()
    {
        var raiz=new GameObject("Exploradora"); raiz.transform.SetParent(transform); raiz.transform.position=new Vector2(-7,.65f);
        personaje=raiz.AddComponent<Rigidbody2D>(); personaje.freezeRotation=true;
        personaje.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
        personaje.bodyType=RigidbodyType2D.Kinematic;
        var collider=raiz.AddComponent<CapsuleCollider2D>(); collider.size=new Vector2(.46f,1.1f);
        figura=new GameObject("Aspecto").transform; figura.SetParent(raiz.transform,false);
        for(int i=0;i<8;i++) poses[i]=CargarArte("Sprites/Personaje/Exploradora"+i);
        exploradora=Arte("Exploradora ilustrada","Exploradora4",Vector2.zero,new Vector2(.85f,1.55f),12);
        exploradora.transform.SetParent(figura,false);
        exploradora.transform.localPosition=new Vector2(0,.225f);

    }

    void Update()
    {
        ActualizarIndicadores();
        if(fase==Fase.Prueba) DetectarInputPrueba();
        camara.orthographicSize=Mathf.Max(5.65f,10.3f/camara.aspect);
        if(fondo) {
            float escala=Mathf.Max(camara.orthographicSize*2*camara.aspect/fondo.sprite.bounds.size.x,camara.orthographicSize*2/fondo.sprite.bounds.size.y);
            fondo.transform.localScale=Vector3.one*escala;
        }
        if(exploradora)
        {
            int pose=fase==Fase.Prueba ? (personaje.position.y<.3f ? 6 : (int)(Time.time*7)%4) : fase==Fase.Victoria ? 7 : fase==Fase.Derrota ? 6 : 4;
            exploradora.sprite=poses[pose];
            exploradora.sharedMaterial.mainTexture=poses[pose].texture;
            float escala=1.55f/exploradora.sprite.bounds.size.y;
            exploradora.transform.localScale=new Vector3(escala,escala,1);
        }
    }

    void DetectarInputPrueba()
    {
        var kb = Keyboard.current;
        if(kb==null) return;
        // en suelo: no va hacia arriba (despues del salto sube con y>0)
        bool enSuelo = personaje.linearVelocity.y <= 0.5f;
        if((kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame) && enSuelo)
            saltoSolicitado = true;
        if(kb.rKey.wasPressedThisFrame){ sonidos?.Boton(); Reiniciar(false); }
    }

    void CapturarClic()
    {
        if(!camara || fase!=Fase.Construccion || Event.current.type!=EventType.MouseDown)return;
        if(Event.current.button==1){if(seleccionado>=0)sonidos?.Cancelar();seleccionado=-1;foreach(var punto in indicadores)punto.color=oro;return;}
        if(Event.current.button!=0)return;
        Vector2 raton=Event.current.mousePosition;
        Vector2 cursor=camara.ScreenToWorldPoint(new Vector3(raton.x,Screen.height-raton.y,0));
        for(int i=0;i<puntos.Length;i++) if(Vector2.Distance(cursor,puntos[i])<.45f){usandoTeclado=false;cursorTeclado=i;Elegir(i);break;}
        for(int i=0;i<indicadores.Count;i++) indicadores[i].color=i==seleccionado?Color.white:oro;
    }

    void ActualizarIndicadores()
    {
        for(int i=0;i<indicadores.Count;i++)
        {
            bool enfocado=usandoTeclado && fase==Fase.Construccion && i==cursorTeclado;
            indicadores[i].color=i==seleccionado?Color.white:enfocado?new Color(.25f,.95f,1):oro;
            indicadores[i].transform.localScale=Vector3.one*(enfocado?.30f:.21f);
        }
    }

    void CancelarSeleccion()
    {
        if(seleccionado>=0)sonidos?.Cancelar();
        seleccionado=-1;
    }

    void Deshacer()
    {
        if(fase!=Fase.Construccion || tablones.Count==0)return;
        int ultimo=tablones.Count-1;
        Destroy(tablones[ultimo]);tablones.RemoveAt(ultimo);enlaces.RemoveAt(ultimo);
        seleccionado=-1;mensaje="Último tablón retirado.";
    }

    void CapturarTeclado()
    {
        var evento=Event.current;
        if(evento.type!=EventType.KeyDown || evento.control || evento.alt || evento.command)return;
        KeyCode tecla=evento.keyCode;
        bool numero=tecla>=KeyCode.Alpha1 && tecla<=KeyCode.Alpha6;
        bool direccion=tecla==KeyCode.LeftArrow || tecla==KeyCode.RightArrow || tecla==KeyCode.A || tecla==KeyCode.D;
        bool aceptar=tecla==KeyCode.Space;
        bool probar=tecla==KeyCode.Return || tecla==KeyCode.KeypadEnter;
        bool deshacer=tecla==KeyCode.Z || tecla==KeyCode.Backspace;
        bool reiniciar=tecla==KeyCode.R;
        bool limpiar=tecla==KeyCode.C;
        bool cancelar=tecla==KeyCode.Escape;
        bool audio=tecla==KeyCode.M;
        if(!(numero || direccion || aceptar || probar || deshacer || reiniciar || limpiar || cancelar || audio))return;
        evento.Use();GUI.FocusControl(null);
        if(audio){if(musica){sonidos?.Boton();musica.Alternar();}return;}
        if(fase==Fase.Prueba)
        {
            bool saltar=tecla==KeyCode.UpArrow||tecla==KeyCode.W||tecla==KeyCode.Space;
            if(saltar && personaje.linearVelocity.y<=0.5f) saltoSolicitado=true;
            if(reiniciar){sonidos?.Boton();Reiniciar(false);}
            return;
        }
        if(limpiar){sonidos?.Boton();Reiniciar(true);return;}
        if(reiniciar){sonidos?.Boton();Reiniciar(false);return;}
        if(probar){sonidos?.Boton();if(fase==Fase.Construccion)Probar();else Reiniciar(false);return;}
        if(fase!=Fase.Construccion)return;
        if(cancelar){CancelarSeleccion();return;}
        if(deshacer){if(tablones.Count>0){sonidos?.Boton();Deshacer();}return;}
        usandoTeclado=true;
        if(direccion)
        {
            int avance=tecla==KeyCode.LeftArrow || tecla==KeyCode.A?-1:1;
            int siguiente=Mathf.Clamp(cursorTeclado+avance,0,puntos.Length-1);
            if(siguiente!=cursorTeclado)sonidos?.Boton();
            cursorTeclado=siguiente;
        }
        if(numero){cursorTeclado=(int)tecla-(int)KeyCode.Alpha1;Elegir(cursorTeclado);}
        else if(aceptar)Elegir(cursorTeclado);
        ActualizarIndicadores();
    }

    void Elegir(int indice)
    {
        if(seleccionado<0){sonidos?.Seleccionar();seleccionado=indice;mensaje="Ahora selecciona un anclaje vecino.";return;}
        if(indice==seleccionado){sonidos?.Cancelar();seleccionado=-1;return;}
        if(Mathf.Abs(indice-seleccionado)!=1){sonidos?.Error();mensaje="El tablón solo alcanza hasta el siguiente anclaje.";seleccionado=indice;return;}
        int enlace=Mathf.Min(indice,seleccionado);
        if(enlaces.Contains(enlace)){sonidos?.Error();mensaje="Ya hay un tablón entre esos anclajes.";seleccionado=-1;return;}
        if(tablones.Count>=5){sonidos?.Error();mensaje="Ya utilizaste los cinco tablones.";seleccionado=-1;return;}
        var pieza=Arte("Tablón "+(enlace+1),"Tablon",(puntos[enlace]+puntos[enlace+1])*.5f+Vector2.down*.13f,new Vector2(2.12f,.43f),6);
        var col=pieza.gameObject.AddComponent<BoxCollider2D>();
        col.size=new Vector2(2.04f/pieza.transform.localScale.x,.24f/pieza.transform.localScale.y);
        tablones.Add(pieza.gameObject);enlaces.Add(enlace);seleccionado=-1;
        sonidos?.Colocar();
        mensaje=tablones.Count==5?"¡Puente completo! Pulsa PROBAR para cruzar.":"Tablón colocado. Continúa hasta unir las dos orillas.";
    }

    void FixedUpdate()
    {
        if(fase!=Fase.Prueba)return;
        tiempo+=Time.fixedDeltaTime;

        // --- Movimiento del jugador ---
        var kb = Keyboard.current;
        bool derecha = kb!=null && (kb.rightArrowKey.isPressed || kb.dKey.isPressed);
        bool izquierda = kb!=null && (kb.leftArrowKey.isPressed || kb.aKey.isPressed);
        float velX = derecha ? 3.2f : izquierda ? -1.8f : 1.0f;
        bool enSuelo = Mathf.Abs(personaje.linearVelocity.y) < 0.2f;
        float velY = (saltoSolicitado && enSuelo) ? 8f : personaje.linearVelocity.y;
        saltoSolicitado = false;
        personaje.linearVelocity = new Vector2(velX, velY);
        if(exploradora != null) exploradora.flipX = velX < 0;
        figura.localRotation = Quaternion.Euler(0,0,Mathf.Sin(tiempo*12)*3);

        // --- Viento que destruye tablón ---
        if(!vientoAvisoActivo && tablones.Count > 0)
        {
            tiempoVientoProximo -= Time.fixedDeltaTime;
            if(tiempoVientoProximo <= 0)
            {
                indiceTablon = Random.Range(0, tablones.Count);
                vientoAvisoActivo = true;
                tiempoAlertaViento = 1.5f;
                tiempoVientoProximo = 9999f; // solo una ráfaga por intento
                mensaje = "¡RÁFAGA DE VIENTO! ¡Un tablón está cediendo!";
                sonidos?.Error();
            }
        }
        if(vientoAvisoActivo)
        {
            tiempoAlertaViento -= Time.fixedDeltaTime;
            // Sacudir y parpadear el tablón objetivo
            if(indiceTablon >= 0 && indiceTablon < tablones.Count && tablones[indiceTablon] != null)
            {
                tablones[indiceTablon].transform.eulerAngles = new Vector3(0,0,Mathf.Sin(tiempo*22f)*12f);
                var sr = tablones[indiceTablon].GetComponent<SpriteRenderer>();
                if(sr != null) sr.color = Mathf.Sin(tiempo*18f)>0 ? new Color(1f,.25f,.05f) : Color.white;
            }
            if(tiempoAlertaViento <= 0)
            {
                if(indiceTablon >= 0 && indiceTablon < tablones.Count)
                {
                    Destroy(tablones[indiceTablon]);
                    tablones.RemoveAt(indiceTablon);
                    enlaces.RemoveAt(indiceTablon);
                }
                vientoAvisoActivo = false;
                indiceTablon = -1;
                mensaje = "¡Tablón perdido! Salta el hueco con  ↑  o  W";
            }
        }

        if(personaje.position.y < -3.7f || tiempo > 35){Terminar(false);}
        else if(personaje.position.x > 7.4f && personaje.position.y > 0){Terminar(true);}
    }

    void Probar()
    {
        fase=Fase.Prueba;seleccionado=-1;intentos++;tiempo=0;
        saltoSolicitado=false;
        tiempoVientoProximo=Random.Range(5f,9f);
        vientoAvisoActivo=false; indiceTablon=-1; tiempoAlertaViento=0f;
        foreach(var punto in indicadores)punto.color=oro;
        personaje.bodyType=RigidbodyType2D.Dynamic;personaje.WakeUp();
        mensaje="¡En marcha! Usa  →/D  para ir rápido  y  ↑/W  para saltar.";
    }

    void Terminar(bool gano)
    {
        fase=gano?Fase.Victoria:Fase.Derrota;
        if(!gano && musica)musica.AlPerder();
        personaje.linearVelocity=Vector2.zero;personaje.bodyType=RigidbodyType2D.Kinematic;
        figura.localRotation=Quaternion.identity;
        mensaje=gano?"¡Llegaste a las ruinas! El puente está completo.":"Falta camino: revisa los espacios entre las orillas.";
    }

    void Reiniciar(bool borrar)
    {
        if(musica)musica.AlReintentar();
        fase=Fase.Construccion;seleccionado=-1;cursorTeclado=0;tiempo=0;
        saltoSolicitado=false; vientoAvisoActivo=false; indiceTablon=-1;
        tiempoVientoProximo=Random.Range(5f,9f);
        // Restaurar color y rotación de tablones si quedaron parpadando
        foreach(var tablon in tablones) if(tablon!=null){tablon.transform.eulerAngles=Vector3.zero;var sr=tablon.GetComponent<SpriteRenderer>();if(sr!=null)sr.color=Color.white;}
        personaje.bodyType=RigidbodyType2D.Kinematic;personaje.linearVelocity=Vector2.zero;
        personaje.position=new Vector2(-7,.65f);figura.localRotation=Quaternion.identity;
        if(borrar){foreach(var tabla in tablones)Destroy(tabla);tablones.Clear();enlaces.Clear();}
        foreach(var punto in indicadores)punto.color=oro;
        mensaje="Selecciona dos anclajes vecinos para colocar un tablón.";
    }

    void Estilos()
    {
        if(titulo!=null)return;
        titulo=new GUIStyle(GUI.skin.label){fontSize=38,fontStyle=FontStyle.Bold};titulo.normal.textColor=crema;
        texto=new GUIStyle(GUI.skin.label){fontSize=24,wordWrap=true};texto.normal.textColor=crema;
        pequeno=new GUIStyle(texto){fontSize=17};pequeno.normal.textColor=crema;
        numero=new GUIStyle(titulo){fontSize=50};
        boton=new GUIStyle(GUI.skin.button){fontSize=21,fontStyle=FontStyle.Bold};
        boton.normal.textColor=crema;boton.hover.textColor=Color.white;boton.active.textColor=oro;
        var madera=Resources.Load<Texture2D>("Sprites/Entorno/Tablon");
        boton.normal.background=madera;boton.hover.background=madera;boton.active.background=madera;
        boton.border=new RectOffset(48,48,12,12);
        var hoja=Resources.Load<Texture2D>("Imagenes/InterfazOriginal");
        var panel=RecortarInterfaz(hoja,new RectInt(25,24,770,290));
        medallon=RecortarInterfaz(hoja,new RectInt(985,0,405,350));
        maderaCartel=RecortarInterfaz(hoja,new RectInt(240,130,320,55));
        cartel=new GUIStyle { border=new RectOffset(105,105,48,48) };
        cartel.normal.background=panel ? panel : madera;
        rotulo=new GUIStyle(pequeno){alignment=TextAnchor.MiddleCenter,fontStyle=FontStyle.Bold,fontSize=20};
        rotulo.normal.textColor=oro;
        subtitulo=new GUIStyle(pequeno){alignment=TextAnchor.MiddleCenter,fontSize=18};
        subtitulo.normal.textColor=crema;
        titulo.font=Font.CreateDynamicFontFromOSFont("Georgia",38);
        titulo.alignment=TextAnchor.MiddleCenter;
        titulo.normal.textColor=new Color(1,.91f,.60f);
        numero.alignment=TextAnchor.MiddleCenter;
    }

    Texture2D RecortarInterfaz(Texture2D hoja,RectInt zona)
    {
        if(!hoja || !hoja.isReadable)return null;
        var pixeles=hoja.GetPixels(zona.x,zona.y,zona.width,zona.height);
        for(int i=0;i<pixeles.Length;i++) pixeles[i].a=Mathf.InverseLerp(.68f,.97f,pixeles[i].a);
        var textura=new Texture2D(zona.width,zona.height,TextureFormat.RGBA32,false);
        textura.SetPixels(pixeles);textura.Apply();textura.wrapMode=TextureWrapMode.Clamp;
        return textura;
    }

    void Cartel(Rect rect)
    {
        var anterior=GUI.color;
        GUI.color=new Color(0,0,0,.42f);
        GUI.Box(new Rect(rect.x+3,rect.y+6,rect.width,rect.height),GUIContent.none,cartel);
        GUI.color=anterior;
        GUI.Box(rect,GUIContent.none,cartel);
        // La zona de lectura es sólida; las hojas no cuentan como espacio para texto.
        var interior=new Rect(rect.x+24,rect.y+20,rect.width-48,rect.height-40);
        Caja(interior,new Color(.24f,.105f,.035f));
        if(maderaCartel)GUI.DrawTexture(interior,maderaCartel,ScaleMode.StretchToFill);
        Caja(interior,new Color(.035f,.018f,.009f,.64f));
        Color borde=new Color(.81f,.58f,.20f);
        Caja(new Rect(interior.x,interior.y,interior.width,2),borde);
        Caja(new Rect(interior.x,interior.yMax-2,interior.width,2),borde);
        Caja(new Rect(interior.x,interior.y,2,interior.height),borde);
        Caja(new Rect(interior.xMax-2,interior.y,2,interior.height),borde);
    }

    void Etiqueta(Rect rect,string contenido,GUIStyle estilo)
    {
        var ajustado=new GUIStyle(estilo){clipping=TextClipping.Clip,padding=new RectOffset(0,0,0,0)};
        var valor=new GUIContent(contenido);
        while(ajustado.fontSize>12 &&
            (ajustado.CalcHeight(valor,rect.width)>rect.height ||
             (!ajustado.wordWrap && ajustado.CalcSize(valor).x>rect.width))) ajustado.fontSize--;
        GUI.Label(rect,valor,ajustado);
    }

    void TituloTallado(Rect rect,string etiqueta)
    {
        var anterior=GUI.color;GUI.color=new Color(.12f,.07f,.02f,.9f);
        Etiqueta(new Rect(rect.x+2,rect.y+3,rect.width-2,rect.height-3),etiqueta,titulo);
        GUI.color=anterior;Etiqueta(rect,etiqueta,titulo);
    }

    void Caja(Rect rect,Color color)
    {
        Color anterior=GUI.color;GUI.color=color;GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=anterior;
    }

    bool Boton(Rect rect,string etiqueta,Color color)
    {
        Color anterior=GUI.backgroundColor;GUI.backgroundColor=new Color(.42f,.32f,.23f);bool pulso=GUI.Button(rect,etiqueta,boton);GUI.backgroundColor=anterior;if(pulso)sonidos?.Boton();return pulso;
    }

    void OnGUI()
    {
        Estilos();
        CapturarTeclado();
        CapturarClic();
        GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(Screen.width/1440f,Screen.height/900f,1));
        if(musica && Boton(new Rect(880,54,160,52),musica.Silenciada?"MÚSICA: OFF":"MÚSICA: ON",crema))musica.Alternar();
        Cartel(new Rect(22,4,850,150));
        if(medallon)GUI.DrawTexture(new Rect(42,30,90,90),medallon,ScaleMode.ScaleToFit);
        TituloTallado(new Rect(130,37,690,55),"EL PUENTE DE LA SELVA");
        Etiqueta(new Rect(130,94,690,30),"F L U X L A B   /   CONSTRUYE · PRUEBA · CRUZA",subtitulo);
        Cartel(new Rect(1050,4,370,150));
        Etiqueta(new Rect(1080,40,310,35),"01 / RUINAS PERDIDAS",rotulo);
        Etiqueta(new Rect(1080,82,310,48),"UNA AVENTURA ENTRE CASCADAS",subtitulo);
        Cartel(new Rect(24,170,300,230));
        Etiqueta(new Rect(60,205,230,32),"TUS MATERIALES",rotulo);
        Etiqueta(new Rect(60,242,230,66),(5-tablones.Count).ToString("00")+" / 05",numero);
        Etiqueta(new Rect(60,312,230,32),"tablones disponibles",subtitulo);
        for(int i=0;i<5;i++)Caja(new Rect(76+i*40,355,28,6),i<5-tablones.Count?oro:new Color(.25f,.31f,.25f));
        Cartel(new Rect(1060,170,360,175));
        Etiqueta(new Rect(1090,208,300,34),fase==Fase.Construccion?"PREPARA EL CAMINO":fase==Fase.Prueba?"¡EN MARCHA!":"FIN DEL RECORRIDO",rotulo);
        Etiqueta(new Rect(1090,251,300,32),"INTENTOS  "+intentos.ToString("00"),subtitulo);
        Cartel(new Rect(22,728,1398,170));
        Etiqueta(new Rect(70,768,755,65),mensaje,texto);
        string pista=fase==Fase.Prueba
            ?"↑/W: SALTAR   →/D: IR RÁPIDO   ←/A: RETROCEDER   R: Reintentar"
            :"FLECHAS / A D: mover - ESPACIO / 1-6: elegir - ENTER: probar\nZ: deshacer - R: reintentar - C: limpiar - ESC: cancelar - M: musica";
        Etiqueta(new Rect(70,831,755,50),pista,pequeno);
        GUI.enabled=fase==Fase.Construccion && tablones.Count>0;
        if(Boton(new Rect(865,779,170,72),"DESHACER",crema))
        Deshacer();
        GUI.enabled=fase==Fase.Construccion;
        if(Boton(new Rect(1045,779,150,72),"LIMPIAR",crema))Reiniciar(true);
        GUI.enabled=true;
        if(Boton(new Rect(1205,779,175,72),fase==Fase.Construccion?"PROBAR  →":"REINTENTAR",oro))
        {if(fase==Fase.Construccion)Probar();else Reiniciar(false);}
        GUI.enabled=true;
        // --- Aviso de viento ---
        if(vientoAvisoActivo)
        {
            // Ráfagas visuales: líneas que vuelan de izquierda a derecha
            for(int i=0;i<12;i++)
            {
                float vel=650f+i*90f;
                float x=(Time.time*vel+i*160f)%1750f-200f;
                float y=160f+i*52f;
                float w=65f+(i%4)*40f;
                float h=2f+(i%3);
                Caja(new Rect(x,y,w,h),new Color(.82f,.91f,1f,.35f+(i%3)*.15f));
            }
            // Cartel de alerta
            float pulso=Mathf.Abs(Mathf.Sin(Time.time*8f));
            Caja(new Rect(422,157,596,68),new Color(.28f,.04f,.01f,.90f));
            var ant=GUI.color;
            GUI.color=new Color(1f,.35f+pulso*.3f,.08f,.88f+pulso*.12f);
            Etiqueta(new Rect(430,162,580,60),">>>  ¡ R Á F A G A !  >>>",titulo);
            GUI.color=ant;
        }
        // --- Instrucciones al inicio de la prueba ---
        if(fase==Fase.Prueba && tiempo<4f)
        {
            float alpha=tiempo<3f?1f:(4f-tiempo);
            var ant=GUI.color;GUI.color=new Color(1,1,1,alpha);
            Cartel(new Rect(435,195,570,220));
            Etiqueta(new Rect(465,222,510,32),"CONTROLES DE CRUCE",rotulo);
            Etiqueta(new Rect(465,262,510,34),"↑  /  W    →    SALTAR",texto);
            Etiqueta(new Rect(465,300,510,34),"→  /  D    →    IR RÁPIDO",texto);
            Etiqueta(new Rect(465,338,510,34),"←  /  A    →    RETROCEDER",texto);
            Etiqueta(new Rect(465,376,510,24),"El viento puede arrancar un tablón — ¡prepárate!",pequeno);
            GUI.color=ant;
        }
        if(fase==Fase.Victoria || fase==Fase.Derrota)
        {
            Cartel(new Rect(330,255,780,280));
            if(medallon)GUI.DrawTexture(new Rect(675,224,90,90),medallon,ScaleMode.ScaleToFit);
            TituloTallado(new Rect(365,320,710,60),fase==Fase.Victoria?"¡CRUCE COMPLETADO!":"¡LA EXPLORADORA CAYÓ!");
            Etiqueta(new Rect(405,395,630,85),fase==Fase.Victoria?"Tu exploradora llegó a salvo. ¡Buen trabajo!":"El viento derribó un tablón. Usa  ↑ / W  para saltar el hueco.\nPulsa REINTENTAR e intenta cruzar de nuevo.",texto);
        }
        GUI.matrix=Matrix4x4.identity;
    }
}


