using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace SunberryVillage.Events.Phone;

internal class EventScriptPhoneConfession : ICustomEventScript
{
	private static Vector2 _screenCenterPosition;
	private static Vector2 _phonePositionOffset;
	private static Vector2 _phonePosition;

	private static Texture2D _phoneTexture;
	private static Texture2D _messageTexture;
	private static Texture2D _phoneUiTexture;

	private static string TranslatedContactName;

	private readonly MessageList Messages;

	private int fadeInTimer = 60;
	private int fadeOutTimer = 60;
	private int timer;
	private bool done;

	public EventScriptPhoneConfession()
	{
		_screenCenterPosition = new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width / 2f,
			Game1.graphics.GraphicsDevice.Viewport.Height / 2f);

		_phoneTexture = Globals.GameContent.Load<Texture2D>("SunberryTeam.SBV/Phone");
		_messageTexture = Globals.GameContent.Load<Texture2D>("SunberryTeam.SBV/Phone/MessageBox");
		_phoneUiTexture = Globals.GameContent.Load<Texture2D>("SunberryTeam.SBV/Phone/Overlay");

		_phonePositionOffset = new Vector2(_phoneTexture.Width * 2, _phoneTexture.Height * 2);
		_phonePosition = _screenCenterPosition - _phonePositionOffset;

		List<MessageBox> ChronologicalList =
		[
			new(Utils.GetTranslationWithPlaceholder("PhoneConfession.Message.1"), MessageBox.MessageSource.Derya),
			new(Utils.GetTranslationWithPlaceholder("PhoneConfession.Message.2"), MessageBox.MessageSource.Diala),
			new(Utils.GetTranslationWithPlaceholder("PhoneConfession.Message.3"), MessageBox.MessageSource.Derya),
			new(Utils.GetTranslationWithPlaceholder("PhoneConfession.Message.4"), MessageBox.MessageSource.Derya),
			new(Utils.GetTranslationWithPlaceholder("PhoneConfession.Message.5"), MessageBox.MessageSource.Diala),
			new(Utils.GetTranslationWithPlaceholder("PhoneConfession.System.Yesterday"), MessageBox.MessageSource.System),
			new(Utils.GetTranslationWithPlaceholder("PhoneConfession.Message.6"), MessageBox.MessageSource.Derya),
			new(Utils.GetTranslationWithPlaceholder("PhoneConfession.Message.7") + " <", MessageBox.MessageSource.Diala),
			new(Utils.GetTranslationWithPlaceholder("PhoneConfession.Message.8"), MessageBox.MessageSource.Derya)
		];
		ChronologicalList.Reverse();

		Messages = new MessageList(this);
		Messages.Messages.AddRange(ChronologicalList);

		TranslatedContactName = Game1.getCharacterFromName("DialaSBV").getName();

