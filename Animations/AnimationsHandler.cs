using Microsoft.Xna.Framework;
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
	internal static Dictionary<string, AnimationDataModel> AnimationData;

	/// <summary>
	/// Resets sprites in case user ended day while animations were in progress.
	/// </summary>
	public static void DayEnd(object sender, DayEndingEventArgs args)
	{
		try
		{
			foreach (NPC npc in AnimationData.Select(entry => Game1.getCharacterFromName(entry.Value.NpcName)))
			{
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
			Log.Error($"Failed in SBV Animations Day End reset:\n{e}");
		}
	}
}

#pragma warning disable CS0649 // Remove unused variable

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

#pragma warning restore CS0649