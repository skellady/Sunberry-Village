using HarmonyLib;
using SunberryVillage.Integration.Patches;
using SunberryVillage.Objects;
using System;
using System.Reflection;

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
			MethodInfo freezeTime = AccessTools.Method("CJBCheatsMenu.Framework.Cheats.Time.FreezeTimeCheat:ShouldFreezeTime");

            if (freezeTime is not null)
			{
				Harmony.Patch(
					original: freezeTime,
					transpiler: new HarmonyMethod(AccessTools.Method(typeof(CJBPatches), nameof(CJBPatches.ShouldFreezeTime_Transpiler)))
				);
			}
			else
			{
				Log.Trace("Could not find method matching <CJBCheatsMenu.Framework.Cheats.Time.FreezeTimeCheat:ShouldFreezeTime>. Skipping patch.");
			}
		}

		if (Globals.ModRegistry.IsLoaded("ApryllForever.PolyamorySweetLove"))
		{
			MethodInfo polysweet = AccessTools.Method("PolyamorySweetLove.NPCPatches:NPC_tryToReceiveActiveObject_Prefix");

			if (polysweet is not null)
			{
                Harmony.Patch(
                    original: polysweet,
                    prefix: new HarmonyMethod(AccessTools.Method(typeof(PSLPatches), nameof(PSLPatches.NPC_tryToReceiveActiveObject_Prefix_Prefix)))
                );
            }
            else
            {
                Log.Trace("Could not find method matching <PolyamorySweetLove.NPCPatches:NPC_tryToReceiveActiveObject_Prefix>. Skipping patch.");
            }
        }
	}
}
