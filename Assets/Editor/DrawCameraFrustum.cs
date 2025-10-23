using UnityEditor.Rendering.Canvas.ShaderGraph;
using UnityEngine;

public class DrawCameraFrustum : MonoBehaviour
{
    [SerializeField] private Color gizmoColor;
    [SerializeField] private Camera cam;

    private void OnDrawGizmos()
    {
        var oldColor = Gizmos.color;
        Gizmos.color = gizmoColor;

        var height = cam.orthographicSize * 2;
        var width = 16 * height / 9;

        Gizmos.DrawWireCube(cam.transform.position, new Vector3(width, height, 0));

        Gizmos.color = oldColor;
    }
}
