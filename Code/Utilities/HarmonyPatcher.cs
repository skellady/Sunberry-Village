using HarmonyLib;
using SunberryVillage.Integration.Patches;
using SunberryVillage.Objects;
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

		if (Globals.ModRegistry.IsLoaded("ApryllForever.PolyamorySweetLove"))
		{
            Harmony.Patch(
			    original: AccessTools.Method("PolyamorySweetLove.NPCPatches:NPC_tryToReceiveActiveObject_Prefix"),
			    prefix: new HarmonyMethod(AccessTools.Method(typeof(PSLPatches), nameof(PSLPatches.NPC_tryToReceiveActiveObject_Prefix_Prefix)))
			);
        }
	}
}
