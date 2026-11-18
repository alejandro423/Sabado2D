using UnityEngine;

[ExecuteAlways, RequireComponent(typeof(Camera))]
public class PelotaEncuadreSimple : MonoBehaviour
{
    void LateUpdate()
    {
        var camara = GetComponent<Camera>();
        camara.orthographic = true;
        // Todo el recinto de 16.4 x 6.2 unidades permanece visible.
        camara.orthographicSize = Mathf.Max(4.6f, 9f / Mathf.Max(0.1f, camara.aspect));
        transform.position = new Vector3(0,0,-10);
        transform.rotation = Quaternion.identity;
    }
}
