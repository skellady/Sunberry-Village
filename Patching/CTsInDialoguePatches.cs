using System.Collections.Generic;
using System.Text.RegularExpressions;
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
class CTsInDialoguePatches
{

	/// <summary>
	/// Patches <c>Dialogue.checkForSpecialDialogueAttributes</c> to check for CT addition.<br />
	/// Syntax:  ===CTName=== for a CT with a duration of 1 day, or
	///			 ===CTName/[number]=== for a CT with a duration of [number] days.<br />
	///	Example: ===MyConversationTopic1/20=== would add a CT with name MyConversationTopic1 and duration of 20 days.
	/// </summary>
	[HarmonyPatch(typeof(Dialogue), "checkForSpecialDialogueAttributes")]
	[HarmonyPostfix]
	public static void checkForSpecialDialogueAttributes_Postfix(Dialogue __instance)
	{
		// get current dialogue line
		string currentDialogue = __instance.dialogues[__instance.currentDialogueIndex];

		// define pattern and search for matches in current line
		Regex pattern = new(@"={3}(?<ctName>[A-Za-z0-9_]+)(?:\/(?<duration>\d+))?={3}");
		MatchCollection matches = pattern.Matches(currentDialogue);

		// for each match, add a CT with the corresponding ctName group. If duration is specified, use that duration; otherwise, default to 1.
		foreach(Match match in matches)
		{
			string ctName = match.Groups["ctName"].Value;
			int duration = (match.Groups["duration"].Value != "") ? int.Parse(match.Groups["duration"].Value) : 1;

			if (Game1.player.activeDialogueEvents.ContainsKey(ctName))
				Game1.player.activeDialogueEvents[ctName] = duration;
			else
				Game1.player.activeDialogueEvents.Add(ctName, duration);
		}

		// remove any matches from dialogue
		if (matches.Count > 0)
			__instance.dialogues[__instance.currentDialogueIndex] = pattern.Replace(currentDialogue, "");
	}
}

#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression