using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using SunberryVillage.Utilities;
using System.Collections.Generic;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace SunberryVillage.Tarot;

internal class EventScriptDialaTarot : ICustomEventScript
{
	private readonly Vector2 _screenCenterPosition;
	private readonly Texture2D _cardBackTexture;

	private const int CardWidth = 92;
	private const int CardHeight = 139;

	private const int SpacerWidth = 40;

	private Vector2 _card1Pos;
	private Vector2 _card2Pos;
	private Vector2 _card3Pos;

	private readonly Vector2 _textPos;

	private float _card1SquishFactor;
	private float _card2SquishFactor;
	private float _card3SquishFactor;

	private int _phaseTimer;
	private int _phase;

	private readonly TarotCard _card1;
	private readonly TarotCard _card2;
	private readonly TarotCard _card3;

	private float _textOpacity;
	private float _bgOpacity;

	public EventScriptDialaTarot()
	{
		_screenCenterPosition = new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width / 2f,
			Game1.graphics.GraphicsDevice.Viewport.Height / 2f);

		_cardBackTexture = Game1.content.Load<Texture2D>("SunberryTeam.SBV/Tarot/CardBack");

		_card2Pos = _screenCenterPosition + new Vector2(0f - CardWidth * 2, -1200f);
		_card1Pos = _card2Pos + new Vector2(0f - SpacerWidth - CardWidth * 4, 0f);
		_card3Pos = _card2Pos + new Vector2(SpacerWidth + CardWidth * 4, 0f);

		_textPos = _screenCenterPosition + new Vector2(-6 * CardWidth, CardHeight * 2 + SpacerWidth);

		_card1SquishFactor = 4f;
		_card2SquishFactor = 4f;
		_card3SquishFactor = 4f;

		_phase = 0;
		_phaseTimer = 3000;

		List<TarotCard> cards = TarotHandler.GetAllTarotCardsWithConditionsMet();

		_card1 = cards.GetRandomElementFromList(removeElement: true);
		_card2 = cards.GetRandomElementFromList(removeElement: true);
		_card3 = cards.GetRandomElementFromList(removeElement: true);

		// apply in reverse order so they show up in the correct order on the buff bar
		_card3.ApplyBuff("tarot3");
		_card2.ApplyBuff("tarot2");
		_card1.ApplyBuff("tarot1");
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

		b.Draw(
			texture: Game1.mouseCursors,
			position: Vector2.Zero,
			sourceRectangle: new Rectangle(0, 1453, 638, 195),
			color: Color.White * _bgOpacity,
			rotation: 0f,
			origin: Vector2.Zero,
			scale: 4f,
			effects: SpriteEffects.None,
			layerDepth: 0f
		);

		b.Draw(
			texture: Game1.mouseCursors,
			position: new Vector2(0f, 781),
			sourceRectangle: new Rectangle(0, 1453, 638, 195),
			color: Color.White * _bgOpacity,
			rotation: 0f,
			origin: Vector2.Zero,
			scale: 4f,
			effects: SpriteEffects.None,
			layerDepth: 0f
		);

		b.Draw(
			texture: _cardBackTexture,
			position: _card1Pos,
			sourceRectangle: new Rectangle(0, 0, 92, 139),
			color: Color.White,
			rotation: 0f,
			origin: new Vector2(0f, 0f),
			scale: new Vector2(_card1SquishFactor, 4f),
			effects: SpriteEffects.None,
			layerDepth: 1f
		);

		if (_phase is >= 5 and <= 19)
		{
			b.Draw(
				texture: _card1.Texture.Value,
				position: _card1Pos,
				sourceRectangle: new Rectangle(0, 0, 92, 139),
				color: Color.White,
				rotation: 0f,
				origin: new Vector2(0f, 0f),
				scale: new Vector2(_card1SquishFactor, 4f),
				effects: SpriteEffects.None,
				layerDepth: 2f
			);
		}

		b.Draw(
			texture: _cardBackTexture,
			position: _card2Pos,
			sourceRectangle: null,
			color: Color.White,
			rotation: 0f,
			origin: new Vector2(0f, 0f),
			scale: new Vector2(_card2SquishFactor, 4f),
			effects: SpriteEffects.None,
			layerDepth: 1f
		);

