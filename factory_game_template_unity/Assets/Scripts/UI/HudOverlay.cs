using UnityEngine;

public class HudOverlay : MonoBehaviour
{
    public BuildSystem buildSystem;
    public string title = "Factory Game Template";
    public Vector2 padding = new Vector2(16f, 16f);

    private void OnGUI()
    {
        Rect rect = new Rect(padding.x, padding.y, 420f, 180f);
        GUILayout.BeginArea(rect, GUI.skin.box);
        GUILayout.Label(title);

        if (buildSystem != null)
        {
            GUILayout.Label("Build Mode: " + (buildSystem.IsBuildMode ? "ON" : "OFF"));
            GUILayout.Label("Selected: " + (string.IsNullOrEmpty(buildSystem.SelectedId) ? "-" : buildSystem.SelectedId));
        }

        GUILayout.Space(6f);
        GUILayout.Label("B: Build Mode  Q/E: Switch  Z/C: Rotate");
        GUILayout.Label("LMB: Place  F: Interact  Esc: Unlock Mouse");
        GUILayout.EndArea();
    }
}
