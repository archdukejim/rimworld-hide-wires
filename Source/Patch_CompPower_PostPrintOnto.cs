using HarmonyLib;
using RimWorld;
using Verse;

namespace HideWires
{
    /// <summary>
    /// Skips printing the building-to-grid connection stub when the toggle is on.
    ///
    /// Target: RimWorld.CompPower.PostPrintOnto(Verse.SectionLayer), whose body is
    /// effectively:
    ///
    ///     base.PostPrintOnto(layer);
    ///     if (connectParent != null)
    ///         PowerNetGraphics.PrintWirePieceConnecting(layer, parent, connectParent.parent, forPowerOverlay: false);
    ///
    /// That PrintWirePieceConnecting call (forPowerOverlay: false) IS the thin stub we
    /// want gone. Suppressing the whole method is safe because ThingComp.PostPrintOnto -
    /// the base call - is empty, so the stub is the only thing this override contributes.
    ///
    /// What we deliberately do NOT patch:
    ///   - RimWorld.CompPower.CompPrintForPowerGrid(SectionLayer), which feeds
    ///     RimWorld.SectionLayer_ThingsPowerGrid and calls the same helper with
    ///     forPowerOverlay: true. Leaving it alone is what makes wires reappear in full
    ///     whenever the power overlay is up (conduit designator selected / Power architect
    ///     tab open), per spec - no extra code needed for that exception.
    ///   - RimWorld.Graphic_LinkedTransmitter.Print, i.e. the conduits' own linked wire
    ///     graphics. Conduits stay fully visible.
    /// </summary>
    [HarmonyPatch(typeof(CompPower), nameof(CompPower.PostPrintOnto))]
    public static class Patch_CompPower_PostPrintOnto
    {
        /// <returns>false to skip the original (stub not printed), true to run it.</returns>
        public static bool Prefix(SectionLayer layer)
        {
            if (!HideWiresState.WiresHidden)
            {
                return true;
            }

            // Belt and braces. In vanilla 1.6 SectionLayer_ThingsPowerGrid builds itself
            // via CompPrintForPowerGrid and never routes through Print/PostPrintOnto, so
            // this should not trigger - but if another mod ever prints things onto the
            // power-grid layer the normal way, we must not eat the overlay's wires.
            if (layer is SectionLayer_ThingsPowerGrid)
            {
                return true;
            }

            return false;
        }
    }
}
