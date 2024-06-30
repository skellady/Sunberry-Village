using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using SunberryVillage.Utilities;
using System.Collections.Generic;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local

namespace SunberryVillage.TextEmojis;

internal class EmojiManager
{
	internal const string EmojiAssetPath = "SunberryTeam.SBV/Emojis";
	internal static IAssetName EmojisAssetName = Globals.GameContent.ParseAssetName(EmojiAssetPath);
	internal static Dictionary<char, Rectangle> EmojiIndices = new();

	#region Logic

	/// <summary>
	/// Initialize emoji data asset.
	/// </summary>
	internal static void Init()
	{
		LoadData();
	}

	private static void LoadData()
	{
        EmojiIndices = Globals.GameContent.Load<Dictionary<char, Rectangle>>(EmojiAssetPath);
	}

	#endregion

	#region Event Hooks

	/// <summary>
	/// Adds emoji event hooks.
	/// </summary>
	internal static void AddEventHooks()
	{
		Globals.EventHelper.Content.AssetRequested += Emoji_AssetRequested;
		Globals.EventHelper.Content.AssetsInvalidated += Emoji_AssetsInvalidated;
		Globals.EventHelper.GameLoop.SaveLoaded += (_, _) => Init();

    }

	/// <summary>
	/// If emoji data asset invalidated, reload asset.
	/// </summary>
	private static void Emoji_AssetsInvalidated(object sender, AssetsInvalidatedEventArgs e)
	{
		if (e.NamesWithoutLocale.Contains(EmojisAssetName))
            EmojiIndices = Globals.GameContent.Load<Dictionary<char, Rectangle>>(EmojiAssetPath);
	}

	/// <summary>
	/// If emoji asset requested, provide empty <c>Dictionary&lt;<see cref="int"/>, <see cref="Rectangle"/>&gt;</c>.
	/// </summary>
	private static void Emoji_AssetRequested(object sender, AssetRequestedEventArgs e)
	{
		if (e.NameWithoutLocale.IsEquivalentTo(EmojiAssetPath))
			e.LoadFrom(() => new Dictionary<char, Rectangle>(), AssetLoadPriority.Low);
	}

	#endregion
}