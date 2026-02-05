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
- Beispiel-Buildables: Miner, Conveyor, Smelter, Storage (runtime erzeugt).
- Resource Nodes mit Interaktion.
- HUD Overlay (OnGUI) fuer Status und Controls.

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
- Esc: Maus freigeben

Projektstruktur
---------------
Assets/
  Scripts/
    Bootstrap/     Runtime-Setup fuer die Szene
    Player/        Controller + Haende
    Build/         Build-Mode + Buildables
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

