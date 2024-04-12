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

	/// <summary>
	/// Patches <c>SpecialOrder.IsTimedQuest</c> for custom special orders with infinite duration
	/// </summary>
	[HarmonyPatch(typeof(SpecialOrder), nameof(SpecialOrder.IsTimedQuest))]
	[HarmonyPrefix]
	public static bool SpecialOrder_IsTimedQuest_Prefix(SpecialOrder __instance, ref bool __result)
	{
		if (!SpecialOrder.TryGetData(__instance.questName.Value, out SpecialOrderData SOData) || !SOData.CustomFields.ContainsKey("SunberryTeam.SBVSMAPI/InfiniteDuration"))
			return true;

		__result = false;
		return false;
	}

	/// <summary>
	/// Patches <c>SpecialOrder.GetDaysLeft</c> for custom special orders with infinite duration
	/// </summary>
	[HarmonyPatch(typeof(SpecialOrder), nameof(SpecialOrder.GetDaysLeft))]
	[HarmonyPrefix]
	public static bool SpecialOrder_GetDaysLeft_Prefix(SpecialOrder __instance, ref int __result)
	{
		if (!SpecialOrder.TryGetData(__instance.questName.Value, out SpecialOrderData SOData) || !SOData.CustomFields.ContainsKey("SunberryTeam.SBVSMAPI/InfiniteDuration"))
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
		if (!SpecialOrder.TryGetData(__instance.questName.Value, out SpecialOrderData SOData) || !SOData.CustomFields.ContainsKey("SunberryTeam.SBVSMAPI/InfiniteDuration"))
			return true;

		__instance.dueDate.Value = SDate.Now().AddDays(99).DaysSinceStart;
		return false;
	}
}

#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression