using UnityEngine;

public static class ItemPalette
{
    public static Color GetColor(string id)
    {
        switch (id)
        {
            case ItemTypes.IronOre:
                return new Color(0.55f, 0.3f, 0.2f, 1f);
            case ItemTypes.CopperOre:
                return new Color(0.75f, 0.45f, 0.2f, 1f);
            case ItemTypes.Limestone:
                return new Color(0.8f, 0.8f, 0.75f, 1f);
            case ItemTypes.IronIngot:
                return new Color(0.7f, 0.7f, 0.75f, 1f);
            case ItemTypes.CopperIngot:
                return new Color(0.8f, 0.5f, 0.3f, 1f);
            case ItemTypes.Concrete:
                return new Color(0.7f, 0.7f, 0.7f, 1f);
            default:
                return new Color(0.2f, 0.9f, 1f, 1f);
        }
    }
}
