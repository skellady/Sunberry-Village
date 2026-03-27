using HarmonyLib;
using StardewValley;
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantAssignment
// ReSharper disable InconsistentNaming

namespace SunberryVillage.Integration.Patches;

// Boilerplate suppression for Harmony patch files
#pragma warning disable IDE0079 // Remove unnecessary suppression
// Method names reflect the original methods that they are patching, hence the naming rule violations
#pragma warning disable IDE1006 // Naming Styles
// Certain parameters have special meanings to Harmony
#pragma warning disable IDE0060 // Remove unused parameter

[HarmonyPatch]
internal class CJBPatches
{
	internal const string MinesString = "Custom_SBV_Mines";

    /*
	 *  Patches
	 */

	/// <summary>
	/// Patches <c>CJBCheatsMenu.Framework.Cheats.Time.FreezeTimeCheat::ShouldFreezeTime</c> to treat Sunberry mines as caves for the freeze time cheat.
	/// If the location is Custom_SBV_Mines, sets isCave = true and returns true when FreezeTimeCaves is enabled.
	/// </summary>
	public static void ShouldFreezeTime_Postfix(object config, GameLocation location, ref bool isCave, ref bool __result)
	{
		if (location?.Name?.Contains(MinesString) != true)
			return;

		isCave = true;

		// Mirror the original logic: return true if FreezeTimeCaves is enabled (or FreezeTime is already on)
		if (!__result)
		{
			try
			{
				bool freezeTimeCaves = (bool)AccessTools.Property(config.GetType(), "FreezeTimeCaves").GetValue(config);
				if (freezeTimeCaves)
					__result = true;
			}
			catch (System.Exception e)
			{
				Log.Error($"Harmony patch <{nameof(CJBPatches)}::{nameof(ShouldFreezeTime_Postfix)}> has encountered an error: \n{e}");
				Log.Trace($"Config type: {config?.GetType().FullName ?? "null"}, Location: {location?.Name ?? "null"}, isCave: {isCave}, __result: {__result}");
			}
		}
	}
}

#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression