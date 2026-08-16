using System.Collections.Generic;
using Verse;

namespace HideWires
{
    /// <summary>
    /// The single source of truth for the toggle, plus the map-mesh regeneration that
    /// has to happen whenever it flips.
    ///
    /// Deliberately NOT persisted: this is a pure view setting, so the mod adds no
    /// Scribe data and stays save-compatible in both directions (adding or removing the
    /// mod from an existing colony changes nothing but what is drawn).
    /// </summary>
    public static class HideWiresState
    {
        /// <summary>
        /// false = vanilla behaviour (wires shown). Default per spec.
        /// </summary>
        public static bool WiresHidden;

        public static void Toggle()
        {
            SetHidden(!WiresHidden);
        }

        public static void SetHidden(bool hidden)
        {
            if (WiresHidden == hidden)
            {
                return;
            }

            WiresHidden = hidden;
            RegenerateWireLayer();
        }

        /// <summary>
        /// The connection stub is *printed into the map mesh*, not drawn every frame, so
        /// flipping the bool alone changes nothing until the mesh that already contains
        /// the wire quads is rebuilt.
        ///
        /// WHICH LAYER (the make-or-break detail):
        ///   RimWorld.CompPower.PostPrintOnto(SectionLayer) is reached from
        ///   Verse.ThingWithComps.Print(SectionLayer), which is called by
        ///   Verse.SectionLayer_Things.TakePrintFrom(Thing). The concrete layer doing
        ///   that for ordinary buildings is Verse.SectionLayer_ThingsGeneral, whose
        ///   relevantChangeTypes is MapMeshFlagDefOf.Things.
        ///
        ///   The power-overlay wires are a DIFFERENT layer:
        ///   RimWorld.SectionLayer_ThingsPowerGrid (MapMeshFlagDefOf.PowerGrid), which
        ///   builds itself from CompPower.CompPrintForPowerGrid - NOT from PostPrintOnto -
        ///   and gates its own SectionLayer_ThingsPowerGrid.DrawLayer() on
        ///   RimWorld.OverlayDrawHandler.ShouldDrawPowerGrid. We never touch that layer,
        ///   which is exactly what gives us "always show while the power overlay is up".
        ///
        /// We use Verse.MapDrawer.RegenerateLayerNow(Type) rather than
        /// MapDrawer.WholeMapChanged(ulong): WholeMapChanged takes a raw UInt64 bitmask
        /// and MapMeshFlagDef exposes no public member for its bit value in the 1.6
        /// assembly, so RegenerateLayerNow is both the exact tool for the job (one layer
        /// type, every section) and the one with a stable public signature.
        /// This is a one-off cost on a deliberate player action, not a per-frame cost.
        /// </summary>
        public static void RegenerateWireLayer()
        {
            if (Current.ProgramState != ProgramState.Playing)
            {
                return;
            }

            List<Map> maps = Find.Maps;
            for (int i = 0; i < maps.Count; i++)
            {
                maps[i].mapDrawer?.RegenerateLayerNow(typeof(SectionLayer_ThingsGeneral));
            }
        }
    }
}
