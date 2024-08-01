//using HarmonyLib;
//using StardewValley;
//using SunberryVillage.Utilities;
//using System;

//// ReSharper disable UnusedMember.Local
//// ReSharper disable UnusedMember.Global
//// ReSharper disable RedundantAssignment
//// ReSharper disable InconsistentNaming

//namespace SunberryVillage.Weather;

//// Boilerplate suppression for Harmony patch files
//#pragma warning disable IDE0079 // Remove unnecessary suppression
//// Method names reflect the original methods that they are patching, hence the naming rule violations
//#pragma warning disable IDE1006 // Naming Styles
//// Certain parameters have special meanings to Harmony
//#pragma warning disable IDE0060 // Remove unused parameter

//[HarmonyPatch]
//internal class WeatherPatches
//{
//	/*
//	 *  Patches
//	 */

//	/// <summary>
//	/// Patches <c>Game1.getWeatherModificationsForDate</c> to make sure the weather is sunny for the Twilight Festival (the base code doesn't do it because the festival doesn't make any map replacements).
//	/// </summary>
//	[HarmonyPatch(typeof(Game1), nameof(Game1.getWeatherModificationsForDate))]
//	[HarmonyPostfix]
//	public static void getWeatherModificationsForDate_Postfix(WorldDate date, ref string __result)
//	{
//		try
//		{
//			if (date.Season == Season.Fall && date.DayOfMonth == 26)
//				__result = "Sun";
//		}
//		catch (Exception e)
//		{
//			Log.Error($"Harmony patch \"{nameof(WeatherPatches)}::{nameof(getWeatherModificationsForDate_Postfix)}\": \n{e}");
//		}
//	}
//}

//#pragma warning restore IDE1006 // Naming Styles
//#pragma warning restore IDE0060 // Remove unused parameter
//#pragma warning restore IDE0079 // Remove unnecessary suppression