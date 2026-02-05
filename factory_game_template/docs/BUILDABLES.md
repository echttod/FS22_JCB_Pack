Buildables hinzufuegen
======================

Dieses Template nutzt `BuildCatalog.gd` als zentralen Katalog. Neue
Buildables werden als Scene angelegt und dann im Katalog registriert.

Schritt 1: Szene anlegen
------------------------
1. Erstelle eine neue Scene (z.B. `scenes/buildables/Assembler.tscn`).
2. Root-Node: `StaticBody3D`
3. Script: `scripts/Buildable.gd`
4. MeshInstance3D + CollisionShape3D anlegen.
5. Den Mesh und Collision auf halbe Hoehe setzen, damit der Root
   direkt auf dem Boden platziert werden kann.

Schritt 2: Katalog erweitern
----------------------------
In `scripts/BuildCatalog.gd` einen Eintrag hinzufuegen:

```
var _buildables := {
    "Assembler": preload("res://scenes/buildables/Assembler.tscn"),
}
```

Schritt 3: Testen
-----------------
Spiel starten, Build-Mode aktivieren, und das neue Buildable platzieren.

