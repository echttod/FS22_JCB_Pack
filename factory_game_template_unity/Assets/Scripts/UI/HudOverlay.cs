using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HudOverlay : MonoBehaviour
{
    public BuildSystem buildSystem;
    public BuildCostProvider costProvider;
    public string title = "Factory Game Template";
    public Vector2 padding = new Vector2(16f, 16f);
    public int fontSize = 14;

    private Canvas _canvas;
    private Text _statusText;
    private Text _controlsText;
    private RectTransform _catalogPanel;
    private readonly List<CatalogButton> _catalogButtons = new List<CatalogButton>();
    private string[] _cachedIds = new string[0];

    private class CatalogButton
    {
        public string id;
        public Button button;
        public Text label;
    }

    private void Start()
    {
        EnsureEventSystem();
        BuildUI();
        RefreshCatalog();
    }

    private void Update()
    {
        UpdateStatus();
        UpdateCatalogVisibility();
        UpdateCatalogSelection();
    }

    private void BuildUI()
    {
        _canvas = CreateCanvas();

        RectTransform statusPanel = CreatePanel("StatusPanel", _canvas.transform, new Vector2(padding.x, -padding.y), new Vector2(360f, 220f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        _statusText = CreateText(statusPanel, "StatusText", string.Empty, fontSize);
        _controlsText = CreateText(statusPanel, "ControlsText", string.Empty, fontSize - 1);

        RectTransform catalogPanel = CreatePanel("CatalogPanel", _canvas.transform, new Vector2(-padding.x, -padding.y), new Vector2(240f, 320f), new Vector2(1f, 1f), new Vector2(1f, 1f));
        _catalogPanel = catalogPanel;
        CreateText(catalogPanel, "CatalogTitle", "Build Catalog", fontSize + 1);
    }

    private void UpdateStatus()
    {
        if (_statusText == null)
        {
            return;
        }

        string selected = buildSystem != null && !string.IsNullOrEmpty(buildSystem.SelectedId) ? buildSystem.SelectedId : "-";
        string costs = buildSystem != null ? FormatCosts(buildSystem.GetSelectedCosts()) : "-";
        string affordable = "Yes";
        if (buildSystem != null && buildSystem.requireCosts && costProvider != null)
        {
            affordable = costProvider.CanAfford(buildSystem.GetSelectedCosts(), buildSystem.PlacementPosition) ? "Yes" : "No";
        }

        string resources = costProvider != null ? costProvider.GetSummary() : "No depot";

        _statusText.text = title
                           + "\nBuild Mode: " + (buildSystem != null && buildSystem.IsBuildMode ? "ON" : "OFF")
                           + "\nSelected: " + selected
                           + "\nCosts: " + costs
                           + "\nAffordable: " + affordable
                           + "\nResources: " + resources;

        if (_controlsText != null)
        {
            _controlsText.text = "B: Build Mode  Q/E: Switch  Z/C: Rotate\n"
                                 + "LMB: Place  F: Interact  Esc: Unlock Mouse\n"
                                 + "F5: Save  F9: Load";
        }
    }

    private void UpdateCatalogVisibility()
    {
        if (_catalogPanel == null)
        {
            return;
        }

        bool shouldShow = buildSystem != null && buildSystem.IsBuildMode;
        if (_catalogPanel.gameObject.activeSelf != shouldShow)
        {
            _catalogPanel.gameObject.SetActive(shouldShow);
        }

        if (!shouldShow)
        {
            return;
        }

        string[] ids = buildSystem != null ? buildSystem.GetIds() : null;
        if (IdsChanged(ids))
        {
            RefreshCatalog();
        }
    }

    private void UpdateCatalogSelection()
    {
        if (_catalogButtons.Count == 0 || buildSystem == null)
        {
            return;
        }

        string selected = buildSystem.SelectedId;
        foreach (CatalogButton entry in _catalogButtons)
        {
            if (entry == null || entry.label == null)
            {
                continue;
            }

            bool isSelected = entry.id == selected;
            entry.label.text = isSelected ? "> " + entry.id : entry.id;
            entry.label.color = isSelected ? new Color(0.9f, 0.95f, 1f, 1f) : Color.white;
        }
    }

    private void RefreshCatalog()
    {
        _cachedIds = buildSystem != null ? buildSystem.GetIds() : new string[0];
        if (_catalogPanel == null)
        {
            return;
        }

        for (int i = _catalogPanel.childCount - 1; i >= 0; i--)
        {
            Transform child = _catalogPanel.GetChild(i);
            if (child.name != "CatalogTitle")
            {
                Destroy(child.gameObject);
            }
        }

        _catalogButtons.Clear();
        if (_cachedIds == null)
        {
            return;
        }

        foreach (string id in _cachedIds)
        {
            CatalogButton entry = new CatalogButton();
            entry.id = id;
            entry.button = CreateButton(_catalogPanel, id);
            entry.label = entry.button.GetComponentInChildren<Text>();
            entry.button.onClick.AddListener(() =>
            {
                if (buildSystem != null)
                {
                    buildSystem.SelectById(id);
                }
            });
            _catalogButtons.Add(entry);
        }
    }

    private bool IdsChanged(string[] ids)
    {
        if (ids == null)
        {
            return _cachedIds != null && _cachedIds.Length > 0;
        }

        if (_cachedIds == null || _cachedIds.Length != ids.Length)
        {
            return true;
        }

        for (int i = 0; i < ids.Length; i++)
        {
            if (_cachedIds[i] != ids[i])
            {
                return true;
            }
        }

        return false;
    }

    private Canvas CreateCanvas()
    {
        GameObject canvasObj = new GameObject("HudCanvas");
        canvasObj.transform.SetParent(transform, false);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private RectTransform CreatePanel(string name, Transform parent, Vector2 position, Vector2 size, Vector2 anchor, Vector2 pivot)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        Image image = panel.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.55f);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.spacing = 6f;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        return rect;
    }

    private Text CreateText(RectTransform parent, string name, string text, int size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        Text uiText = obj.AddComponent<Text>();
        uiText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        uiText.fontSize = size;
        uiText.color = Color.white;
        uiText.text = text;
        uiText.horizontalOverflow = HorizontalWrapMode.Wrap;
        uiText.verticalOverflow = VerticalWrapMode.Overflow;
        return uiText;
    }

    private Button CreateButton(RectTransform parent, string label)
    {
        GameObject buttonObj = new GameObject("Button_" + label);
        buttonObj.transform.SetParent(parent, false);
        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        LayoutElement layout = buttonObj.AddComponent<LayoutElement>();
        layout.preferredHeight = 28f;

        Button button = buttonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        colors.highlightedColor = new Color(0.25f, 0.25f, 0.25f, 0.95f);
        colors.pressedColor = new Color(0.15f, 0.15f, 0.15f, 0.95f);
        button.colors = colors;

        Text text = CreateText(buttonObj.GetComponent<RectTransform>(), "Label", label, fontSize);
        text.alignment = TextAnchor.MiddleLeft;
        text.rectTransform.anchorMin = new Vector2(0f, 0f);
        text.rectTransform.anchorMax = new Vector2(1f, 1f);
        text.rectTransform.offsetMin = new Vector2(10f, 4f);
        text.rectTransform.offsetMax = new Vector2(-10f, -4f);

        return button;
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }

    private static string FormatCosts(List<BuildCost> costs)
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
