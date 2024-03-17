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
			if (behaviorName.Length > 0 && behaviorName[0] == '"' || !AnimationsManager.AnimationData.TryGetValue(behaviorName, out AnimationDataModel animation))
				return;

			if (animation.Size != Vector2.Zero)
			{
				int width = (int)animation.Size.X;
				int height = (int)animation.Size.Y;

				__instance.extendSourceRect(width - 16, height - 32);
				__instance.Sprite.SpriteWidth = width;
				__instance.Sprite.SpriteHeight = height;
			}

			if (animation.ExtraAnimations != null)
			{
				foreach (ExtraAnimation extraAnimation in animation.ExtraAnimations)
				{
					TemporaryAnimatedSprite extraAnim = new(
						textureName: extraAnimation.TextureName,
						sourceRect: new Rectangle(0, 0, (int)extraAnimation.Size.X, (int)extraAnimation.Size.Y),
						animationInterval: extraAnimation.AnimationInterval,
						animationLength: extraAnimation.Frames,
						numberOfLoops: 999999,
						position: __instance.Position + extraAnimation.Offset,
						flicker: false,
						flipped: false,
						layerDepth: 0.0002f,
						alphaFade: 0f,
						color: Color.White,
						scale: 4f,
						scaleChange: 0f,
						rotation: 0f,
						rotationChange: 0f)
					{
						id = extraAnimation.Name.GetHashCode()
					};

					Game1.Multiplayer.broadcastSprites(Utility.getGameLocationOfCharacter(__instance), extraAnim);
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
			if (behaviorName.Length > 0 && behaviorName[0] == '"' || !AnimationsManager.AnimationData.TryGetValue(behaviorName, out AnimationDataModel animationDataModel))
				return true;

			__instance.reloadSprite();
			__instance.Sprite.SpriteWidth = 16;
			__instance.Sprite.SpriteHeight = 32;
			__instance.Sprite.UpdateSourceRect();
			__instance.drawOffset = Vector2.Zero;
			__instance.Halt();
			__instance.movementPause = 1;
			__instance.HideShadow = false;

			if (animationDataModel.ExtraAnimations == null)
				return true;

			foreach (ExtraAnimation extraAnimation in animationDataModel.ExtraAnimations)
			{
				Utility.getGameLocationOfCharacter(__instance).removeTemporarySpritesWithID(extraAnimation.Name.GetHashCode());
			}

			return true;
		}
		catch (Exception e)
		{
			Log.Error($"Harmony patch \"{nameof(AnimationsPatches)}::{nameof(finishRouteBehavior_Prefix)}\" has encountered an error for \"{__instance.Name}\" with behavior \"{behaviorName}\": \n{e}");
			return true;
		}
	}
}

#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression