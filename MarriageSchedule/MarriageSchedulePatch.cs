using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using StardewValley;

namespace SunberryVillage.MarriageSchedule;

[HarmonyPatch]
internal class MarriageSchedulePatch
{


    /// <summary>
    /// Patches NPC.marriageDuties to make Jonghyuk use the Patio on Friday
    /// </summary>
    /// <param name="instructions"></param>
    /// <returns></returns>
    [HarmonyPatch(typeof(NPC), nameof(NPC.marriageDuties))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> marriageDuties_Transpiler(IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        List<CodeInstruction> origInstructions = [..instructions]; // store unaltered instructions in case anything goes wrong
        List<CodeInstruction> newInstructions = [..origInstructions];
        try
        {

            
            MethodInfo m_setUpOutdoorPatioActivity = typeof(NPC).GetMethod(nameof(NPC.setUpForOutdoorPatioActivity));

            List<CodeInstruction> sequenceToFind =
            [
                /*
                 * 	IL_0404: ldarg.0
                 *  IL_0405: call instance void StardewValley.NPC::setUpForOutdoorPatioActivity()
                 *  IL_040a: ret
                 */
                new(OpCodes.Ldarg_0),
                new(OpCodes.Call, m_setUpOutdoorPatioActivity),
                new(OpCodes.Ret)
            ];
            
            int index = HarmonyUtils.FindFirstSequenceMatch(newInstructions, sequenceToFind);
            
            if (index == -1)
            {
                Log.Warn($"Failed to match sequence. Aborting transpiler {nameof(marriageDuties_Transpiler)}.");
                return origInstructions;
            }
            //change jumplabel so jumps are to the start of my code
            CodeInstruction instruction = newInstructions[index + sequenceToFind.Count];
            List<Label> jumpLabels = instruction.labels;
            Label newLabel = generator.DefineLabel();
            instruction.labels = new List<Label>() { newLabel };
            
            int insertIndex = index + sequenceToFind.Count;
            
            MethodInfo m_setupOutdoorPatioActivityForJonghyuk = typeof(MarriageSchedulePatch).
                GetMethod(nameof(setupOutDoorPatioActivityForJonghyuk));
            
            List<CodeInstruction> sequenceToInsert =
            [
                new(OpCodes.Ldarg_0)
                {
                    labels = jumpLabels
                },
                new(OpCodes.Call, m_setupOutdoorPatioActivityForJonghyuk),
                new(OpCodes.Brfalse_S, newLabel),
                new (OpCodes.Ret)
            ];
            
            newInstructions.InsertRange(insertIndex, sequenceToInsert);
            
            return newInstructions;
        }
        catch (Exception e)
        {
            Log.Error($"Harmony patch <{nameof(NPC)}::{nameof(marriageDuties_Transpiler)}> has encountered an error while attempting to transpile <{nameof(NPC)}::{nameof(NPC.marriageDuties)}>: \n{e}");
            Log.Error("Faulty IL:\n\t" + string.Join("\n\t", newInstructions.Select((instruction, i) => $"{i}\t{instruction}")));
            return origInstructions;
        }
    }

    public static bool setupOutDoorPatioActivityForJonghyuk(NPC npc)
    {
        if (npc.Name.Equals("Jonghyuk") && !Game1.isRaining && !Game1.IsWinter &&
            Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Fri") && npc.getSpouse() == Game1.MasterPlayer)
        {
            npc.setUpForOutdoorPatioActivity();
            return true;
        }

        return false;
    }
}