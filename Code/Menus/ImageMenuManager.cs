using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System.Collections.Generic;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local

namespace SunberryVillage.Menus;

internal class ImageMenuManager
{
	internal const string ImageSetsAssetPath = "SunberryTeam.SBVSMAPI/ImageSets";
	internal static IAssetName ImageSetsAssetName = Globals.GameContent.ParseAssetName(ImageSetsAssetPath);

	private static Dictionary<string, ImageSetDataModel>? _imageSetData;

	internal static Dictionary<string, ImageSetDataModel> ImageSetsData
	{
		get => _imageSetData ?? Globals.GameContent.Load<Dictionary<string, ImageSetDataModel>>(ImageSetsAssetPath);
		set => _imageSetData = value;
	}

	internal static void AddEventHooks()
	{
		Globals.EventHelper.Content.AssetRequested += Animations_AssetRequested;
		Globals.EventHelper.Content.AssetsInvalidated += Animations_AssetsInvalidated;
	}

	private static void Animations_AssetRequested(object? sender, AssetRequestedEventArgs e)
	{
		if (e.NameWithoutLocale.IsEquivalentTo(ImageSetsAssetPath))
			e.LoadFrom(() => new Dictionary<string, ImageSetDataModel>()
			{
				["TestId"] = new()
				{
					Id = "TestId",
					Images = [
						new ImageDataModel
						{
							Texture = "LooseSprites\\ControllerMaps",
							SourceRect = new Rectangle(0, 0, 25, 25)
						},
						new ImageDataModel
						{
							Texture = "LooseSprites\\ControllerMaps",
							SourceRect = new Rectangle(0, 0, 25, 25)
						},
						new ImageDataModel
						{
							Texture = "LooseSprites\\ControllerMaps",
							SourceRect = new Rectangle(0, 0, 200, 180),
							Scale = 1
						},
						]
				}
			}, AssetLoadPriority.Low);
	}

	private static void Animations_AssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e)
	{
		if (e.NamesWithoutLocale.Contains(ImageSetsAssetName))
			ImageSetsData = Globals.GameContent.Load<Dictionary<string, ImageSetDataModel>>(ImageSetsAssetPath);
	}
}

#pragma warning disable CS0649

internal class ImageSetDataModel
{
	public string Id;
	public List<ImageDataModel> Images = [];
}

internal class ImageDataModel
{
	public string Texture = "";
	public Rectangle? SourceRect;
	public string Caption = "";
	public int Scale = 4;

	internal Texture2D TextureSheet;
	internal string ParsedCaption;
}

#pragma warning restore CS0649