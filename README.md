# Hide Wires

[Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3784625720) ·
item id `3784625720`

A RimWorld 1.5/1.6 mod that hides the thin power-**connection** stubs drawn between powered
buildings and the power grid, for a cleaner base view. Conduits are never touched.

## Behaviour

| | |
|---|---|
| Default | Wires shown (vanilla) |
| Toggle | Bottom-right overlay row button, or a rebindable hotkey |
| Power overlay active | Wires **always** shown, regardless of the toggle |
| Conduits + their linked graphics | Always fully visible |
| Save data | None — pure rendering change, add/remove at any time |

The hotkey is unbound by default; assign it under *Options > Keyboard configuration >
in-game global > toggle power connection wires*.

## How it works

RimWorld prints the connection stub **into the map mesh**, so hiding it needs two things:
skip the print, and rebuild the mesh that already contains it.

**The print path.** `RimWorld.CompPower.PostPrintOnto(SectionLayer)` calls
`PowerNetGraphics.PrintWirePieceConnecting(layer, parent, connectParent.parent, forPowerOverlay: false)`.
It is reached from `Verse.ThingWithComps.Print` via `Verse.SectionLayer_Things.TakePrintFrom`,
and the concrete layer for ordinary buildings is **`Verse.SectionLayer_ThingsGeneral`**
(`relevantChangeTypes` = `MapMeshFlagDefOf.Things`). A Harmony prefix skips the whole
override when the toggle is on — safe, because `ThingComp.PostPrintOnto` is empty, so the
stub is this override's only contribution.

**The overlay path, deliberately unpatched.** The power overlay's wires come from a
*different* layer, `RimWorld.SectionLayer_ThingsPowerGrid` (`MapMeshFlagDefOf.PowerGrid`),
which builds itself from `CompPower.CompPrintForPowerGrid` — not `PostPrintOnto` — and gates
`DrawLayer()` on `RimWorld.OverlayDrawHandler.ShouldDrawPowerGrid`. Leaving it alone is
what implements "always show while planning wiring"; the exception needs no code of its own.

**The regeneration.** On toggle we call
`Verse.MapDrawer.RegenerateLayerNow(typeof(SectionLayer_ThingsGeneral))` for each loaded map,
so stubs appear/disappear immediately without a camera move. We use this rather than
`MapDrawer.WholeMapChanged(ulong)` because that overload takes a raw `UInt64` bitmask and
`MapMeshFlagDef` exposes no public member for its bit value in the 1.6 assembly.

## Layout

```
About/About.xml                                   mod metadata + Harmony dependency
Defs/KeyBindingDefs/KeyBindings_HideWires.xml     rebindable hotkey (category: Game)
Languages/English/Keyed/HideWires.xml             tooltip text
Textures/UI/Overlays/HideWires.png                placeholder toggle icon (32x32)
Source/HideWiresMod.cs                            Harmony bootstrap
Source/HideWiresState.cs                          the bool + section regeneration
Source/Patch_CompPower_PostPrintOnto.cs           skip the stub print
Source/Patch_PlaySettings_DoPlaySettingsGlobalControls.cs   bottom-right toggle button
Source/HideWiresHotkey.cs                         DefOf + once-per-frame hotkey poll
```

## Building

```bash
dotnet build Source/HideWires.csproj
```

Output goes to `Assemblies/HideWires.dll`. If RimWorld or Harmony live elsewhere:

```bash
dotnet build Source/HideWires.csproj -p:RimWorldManagedDir="D:\Games\RimWorld\RimWorldWin64_Data\Managed"
```

## Status

Compiles clean against RimWorld 1.6.4871. **Not yet run in-game** — see the verification
notes in the handoff: the `SectionLayer_ThingsGeneral` attribution and the
`RegenerateLayerNow` cost are the two things to confirm on a live map.