		if (_phase is >= 10 and <= 19)
		{
			b.Draw(
				texture: _card2.Texture.Value,
				position: _card2Pos,
				sourceRectangle: new Rectangle(0, 0, 92, 139),
				color: Color.White,
				rotation: 0f,
				origin: new Vector2(0f, 0f),
				scale: new Vector2(_card2SquishFactor, 4f),
				effects: SpriteEffects.None,
				layerDepth: 2f
			);
		}

		b.Draw(
			texture: _cardBackTexture,
			position: _card3Pos,
			sourceRectangle: null,
			color: Color.White,
			rotation: 0f,
			origin: new Vector2(0f, 0f),
			scale: new Vector2(_card3SquishFactor, 4f),
			effects: SpriteEffects.None,
			layerDepth: 1f
		);

		switch (_phase)
		{
			case >= 15 and <= 19:
				b.Draw(
					texture: _card3.Texture.Value,
					position: _card3Pos,
					sourceRectangle: new Rectangle(0, 0, 92, 139),
					color: Color.White,
					rotation: 0f,
					origin: new Vector2(0f, 0f),
					scale: new Vector2(_card3SquishFactor, 4f),
					effects: SpriteEffects.None,
					layerDepth: 2f
				);
				break;

			case 6 or 7 or 8:
				b.DrawString(
					spriteFont: Game1.dialogueFont,
					text: Game1.parseText($"{_card1.Name}: {_card1.Description}", Game1.dialogueFont, CardWidth * 12),
					position: _textPos,
					color: Color.White * _textOpacity
				);
				break;

			case 11 or 12 or 13:
				b.DrawString(
					spriteFont: Game1.dialogueFont,
					text: Game1.parseText($"{_card2.Name}: {_card2.Description}", Game1.dialogueFont, CardWidth * 12),
					position: _textPos,
					color: Color.White * _textOpacity
				);
				break;
		}

