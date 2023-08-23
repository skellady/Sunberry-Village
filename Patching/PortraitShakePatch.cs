using System.Collections.Generic;
using HarmonyLib;
using StardewValley;
using StardewValley.Menus;
using SunberryVillage.PortraitShake;
using SunberryVillage.Utilities;

namespace SunberryVillage.Patching;

// Boilerplate suppression for Harmony patch files
#pragma warning disable IDE0079 // Remove unnecessary suppression
// Method names reflect the original methods that they are patching, hence the naming rule violations
#pragma warning disable IDE1006 // Naming Styles
// Certain parameters have special meanings to Harmony
#pragma warning disable IDE0060 // Remove unused parameter

[HarmonyPatch]
class PortraitShakePatch
{

	/// <summary>
	/// Patches <c>DialogueBox.shouldPortraitShake</c> to handle custom shaking portraits.
	/// </summary>
	[HarmonyPatch(typeof(DialogueBox), "shouldPortraitShake")]
	[HarmonyPrefix]
	public static bool shouldPortraitShake_Prefix(Dialogue d, ref bool __result)
	{
		__result = PortraitShakeHandler.PortraitShouldShake.Value;
		return !__result;
	}
}

#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression