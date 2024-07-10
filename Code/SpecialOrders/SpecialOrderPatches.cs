using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData.SpecialOrders;
using StardewValley.SpecialOrders;
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantAssignment
// ReSharper disable InconsistentNaming

namespace SunberryVillage.SpecialOrders;

// Boilerplate suppression for Harmony patch files
#pragma warning disable IDE0079 // Remove unnecessary suppression
// Method names reflect the original methods that they are patching, hence the naming rule violations
#pragma warning disable IDE1006 // Naming Styles
// Certain parameters have special meanings to Harmony
#pragma warning disable IDE0060 // Remove unused parameter

[HarmonyPatch]
internal class SpecialOrderPatches
{
	/*
	 *  Patches
	 */

	// avoiding a transpiler here by simply removing the affected SOs and putting them back after
	// makes SBV special orders to slay monsters only count monsters killed in an SBV location
	// not currently in use because the special orders aren't set up for monster types in the SBV areas

	//[HarmonyPatch(typeof(GameLocation), "onMonsterKilled")]
	//[HarmonyPrefix]
	//public static void GameLocation_onMonsterSlain_Prefix(GameLocation __instance, List<SpecialOrder> __state)
	//{
	//	if (!__instance.Name.Contains("SBV"))
	//		return;
	//
	//	foreach (SpecialOrder order in Game1.player.team.specialOrders.Where(order => order.questKey.Value.Contains("SBV")))
	//	{
	//		__state.Add(order);
	//		Game1.player.team.specialOrders.Remove(order);
	//	}
	//}
	//
	//[HarmonyPatch(typeof(GameLocation), "onMonsterKilled")]
	//[HarmonyPostfix]
	//public static void GameLocation_onMonsterSlain_Postfix(GameLocation __instance, List<SpecialOrder> __state)
	//{
	//	if (!__instance.Name.Contains("SBV"))
	//		return;
	//
	//	foreach (SpecialOrder order in __state)
	//	{
	//		Game1.player.team.specialOrders.Add(order);
	//	}
	//
	//	__state.Clear();
	//}

	/// <summary>
	/// Patches <c>SpecialOrder.IsTimedQuest</c> for custom special orders with infinite duration
	/// </summary>
	[HarmonyPatch(typeof(SpecialOrder), nameof(SpecialOrder.IsTimedQuest))]
	[HarmonyPostfix]
	public static void SpecialOrder_IsTimedQuest_Postfix(SpecialOrder __instance, ref bool __result)
	{
		SpecialOrderData soData = __instance.GetData();

		if (soData is null || (!soData.CustomFields?.ContainsKey("SunberryTeam.SBVSMAPI/InfiniteDuration") ?? true))
			return;

		__result = false;
	}

	/// <summary>
	/// Patches <c>SpecialOrder.GetDaysLeft</c> for custom special orders with infinite duration
	/// </summary>
	[HarmonyPatch(typeof(SpecialOrder), nameof(SpecialOrder.GetDaysLeft))]
	[HarmonyPrefix]
	public static bool SpecialOrder_GetDaysLeft_Prefix(SpecialOrder __instance, ref int __result)
	{
		SpecialOrderData soData = __instance.GetData();

		if (soData is null || (!soData.CustomFields?.ContainsKey("SunberryTeam.SBVSMAPI/InfiniteDuration") ?? true))
			return true;

		__result = 99;
		return false;
	}

	/// <summary>
	/// Patches <c>SpecialOrder.SetDuration</c> for custom special orders with infinite duration
	/// </summary>
	[HarmonyPatch(typeof(SpecialOrder), nameof(SpecialOrder.SetDuration))]
	[HarmonyPrefix]
	public static bool SpecialOrder_SetDuration_Prefix(SpecialOrder __instance)
	{
		SpecialOrderData soData = __instance.GetData();

		if (soData is null || (!soData.CustomFields?.ContainsKey("SunberryTeam.SBVSMAPI/InfiniteDuration") ?? true))
			return true;

		__instance.dueDate.Value = SDate.Now().AddDays(99).DaysSinceStart;
		return false;
	}
}

#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression