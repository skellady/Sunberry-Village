using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using SObject = StardewValley.Object;
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantAssignment
// ReSharper disable InconsistentNaming

namespace SunberryVillage.Items;

// Boilerplate suppression for Harmony patch files
#pragma warning disable IDE0079 // Remove unnecessary suppression
// Method names reflect the original methods that they are patching, hence the naming rule violations
#pragma warning disable IDE1006 // Naming Styles
// Certain parameters have special meanings to Harmony
#pragma warning disable IDE0060 // Remove unused parameter

[HarmonyPatch]
internal class ObjectPatches
{
	/*
	 *  Patches
	 */

	/// <summary>
	/// Patches <c>GameLocation.answerDialogueAction</c> to check for response to ChooseDestination question and handle it accordingly.
	/// </summary>
	[HarmonyPatch(typeof(SObject), nameof(SObject.performUseAction))]
	[HarmonyPrefix]
	public static bool performUseAction_Prefix(SObject __instance, ref GameLocation location, ref bool __result)
	{
		// basically just do exactly what vanilla does for warp totems

		// early out 1 if player is immobile or object shouldn't be interacted with
		if (!Game1.player.canMove || __instance.isTemporarilyInvisible)
		{
			return true;
		}

		// early out 2 if not in normal gameplay (copied from vanilla code) or the item is no the sunberry warp totem
		bool normal_gameplay = !Game1.eventUp && !Game1.isFestival() && !Game1.fadeToBlack && !Game1.player.swimming.Value && !Game1.player.bathingClothes.Value && !Game1.player.onBridge.Value;
		if (!normal_gameplay || !__instance.QualifiedItemId.Equals("(O)skellady.SBVCP_WarpTotemSunberry"))
		{
			return true;
		}

		Game1.player.jitterStrength = 1f;
		Color sprinkleColor = new(101, 27, 104);    // special purple sprinkles >:3
		location.playSound("warrior");
		Game1.player.faceDirection(2);
		Game1.player.CanMove = false;
		Game1.player.temporarilyInvincible = true;
		Game1.player.temporaryInvincibilityTimer = -4000;
		Game1.changeMusicTrack("silence");
		Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[2]
			{
					new FarmerSprite.AnimationFrame(57, 2000, secondaryArm: false, flip: false),
					new FarmerSprite.AnimationFrame((short)Game1.player.FarmerSprite.CurrentFrame, 0, secondaryArm: false, flip: false, totemWarp, behaviorAtEndOfFrame: true)
			});
		TemporaryAnimatedSprite sprite = new(0, 9999f, 1, 999, Game1.player.Position + new Vector2(0f, -96f), flicker: false, flipped: false, verticalFlipped: false, 0f)
		{
			motion = new Vector2(0f, -1f),
			scaleChange = 0.01f,
			alpha = 1f,
			alphaFade = 0.0075f,
			shakeIntensity = 1f,
			initialPosition = Game1.player.Position + new Vector2(0f, -96f),
			xPeriodic = true,
			xPeriodicLoopTime = 1000f,
			xPeriodicRange = 4f,
			layerDepth = 1f
		};
		sprite.CopyAppearanceFromItemId(__instance.QualifiedItemId);
		Game1.Multiplayer.broadcastSprites(location, sprite);
		sprite = new(0, 9999f, 1, 999, Game1.player.Position + new Vector2(-64f, -96f), flicker: false, flipped: false, verticalFlipped: false, 0f)
		{
			motion = new Vector2(0f, -0.5f),
			scaleChange = 0.005f,
			scale = 0.5f,
			alpha = 1f,
			alphaFade = 0.0075f,
			shakeIntensity = 1f,
			delayBeforeAnimationStart = 10,
			initialPosition = Game1.player.Position + new Vector2(-64f, -96f),
			xPeriodic = true,
			xPeriodicLoopTime = 1000f,
			xPeriodicRange = 4f,
			layerDepth = 0.9999f
		};
		sprite.CopyAppearanceFromItemId(__instance.QualifiedItemId);
		Game1.Multiplayer.broadcastSprites(location, sprite);
		sprite = new(0, 9999f, 1, 999, Game1.player.Position + new Vector2(64f, -96f), flicker: false, flipped: false, verticalFlipped: false, 0f)
		{
			motion = new Vector2(0f, -0.5f),
			scaleChange = 0.005f,
			scale = 0.5f,
			alpha = 1f,
			alphaFade = 0.0075f,
			delayBeforeAnimationStart = 20,
			shakeIntensity = 1f,
			initialPosition = Game1.player.Position + new Vector2(64f, -96f),
			xPeriodic = true,
			xPeriodicLoopTime = 1000f,
			xPeriodicRange = 4f,
			layerDepth = 0.9988f
		};
		sprite.CopyAppearanceFromItemId(__instance.QualifiedItemId);
		Game1.Multiplayer.broadcastSprites(location, sprite);
		Game1.screenGlowOnce(sprinkleColor, hold: false);
		Utility.addSprinklesToLocation(location, Game1.player.TilePoint.X, Game1.player.TilePoint.Y, 16, 16, 1300, 20, Color.White, null, motionTowardCenter: true);

		__result = true;
		return false;
	}

	/*
	 *  Helper Methods - copied straight from vanilla
	 */

	private static void totemWarp(Farmer who)
	{
		GameLocation location = who.currentLocation;
		for (int i = 0; i < 12; i++)
		{
			Game1.Multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(354, Game1.random.Next(25, 75), 6, 1, new Vector2(Game1.random.Next((int)who.Position.X - 256, (int)who.Position.X + 192), Game1.random.Next((int)who.Position.Y - 256, (int)who.Position.Y + 192)), flicker: false, Game1.random.NextBool()));
		}
		who.playNearbySoundAll("wand");
		Game1.displayFarmer = false;
		Game1.player.temporarilyInvincible = true;
		Game1.player.temporaryInvincibilityTimer = -2000;
		Game1.player.freezePause = 1000;
		Game1.flashAlpha = 1f;
		DelayedAction.fadeAfterDelay(totemWarpForReal, 1000);
		int j = 0;
		Point playerTile = who.TilePoint;
		for (int x = playerTile.X + 8; x >= playerTile.X - 8; x--)
		{
			Game1.Multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(6, new Vector2(x, playerTile.Y) * 64f, Color.White, 8, flipped: false, 50f)
			{
				layerDepth = 1f,
				delayBeforeAnimationStart = j * 25,
				motion = new Vector2(-0.25f, 0f)
			});
			j++;
		}
	}

	private static void totemWarpForReal()
	{
		Game1.warpFarmer("Custom_SBV_SunberryVillage", 6, 9, flip: false);
		Game1.fadeToBlackAlpha = 0.99f;
		Game1.screenGlow = false;
		Game1.player.temporarilyInvincible = false;
		Game1.player.temporaryInvincibilityTimer = 0;
		Game1.displayFarmer = true;
	}
}

#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression