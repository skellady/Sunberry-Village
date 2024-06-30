using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;

namespace SunberryVillage.Utilities;

internal class Utils
{
	private static readonly Random Random = new();

	/// <summary>
	/// Generates a string of random alphanumeric characters of the specified <paramref name="length"/>.
	/// </summary>
	/// <param name="length">The length of the string to generate.</param>
	/// <returns>A string of random characters of the specified <paramref name="length"/>.</returns>
	internal static string GenerateRandomString(int length)
	{
		const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

		char[] stringChars = new char[length];

		for (int i = 0; i < stringChars.Length; i++)
		{
			stringChars[i] = validChars[Random.Next(validChars.Length)];
		}

		return new string(stringChars);
	}

	internal static string GetTranslationWithPlaceholder(string key) => Globals.TranslationHelper.Get(key).UsePlaceholder(true);
}

internal static class HarmonyUtils
{

	/// <summary>
	/// Determines whether two instructions match. Shortcuts the equality checking for the operand is the provided instruction has a Label as its operand, making matching against branch instructions easier.
	/// </summary>
	/// <param name="instructionToCheck">The instruction to check</param>
	/// <param name="instructionToMatch">The instruction to check against</param>
	/// <param name="opcodeOnly">If true, check only the opcode and ignore the operand</param>
	/// <returns>True if the instructions match, or false otherwise</returns>
	public static bool DoesInstructionMatch(CodeInstruction instructionToCheck, CodeInstruction instructionToMatch, bool opcodeOnly = false)
	{
		if (opcodeOnly)
			return instructionToCheck.opcode.Equals(instructionToMatch.opcode);

		return instructionToCheck.opcode.Equals(instructionToMatch.opcode) && (instructionToCheck.operand is Label ? instructionToMatch.operand is Label : instructionToCheck.operand?.Equals(instructionToMatch.operand) ?? instructionToMatch.operand is null);
	}


	/// <summary>
	/// Compares two sequences of CodeInstructions and determines whether or not they are equal
	/// </summary>
	/// <param name="sequenceToCheck">The list of instructions to check</param>
	/// <param name="sequenceToMatch">The list of instructions to check against</param>
	/// <param name="opcodesOnly">If true, check only the opcodes and ignore the operands</param>
	/// <returns></returns>
	public static bool DoesSequenceMatch(List<CodeInstruction> sequenceToCheck, List<CodeInstruction> sequenceToMatch, bool opcodesOnly = false)
	{
		if (sequenceToMatch.Count != sequenceToCheck.Count)
			return false;

		return !sequenceToMatch.Where((instruction, index) => !DoesInstructionMatch(sequenceToCheck[index], instruction, opcodesOnly)).Any();
	}

	/// <summary>
	/// Returns the index of the first sequence of instructions which matches the provided sequence, or -1 if none is found.
	/// </summary>
	/// <param name="sequenceToCheck">The list of instructions to search</param>
	/// <param name="sequenceToMatch">The list of instructions to try to match</param>
	/// <param name="opcodesOnly">If true, check only the opcodes and ignore the operands</param>
	/// <returns>The index of the first instance of the sequence that matches the provided sequence, or -1 if no such sequence is found</returns>
	public static int FindFirstSequenceMatch(List<CodeInstruction> sequenceToCheck, List<CodeInstruction> sequenceToMatch, bool opcodesOnly = false)
	{
		for (int index = 0; index <= sequenceToCheck.Count - sequenceToMatch.Count; index++)
		{
			if (DoesSequenceMatch(sequenceToCheck.GetRange(index, sequenceToMatch.Count), sequenceToMatch, opcodesOnly))
				return index;
		}

		return -1;
	}

	/// <summary>
	/// Returns all indices of sequences of instructions which match the provided sequence, or -1 if none are found.
	/// </summary>
	/// <param name="sequenceToCheck">The list of instructions to search</param>
	/// <param name="sequenceToMatch">The list of instructions to try to match</param>
	/// <param name="opcodesOnly">If true, check only the opcodes and ignore the operands</param>
	/// <returns>The indices of sequences that match the provided sequence, or -1 if no such sequences are found</returns>
	public static List<int> FindAllSequenceMatches(List<CodeInstruction> sequenceToCheck, List<CodeInstruction> sequenceToMatch, bool opcodesOnly = false)
	{
		List<int> matchIndices = new();

		for (int index = 0; index <= sequenceToCheck.Count - sequenceToMatch.Count; index++)
		{
			if (DoesSequenceMatch(sequenceToCheck.GetRange(index, sequenceToMatch.Count), sequenceToMatch, opcodesOnly))
				matchIndices.Add(index);
		}

		return matchIndices;
	}

	/// <summary>
	/// Returns the index of the first instance of provided CodeInstruction in provided List, or -1 if none is found.
	/// </summary>
	/// <param name="sequenceToSearch">The list of instructions to search</param>
	/// <param name="instructionToFind">The instruction to search for</param>
	/// <param name="opcodeOnly">If true, check only the opcode and ignore the operand</param>
	/// <returns>The index of the first instance of the provided CodeInstruction, or -1 if no such instruction is found</returns>
	public static int FindFirstIndex(List<CodeInstruction> sequenceToSearch, CodeInstruction instructionToFind, bool opcodeOnly = false)
	{
		return sequenceToSearch.FindIndex(inst => DoesInstructionMatch(inst, instructionToFind, opcodeOnly));
	}

	/// <summary>
	/// Returns first instance of provided CodeInstruction in provided List, or null if none is found.
	/// </summary>
	/// <param name="sequenceToSearch">The list of instructions to search</param>
	/// <param name="instructionToFind">The instruction to search for</param>
	/// <param name="opcodeOnly">If true, check only the opcode and ignore the operand</param>
	/// <returns>The first instance of the provided CodeInstruction, or null if no such instruction is found</returns>
	public static CodeInstruction FindFirst(List<CodeInstruction> sequenceToSearch, CodeInstruction instructionToFind, bool opcodeOnly = false)
	{
		return sequenceToSearch.Find(inst => DoesInstructionMatch(inst, instructionToFind, opcodeOnly));
	}
}