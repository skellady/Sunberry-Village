using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using SObject = StardewValley.Object;
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantAssignment
// ReSharper disable InconsistentNaming

namespace SunberryVillage.Objects;

// Boilerplate suppression for Harmony patch files
#pragma warning disable IDE0079 // Remove unnecessary suppression
// Method names reflect the original methods that they are patching, hence the naming rule violations
#pragma warning disable IDE1006 // Naming Styles
// Certain parameters have special meanings to Harmony
#pragma warning disable IDE0060 // Remove unused parameter

[HarmonyPatch]
internal class PSLPatches
{
	internal const string IgnorePSLString = "IgnorePSL";

    /*
	 *  Patches
	 */

    /// <summary>
    /// Patches PSL to ignore its proposal code if the target NPC has the appropriate custom field. Not annotated because it's handled by ApplyConditionalPatches().
    /// </summary>
    [HarmonyPatch("PolyamorySweetLove.NPCPatches", "NPC_tryToReceiveActiveObject_Prefix")]
    [HarmonyPrefix]
    public static bool NPC_tryToReceiveActiveObject_Prefix_Prefix(NPC __0, ref bool __result)
    {
        if (!__0.GetData().CustomFields?.TryGetValue(IgnorePSLString, out string value) ?? true || !bool.Parse(value))
            return true;
        
        __result = true;
        return false;
    }

}

#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression