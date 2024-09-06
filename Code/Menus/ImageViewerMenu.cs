using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;

namespace SunberryVillage.Menus;

internal class ImageViewerMenu : IClickableMenu
{
	public ClickableTextureComponent nextArrow;
	public ClickableTextureComponent prevArrow;

	private int imageIndex;
	private int imageNumber;
	private string[] actionArgs;

	private int imageWidth;
	private int imageHeight;
	private Texture2D imageTexture;
	private Rectangle imageSourceRect;

	public ImageViewerMenu(string[] args, int index) : base()
	{
		this.actionArgs = args;
		this.imageIndex = index;

		if (!ArgUtility.TryGet(args, 1, out string texturePath, out string error, false))
		{
			Log.Error($"Failed to create image viewer menu: {error}");
		}

		imageTexture = Globals.GameContent.Load<Texture2D>(texturePath);
		if (imageTexture is null)
		{
			Log.Error($"Failed to create image viewer menu: Could not load image at {texturePath}");
		}

		if (!ArgUtility.TryGetPoint(args, 2, out Point size, out error))
		{
			Log.Error($"Failed to create image viewer menu: {error}");
		}

		if (!ArgUtility.TryGetInt(args, 4, out imageNumber, out error))
		{
			Log.Error($"Failed to create image viewer menu: {error}");
		}

		if (!ArgUtility.TryGetOptional(args, 5, out string captionKey, out error, ""))
		{
			Log.Error($"Failed to create image viewer menu: {error}");
			return;
		}

		string caption = "";
		if (!captionKey.Equals(""))
		{
			caption = Game1.content.LoadString(captionKey);
		}

		imageWidth = size.X;
		imageHeight = size.Y;

		imageSourceRect = Game1.getArbitrarySourceRect(imageTexture, imageWidth, imageHeight, index);

		int captionWidth = 0;
		int captionHeight = 0;
		if (!string.IsNullOrEmpty(caption))
		{
			Vector2 captionSize = Game1.dialogueFont.MeasureString(Game1.parseText(caption, Game1.dialogueFont, 1240));
			captionWidth = (int)captionSize.X + 64;
			captionHeight = (int)captionSize.Y + 4;
		}

		int menuWidth = Math.Max(imageWidth + borderWidth * 2, captionWidth);
		int menuHeight = imageHeight + 4 + captionHeight;

		Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, menuWidth, menuHeight);

		initialize((int)topLeft.X, (int)topLeft.Y, menuWidth, menuHeight, true);

		if (imageNumber > 1)
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
		if (nextArrow is not null && nextArrow.containsPoint(x, y) && imageIndex < imageNumber - 1)
		{
			Game1.playSound("shwip");
			Game1.activeClickableMenu = new ImageViewerMenu(actionArgs, imageIndex + 1);
		}

		if (prevArrow is not null && prevArrow.containsPoint(x, y) && imageIndex > 1)
		{
			Game1.playSound("shwip");
			Game1.activeClickableMenu = new ImageViewerMenu(actionArgs, imageIndex - 1);
		}

		base.receiveLeftClick(x, y, playSound);
	}

	public override void draw(SpriteBatch b)
	{
		Game1.DrawBox(xPositionOnScreen, yPositionOnScreen, imageWidth + 2 * borderWidth, imageHeight + 2 * borderWidth);

		b.Draw(imageTexture, new Rectangle(xPositionOnScreen + borderWidth, yPositionOnScreen + borderWidth, imageWidth, imageHeight), imageSourceRect, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1f);
		//Game1.drawDialogueBox(xPositionOnScreen + imageWidth / 2, yPositionOnScreen + imageHeight + 4, false, false, "Test");
		//Game1.drawWithBorder("Test", Color.White, Color.White, new Vector2(xPositionOnScreen + imageWidth / 2, yPositionOnScreen + imageHeight));

		nextArrow?.draw(b);
		prevArrow?.draw(b);

		base.draw(b);
		drawMouse(b);
	}
}
