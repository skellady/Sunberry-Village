using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using SunberryVillage.Utilities;
using System;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantAssignment
// ReSharper disable InconsistentNaming

namespace SunberryVillage.Animations;

// Boilerplate suppression for Harmony patch files
#pragma warning disable IDE0079 // Remove unnecessary suppression
// Method names reflect the original methods that they are patching, hence the naming rule violations
#pragma warning disable IDE1006 // Naming Styles
// Certain parameters have special meanings to Harmony
#pragma warning disable IDE0060 // Remove unused parameter

[HarmonyPatch]
internal class AnimationsPatches
{
	/// <summary>
	/// Patches <c>NPC.startRouteBehavior</c> to check if the specified animation is defined in the animation data asset, and if so, sets the appropriate values for the NPC.
	/// </summary>
	/// <param name="__instance">The NPC being checked.</param>
	/// <param name="behaviorName">The animation to check the asset for.</param>
	[HarmonyPatch(typeof(NPC), "startRouteBehavior")]
	[HarmonyPostfix]
	internal static void startRouteBehavior_Postfix(ref NPC __instance, string behaviorName)
	{
		try
		{
			if (behaviorName.Length > 0 && behaviorName[0] == '"')
				return;

			if (!AnimationsHandler.AnimationData.TryGetValue(behaviorName, out AnimationDataModel animation))
				return;

			if (animation.Size != Vector2.Zero)
			{
				__instance.extendSourceRect((int)animation.Size.X - 16, (int)animation.Size.Y - 32);
				__instance.Sprite.SpriteWidth = (int)animation.Size.X;
				__instance.Sprite.SpriteHeight = (int)animation.Size.Y;
			}

			if(animation.ExtraAnimations != null)
			{
				foreach (var extraAnimation in animation.ExtraAnimations)
				{
					Multiplayer multiplayer = Globals.Helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
					multiplayer.broadcastSprites(Utility.getGameLocationOfCharacter(__instance),
						new TemporaryAnimatedSprite(extraAnimation.TextureName,
						new Microsoft.Xna.Framework.Rectangle(0, 0, (int)extraAnimation.Size.X, (int)extraAnimation.Size.Y),
						extraAnimation.AnimationInterval,
						extraAnimation.Frames,
						999999,
						__instance.Position + extraAnimation.Offset,
						flicker: false,
						flipped: false,
						0.0002f,
						0f,
						Color.White,
						4f,
						0f,
						0f,
						0f)
						{
							id = extraAnimation.Name.GetHashCode()
						});
                }
			}

			__instance.drawOffset = animation.Offset;
			__instance.Sprite.ignoreSourceRectUpdates = false;
			__instance.HideShadow = animation.HideShadow;
		}
		catch (Exception e)
		{
			Log.Error($"Harmony patch \"{nameof(AnimationsPatches)}::{nameof(startRouteBehavior_Postfix)}\" has encountered an error for {__instance.Name} with {behaviorName}: \n{e}");
		}
	}

	/// <summary>
	/// Patches <c>NPC.finishRouteBehavior</c> to clean up any changes made to the NPC as part of the animation.
	/// </summary>
	/// <param name="__instance">The NPC being modified.</param>
	/// <param name="behaviorName">The animation that was just concluded.</param>
	/// <returns><c>True</c> to run the original method's code after this method completes.</returns>
	[HarmonyPatch(typeof(NPC), "finishRouteBehavior")]
	[HarmonyPrefix]
	private static bool finishRouteBehavior_Prefix(ref NPC __instance, string behaviorName)
	{
		try
		{
			if (behaviorName.Length > 0 && behaviorName[0] == '"')
				return true;

			if (AnimationsHandler.AnimationData.TryGetValue(behaviorName, out AnimationDataModel animationDataModel))
            {
				if(animationDataModel.ExtraAnimations != null)
				{
					foreach(var extraAnimation in animationDataModel.ExtraAnimations) { 
                        Utility.getGameLocationOfCharacter(__instance).removeTemporarySpritesWithID(extraAnimation.Name.GetHashCode());
                    }
				}
            } else
            {
                return true;
            }

			__instance.reloadSprite();
			__instance.Sprite.SpriteWidth = 16;
			__instance.Sprite.SpriteHeight = 32;
			__instance.Sprite.UpdateSourceRect();
			__instance.drawOffset = Vector2.Zero;
			__instance.Halt();
			__instance.movementPause = 1;
			__instance.HideShadow = false;
			return true;
		}
		catch (Exception e)
		{
			Log.Error($"Harmony patch \"{nameof(AnimationsPatches)}::{nameof(finishRouteBehavior_Prefix)}\" has encountered an error for {__instance.Name} with {behaviorName}: \n{e}");
			return true;
		}
	}
}

#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression