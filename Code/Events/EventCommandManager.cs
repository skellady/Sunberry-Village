using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Audio;
using SunberryVillage.Events.Phone;
using SunberryVillage.Events.Tarot;
using SunberryVillage.Utilities;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace SunberryVillage.Events;
internal class EventCommandManager
{
	internal static EventScriptPhoneConfession phoneScript;

	internal static bool WizardWarpInitialized = false;
	private static WizardWarpHelper warpHelper;

	internal static void AddEventHooks()
	{
		Globals.EventHelper.GameLoop.GameLaunched += RegisterEventCommands;
	}

	private static void RegisterEventCommands(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
	{
		// Tarot cutscene command
		Event.RegisterCommand("SBVTarotCutscene", (ev, _, _) => {
			ev.currentCustomEventScript = new EventScriptDialaTarot();
			ev.CurrentCommand++;
		});

		// Tarot cutscene command
		Event.RegisterCommand("SunberryTeam.SBVSMAPI_PhoneConfession", (ev, _, context) =>
		{
			if (phoneScript is null)
			{
				phoneScript = new EventScriptPhoneConfession();
				ev.currentCustomEventScript = phoneScript;
			}
			else if (phoneScript.update(context.Time, ev))
			{
				ev.currentCustomEventScript = null;
				ev.CurrentCommand++;
				phoneScript = null;
			}
		});

		// Wizard sparkles
		Event.RegisterCommand("SunberryTeam.SBVSMAPI_WizardWarpSprite", (ev, args, eventContext) => {

			if (!WizardWarpInitialized)
			{
				warpHelper = new WizardWarpHelper(ev, args, eventContext);

				if (warpHelper.HasError)
				{
					ev.CurrentCommand++;
					return;
				}

				WizardWarpInitialized = true;
			}

			if (!warpHelper.Update())
				return;

			WizardWarpInitialized = false;
			ev.CurrentCommand++;
		});
	}

	internal class WizardWarpHelper
	{
		internal bool HasError;

		private readonly Event _ev;
		private string[] _args;
		private readonly EventContext _eventContext;

		private enum AnimState
		{
			Warping,
			Landing,
			Landed,
			Finished
		}
		private AnimState _state;

		private readonly Vector2 _targetPos;
		private int _timer;
		private int _sparkleTimer;
		private int _errorTimer;
		private bool _linedUp;

		private readonly List<TemporaryAnimatedSprite> _warpingSprites = new();
		private TemporaryAnimatedSprite _landingSprite;
		private TemporaryAnimatedSprite _landedSprite;
		private TemporaryAnimatedSprite _finishedSprite;

		public WizardWarpHelper(Event ev, string[] args, EventContext eventContext)
		{
			_ev = ev;
			_args = args;
			_eventContext = eventContext;

			if (!ArgUtility.HasIndex(args, 2))
			{
				ev.LogCommandError(args, "Not enough arguments supplied");
				HasError = true;
				return;
			}

			if (!ArgUtility.TryGetInt(args, 1, out int xPos, out string error) || !ArgUtility.TryGetInt(args, 2, out int yPos, out error))
			{
				ev.LogCommandError(args, error);
				HasError = true;
				return;
			}
			
			//if (!ArgUtility.TryGetOptionalDirection(args, 3, out int direction, out error, 2))
			//{
			//	ev.LogCommandError(args, error);
			//	HasError = true;
			//	return;
			//}

			// formula for initial velocity given distance, time, and final velocity:
			// u = (2 * s / t) - v where:
			// - u is the initial velocity
			// - s is the distance
			// - t is the time
			// - v is the final velocity

			// formula for deleceration given distance, initial and final velocity:
			// a = (v^2 - u^2) / (2s) where:
			// - a is the deceleration
			// - v is the final velocity
			// - u is the initial velocity
			// - s is the distance over which the deceleration occurs

			Point targetTile = new(xPos, yPos - 1);
			_targetPos = targetTile.ToVector2() * 64f + new Vector2(4f, -16f); // i'm not gonna pretend to understand why this offset is needed. fucking npc drawing code

			int numberOfWarpClones = Utils.Random.Next(6, 10);
			const int numberOfTrails = 3;
			int initialRotationDeg = Utils.Random.Next(360);
			float rotationDeg = 360f / numberOfWarpClones;
			const float time = 150f;
			const float alphaFade = -1f / time;
			int delay = 1000 / numberOfWarpClones;

			Vector2 distanceVector = new Vector2(12f, 0f) * 64f;

			for (int i = 0; i < numberOfWarpClones; i++)
			{
				Vector2 offset = -distanceVector.RotateDegrees(initialRotationDeg + i * rotationDeg);
				Vector2 initialPos = _targetPos - offset;
				Vector2 finalVelocity = Vector2.Zero;
				Vector2 initialVelocity = new(2 * offset.X / time, 2 * offset.Y / time);
				
				float accelX = (float) ((Math.Pow(finalVelocity.X, 2) - Math.Pow(initialVelocity.X, 2)) / (2 * offset.X));
				float accelY = (float) ((Math.Pow(finalVelocity.Y, 2) - Math.Pow(initialVelocity.Y, 2)) / (2 * offset.Y));
				
				Vector2 accel = new(accelX, accelY);

				for (int j = 0; j < numberOfTrails; j++)
				{
					_warpingSprites.Add(new TemporaryAnimatedSprite(
						textureName: "Characters/Wizard",
						sourceRect: new Rectangle(32, 160, 16, 32),
						position: initialPos,
						false,
						0f,
						Color.Lerp(Color.White, Color.MediumPurple, 0.25f * j)
					)
					{
						scale = 4f,
						motion = initialVelocity,
						alpha = 0f,
						alphaFade = alphaFade /*+ (0.001f * j)*/,
						drawAboveAlwaysFront = true,
						stopAcceleratingWhenVelocityIsZero = true,
						acceleration = accel,
						usePreciseTiming = true,
						Parent = _eventContext.Location
					});

					DelayedAction.addTemporarySpriteAfterDelay(_warpingSprites[i * numberOfTrails + j], _eventContext.Location, (i + j) * delay);
				}
			}

			Game1.playSound("wand", out ICue sound);
			_ev.TrackSound(sound);

			_state = AnimState.Warping;
		}

		public bool Update()
		{
			GameTime time = Game1.currentGameTime;

			// force an error-out if command takes more than 5 seconds
			_errorTimer += time.ElapsedGameTime.Milliseconds;
			if (_errorTimer > 5000)
			{
				HasError = true;
				return true;
			}

			switch (_state)
			{
				case AnimState.Warping:
					{
						_sparkleTimer += time.ElapsedGameTime.Milliseconds;
						if (_sparkleTimer > 30)
						{
							_sparkleTimer = 0;

							TemporaryAnimatedSprite warpSprite = _warpingSprites.GetRandomElementFromList();

							_eventContext.Location.temporarySprites.Add(new TemporaryAnimatedSprite(Utils.Random.Next(10, 12), warpSprite.Position + new Vector2(Utils.Random.Next(33) - 16, Utils.Random.Next(33) - 8) * 4f, Color.MediumPurple)
							{
								motion = warpSprite.motion / 4f,
								alphaFade = 0.01f,
								layerDepth = 0.8f,
								scale = 1f,
								alpha = 0.85f
							});
						}

						if (_warpingSprites[^3].motion == Vector2.Zero && _warpingSprites[^3].alpha >= 1f)
						{
							_linedUp = true;
						}

						if (_linedUp)
						{
							foreach (TemporaryAnimatedSprite sprite in _warpingSprites)
							{
								sprite.motion = (_targetPos - sprite.Position) / 10f;
							}

							//if (!_linedUp)
							//{
							//	_linedUp = true;
							//
							//	// rough but idc it gets the job done
							//	// throw some sparkles around to hide how crappy this part looks lol
							//	foreach (TemporaryAnimatedSprite sprite in _warpingSprites)
							//	{
							//		sprite.motion = (_targetPos - sprite.Position);
							//		sprite.acceleration = -sprite.motion;
							//	}
							//}

							_timer += time.ElapsedGameTime.Milliseconds;
						}

						if (_timer >= 250)
						{
							_timer = 0;

							_landingSprite = new TemporaryAnimatedSprite(
								textureName: "Characters/Wizard",
								sourceRect: new Rectangle(16, 160, 16, 32),
								position: _targetPos,
								flipped: false,
								alphaFade: 0f,
								color: Color.White
							)
							{
								scale = 4f,
								Parent = _eventContext.Location
							};

							foreach(TemporaryAnimatedSprite sprite in _warpingSprites)
								_eventContext.Location.temporarySprites.Remove(sprite);

							_eventContext.Location.temporarySprites.Add(_landingSprite);

							_state = AnimState.Landing;
						}

						return false;
					}

				case AnimState.Landing:
					{
						_sparkleTimer += time.ElapsedGameTime.Milliseconds;
						if (_sparkleTimer > 40)
						{
							_sparkleTimer = 0;

							_eventContext.Location.temporarySprites.Add(new TemporaryAnimatedSprite(Utils.Random.Next(10, 12), _landingSprite.Position + new Vector2(Utils.Random.Next(33) - 16, Utils.Random.Next(33) - 8) * 4f, Color.MediumPurple)
							{
								motion = _landingSprite.motion / 4f,
								alphaFade = 0.01f,
								layerDepth = 0.8f,
								scale = 1f,
								alpha = 0.85f
							});
						}

						_timer += Game1.currentGameTime.ElapsedGameTime.Milliseconds;
						
						if (_timer >= 150)
						{
							_timer = 0;
							
							_landedSprite = new TemporaryAnimatedSprite(
								textureName: "Characters/Wizard",
								sourceRect: new Rectangle(0, 160, 16, 32),
								position: _targetPos,
								flipped: false,
								alphaFade: 0f,
								color: Color.White
							)
							{
								scale = 4f,
								Parent = _eventContext.Location
							};

							_eventContext.Location.temporarySprites.Remove(_landingSprite);
							_eventContext.Location.temporarySprites.Add(_landedSprite);

							_state = AnimState.Landed;
						}

						return false;
					}

				case AnimState.Landed:
					{
						_sparkleTimer += time.ElapsedGameTime.Milliseconds;
						if (_sparkleTimer > 50)
						{
							_sparkleTimer = 0;
							
							_eventContext.Location.temporarySprites.Add(new TemporaryAnimatedSprite(Utils.Random.Next(10, 12), _landedSprite.Position + new Vector2(Utils.Random.Next(33) - 16, Utils.Random.Next(33) - 8) * 4f, Color.MediumPurple)
							{
								motion = _landedSprite.motion / 4f,
								alphaFade = 0.01f,
								layerDepth = 0.8f,
								scale = 1f,
								alpha = 0.85f
							});
						}

						_timer += Game1.currentGameTime.ElapsedGameTime.Milliseconds;
						
						if (_timer >= 150)
						{
							_timer = 0;
							
							_finishedSprite = new TemporaryAnimatedSprite(
								textureName: "Characters/Wizard",
								sourceRect: new Rectangle(32, 128, 16, 32),
								position: _targetPos,
								flipped: false,
								alphaFade: 0f,
								color: Color.White
							)
							{
								scale = 4f,
								Parent = _eventContext.Location
							};

							_eventContext.Location.temporarySprites.Remove(_landedSprite);
							_eventContext.Location.temporarySprites.Add(_finishedSprite);

							_state = AnimState.Finished;
						}

						return false;
					}

				case AnimState.Finished:
					{
						_sparkleTimer += time.ElapsedGameTime.Milliseconds;
						if (_sparkleTimer > 60)
						{
							_sparkleTimer = 0;
							
							_eventContext.Location.temporarySprites.Add(new TemporaryAnimatedSprite(Utils.Random.Next(10, 12), _finishedSprite.Position + new Vector2(Utils.Random.Next(33) - 16, Utils.Random.Next(33) - 8) * 4f, Color.MediumPurple)
							{
								motion = _finishedSprite.motion / 4f,
								alphaFade = 0.01f,
								layerDepth = 0.8f,
								scale = 1f,
								alpha = 0.85f
							});
						}

						_timer += Game1.currentGameTime.ElapsedGameTime.Milliseconds;

						if (_timer >= 150)
						{
							_eventContext.Location.temporarySprites.Remove(_finishedSprite);

							NPC wizard = _ev.actors.Find(npc => npc.Name.Equals("Wizard"));
							if (wizard is not null)
							{
								wizard.position.X = _ev.OffsetPositionX(_targetPos.X);
								wizard.position.Y = _ev.OffsetPositionY(_targetPos.Y + 80f);
								wizard.IsWalkingInSquare = false;
							}

							return true;
						}
						return false;
					}

				default:
					return false;
			}
		}

	}
}
