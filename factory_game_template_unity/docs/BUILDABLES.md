Buildables hinzufuegen (Unity)
==============================

Das Template nutzt `BuildCatalog` als zentrale Liste von Buildables.
Du kannst Prefabs im Inspector setzen oder sie im Code registrieren.

Variante A: Prefab im Inspector
-------------------------------
1) Prefab erstellen, z.B. `Assets/Prefabs/Assembler.prefab`
2) Root: leeres GameObject
3) Mesh + Collider als Child, Pivot am Boden
4) `Buildable` Script am Root
5) BuildCatalog im Scene-Objekt auswaehlen und Eintrag hinzufuegen

Variante B: Runtime-Registrierung
---------------------------------
Siehe `TemplateBootstrap`. Dort werden Beispiel-Buildables per Code
erzeugt und registriert:

```
catalog.Register("Conveyor", conveyorPrefab);
```

Tipps
-----
- Pivot am Boden spart dir Platzierungs-Offsets.
- Colliders nur am Root halten, damit Bounds sauber sind.
- Miner/Conveyor/Smelter/Storage nutzen die Item-Skripte.
- Build-Kosten werden in `TemplateBootstrap` als `BuildCost` gesetzt.