		timer = 0;
	}

	public void drawAboveAlwaysFront(SpriteBatch b)
	{
		// black background
		b.Draw(
			texture: Game1.staminaRect,
			destinationRectangle: new Rectangle(0, 0, 1920, 1080),
			sourceRectangle: Game1.staminaRect.Bounds,
			color: Color.Black,
			rotation: 0f,
			origin: Vector2.Zero,
			effects: SpriteEffects.None,
			layerDepth: 0f
		);

		// phone
		b.Draw(
			texture: _phoneTexture,
			position: _phonePosition,
			sourceRectangle: new Rectangle(0, 0, _phoneTexture.Width, _phoneTexture.Height),
			color: Color.White,
			rotation: 0f,
			origin: Vector2.Zero,
			scale: 4f,
			effects: SpriteEffects.None,
			layerDepth: 0.5f
		);
		
		Messages.Draw(b);

		// phone ui top section
		b.Draw(
			texture: _phoneUiTexture,
			position: _phonePosition,
			sourceRectangle: new Rectangle(0, 0, _phoneUiTexture.Width, _phoneUiTexture.Height),
			color: Color.White,
			rotation: 0f,
			origin: Vector2.Zero,
			scale: 4f,
			effects: SpriteEffects.None,
			layerDepth: 0.5f
		);

		b.DrawString(
			spriteFont: Game1.smallFont,
			text: TranslatedContactName,
			//position: _textPos,
			new Vector2(_screenCenterPosition.X - 30, _phonePosition.Y + 174),
			color: Color.Black,
			0f,
			Vector2.Zero,
			Vector2.One,
			SpriteEffects.None,
			0.51f
		);

		// draw fade
		b.Draw(
			texture: Game1.staminaRect,
			destinationRectangle: new Rectangle(0, 0, 1920, 1080),
			sourceRectangle: Game1.staminaRect.Bounds,
			color: Color.Black * ((float)fadeInTimer / 60),
			rotation: 0f,
			origin: Vector2.Zero,
			effects: SpriteEffects.None,
			layerDepth: 0f
		);

		if (done)
		{
			b.Draw(
				texture: Game1.staminaRect,
				destinationRectangle: new Rectangle(0, 0, 1920, 1080),
				sourceRectangle: Game1.staminaRect.Bounds,
				color: Color.Black * ((float)(60 - fadeOutTimer) / 60),
				rotation: 0f,
				origin: Vector2.Zero,
				effects: SpriteEffects.None,
				layerDepth: 0f
			);
		}
	}

	public void draw(SpriteBatch b) { }

	public bool update(GameTime time, Event e)
	{
		_screenCenterPosition = new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width / 2f,
			Game1.graphics.GraphicsDevice.Viewport.Height / 2f);

		if (fadeInTimer > 0)
		{
			fadeInTimer--;
			return false;
		}

		if (done)
		{
			// ReSharper disable once InvertIf
			if (fadeOutTimer > 0)
			{
				fadeOutTimer--;
				return false;
			}

			return true;
		}

		timer++;

		Messages.Update(timer);

		return false;
	}

	internal class MessageBox
	{
		internal int MaxWidth = 240;
		
		internal string Contents;
		internal string DisplayContents;
		internal MessageSource Source;
		internal bool Typing;
		internal Vector2 Size;

		internal enum MessageSource
		{
			Diala,
			Derya,
			System
		}

		internal MessageBox(string contents, MessageSource source, bool typing = false)
		{
			Contents = Game1.parseText(contents, Game1.smallFont, MaxWidth);
			Source = source;
			Typing = typing;

			DisplayContents = Typing ? " " : Contents;

			RecalculateSize();
		}

		private void RecalculateSize()
		{

			Size = Typing ? Game1.smallFont.MeasureString(". . .") + new Vector2(8, 6) : Game1.smallFont.MeasureString(DisplayContents) + new Vector2(8, 6);
		}

		internal void Draw(SpriteBatch b, int yPosition)
		{
			switch (Source)
			{
				case MessageSource.Diala:
				{
					IClickableMenu.drawTextureBox(b: b,
						texture: _messageTexture,
						sourceRect: _messageTexture.Bounds,
						x: (int) _phonePosition.X + 50,
						y: yPosition - (int)Size.Y,
						width: (int) Size.X,
						height: (int) Size.Y,
						color: Color.White,
						scale: 2f,
						drawShadow: false,
						draw_layer: 0.51f
					);

					b.DrawString(
						spriteFont: Game1.smallFont,
						text: DisplayContents,
						position: new Vector2(x: _phonePosition.X + 56, y: yPosition - (int)Size.Y + 3),
						color: Color.Black,
						rotation: 0f,
						origin: Vector2.Zero,
						scale: Vector2.One,
						effects: SpriteEffects.None,
						layerDepth: 0.52f
					);
					break;
				}

				case MessageSource.Derya:
				{
					IClickableMenu.drawTextureBox(b: b,
						texture: _messageTexture,
						sourceRect: _messageTexture.Bounds,
						x: (int)_phonePosition.X + _phoneTexture.Width * 4 - 50 - (int)Size.X,
						y: yPosition - (int)Size.Y,
						width: (int)Size.X,
						height: (int)Size.Y,
						color: Color.DodgerBlue * 0.8f,
						scale: 2f,
						drawShadow: false,
						draw_layer: 0.51f
					);

					b.DrawString(
						spriteFont: Game1.smallFont,
						text: DisplayContents,
						position: new Vector2(x: _phonePosition.X + _phoneTexture.Width * 4 - 50 - Size.X + 4, y: yPosition - (int)Size.Y + 4),
						color: Color.Black,
						rotation: 0f,
						origin: Vector2.Zero,
						scale: Vector2.One,
						effects: SpriteEffects.None,
						layerDepth: 0.52f
					);
					break;
				}

				case MessageSource.System:
				{
					b.DrawString(
						spriteFont: Game1.smallFont,
						text: DisplayContents,
						position: new Vector2(x: _screenCenterPosition.X - Size.X / 2, y: yPosition - (int)Size.Y),
						color: Color.Black * 0.75f,
						rotation: 0f,
						origin: Vector2.Zero,
						scale: Vector2.One,
						effects: SpriteEffects.None,
						layerDepth: 0.52f
					);
					break;
				}

				default:
					throw new Exception("Message source not recognized.");
			}
		}

		internal void Update(int time)
		{
			if (DisplayContents == Contents)
				return;

			if (!Typing)
			{
				DisplayContents = Contents;
				RecalculateSize();
				return;
			}

			if (time % 30 != 0)
				return;

			switch (DisplayContents)
			{
				case ". . .":
				case " ":
					DisplayContents = ".";
					break;

				default:
					DisplayContents += " .";
					break;
			}

			RecalculateSize();
		}

	}

	internal class MessageList
	{
		internal List<MessageBox> Messages;
		internal EventScriptPhoneConfession Script;

		internal int SpacerHeight = 5;
		internal int BottomEdge = 85;
		internal int CurrentYPos;

		internal Dictionary<int, Action> TimedActions;

		internal MessageList(EventScriptPhoneConfession script)
		{
			Script = script;
			Messages = [];
			InitializeTimedActions();
		}

		internal void Draw(SpriteBatch b)
		{
			CurrentYPos = (int)_phonePosition.Y + _phoneTexture.Height * 4 - BottomEdge;

			foreach (MessageBox message in Messages.TakeWhile(_ => CurrentYPos >= (int)_phonePosition.Y + 200))
			{
				message.Draw(b, CurrentYPos);
				
				CurrentYPos -= (int)message.Size.Y + SpacerHeight;
			}
		}

		internal void Update(int time)
		{
			if (TimedActions.TryGetValue(time, out Action action))
			{
				action.Invoke();
			}

			foreach (MessageBox message in Messages)
			{
				message.Update(time);
			}
		}

		private void InitializeTimedActions()
		{
			// hesitation to send message - ... shows up then goes away, then shows up, then goes away

			TimedActions = new Dictionary<int, Action>
			{
				[SecondsToTicks(3)] = () =>
				{
					Messages.Insert(0, new MessageBox(Utils.GetTranslationWithPlaceholder("PhoneConfession.Message.9"), MessageBox.MessageSource.Diala, true));
				},
				[SecondsToTicks(7)] = () =>
				{
					Messages.RemoveAt(0);
				},
				[SecondsToTicks(9)] = () =>
				{
					Messages.Insert(0, new MessageBox(Utils.GetTranslationWithPlaceholder("PhoneConfession.Message.9"), MessageBox.MessageSource.Diala, true));
				},
				[SecondsToTicks(11)] = () =>
				{
					Messages.RemoveAt(0);
				},
				[SecondsToTicks(14)] = () =>
				{
					Messages.Insert(0, new MessageBox(Utils.GetTranslationWithPlaceholder("PhoneConfession.Message.9"), MessageBox.MessageSource.Diala, true));
				},
				[SecondsToTicks(17.8f)] = () =>
				{
					Messages[0].Typing = false;
					Messages.Insert(1, new MessageBox(Utils.GetTranslationWithPlaceholder("PhoneConfession.System.Today"), MessageBox.MessageSource.System));
				},
				[SecondsToTicks(18.6f)] = () =>
				{
					Messages.Insert(0, new MessageBox(Utils.GetTranslationWithPlaceholder("PhoneConfession.Message.10"), MessageBox.MessageSource.Diala, true));
				},
				[SecondsToTicks(19.5f)] = () =>
				{
					Messages[0].Typing = false;
				},
				[SecondsToTicks(20.5f)] = () =>
				{
					Messages.Insert(0, new MessageBox(Utils.GetTranslationWithPlaceholder("PhoneConfession.Message.11")+"*", MessageBox.MessageSource.Diala, true));
				},
				[SecondsToTicks(21.1f)] = () =>
				{
					Messages[0].Typing = false;
				},
				[SecondsToTicks(22)] = () =>
				{
					Messages.Insert(0, new MessageBox(Utils.GetTranslationWithPlaceholder("PhoneConfession.Message.12"), MessageBox.MessageSource.Diala, true));
				},
				[SecondsToTicks(23.5f)] = () =>
				{
					Messages[0].Typing = false;
				},
				[SecondsToTicks(27)] = () =>
				{
					Script.done = true;
				}
			};


			//new("WITH YOU", MessageBox.MessageSource.Diala),
			//new("WITH", MessageBox.MessageSource.Diala),
			//new("witj", MessageBox.MessageSource.Diala),
			//new("i think im in love u", MessageBox.MessageSource.Diala),
			//new("Today", MessageBox.MessageSource.System),
		}

		private static int SecondsToTicks(float seconds)
		{
			return (int) seconds * 60;
		}
	}
}
