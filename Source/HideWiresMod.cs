using System.Reflection;
using HarmonyLib;
using Verse;

namespace HideWires
{
    /// <summary>
    /// Harmony bootstrap. [StaticConstructorOnStartup] runs this once, after defs and
    /// content (textures) have loaded, which is also what makes it safe for
    /// <see cref="HideWiresContent"/> to resolve its texture at static-init time.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class HideWiresMod
    {
        public const string HarmonyId = "archdukejim.hidewires";

        static HideWiresMod()
        {
            var harmony = new Harmony(HarmonyId);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
