using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Persona 4 - Grupo5: detectar la llegada a la meta, mostrar "¡Ganaste!" y permitir reiniciar.
// Ponerlo en el objeto "CentroMeta" (posición 0, 7.2, 0) de la escena Grupo5.
// Funciona de dos formas:
//  1) Por posición: cuando el auto llega a la Y de la meta (el auto actual no tiene collider).
//  2) Por trigger: si el auto tiene Collider2D + Rigidbody2D (lo que agrega la parte de conos).
public class MetaVictoria : MonoBehaviour
{
    [Tooltip("Auto del jugador. Si se deja vacío se busca 'Auto jugador' o el objeto con tag Player.")]
    [SerializeField] private Transform auto;
    [SerializeField] private float margen = 0.05f;
    [SerializeField] private Vector2 tamanoTrigger = new Vector2(8f, 1f);

    private bool gano;
    private GUIStyle estiloTitulo;
    private GUIStyle estiloBoton;

    private void Awake()
    {
        if (auto == null)
        {
            GameObject encontrado = GameObject.Find("Auto jugador");
            if (encontrado == null)
            {
                try { encontrado = GameObject.FindWithTag("Player"); } catch (UnityException) { }
            }
            if (encontrado != null)
                auto = encontrado.transform;
        }

        // Trigger sobre la franja de cuadros (Y de 6.7 a 7.7)
        BoxCollider2D trigger = GetComponent<BoxCollider2D>();
        if (trigger == null)
            trigger = gameObject.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = tamanoTrigger;
    }

    private void Update()
    {
        if (!gano && auto != null && auto.position.y >= transform.position.y - margen)
            Ganar();

        if (gano && Keyboard.current != null &&
            (Keyboard.current.rKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
            Reiniciar();
    }

    private void OnTriggerEnter2D(Collider2D otro)
    {
        if (gano)
            return;
        if ((auto != null && otro.transform.IsChildOf(auto)) || otro.CompareTag("Player"))
            Ganar();
    }

    private void Ganar()
    {
        gano = true;
        Debug.Log("¡Ganaste!");
    }

    public void Reiniciar()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnGUI()
    {
        if (!gano)
            return;

        if (estiloTitulo == null)
        {
            estiloTitulo = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            estiloTitulo.normal.textColor = Color.yellow;
            estiloBoton = new GUIStyle(GUI.skin.button);
        }
        estiloTitulo.fontSize = Mathf.RoundToInt(Screen.height * 0.12f);
        estiloBoton.fontSize = Mathf.RoundToInt(Screen.height * 0.04f);

        GUI.color = new Color(0f, 0f, 0f, 0.6f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float ancho = Screen.width;
        float alto = Screen.height;
        GUI.Label(new Rect(0, alto * 0.25f, ancho, alto * 0.2f), "¡Ganaste!", estiloTitulo);

        Rect boton = new Rect(ancho * 0.5f - alto * 0.2f, alto * 0.55f, alto * 0.4f, alto * 0.1f);
        if (GUI.Button(boton, "Reiniciar (R)", estiloBoton))
            Reiniciar();
    }
}
