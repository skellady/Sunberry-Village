using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley.BellsAndWhistles;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantAssignment
// ReSharper disable InconsistentNaming

namespace SunberryVillage.TextEmojis;

// Boilerplate suppression for Harmony patch files
#pragma warning disable IDE0079 // Remove unnecessary suppression
// Method names reflect the original methods that they are patching, hence the naming rule violations
#pragma warning disable IDE1006 // Naming Styles
// Certain parameters have special meanings to Harmony
#pragma warning disable IDE0060 // Remove unused parameter

[HarmonyPatch]
internal class EmojiPatches
{

    [HarmonyPatch(typeof(SpriteText), "IsSpecialCharacter")]
    [HarmonyPostfix]
    public static void IsSpecialCharacterPostfix(char c, ref bool __result)
    {
        if(EmojiManager.EmojiIndices.ContainsKey(c))
            __result = true;
    }

    [HarmonyPatch(typeof(SpriteText), "getSourceRectForChar")]
    [HarmonyPrefix]
    public static bool getSourceRectForCharPostfix(char c, ref bool junimoText, ref Rectangle __result)
    {
	    if (!EmojiManager.EmojiIndices.TryGetValue(c, out Rectangle rect))
		    return true;

	    __result = rect;
        return false;
    }

}

#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression