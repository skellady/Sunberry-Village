using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.GameData.FishPonds;
using SObject = StardewValley.Object;
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantAssignment
// ReSharper disable InconsistentNaming

namespace SunberryVillage.Objects;

// Boilerplate suppression for Harmony patch files
#pragma warning disable IDE0079 // Remove unnecessary suppression
// Method names reflect the original methods that they are patching, hence the naming rule violations
#pragma warning disable IDE1006 // Naming Styles
// Certain parameters have special meanings to Harmony
#pragma warning disable IDE0060 // Remove unused parameter

[HarmonyPatch]
internal class BuildingPatches
{
	/*
	 *  Patches
	 */

	/// <summary>
	/// Patches <c>FishPond.doFishSpecificWaterColoring</c> to allow for custom water colors. Only gets called once a day per pond so it's fine to pull fish data.
	/// </summary>
	[HarmonyPatch(typeof(FishPond), "doFishSpecificWaterColoring")]
	[HarmonyPrefix]
	public static bool doFishSpecificWaterColoring_Prefix(FishPond __instance)
	{
		FishPondData data = __instance.GetFishPondData();

		if (__instance.currentOccupants.Value < 1 || data?.CustomFields is null || !data.CustomFields.TryGetValue("SunberryTeam.SBVSMAPI_FishPondWaterColor", out string waterColor))
		{
			__instance.overrideWaterColor.Value = Color.White;
			return true;
		}

		Color? parsedColor = Utility.StringToColor(waterColor);

		if (!parsedColor.HasValue)
			return true;

		if (!data.CustomFields.TryGetValue("SunberryTeam.SBVSMAPI_FishPondWaterBlendMode", out string blendMode))
		{
			blendMode = "soft";
		}

		if (blendMode == "hard")
		{
			if (__instance.currentOccupants.Value > 2)
			{
				__instance.overrideWaterColor.Value = parsedColor.Value;
			}
		}
		else
		{
			float lerpNum = (float) __instance.currentOccupants.Value / 10;

			Color lerpColor = Color.Lerp(__instance.GetParentLocation().waterColor.Value, parsedColor.Value, lerpNum);
			__instance.overrideWaterColor.Value = lerpColor;
		}
		
		return false;
	}

	/// <summary>
	/// Patches <c>FishPond.addFishToPond</c> to update water color right away.
	/// </summary>
	[HarmonyPatch(typeof(FishPond), "addFishToPond")]
	[HarmonyPostfix]
	public static void addFishToPond_Postfix(FishPond __instance)
	{
		doFishSpecificWaterColoring_Prefix(__instance);
	}

	/// <summary>
	/// Patches <c>FishPond.CatchFish</c> to update water color right away.
	/// </summary>
	[HarmonyPatch(typeof(FishPond), nameof(FishPond.CatchFish))]
	[HarmonyPostfix]
	public static void CatchFish_Postfix(FishPond __instance)
	{
		doFishSpecificWaterColoring_Prefix(__instance);
	}

	/// <summary>
	/// Patches <c>Object.performUseAction</c> to handle Sunberry totem logic.
	/// </summary>
	[HarmonyPatch(typeof(SObject), nameof(SObject.performUseAction))]
	[HarmonyPrefix]
	public static bool performUseAction_Prefix(SObject __instance, ref GameLocation location, ref bool __result)
	{
		// basically just do exactly what vanilla does for warp totems

		// skip if item is not the sunberry warp totem or shouldn't be interacted with, or if not in normal gameplay (copied from vanilla code)
		if (!__instance.QualifiedItemId.Equals("(O)skellady.SBVCP_WarpTotemSunberry") || !Game1.player.CanMove ||
		    __instance.isTemporarilyInvisible || Game1.eventUp || Game1.isFestival() || Game1.fadeToBlack ||
		    Game1.player.swimming.Value || Game1.player.bathingClothes.Value || Game1.player.onBridge.Value)
			return true;

		Game1.player.jitterStrength = 1f;
		Game1.player.faceDirection(2);
		Game1.player.CanMove = false;
		Game1.player.temporarilyInvincible = true;
		Game1.player.temporaryInvincibilityTimer = -4000;
		
		location.playSound("warrior");
		Game1.changeMusicTrack("silence");

		Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[]
			{
				new(57, 2000, secondaryArm: false, flip: false),
				new((short)Game1.player.FarmerSprite.CurrentFrame, 0, secondaryArm: false, flip: false, totemWarp, behaviorAtEndOfFrame: true)
			});

		Vector2 spritePos1 = Game1.player.Position + new Vector2(0f, -96f);
		Vector2 spritePos2 = spritePos1 + new Vector2(-64f, 0f);

		TemporaryAnimatedSprite sprite1 = new(
			initialParentTileIndex: 0,
			animationInterval: 9999f,
			animationLength: 1,
			numberOfLoops: 999,
			position: spritePos1,
			flicker: false,
			flipped: false,
			verticalFlipped: false,
			rotation: 0f)
		{
			motion = new Vector2(0f, -1f),
			scaleChange = 0.01f,
			alpha = 1f,
			alphaFade = 0.0075f,
			shakeIntensity = 1f,
			initialPosition = spritePos1,
			xPeriodic = true,
			xPeriodicLoopTime = 1000f,
			xPeriodicRange = 4f,
			layerDepth = 1f
		};
		sprite1.CopyAppearanceFromItemId(__instance.QualifiedItemId);

		TemporaryAnimatedSprite sprite2 = sprite1.getClone();
		sprite2.motion = new Vector2(0f, -0.5f);
		sprite2.scaleChange = 0.005f;
		sprite2.scale = 0.5f;
		sprite2.delayBeforeAnimationStart = 10;
		sprite2.position = spritePos2;
		sprite2.initialPosition = spritePos2;
		sprite2.layerDepth = 0.9999f;

		TemporaryAnimatedSprite sprite3 = sprite2.getClone();
		sprite3.delayBeforeAnimationStart = 20;
		sprite3.layerDepth = 0.9988f;

		Game1.Multiplayer.broadcastSprites(location, sprite1, sprite2, sprite3);

		Color sprinkleColor = new(101, 27, 104);    // special purple sprinkles >:3
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