using UnityEngine;

public class HudOverlay : MonoBehaviour
{
    public BuildSystem buildSystem;
    public BuildWallet wallet;
    public string title = "Factory Game Template";
    public Vector2 padding = new Vector2(16f, 16f);

    private void OnGUI()
    {
        Rect rect = new Rect(padding.x, padding.y, 440f, 230f);
        GUILayout.BeginArea(rect, GUI.skin.box);
        GUILayout.Label(title);

        if (buildSystem != null)
        {
            GUILayout.Label("Build Mode: " + (buildSystem.IsBuildMode ? "ON" : "OFF"));
            GUILayout.Label("Selected: " + (string.IsNullOrEmpty(buildSystem.SelectedId) ? "-" : buildSystem.SelectedId));
            GUILayout.Label("Costs: " + FormatCosts(buildSystem.GetSelectedCosts()));
            if (wallet != null && buildSystem.requireCosts)
            {
                bool affordable = wallet.CanAfford(buildSystem.GetSelectedCosts());
                GUILayout.Label("Affordable: " + (affordable ? "Yes" : "No"));
            }
        }

        if (wallet != null)
        {
            GUILayout.Label("Wallet: " + wallet.GetSummary());
        }

        GUILayout.Space(6f);
        GUILayout.Label("B: Build Mode  Q/E: Switch  Z/C: Rotate");
        GUILayout.Label("LMB: Place  F: Interact  Esc: Unlock Mouse");
        GUILayout.Label("F5: Save  F9: Load");
        GUILayout.EndArea();

        if (buildSystem != null && buildSystem.IsBuildMode)
        {
            DrawBuildCatalog();
        }
    }

    private void DrawBuildCatalog()
    {
        string[] ids = buildSystem.GetIds();
        if (ids == null || ids.Length == 0)
        {
            return;
        }

        float width = 240f;
        float height = Mathf.Min(320f, 60f + ids.Length * 30f);
        Rect rect = new Rect(Screen.width - width - padding.x, padding.y, width, height);
        GUILayout.BeginArea(rect, GUI.skin.box);
        GUILayout.Label("Build Catalog");

        foreach (string id in ids)
        {
            bool selected = id == buildSystem.SelectedId;
            string label = selected ? "> " + id : id;
            if (GUILayout.Button(label))
            {
                buildSystem.SelectById(id);
            }
        }

        GUILayout.EndArea();
    }

    private static string FormatCosts(System.Collections.Generic.List<BuildCost> costs)
    {
        if (costs == null || costs.Count == 0)
        {
            return "Free";
        }

        string text = string.Empty;
        for (int i = 0; i < costs.Count; i++)
        {
            BuildCost cost = costs[i];
            if (i > 0)
            {
                text += ", ";
            }
            text += cost.id + " x" + cost.amount;
        }
        return text;
    }
}
