using HarmonyLib;
using StardewValley;
using StardewValley.Menus;
using SunberryVillage.PortraitShake;
using SunberryVillage.Utilities;
using System;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantAssignment
// ReSharper disable InconsistentNaming

namespace SunberryVillage.Patching;

// Boilerplate suppression for Harmony patch files
#pragma warning disable IDE0079 // Remove unnecessary suppression
// Method names reflect the original methods that they are patching, hence the naming rule violations
#pragma warning disable IDE1006 // Naming Styles
// Certain parameters have special meanings to Harmony
#pragma warning disable IDE0060 // Remove unused parameter

[HarmonyPatch]
internal class PortraitShakePatches
{

	/// <summary>
	/// Patches <c>DialogueBox.shouldPortraitShake</c> to handle custom shaking portraits.
	/// </summary>
	[HarmonyPatch(typeof(DialogueBox), "shouldPortraitShake")]
	[HarmonyPrefix]
	public static bool shouldPortraitShake_Prefix(Dialogue d, ref bool __result)
	{
		try
		{
			__result = PortraitShakeHandler.PortraitShouldShake.Value;
			return !__result;
		}
		catch (Exception e)
		{
			Log.Error($"Harmony patch \"{nameof(PortraitShakePatches)}::{nameof(shouldPortraitShake_Prefix)}\" has encountered an error while handling dialogue \"{d.dialogues[d.currentDialogueIndex]}\": \n{e}");
			return true;
		}
	}

	/// <summary>
	/// Patches <c>DialogueBox.receiveLeftClick</c> to check if the portrait should shake whenever the text is advanced.
	/// </summary>
	/// <param name="__instance"></param>
	[HarmonyPatch(typeof(DialogueBox), nameof(DialogueBox.receiveLeftClick))]
	[HarmonyPostfix]
	public static void receiveLeftClick_Postfix(DialogueBox __instance)
	{
		try
		{
			if (__instance.characterDialogue is not null)
				PortraitShakeHandler.SetShake(__instance.characterDialogue);
		}
		catch (Exception e)
		{
			Log.Error($"Harmony patch \"{nameof(PortraitShakePatches)}::{nameof(shouldPortraitShake_Prefix)}\" has encountered an error while handling dialogue box for dialogue \"{__instance.characterDialogue?.dialogues[__instance.characterDialogue?.currentDialogueIndex ?? 0] ?? "[unknown dialogue]"}\": \n{e}");
		}
	}
}

#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression