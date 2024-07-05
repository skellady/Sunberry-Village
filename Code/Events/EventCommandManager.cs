using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using SunberryVillage.Events.Phone;
using SunberryVillage.Events.Tarot;
using SunberryVillage.Utilities;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace SunberryVillage.Events;
internal class EventCommandManager
{
	internal static EventScriptPhoneConfession PhoneScript;
	internal static WizardWarpIn WarpIn;
	internal static WizardWarpOut WarpOut;

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

		// Phone cutscene (WIP)
		Event.RegisterCommand("SunberryTeam.SBVSMAPI_PhoneConfession", (ev, _, context) =>
		{
			if (PhoneScript is null)
			{
				PhoneScript = new EventScriptPhoneConfession();
				ev.currentCustomEventScript = PhoneScript;
			}
			else if (PhoneScript.update(context.Time, ev))
			{
				ev.currentCustomEventScript = null;
				ev.CurrentCommand++;
				PhoneScript = null;
			}
		});

		WarpIn = new WizardWarpIn();
		WarpOut = new WizardWarpOut();

		// Wizard warp in
		Event.RegisterCommand("SunberryTeam.SBVSMAPI_WizardWarpIn", (ev, args, eventContext) => WarpIn.Initialize(ev, args, eventContext));
		Event.RegisterCommand("SunberryTeam.SBVSMAPI_WaitForWizardWarpIn", (ev, _, _) =>
		{
			if (!WarpIn.IsActive)
				ev.CurrentCommand++;
		});

