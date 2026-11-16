using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Hace que el gatito tome cafe al presionar ENTER o ESPACIO.
/// Pon este script en un GameObject que tenga un SpriteRenderer.
/// Si no le asignas sprites a mano, los carga solos desde Assets/Resources/GatoCafe/
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class GatoCafeAnimator : MonoBehaviour
{
    [Header("Sprites (opcional: si queda vacio se cargan de Resources/GatoCafe)")]
    public Sprite[] frames;

    [Header("Animacion")]
    [Tooltip("Indices de frames (empiezan en 0) en el orden en que se reproducen al tomar cafe.")]
    public int[] sequence = { 1, 2, 3, 3, 2, 4, 5, 5, 6, 7 };

    [Tooltip("Frame que se muestra en reposo (cuando no esta tomando cafe).")]
    public int idleFrame = 0;

    [Tooltip("Segundos que dura cada frame.")]
    public float frameDuration = 0.15f;

    [Tooltip("Si es true, ignora ENTER/ESPACIO mientras la animacion esta corriendo.")]
    public bool ignoreInputWhilePlaying = true;

    SpriteRenderer sr;
    bool playing;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        if (frames == null || frames.Length == 0)
        {
            frames = Resources.LoadAll<Sprite>("GatoCafe");
            System.Array.Sort(frames, (a, b) => string.CompareOrdinal(a.name, b.name));
        }

        if (frames.Length == 0)
            Debug.LogWarning("GatoCafeAnimator: no hay sprites. Falta la carpeta Assets/Resources/GatoCafe (rama de Said) o asignarlos en el Inspector.");
        else
            ShowFrame(idleFrame);
    }

    void Update()
    {
        if (PressedKey() && !(playing && ignoreInputWhilePlaying))
            PlayDrinkAnimation();
    }

    bool PressedKey()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb == null) return false;
        return kb.enterKey.wasPressedThisFrame
            || kb.numpadEnterKey.wasPressedThisFrame
            || kb.spaceKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Return)
            || Input.GetKeyDown(KeyCode.KeypadEnter)
            || Input.GetKeyDown(KeyCode.Space);
#endif
    }

    public void PlayDrinkAnimation()
    {
        if (frames == null || frames.Length == 0) return;
        StopAllCoroutines();
        StartCoroutine(DrinkRoutine());
    }

    IEnumerator DrinkRoutine()
    {
        playing = true;
        var wait = new WaitForSeconds(frameDuration);
        foreach (int i in sequence)
        {
            ShowFrame(i);
            yield return wait;
        }
        ShowFrame(idleFrame);
        playing = false;
    }

    void ShowFrame(int i)
    {
        if (i < 0 || i >= frames.Length) return;
        sr.sprite = frames[i];
    }

    // ---------- Auto-creacion: solo dale Play en cualquier escena ----------
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoCrear()
    {
#if UNITY_2023_1_OR_NEWER
        if (FindFirstObjectByType<GatoCafeAnimator>() != null) return;
#else
        if (FindObjectOfType<GatoCafeAnimator>() != null) return;
#endif
        var cam = Camera.main;
        if (cam == null)
        {
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            cam = camGo.AddComponent<Camera>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 2.2f;
        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.transform.rotation = Quaternion.identity;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.97f, 0.93f, 0.85f);

        var go = new GameObject("GatoCafe");
        go.transform.position = new Vector3(0f, -1.8f, 0f);
        go.AddComponent<SpriteRenderer>();
        go.AddComponent<GatoCafeAnimator>();
    }

    void OnGUI()
    {
        var style = new GUIStyle(GUI.skin.label) { fontSize = 22, alignment = TextAnchor.UpperCenter };
        GUI.Label(new Rect(0, 10, Screen.width, 40), "Presiona ENTER o ESPACIO", style);
    }
}
