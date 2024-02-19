using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using SunberryVillage.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace SunberryVillage.Portrait;

internal class PortraitManager
{
	internal const string PortraitAssetPath = "SunberryTeam.SBV/PortraitsToShake";
	internal static IAssetName PortraitAssetName = Globals.GameContent.ParseAssetName(PortraitAssetPath);
	internal static Dictionary<string, PortraitModel> PortraitsDict = new();
	internal static readonly PerScreen<bool> PortraitShouldShake = new();

	#region Logic

	private static void Init()
	{
		PortraitsDict = Globals.GameContent.Load<Dictionary<string, PortraitModel>>(PortraitAssetPath);
	}

	/// <summary>
	/// Checks to see if the portrait for the given <paramref name="dialogue"/> should shake, and caches the value in a <see cref="PerScreen{bool}"/>.
	/// </summary>
	/// <param name="dialogue">The dialogue being displayed on this screen, which is being checked to see if the portrait should shake.</param>
	internal static void SetShake(Dialogue dialogue)
	{
		if (dialogue?.speaker is null)
		{
			PortraitShouldShake.Value = false;
			return;
		}

		string name = dialogue.speaker.Name;
		int index = dialogue.getPortraitIndex();

		PortraitShouldShake.Value = PortraitsDict.ContainsKey(name) && PortraitsDict[name].IndexList.Contains(index);
	}

	#endregion

	#region Event Hooks

	/// <summary>
	/// Adds Portrait Shake event hooks.
	/// </summary>
	internal static void AddEventHooks()
	{
		Globals.EventHelper.Content.AssetRequested += Portrait_AssetRequested;
		Globals.EventHelper.Content.AssetsInvalidated += Portrait_AssetsInvalidated;
		Globals.EventHelper.GameLoop.SaveLoaded += (_, _) => Init();
		Globals.EventHelper.Display.MenuChanged += CheckForPortraitShake;
	}

	private static void Portrait_AssetsInvalidated(object sender, AssetsInvalidatedEventArgs e)
	{
		if (e.NamesWithoutLocale.Contains(PortraitAssetName))
			PortraitsDict = Globals.GameContent.Load<Dictionary<string, PortraitModel>>(PortraitAssetPath);
	}

	private static void Portrait_AssetRequested(object sender, AssetRequestedEventArgs e)
	{
		if (e.NameWithoutLocale.IsEquivalentTo(PortraitAssetPath))
			e.LoadFrom(() => new Dictionary<string, PortraitModel>(), AssetLoadPriority.Low);
	}

	/// <summary>
	/// When a DialogueBox menu opens, checks to see if the portrait should shake. 
	/// </summary>
	private static void CheckForPortraitShake(object sender, MenuChangedEventArgs e)
	{
		if (e.NewMenu is DialogueBox { characterDialogue: { } charDialogue })
			SetShake(charDialogue);
	}

	#endregion
}

/// <summary>
/// Data model for Content Patcher integration.
/// </summary>
internal class PortraitModel
{
	public List<int> IndexList = new();
}