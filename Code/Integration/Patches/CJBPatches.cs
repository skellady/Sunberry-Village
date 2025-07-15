using HarmonyLib;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantAssignment
// ReSharper disable InconsistentNaming

namespace SunberryVillage.Integration.Patches;

// Boilerplate suppression for Harmony patch files
#pragma warning disable IDE0079 // Remove unnecessary suppression
// Method names reflect the original methods that they are patching, hence the naming rule violations
#pragma warning disable IDE1006 // Naming Styles
// Certain parameters have special meanings to Harmony
#pragma warning disable IDE0060 // Remove unused parameter

[HarmonyPatch]
internal class CJBPatches
{
	internal const string MinesString = "Custom_SBV_Mines";

    /*
	 *  Patches
	 */

    /// <summary>
    /// Patches <c>CJBCheatsMenu.Framework.Cheats.Time.FreezeTimeCheat</c> to treat Sunberry mines as caves for the freeze time cheat.
    /// </summary>
    public static IEnumerable<CodeInstruction> ShouldFreezeTime_Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		List<CodeInstruction> origInstructions = [.. instructions]; // store unaltered instructions in case anything goes wrong
		CodeMatcher matcher = new(instructions);

		try
		{
			// get needed method info
			MethodInfo m_stringContains = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)]);
			MethodInfo m_locationNameGetter = typeof(GameLocation).GetProperty(nameof(GameLocation.Name)).GetGetMethod();

			// get needed labels
            Label jumpIfSunberryMinesLabel = (Label)matcher.Start().Advance(2).Instruction.operand;
			Label jumpIfNotSunberryMinesLabel = (Label)matcher.MatchStartForward(
					new CodeMatch(OpCodes.Brfalse_S),
					new CodeMatch(OpCodes.Ldc_I4_1)
				).Instruction.operand;

            // check if given location is null. if not, check if it contains mines string in the name
            // jump accordingly
            matcher.Start().Insert(
                    new CodeInstruction(OpCodes.Ldarg_2),
					new CodeInstruction(OpCodes.Ldnull),
					new CodeInstruction(OpCodes.Ceq),
                    new CodeInstruction(OpCodes.Brtrue_S, jumpIfNotSunberryMinesLabel),
                    new CodeInstruction(OpCodes.Ldarg_2),
					new CodeInstruction(OpCodes.Call, m_locationNameGetter),
					new CodeInstruction(OpCodes.Ldstr, MinesString),
					new CodeInstruction(OpCodes.Call, m_stringContains),
					new CodeInstruction(OpCodes.Brtrue_S, jumpIfSunberryMinesLabel)
				);

            return matcher.InstructionEnumeration();
		}
		catch (Exception e)
		{
			Log.Error($"Harmony patch <{nameof(CJBPatches)}::{nameof(ShouldFreezeTime_Transpiler)}> has encountered an error while attempting to transpile <CJBCheatsMenu.Framework.Cheats.Time.FreezeTimeCheat::ShouldFreezeTime>: \n{e}");
			Log.Trace("Faulty IL:\n\t" + string.Join("\n\t", matcher.Instructions().Select((instruction, i) => $"{i}\t{instruction}")));
			return origInstructions;
		}
	}
}

#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression