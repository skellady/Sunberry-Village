using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI.Utilities;
using StardewValley;
using SunberryVillage.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using xTile.Dimensions;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantAssignment
// ReSharper disable InconsistentNaming

namespace SunberryVillage.Mines;

// Boilerplate suppression for Harmony patch files
#pragma warning disable IDE0079 // Remove unnecessary suppression
// Method names reflect the original methods that they are patching, hence the naming rule violations
#pragma warning disable IDE1006 // Naming Styles
// Certain parameters have special meanings to Harmony
#pragma warning disable IDE0060 // Remove unused parameter

[HarmonyPatch]
internal class MinesPatches
{
	// keeping this code for a bit to make sure i haven't fucked anything up

	///// <summary>
	///// Patches <c>GameLocation.performAction</c> to check for the "ChooseWarp" action and handle it accordingly.<br />
	///// Proper syntax for ChooseWarp is as follows: <c>ChooseWarp "[Option1StringKey]" [x1] [y1] [LocationName1] "[Option2StringKey]" [x2] [y2] [LocationName2]</c><br />
	///// OptionStringKey should refer to a string defined in <c>Strings/StringsFromCSFiles</c> or another game content file. <c>x</c> and <c>y</c> are the tile coords to warp to, and <c>LocationName</c> is the internal name of the map to warp to.
	///// </summary>
	//[HarmonyPatch(typeof(GameLocation), nameof(GameLocation.performAction))]
	//[HarmonyPrefix]
	//public static bool performAction_Prefix(string action, Farmer who, Location tileLocation)
	//{
	//	try
	//	{
	//		if (action.Trim().StartsWith("ChooseWarp ") && who.IsLocalPlayer)
	//		{
	//			// strip out "ChooseWarp " and then split the rest of the string on spaces
	//			string[] actionParams = action.Remove(0, "ChooseWarp ".Length).Split(' ');

	//			// number of parameters should be a multiple of 4 greater than 0
	//			if (actionParams.Length < 1 || actionParams.Length % 4 != 0)
	//				throw new Exception("Incorrect number of arguments provided to ChooseWarp action." +
	//					"\nProper syntax for ChooseWarp is as follows: ChooseWarp \"[Option1StringKey]\" [x1] [y1] [LocationName1] \"[Option2StringKey]\" [x2] [y2] [LocationName2].");

	//			List<Response> responses = new();

	//			for (int index = 0; index < actionParams.Length; index += 4)
	//			{
	//				responses.Add(new Response(string.Join('¦', actionParams[(index + 1)..(index + 4)]), Game1.content.LoadString(actionParams[index].Replace("\"", ""))));
	//			}
	//			responses.Add(new Response("Cancel", Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.10993")).SetHotKey(Keys.Escape));

	//			// logging
	//			Log.Trace($"ChooseWarp dialogue created.\nOptions:\n\t{string.Join("\n\t", responses.Select(r => r.responseKey))}");

	//			Game1.currentLocation.createQuestionDialogue(" ", responses.ToArray(), "ChooseWarp");
	//			return false;
	//		}

	//		return true;
	//	}
	//	catch (Exception e)
	//	{
	//		Log.Error($"Harmony patch \"{nameof(performAction_Prefix)}\" has encountered an error while handling \"{action}\" at ({tileLocation.X}, {tileLocation.Y}) on {Game1.currentLocation}: \n{e}");
	//		return true;
	//	}
	//}


}

#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression