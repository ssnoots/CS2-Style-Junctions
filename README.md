[h1]CS2 Style Junctions[/h1]

[b]CS2 Style Junctions[/b] is a Cities: Skylines mod that automatically gives road junctions a cleaner, smoother, CS2-inspired look as you build them.

The mod applies rounded junction geometry based on the type of roads being connected. Small streets get tighter corners, avenues get smoother curves, arterials get wider turns, and highway/ramp connections get more natural sweeping merges.


[h1]What It Does[/h1]

When you build a new junction, the mod automatically selects an appropriate corner radius based on the road classes involved.

You can customize these radii using an in-game settings panel, allowing you to fine-tune how rounded each road type should be.

[h2]Default Behavior[/h2]

[list]
[*][b]Small streets[/b] — tight residential-style corners
[*][b]Medium roads / avenues[/b] — moderate rounded corners
[*][b]Large arterials[/b] — wider, smoother corners
[*][b]Highways[/b] — large sweeping curves
[*][b]Ramp-style junctions[/b] — extra-wide radii for smoother highway merge behavior
[/list]

The mod also detects nearby junctions. If two close junctions would create overlapping curves, the radii are automatically scaled down to prevent visual glitches, overlap, or geometry inversion.


[h1]What It Does Not Do[/h1]

Cities: Skylines 1 has engine limitations that prevent certain CS2-style road features from being recreated exactly.

This mod works within CS1’s network system to improve junction appearance, but it cannot add features the game engine does not support.

[h2]Engine-Limited Features[/h2]

[list]
[*]True CS2-style continuous-pavement ramps
[*]Tapered medians
[*]Curved lane markings through junction nodes
[*]Variable lane counts along a single road segment
[/list]


[h1]In-Game Panel[/h1]

Press [b]Ctrl + Shift + J[/b] to open the settings panel, or use your chosen custom hotkey.

The panel includes six sliders for different road classes and ramp scenarios. The slider that matches your currently selected road is highlighted, making it easy to see which value affects the road you are about to build.

[h2]Panel Buttons[/h2]

[b]Save Settings[/b]  
Saves your slider values so they persist after restarting the game.

[b]Re-tune Existing Junctions[/b]  
Re-applies your current slider settings to junctions built during the current session. Junctions that already existed when the save was loaded remain untouched.

[b]Toggle Hotkey[/b]  
Allows you to rebind the panel hotkey. Press [b]Escape[/b] to cancel rebinding.


[h1]Settings Menu[/h1]

Available through:

[b]Content Manager → Mods[/b]

[h2]Options[/h2]

[b]Enable automatic junction polish[/b]  
Master toggle for the mod.

[b]Also apply to nodes that existed before the save was loaded[/b]  
Disabled by default. Enable this only if you want the mod to retroactively polish older infrastructure.

[b]Warning:[/b] enabling this may overwrite custom per-node edits from mods such as Node Controller Renewal or Move It.


[h1]Requirements[/h1]

These mods must be subscribed and enabled:

[list]
[*][b]Harmony 2.2.2-0 / Mod Dependency 2.0[/b] by boformer  
Workshop ID: [code]2040656402[/code]

[*][b]Node Controller Renewal[/b] by macsergey  
Workshop ID: [code]2472062376[/code]
[/list]


[h1]Recommended Mods[/h1]

These are not required, but work well alongside this mod:

[list]
[*]Move It
[*]Network Anarchy by Quboid
[*]Intersection Marking Tool by macsergey
[*]Traffic Manager: President Edition
[*]Network Multitool
[/list]


[h1]Safe to Enable Mid-Save?[/h1]

[b]Yes.[/b]

The mod modifies junction geometry at render-time and does not write permanent geometry changes to your save file. Disabling or unsubscribing from the mod will revert junctions back to vanilla CS1 geometry.

Your save remains compatible with vanilla Cities: Skylines.


[h1]Known Limitations[/h1]

[list]
[*]The mod cannot create true CS2-style continuous-pavement ramps.
[*]Very short road segments between nearby junctions may have their corner radii reduced to avoid geometry issues.
[*]Custom workshop roads with unusual lane counts may be classified differently than expected.
[*]Tunnels and underground nodes are excluded from polishing.
[*]Mods that modify [code]NetSegment.CalculateCorner[/code] or network release behavior may cause unpredictable results.
[/list]


[h1]Performance[/h1]

The mod is designed to be lightweight.

Most visible corners use cached decisions, with only new junctions needing classification. Medium-sized cities should see minimal performance impact. Very large cities with many visible interchanges may experience slightly more overhead.


[h1]Reporting Issues[/h1]

When reporting a bug, please include:

[list]
[*]A screenshot of the issue
[*]A list of enabled mods
[*]Your log file:
[/list]

[code]Cities_Skylines\Cities_Data\output_log.txt[/code]

Anything tagged [code][CS2SJ][/code] in the log is from this mod.


[h1]Acknowledgments[/h1]

Built on top of the work of macsergey and boformer, including Node Controller Renewal, Intersection Marking Tool, Network Multitool, and CitiesHarmony.

This mod would not exist without their groundwork.

If any credits are incorrect, please let me know and I will update them.


[h1]Version[/h1]

[b]v1.0[/b] — First public release.

See [code]CHANGELOG.md[/code] for full development history.
