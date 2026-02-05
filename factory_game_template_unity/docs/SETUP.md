Setup (Unity)
=============

Dieser Guide zeigt, wie du die Template-Scene in wenigen Minuten
aufsetzt.

1) Neue Scene anlegen
---------------------
- File > New Scene
- Speichern als `Assets/Scenes/FactoryTemplate.unity`

2) Bootstrap-Objekt
-------------------
- Leeres GameObject: `Bootstrap`
- Script `TemplateBootstrap` hinzufuegen
- Play druecken

Das Script erstellt automatisch:
- Player mit CharacterController
- Kamera + sichtbare Haende
- BuildSystem + BuildCatalog
- HUD Overlay
- Ground Plane + Directional Light
- Beispiel Resource Nodes

3) Optional: Eigene Prefabs
---------------------------
Du kannst die automatisch erzeugten Buildables spaeter durch eigene
Prefabs ersetzen. Siehe `docs/BUILDABLES.md`.

