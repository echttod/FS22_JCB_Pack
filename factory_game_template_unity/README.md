Factory Game Template (Unity, Satisfactory-like)
================================================

Dieses Projekt ist ein kompletter Startpunkt fuer ein First-Person
Fabrikgame mit sichtbaren Haenden. Es ist ein Unity-Template mit
Scripts, die eine spielbare Grundschleife erstellen.

Highlights
----------
- First-Person Controller (CharacterController).
- Sichtbare Haende (Viewmodel), optionaler Platzhalter im Code.
- Build-Mode mit Ghost-Preview, Grid-Snap und Rotation.
- Build-Katalog UI im Build-Mode.
- Beispiel-Buildables: Miner, Conveyor, Smelter, Constructor, Assembler, Storage, Depot, Generator, PowerPole.
- Build-Kosten aus Depots (naechstes Depot wird bevorzugt).
- Research/Tech-Tier System (R).
- Recipe System fuer Smelter.
- Power Grid (Generatoren vs. Verbraucher) + sichtbare Wires.
- Save/Load (F5/F9, inkl. Storage/Smelter/Miner/Conveyor).
- Prefabs in `Assets/Resources/Buildables`.
- Meshes in `Assets/Resources/Meshes`.
- Resource Nodes mit Interaktion.
- HUD Overlay (UGUI) fuer Status, Power und Research.

Quick Start
-----------
1) Unity 2022.3 LTS oeffnen.
2) Projektordner waehlen: `factory_game_template_unity`
3) Scene oeffnen: `Assets/Scenes/FactoryTemplate.unity`
4) Play druecken.

Optional: Eigene Scene
----------------------
Falls du eine eigene Scene willst:
- Leeres GameObject anlegen -> Script `TemplateBootstrap` zuweisen.

Controls (Default)
------------------
- WASD: Laufen
- Maus: Blicken
- Space: Springen
- Shift: Sprinten
- F: Interagieren
- B: Build-Mode an/aus
- Q/E: Buildable wechseln
- Z/C: Buildable drehen
- Linke Maustaste: Platzieren
- R: Research
- F5: Speichern
- F9: Laden
- Esc: Maus freigeben

Projektstruktur
---------------
Assets/
  Scripts/
    Bootstrap/     Runtime-Setup fuer die Szene
    Items/         Items + BuildDepot/Kosten
    Player/        Controller + Haende
    Build/         Build-Mode + Buildables
    Systems/       Save/Load + Kosten
    Power/         Power Grid
    Recipes/       Recipe Book
    Tech/          Research/Tech Tree
    World/         Resource Nodes
    UI/            HUD Overlay
docs/
  Setup und Gameplay Hinweise

Naechste Schritte (Ideen)
-------------------------
- Phase 1 Loop: Miner -> Conveyor -> Smelter -> Storage
- Produktionsketten (Input/Output, Rezepte)
- Items + Conveyor-Logik
- Stromversorgung/Power Grid
- Research/Progression
- Build-Katalog UI

