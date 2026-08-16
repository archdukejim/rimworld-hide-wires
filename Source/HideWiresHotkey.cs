using RimWorld;
using Verse;

namespace HideWires
{
    [DefOf]
    public static class HideWiresDefOf
    {
        public static KeyBindingDef HideWires_ToggleWires;

        static HideWiresDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HideWiresDefOf));
        }
    }

    /// <summary>
    /// Polls the hotkey. GameComponent subclasses are instantiated automatically for every
    /// loaded assembly, so no def registration is needed - only the (Game) constructor.
    /// </summary>
    public class HideWiresHotkey : GameComponent
    {
        public HideWiresHotkey(Game game)
        {
        }

        /// <summary>
        /// GameComponentUpdate runs exactly once per frame.
        ///
        /// This matters: Verse.KeyBindingDef.JustPressed is backed by Input.GetKeyDown, so
        /// polling it from an OnGUI hook instead would fire once per GUI *event* - several
        /// times in the same frame - and the toggle would flip back and forth. (The
        /// OnGUI-safe sibling is KeyBindingDef.KeyDownEvent, which consumes Event.current.)
        /// </summary>
        public override void GameComponentUpdate()
        {
            if (Current.ProgramState != ProgramState.Playing || Find.CurrentMap == null)
            {
                return;
            }

            // Don't steal the key while the player is typing in a text field / dialog.
            if (Find.WindowStack.WindowsForcePause)
            {
                return;
            }

            if (HideWiresDefOf.HideWires_ToggleWires.JustPressed)
            {
                HideWiresState.Toggle();
            }
        }
    }
}
