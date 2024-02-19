using Microsoft.Xna.Framework;
using StardewValley;
using SunberryVillage.Tarot;
using SunberryVillage.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunberryVillage.Events;
internal class EventCommandManager
{
	internal static bool WizardWarpInitialized = false;
	private static WizardWarpHelper warpHelper;

	internal static void AddEventHooks()
	{
		Globals.EventHelper.GameLoop.GameLaunched += RegisterEventCommands;
	}

	private static void RegisterEventCommands(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
	{
		// Tarot cutscene command
		Event.RegisterCommand("SBVTarotCutscene", (ev, args, eventContext) => {
			ev.currentCustomEventScript = new EventScriptDialaTarot();
			ev.CurrentCommand++;
		});

		// Wizard sparkles
		Event.RegisterCommand("SunberryTeam.SBVSMAPI_WizardWarpSprite", (ev, args, eventContext) => {

			if (!WizardWarpInitialized)
			{
				warpHelper = new (ev, args, eventContext);
				WizardWarpInitialized = true;
			}

			if (warpHelper.Update())
			{
				WizardWarpInitialized = false;
				ev.CurrentCommand++;
				return;
			}
			else
			{
				return;
			}

			//eventContext.Location.TemporarySprites.Add(
			//	new TemporaryAnimatedSprite(
			//		textureName: "LooseSprites\\Cursors",
			//		sourceRect: new Rectangle(387, 1965, 16, 31),
			//		animationInterval: 9999f,
			//		animationLength: 1,
			//		numberOfLoops: 999999,
			//		position: new Vector2(xVal, yVal) * 64f + new Vector2(0f, 4f),
			//		flicker: false,
			//		flipped: false,
			//		layerDepth: 1f,
			//		alphaFade: 0f,
			//		color: Color.White,
			//		scale: 4f,
			//		scaleChange: 0f,
			//		rotation: 0f,
			//		rotationChange: 0f)
			//	{
			//		motion = new Vector2(2f, -2f),
			//		acceleration = new Vector2(0.1f, 0f),
			//		scaleChange = -0.02f,
			//		alphaFade = 0.001f
			//	});
		});
	}

	private class WizardWarpHelper
	{
		int xVal = 0;
		int yVal = 0;

		int counter = 0;

		TemporaryAnimatedSprite warp;

		public WizardWarpHelper(Event ev, string[] args, EventContext eventContext)
		{
			if (args.Length < 3)
			{
				ev.LogCommandError(args, "Not enough arguments supplied", false);
			}
			else
			{
				bool xParsed = int.TryParse(args[1], out xVal);
				bool yParsed = int.TryParse(args[2], out yVal);

				if (!xParsed || !yParsed)
				{
					ev.LogCommandError(args, "One or more arguments not provided in correct format", false);
				}
			}

			warp = new TemporaryAnimatedSprite(
				textureName: "LooseSprites\\Cursors",
				sourceRect: new Rectangle(387, 1965, 16, 31),
				animationInterval: 9999f,
				animationLength: 1,
				numberOfLoops: 999999,
				position: new Vector2(xVal - 10, yVal - 10) * 64f,
				flicker: false,
				flipped: false,
				layerDepth: 1f,
				alphaFade: 0f,
				color: Color.White * 0f,
				scale: 0f,
				scaleChange: 0.05f,
				rotation: 0f,
				rotationChange: 0f
			)
			{
				motion = new Vector2(2f, 2f),
				alphaFade = -0.1f
			};
		}

		internal bool Update()
		{
			if (warp.Position.X / 64f >= xVal && warp.Position.Y / 64f >= yVal)
			{
				warp.Position = new(xVal, yVal);
				warp.motion = Vector2.Zero;
			}

			counter++;
			
			if (warp.color.A < 255)
				return false;

			return true;

		}
	}
}