		// Wizard warp out
		Event.RegisterCommand("SunberryTeam.SBVSMAPI_WizardWarpOut", (ev, args, eventContext) => WarpOut.Initialize(ev, args, eventContext));
		Event.RegisterCommand("SunberryTeam.SBVSMAPI_WaitForWizardWarpOut", (ev, _, _) =>
		{
			if (!WarpOut.IsActive)
				ev.CurrentCommand++;
		});
	}

	internal class WizardWarpIn
	{
		internal bool IsActive;

		private Event _ev;
		private string[] _args;
		private EventContext _eventContext;

		private enum AnimState
		{
			Warping,
			Landing,
			Landed,
			Finished
		}
		private AnimState _state;

		private Vector2 _targetPos;
		private int _timer;
		private int _sparkleTimer;
		private int _errorTimer;
		private bool _linedUp;

		private readonly List<TemporaryAnimatedSprite> _warpingSprites = new();
		private TemporaryAnimatedSprite _landingSprite;
		private TemporaryAnimatedSprite _landedSprite;
		private TemporaryAnimatedSprite _finishedSprite;

		private NPC _wizard;

		private bool _initialized;

		public void Initialize(Event ev, string[] args, EventContext eventContext)
		{
			if (_initialized)
				return;

			_ev = ev;
			_args = args;
			_eventContext = eventContext;

			if (!ArgUtility.HasIndex(_args, 2))
			{
				_ev.LogCommandErrorAndSkip(_args, "Not enough arguments supplied");
				return;
			}

			_wizard = _ev.actors.Find(npc => npc.Name.Equals("Wizard"));
			if (_wizard is null)
			{
				_ev.LogCommandErrorAndSkip(_args, "Wizard not present in event");
				return;
			}

			if (!ArgUtility.TryGetInt(_args, 1, out int xPos, out string error)
			    || !ArgUtility.TryGetInt(_args, 2, out int yPos, out error)
			    || !ArgUtility.TryGetOptionalBool(_args, 3, out bool @continue, out error))
			{
				_ev.LogCommandErrorAndSkip(_args, error);
				return;
			}

			_timer = 0;
			_sparkleTimer = 0;
			_errorTimer = 0;
			_linedUp = false;
			_initialized = true;

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
						alphaFade = alphaFade + 0.001f * j,
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
			
			DelayedAction.functionAfterDelay(Update, 15);
			
			_state = AnimState.Warping;

			if (@continue)
			{
				_ev.CurrentCommand++;
				_initialized = false;
			}

			IsActive = true;
		}
		
		private void Update()
		{
			GameTime time = Game1.currentGameTime;

			// if event is over back out
			if (_ev.CurrentCommand >= _ev.eventCommands.Length)
			{
				_initialized = false;
				IsActive = false;
				return;
			}

			// force an error-out if command takes more than 5 seconds
			_errorTimer += time.ElapsedGameTime.Milliseconds;
			if (_errorTimer > 5000)
			{
				_ev.LogCommandErrorAndSkip(_args, "Command timed out");
				_initialized = false;
				IsActive = false;
				return;
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

							_warpingSprites.Clear();
							_eventContext.Location.temporarySprites.Add(_landingSprite);

							_state = AnimState.Landing;
						}

						break;
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

						break;
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

						break;
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

							_wizard.position.X = _ev.OffsetPositionX(_targetPos.X);
							_wizard.position.Y = _ev.OffsetPositionY(_targetPos.Y + 80f);
							_wizard.IsWalkingInSquare = false;

							_initialized = false;
							IsActive = false;
							_ev.CurrentCommand++;
							return;
						}

						break;
					}
			}

			DelayedAction.functionAfterDelay(Update, 15);
		}
	}

	internal class WizardWarpOut
	{
		internal bool IsActive;

		private Event _ev;
		private string[] _args;
		private EventContext _eventContext;
		
		//private TemporaryAnimatedSprite _warpingSprite;
		private TemporaryAnimatedSprite _warpingSpriteWhite;

		private NPC _wizard;

		private bool _initialized;

		public void Initialize(Event ev, string[] args, EventContext eventContext)
		{
			if (_initialized)
				return;

			_ev = ev;
			_args = args;
			_eventContext = eventContext;

			_initialized = true;

			_wizard = _ev.actors.Find(npc => npc.Name.Equals("Wizard"));
			if (_wizard is null)
			{
				_ev.LogCommandErrorAndSkip(_args, "Wizard not present in event");
				return;
			}

			if (!ArgUtility.TryGetOptionalBool(_args, 1, out bool @continue, out string error))
			{
				_ev.LogCommandErrorAndSkip(_args, error);
				return;
			}

			int warpTime = 200;
			float alphaChange = 1 / (warpTime / 16f);
			
			//_warpingSprite = new TemporaryAnimatedSprite(
			//	textureName: "Characters\\Wizard", 
			//	sourceRect: new Rectangle(0, 128, 16, 32),
			//	position: _wizard.Position + new Vector2(0, -80f),
			//	flipped: false,
			//	alphaFade: alphaChange,
			//	color: Color.White
			//)
			//{
			//	scale = 4f
			//};

			Texture2D wizardSheet = Globals.GameContent.Load<Texture2D>("Characters/Wizard");
			int width = wizardSheet.Width;
			int height = wizardSheet.Height;
			Color[] data = new Color[width * height];
			wizardSheet.GetData(data);

			Texture2D whiteOverlay = new (Game1.graphics.GraphicsDevice, width, height);
			whiteOverlay.SetData(GetWhiteWizardSprite(data));

			_warpingSpriteWhite = new TemporaryAnimatedSprite(
				textureName: "Characters\\Wizard", 
				sourceRect: new Rectangle(0, 128, 16, 32),
				position: _wizard.Position + new Vector2(0, -80f),
				flipped: false,
				alphaFade: alphaChange,
				color: Color.White
			)
			{
				scale = 4f,
				texture = whiteOverlay,
				alpha = 0f,
				alphaFade = -alphaChange
			};

			Game1.playSound("wand", out ICue sound);
			_ev.TrackSound(sound);
			
			DelayedAction.functionAfterDelay(() =>
			{
				_wizard.position.X = -6400f;
				_wizard.position.Y = -6400f;
				_wizard.IsWalkingInSquare = false;

				_warpingSpriteWhite.alphaFade = alphaChange;
			}, warpTime);
			
			DelayedAction.functionAfterDelay(() =>
			{
				IsActive = false;
				if (!@continue)
				{
					_initialized = false;
					_ev.CurrentCommand++;
				}
			}, warpTime * 2);
			
			//_eventContext.Location.temporarySprites.Add(_warpingSprite);
			_eventContext.Location.temporarySprites.Add(_warpingSpriteWhite);


			if (@continue)
			{
				_ev.CurrentCommand++;
				_initialized = false;
			}

			IsActive = true;
		}

		private static Color[] GetWhiteWizardSprite(Color[] data)
		{
			if (data is null)
			{
				throw new ArgumentNullException(nameof(data));
			}

			Color[] output = new Color[data.Length];

			for (int i = 0; i < data.Length; i++)
			{
				output[i] = data[i].A > 0 ? Color.White : Color.Transparent;
			}

			return output;
		}
	}

}
