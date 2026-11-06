# DESERT RUSH · The Sunken Kingdom

An original stylized desert runner for Unity 6000.5.2f1 and URP 17.5.0.

## Open and play

1. Add this folder to Unity Hub and open it with Unity 6000.5.2f1. Let the editor finish package import.
2. Open `Assets/_Project/Scenes/MainMenu.unity` and enter Play mode. If the scenes have yet to be generated, run **Tools → Desert Rush → Asset Generator**. The generator opens the menu scene and registers both scenes for builds.
3. Select **Start Expedition**. The gameplay scene also runs directly from Play mode.

| Action | Keyboard | Gamepad |
| --- | --- | --- |
| Change lane | A / D, Left / Right | Left stick |
| Jump | W, Up, Space | South button |
| Slide | S, Down | East button |
| Pause | Escape | Start |

## Assets and code

The editor generator saves reusable meshes, materials, procedural textures, prefabs, route profiles, WAVs, and scene wiring. Its output uses stable asset paths. Regenerating overwrites generated assets at those paths, so duplicate an asset before customizing it. The explorer uses articulated mesh parts and procedural pose blending. Each route row reserves a safe lane at most one lane from the previous safe lane. The route pools scenery, hazards, pickups, and segment containers. Desktop input can be replaced through `IRunnerInput`.

Coin balance, outfits, lifetime stats, mission claims, audio volume, and quality settings persist locally in Unity PlayerPrefs. Runs bank their score and gold on collision-driven game over.

## Production scope

This is a stylized playable foundation. The procedural explorer rig and animated poses are supplied in C#; a skinned Humanoid Avatar and imported animation clips remain production work. The sand sink and broken bridge deck use collision hazards. Heat refraction, moving clouds, dynamic bridge collapse, and hardware-specific performance profiling remain open.


## Windows build

Run **Tools → Desert Rush → Build Windows Player** to build a 64-bit executable into the sibling Desert Rush Windows folder.
