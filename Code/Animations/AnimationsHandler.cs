using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using SunberryVillage.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local

namespace SunberryVillage.Animations;

internal class AnimationsHandler
{
	internal const string AnimationsAssetPath = "SunberryTeam.SBV/Animations";
	internal static IAssetName AnimationsAssetName = Globals.GameContent.ParseAssetName(AnimationsAssetPath);
	internal static Dictionary<string, AnimationDataModel> AnimationData;

	#region Logic

	/// <summary>
	/// Initialize animation data asset.
	/// </summary>
	private static void Init()
	{
		AnimationData = Globals.GameContent.Load<Dictionary<string, AnimationDataModel>>(AnimationsAssetPath);
	}

	#endregion

	#region Event Hooks

	/// <summary>
	/// Adds Animations event hooks.
	/// </summary>
	internal static void AddEventHooks()
	{
		Globals.EventHelper.Content.AssetRequested += Animations_AssetRequested;
		Globals.EventHelper.Content.AssetsInvalidated += Animations_AssetsInvalidated;
		Globals.EventHelper.GameLoop.SaveLoaded += (_, _) => Init();
		Globals.EventHelper.GameLoop.DayEnding += DayEnd;
	}

	/// <summary>
	/// If animation data asset invalidated, reload asset.
	/// </summary>
	private static void Animations_AssetsInvalidated(object sender, AssetsInvalidatedEventArgs e)
	{
		if (e.NamesWithoutLocale.Contains(AnimationsAssetName))
			AnimationData = Globals.GameContent.Load<Dictionary<string, AnimationDataModel>>(AnimationsAssetPath);
	}

	/// <summary>
	/// If animation asset requested, provide empty <c>Dictionary&lt;<see cref="string"/>, <see cref="AnimationDataModel"/>&gt;</c>.
	/// </summary>
	private static void Animations_AssetRequested(object sender, AssetRequestedEventArgs e)
	{
		if (e.NameWithoutLocale.IsEquivalentTo(AnimationsAssetPath))
			e.LoadFrom(() => new Dictionary<string, AnimationDataModel>(), AssetLoadPriority.Low);
	}

	/// <summary>
	/// Resets sprites in case user ended day while animations were in progress.
	/// </summary>
	public static void DayEnd(object sender, DayEndingEventArgs args)
	{
		try
		{
			foreach (string npcName in AnimationData.Select(entry => entry.Value.NpcName))
			{
				NPC npc = Game1.getCharacterFromName(npcName);

				if (npc is null)
				{
					Log.Warn($"Failed to find NPC named \"{npcName}\" from asset {AnimationsAssetPath}. Skipping this entry.");
					continue;
				}

				npc.Sprite.SpriteHeight = 32;
				npc.Sprite.SpriteWidth = 16;
				npc.Sprite.ignoreSourceRectUpdates = false;
				npc.Sprite.UpdateSourceRect();
				npc.drawOffset.Value = Vector2.Zero;
				npc.IsInvisible = false;
				npc.HideShadow = false;
			}
		}
		catch (Exception e)
		{
			Log.Error($"Failed in {nameof(AnimationsHandler)}::{nameof(DayEnd)}:\n{e}");
		}
	}

	#endregion
}

/// <summary>
/// Data model for Content Patcher integration
/// </summary>
internal class AnimationDataModel
{
	public string NpcName;
	public Vector2 Size;
	public Vector2 Offset;
	public bool HideShadow;
}