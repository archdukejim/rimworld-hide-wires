using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace HideWires
{
    /// <summary>
    /// Adds our toggle to the bottom-right overlay-toggle row.
    ///
    /// Target: RimWorld.PlaySettings.DoPlaySettingsGlobalControls(Verse.WidgetRow, bool),
    /// the same method vanilla uses to lay out "show zones", "show roof overlay", etc.
    /// A postfix appends our icon after the vanilla ones.
    /// </summary>
    [HarmonyPatch(typeof(PlaySettings), nameof(PlaySettings.DoPlaySettingsGlobalControls))]
    public static class Patch_PlaySettings_DoPlaySettingsGlobalControls
    {
        public static void Postfix(WidgetRow row, bool worldView)
        {
            // worldView == true is the planet view, where a map-mesh toggle is meaningless.
            if (worldView || row == null)
            {
                return;
            }

            // Verse.WidgetRow.ToggleableIcon takes the bool by ref, so hand it a copy and
            // route any change through SetHidden - that is what triggers the section
            // regeneration. Writing HideWiresState.WiresHidden directly would flip the
            // flag but leave the stale wire quads sitting in the map mesh.
            bool hidden = HideWiresState.WiresHidden;

            row.ToggleableIcon(
                ref hidden,
                HideWiresContent.ToggleIcon,
                "HideWires.ToggleTooltip".Translate(),
                SoundDefOf.Mouseover_ButtonToggle,
                null);

            if (hidden != HideWiresState.WiresHidden)
            {
                HideWiresState.SetHidden(hidden);
            }
        }
    }

    /// <summary>
    /// Texture holder. [StaticConstructorOnStartup] guarantees content is loaded before
    /// ContentFinder runs, which is required for any texture resolved at static-init time.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class HideWiresContent
    {
        // Placeholder icon: Textures/UI/Overlays/HideWires.png
        // RimWorld resolves texture paths relative to the mod's Textures/ folder and
        // without the file extension. Swap the PNG for real art; the path stays the same.
        public static readonly Texture2D ToggleIcon =
            ContentFinder<Texture2D>.Get("UI/Overlays/HideWires");
    }
}
