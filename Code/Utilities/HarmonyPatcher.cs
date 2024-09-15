using HarmonyLib;
using SunberryVillage.Integration.Patches;
using System;

namespace SunberryVillage.Utilities;

internal class HarmonyPatcher
{
	internal static readonly Harmony Harmony = new(Globals.UUID);

	internal static void ApplyPatches()
	{
		Log.Trace("Patching methods.");

		try
		{
			Harmony.PatchAll();
		}
		catch (Exception ex)
		{
			Log.Error($"Exception encountered while patching: {ex}");
		}
	}

	internal static void ApplyConditionalPatches()
	{
		if (Globals.ModRegistry.IsLoaded("CJBok.CheatsMenu"))
		{
			Harmony.Patch(
				original: AccessTools.Method("CJBCheatsMenu.Framework.Cheats.Time.FreezeTimeCheat:ShouldFreezeTime"),
				transpiler: new HarmonyMethod(AccessTools.Method(typeof(CJBPatches), nameof(CJBPatches.ShouldFreezeTime_Transpiler)))
			);
		}
	}
}