		if (_phase is 16 or 17 or 18)
		{
			b.DrawString(
				spriteFont: Game1.dialogueFont,
				text: Game1.parseText($"{_card3.Name}: {_card3.Description}", Game1.dialogueFont, CardWidth * 12),
				//screenCenterPosition + new Vector2(-(Game1.dialogueFont.MeasureString(card3.Description).X / 2), cardHeight * 2 + spacerWidth),
				position:
				//screenCenterPosition + new Vector2(-(Game1.dialogueFont.MeasureString(card3.Description).X / 2), cardHeight * 2 + spacerWidth),
				_textPos,
				color: Color.White * _textOpacity
			);
		}
	}

	public void draw(SpriteBatch b)
	{
	}

	public bool update(GameTime time, Event e)
	{
		switch (_phase)
		{
			case 0:
				{
					_phaseTimer -= time.ElapsedGameTime.Milliseconds;
					if (_phaseTimer <= 0)
					{
						_phase = 1;
						_bgOpacity = 1f;
					}
					else
					{
						_bgOpacity = (3000 - _phaseTimer) / 3000f;
					}
					return false;
				}

			case 1:
				{
					if (_card1Pos.Y >= _screenCenterPosition.Y - CardHeight * 2)
					{
						_card1Pos.Y = _screenCenterPosition.Y - CardHeight * 2;
						_phase = 2;
					}
					else
					{
						_card1Pos.Y += 10f;
					}
					return false;
				}

			case 2:
				{
					if (_card2Pos.Y >= _screenCenterPosition.Y - CardHeight * 2)
					{
						_card2Pos.Y = _screenCenterPosition.Y - CardHeight * 2;
						_phase = 3;
					}
					else
					{
						_card2Pos.Y += 10f;
					}
					return false;
				}

			case 3:
				{
					if (_card3Pos.Y >= _screenCenterPosition.Y - CardHeight * 2)
					{
						_card3Pos.Y = _screenCenterPosition.Y - CardHeight * 2;
						_phase = 4;
					}
					else
					{
						_card3Pos.Y += 10f;
					}
					return false;
				}

			case 4:
				{
					if (_card1SquishFactor <= 0)
					{
						_card1SquishFactor = 0;
						_phase = 5;
					}
					else
					{
						_card1SquishFactor -= 0.2f;
						_card1Pos.X += 10f;
					}
					return false;
				}

			case 5:
				{

					if (_card1SquishFactor >= 4)
					{
						_card1SquishFactor = 4;
						_phase = 6;
						_phaseTimer = 500;
						_textOpacity = 0f;
					}
					else
					{
						_card1SquishFactor += 0.2f;
						_card1Pos.X -= 10f;
					}
					return false;
				}

			case 6:
				{
					_phaseTimer -= time.ElapsedGameTime.Milliseconds;
					if (_phaseTimer <= 0)
					{
						_phase = 7;
						_phaseTimer = 5000;
						_textOpacity = 1f;
					}
					else
					{
						_textOpacity = (500 - _phaseTimer) / 500f;
					}
					return false;
				}

			case 7:
				{
					_phaseTimer -= time.ElapsedGameTime.Milliseconds;

					if (_phaseTimer > 0)
						return false;

					_phase = 8;
					_phaseTimer = 500;
					return false;
				}

			case 8:
				{
					_phaseTimer -= time.ElapsedGameTime.Milliseconds;
					if (_phaseTimer <= 0)
					{
						_phase = 9;
					}
					else
					{
						_textOpacity = _phaseTimer / 500f;
					}
					return false;
				}

			case 9:
				{
					if (_card2SquishFactor <= 0)
					{
						_card2SquishFactor = 0;
						_phase = 10;
					}
					else
					{
						_card2SquishFactor -= 0.2f;
						_card2Pos.X += 10f;
					}
					return false;
				}

			case 10:
				{

					if (_card2SquishFactor >= 4)
					{
						_card2SquishFactor = 4;
						_phase = 11;
						_phaseTimer = 0;
						_textOpacity = 0f;
					}
					else
					{
						_card2SquishFactor += 0.2f;
						_card2Pos.X -= 10f;
					}
					return false;
				}

			case 11:
				{
					_phaseTimer -= time.ElapsedGameTime.Milliseconds;
					if (_phaseTimer <= 0)
					{
						_phase = 12;
						_phaseTimer = 5000;
						_textOpacity = 1f;
					}
					else
					{
						_textOpacity = (500 - _phaseTimer) / 500f;
					}
					return false;
				}

			case 12:
				{
					_phaseTimer -= time.ElapsedGameTime.Milliseconds;

					if (_phaseTimer > 0)
						return false;

					_phase = 13;
					_phaseTimer = 500;
					return false;
				}

			case 13:
				{
					_phaseTimer -= time.ElapsedGameTime.Milliseconds;
					if (_phaseTimer <= 0)
					{
						_phase = 14;
					}
					else
					{
						_textOpacity = _phaseTimer / 500f;
					}
					return false;
				}

			case 14:
				{
					if (_card3SquishFactor <= 0)
					{
						_card3SquishFactor = 0;
						_phase = 15;
					}
					else
					{
						_card3SquishFactor -= 0.2f;
						_card3Pos.X += 10f;
					}
					return false;
				}

			case 15:
				{

					if (_card3SquishFactor >= 4)
					{
						_card3SquishFactor = 4;
						_phase = 16;
						_phaseTimer = 500;
						_textOpacity = 0f;
					}
					else
					{
						_card3SquishFactor += 0.2f;
						_card3Pos.X -= 10f;
					}
					return false;
				}

			case 16:
				{
					_phaseTimer -= time.ElapsedGameTime.Milliseconds;
					if (_phaseTimer <= 0)
					{
						_phase = 17;
						_phaseTimer = 5000;
						_textOpacity = 1f;
					}
					else
					{
						_textOpacity = (500 - _phaseTimer) / 500f;
					}
					return false;
				}

			case 17:
				{
					_phaseTimer -= time.ElapsedGameTime.Milliseconds;

					if (_phaseTimer > 0)
						return false;

					_phase = 18;
					_phaseTimer = 500;
					return false;
				}

			case 18:
				{
					_phaseTimer -= time.ElapsedGameTime.Milliseconds;
					if (_phaseTimer <= 0)
					{
						_phase = 19;
					}
					else
					{
						_textOpacity = _phaseTimer / 500f;
					}
					return false;
				}

			default:
				return true;
		}
	}
}