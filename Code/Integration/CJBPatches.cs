using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using SunberryVillage.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using SObject = StardewValley.Object;
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantAssignment
// ReSharper disable InconsistentNaming

namespace SunberryVillage.Integration;

// Boilerplate suppression for Harmony patch files
#pragma warning disable IDE0079 // Remove unnecessary suppression
// Method names reflect the original methods that they are patching, hence the naming rule violations
#pragma warning disable IDE1006 // Naming Styles
// Certain parameters have special meanings to Harmony
#pragma warning disable IDE0060 // Remove unused parameter

[HarmonyPatch]
internal class CJBPatches
{
	/*
	 *  Patches
	 */

	/// <summary>
	/// Patches <c>CJBCheatsMenu.Framework.Cheats.Time.FreezeTimeCheat</c> to treat Sunberry mines as caves for the freeze time cheat.
	/// </summary>
	[HarmonyPatch("CJBCheatsMenu.Framework.Cheats.Time.FreezeTimeCheat", "ShouldFreezeTime")]
	[HarmonyTranspiler]
	public static IEnumerable<CodeInstruction> ShouldFreezeTime_Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		List<CodeInstruction> origInstructions = new(instructions); // store unaltered instructions in case anything goes wrong
		CodeMatcher matcher = new(instructions);

		try
		{
			MethodInfo m_stringContains = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) });
			MethodInfo m_locationNameGetter = typeof(GameLocation).GetProperty(nameof(GameLocation.Name)).GetGetMethod();

			matcher.MatchEndForward(new CodeMatch(OpCodes.Brtrue_S));
			Label jumpLabel = (Label)matcher.Instruction.operand;

			matcher.Start();
			matcher.Insert(
					new CodeInstruction(OpCodes.Ldarg_2),
					new CodeInstruction(OpCodes.Call, m_locationNameGetter),
					new CodeInstruction(OpCodes.Ldstr, "Custom_SBV_Mines"),
					new CodeInstruction(OpCodes.Call, m_stringContains),
					new CodeInstruction(OpCodes.Brtrue_S, jumpLabel)
				);

			return matcher.InstructionEnumeration();
		}
		catch (Exception e)
		{
			Log.Error($"Harmony patch <{nameof(CJBPatches)}::{nameof(ShouldFreezeTime_Transpiler)}> has encountered an error while attempting to transpile <CJBCheatsMenu.Framework.Cheats.Time.FreezeTimeCheat::ShouldFreezeTime>: \n{e}");
			Log.Error("Faulty IL:\n\t" + string.Join("\n\t", matcher.Instructions().Select((instruction, i) => $"{i}\t{instruction}")));
			return origInstructions;
		}
	}
}

#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression