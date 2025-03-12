using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley.TokenizableStrings;

namespace SunberryVillage.Menus;

internal class ImageMenu : IClickableMenu
{
	public ClickableTextureComponent nextArrow;
	public ClickableTextureComponent prevArrow;

	public List<ImageDataModel> Images = [];
	public int Index;
	public ImageDataModel Image;

	public ImageMenu(string id)
	{
		if (!ImageMenuManager.ImageSetsData.TryGetValue(id, out ImageSetDataModel imageSet) || imageSet is null)
		{
			Log.Error($"Failed to create image viewer menu: could not match provided image set ID \"{id}\" to an entry in {ImageMenuManager.ImageSetsAssetPath}.");
			Game1.activeClickableMenu.emergencyShutDown();
			return;
		}

		Images = imageSet.Images;

		if (!Images.Any())
		{
			Log.Error($"Failed to create image viewer menu: image set \"{id}\" contains no image entries.");
			Game1.activeClickableMenu.emergencyShutDown();
			return;
		}

		foreach (ImageDataModel image in Images) {
			if (string.IsNullOrEmpty(image.Texture))
			{
				Log.Error($"Failed to create image viewer menu: image set \"{id}\" contains an image with no Texture path.");
				Game1.activeClickableMenu.emergencyShutDown();
				return;
			}

			if (!TryLoadTexture(image.Texture, out image.TextureSheet) || image.TextureSheet is null)
			{
				Log.Error($"Failed to create image viewer menu: could not load texture with provided path \"{image.Texture}\" while handling image set \"{id}\".");
				Game1.activeClickableMenu.emergencyShutDown();
				return;
			}

			image.SourceRect ??= new Rectangle(0, 0, image.TextureSheet.Width, image.TextureSheet.Height);
			image.ParsedCaption = TokenParser.ParseText(image.Caption);
		}

		Index = 0;
		Image = Images[Index];

		int captionWidth = 0;
		int captionHeight = 0;
		if (!string.IsNullOrEmpty(Image.ParsedCaption))
		{
			Vector2 captionSize = Game1.dialogueFont.MeasureString(Game1.parseText(Image.ParsedCaption, Game1.dialogueFont, 1240));
			captionWidth = (int)captionSize.X + 64;
			captionHeight = (int)captionSize.Y + 4;
		}

		int menuWidth = Math.Max(Image.SourceRect?.Width ?? 0 + borderWidth * 2, captionWidth);
		int menuHeight = Image.SourceRect?.Height ?? 0 + 4 + captionHeight;

		Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, menuWidth, menuHeight);

		initialize((int)topLeft.X, (int)topLeft.Y, menuWidth, menuHeight, true);

		if (Images.Count > 1)
		{
			nextArrow = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 128, yPositionOnScreen, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
			{
				myID = 101,
				leftNeighborID = 102,
				leftNeighborImmutable = true,
				downNeighborID = -99998
			};

			prevArrow = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 64, yPositionOnScreen, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
			{
				myID = 102,
				rightNeighborID = 101,
				rightNeighborImmutable = true,
				downNeighborID = -99998
			};
		}

		if (Game1.options.snappyMenus && Game1.options.gamepadControls)
		{
			populateClickableComponentList();
			snapToDefaultClickableComponent();
		}
	}

	public override void performHoverAction(int x, int y)
	{
		nextArrow?.tryHover(x, y, 0.25f);
		prevArrow?.tryHover(x, y, 0.25f);

		base.performHoverAction(x, y);
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (nextArrow is not null && nextArrow.containsPoint(x, y) && Index < Images.Count - 1)
		{
			Game1.playSound("shwip");
			Index++;
			RecalculateMenuSize();
		}

		if (prevArrow is not null && prevArrow.containsPoint(x, y) && Index > 0)
		{
			Game1.playSound("shwip");
			Index--;
			RecalculateMenuSize();
		}

		base.receiveLeftClick(x, y, playSound);
	}

	private void RecalculateMenuSize()
	{
		int captionWidth = 0;
		int captionHeight = 0;
		if (!string.IsNullOrEmpty(Images[Index].ParsedCaption))
		{
			Vector2 captionSize = Game1.dialogueFont.MeasureString(Game1.parseText(Images[Index].ParsedCaption, Game1.dialogueFont, 1240));
			captionWidth = (int)captionSize.X + 64;
			captionHeight = (int)captionSize.Y + 4;
		}

		int menuWidth = Math.Max(Images[Index].SourceRect?.Width ?? 0 + borderWidth * 2, captionWidth);
		int menuHeight = Images[Index].SourceRect?.Height ?? 0 + 4 + captionHeight;

		Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, menuWidth, menuHeight);

		xPositionOnScreen = (int)topLeft.X;
		yPositionOnScreen = (int)topLeft.Y;
		width = menuWidth;
		height = menuHeight;

		nextArrow.visible = Index < Images.Count - 1;
		prevArrow.visible = Index > 0;

		nextArrow.setPosition(xPositionOnScreen + width + 16, yPositionOnScreen + 50);
		prevArrow.setPosition(xPositionOnScreen - 16, yPositionOnScreen + 50);
	}

	public override void draw(SpriteBatch b)
	{
		if (!Game1.options.showClearBackgrounds)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
		}

		int width = Image.SourceRect?.Width ?? 0;
		int height = Image.SourceRect?.Height ?? 0;

		Game1.DrawBox(xPositionOnScreen, yPositionOnScreen, width + 2 * borderWidth, height + 2 * borderWidth);

		//b.Draw(Image.TextureSheet,
		//	new Rectangle(xPositionOnScreen + borderWidth,
		//		yPositionOnScreen + borderWidth,
		//		width,
		//		height),
		//	Image.SourceRect,
		//	Color.White,
		//	0f,
		//	Vector2.Zero,
		//	SpriteEffects.None,
		//	1f);

		b.Draw(Image.TextureSheet,
			new Vector2(xPositionOnScreen,
				yPositionOnScreen),
			Image.SourceRect,
			Color.White,
			0f,
			Vector2.Zero,
			Image.Scale,
			SpriteEffects.None,
			1f);

		//Game1.drawDialogueBox(xPositionOnScreen + imageWidth / 2, yPositionOnScreen + imageHeight + 4, false, false, "Test");
		//Game1.drawWithBorder("Test", Color.White, Color.White, new Vector2(xPositionOnScreen + imageWidth / 2, yPositionOnScreen + imageHeight));

		nextArrow?.draw(b);
		prevArrow?.draw(b);

		base.draw(b);
		drawMouse(b);
	}

	private static bool TryLoadTexture(string textureName, out Texture2D? texture)
	{
		try
		{
			if (Game1.content.DoesAssetExist<Texture2D>(textureName))
			{
				texture = Game1.content.Load<Texture2D>(textureName);
				return true;
			}

			texture = null;
			return false;
		}
		catch (Exception)
		{
			texture = null;
			return false;
		}
	}
}
