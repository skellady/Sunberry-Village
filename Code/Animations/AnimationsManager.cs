using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local

namespace SunberryVillage.Animations;

internal class AnimationsManager
{
	internal const string ScheduleAnimationsAssetPath = "SunberryTeam.SBV/Animations";
	internal const string SpouseAnimationsAssetPath = "SunberryTeam.SBV/SpouseAnimations";
	internal static IAssetName AnimationsAssetName = Globals.GameContent.ParseAssetName(ScheduleAnimationsAssetPath);
	internal static IAssetName SpouseAnimationsAssetName = Globals.GameContent.ParseAssetName(SpouseAnimationsAssetPath);
	internal static Dictionary<string, AnimationDataModel> ScheduleAnimationData = new();
	internal static Dictionary<string, SpouseAnimationDataModel> SpouseAnimationData = new();

	#region Logic

	/// <summary>
	/// Initialize animation data asset.
	/// </summary>
	internal static void Init()
	{
		LoadData();
	}

	private static void LoadData()
	{
		ScheduleAnimationData = Globals.GameContent.Load<Dictionary<string, AnimationDataModel>>(ScheduleAnimationsAssetPath);
		SpouseAnimationData = Globals.GameContent.Load<Dictionary<string, SpouseAnimationDataModel>>(SpouseAnimationsAssetPath);
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
			ScheduleAnimationData =
				Globals.GameContent.Load<Dictionary<string, AnimationDataModel>>(ScheduleAnimationsAssetPath);
		if (e.NamesWithoutLocale.Contains(SpouseAnimationsAssetName))
			SpouseAnimationData = Globals.GameContent.Load<Dictionary<string, SpouseAnimationDataModel>>(SpouseAnimationsAssetPath);
	}

	/// <summary>
	/// If animation asset requested, provide empty <c>Dictionary&lt;<see cref="string"/>, <see cref="AnimationDataModel"/>&gt;</c>.
	/// </summary>
	private static void Animations_AssetRequested(object sender, AssetRequestedEventArgs e)
	{
		if (e.NameWithoutLocale.IsEquivalentTo(ScheduleAnimationsAssetPath))
			e.LoadFrom(() => new Dictionary<string, AnimationDataModel>(), AssetLoadPriority.Low);
		else if (e.NameWithoutLocale.IsEquivalentTo(SpouseAnimationsAssetPath))
			e.LoadFrom(() => new Dictionary<string, SpouseAnimationDataModel>(), AssetLoadPriority.Low);
			
	}

	/// <summary>
	/// Resets sprites in case user ended day while animations were in progress.
	/// </summary>
	public static void DayEnd(object sender, DayEndingEventArgs args)
	{
		try
		{
			foreach (var animation in ScheduleAnimationData.Values)
			{
				NPC npc = Game1.getCharacterFromName(animation.NpcName);

				if (npc is null)
				{
					Log.Warn($"Failed to find NPC named \"{animation.NpcName}\" from asset {ScheduleAnimationsAssetPath}. Skipping this entry.");
					continue;
				}

				npc.Sprite.SpriteHeight = 32;
				npc.Sprite.SpriteWidth = 16;
				npc.Sprite.ignoreSourceRectUpdates = false;
				npc.Sprite.UpdateSourceRect();
				npc.drawOffset = Vector2.Zero;
				npc.IsInvisible = false;
				npc.HideShadow = false;

				if (animation.ExtraAnimations is not null)
				{
					foreach (var extraAnimation in animation.ExtraAnimations)
					{
						Utility.getGameLocationOfCharacter(npc).removeTemporarySpritesWithID(extraAnimation.Name.GetHashCode());
					}
				}
			}
		}
		catch (Exception e)
		{
			Log.Error($"Failed in {nameof(AnimationsManager)}::{nameof(DayEnd)}:\n{e}");
		}
	}

	#endregion
}

#pragma warning disable CS0649

/// <summary>
/// Data model for NPC animations
/// </summary>
internal class AnimationDataModel
{
	public string NpcName;
	public Vector2 Size;
	public Vector2 Offset;
	public bool HideShadow;
	public List<ExtraAnimation> ExtraAnimations;
}

/// <summary>
/// Data model for extra animations
/// </summary>
internal class ExtraAnimation
{
	public string Name;
	public Vector2 Offset;
	public Vector2 Size;
	public float AnimationInterval = 5000f;
	public int Frames;
	public string TextureName;
}

internal class SpouseAnimationDataModel
{
	public string NpcName;
	public Vector2 Size;
	public Vector2 Offset;
	public bool HideShadow;
	public List<int[]> SpriteAnimationFrames;
	public List<ExtraAnimation> ExtraAnimations;
}

#pragma warning restore CS0649