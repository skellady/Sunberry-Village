using StardewModdingAPI.Utilities;
using StardewValley;
using SunberryVillage.Utilities;
using System.Collections.Generic;

namespace SunberryVillage.PortraitShake;

internal class PortraitShakeHandler
{
	internal static Dictionary<string, PortraitShakeModel> PortraitsDict = new();

	internal static readonly PerScreen<bool> PortraitShouldShake = new();

	/// <summary>
	/// Checks whether the given NPC and portrait index are in the data asset.
	/// </summary>
	/// <param name="dialogue">The dialogue to check.</param>
	/// <returns><c>True</c> if the portrait should shake, <c>False</c> otherwise.</returns>
	internal static bool ShouldPortraitShake(Dialogue dialogue)
	{
		string name = dialogue.speaker.Name;
		int index = dialogue.getPortraitIndex();

		return PortraitsDict.ContainsKey(name) && PortraitsDict[name].IndexList.Contains(index);
	}

	/// <summary>
	/// Caches the return value per-screen to reduce the amount of code being called each tick.
	/// </summary>
	/// <param name="dialogue">The dialogue being displayed on this screen, which is being checked to see if the portrait should shake.</param>
	internal static void SetShake(Dialogue dialogue)
	{
		PortraitShouldShake.Value = ShouldPortraitShake(dialogue);
	}

	/// <summary>
	/// Reloads the internal asset once per day if it has changed since the last load.
	/// </summary>
	internal static void ReloadAsset()
	{
		PortraitsDict = Globals.GameContent.Load<Dictionary<string, PortraitShakeModel>>("SunberryTeam.SBV/PortraitsToShake");
	}
}