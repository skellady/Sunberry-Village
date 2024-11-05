using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Collections.Generic;
using System.Linq;

namespace SunberryVillage.Lighting
{
	internal static class LightingManager
	{
		internal static Dictionary<string, LightDataModel> Lights = new();              // contains lights defined in asset "SunberryTeam.SBV/Lights"
		
		internal const string LightsAssetPath = "SunberryTeam.SBV/Lights";
		internal static IAssetName LightsAssetName = Globals.GameContent.ParseAssetName(LightsAssetPath);

		#region Logic

		/// <summary>
		/// Initialize light data asset.
		/// </summary>
		private static void Init()
		{
			Lights = Globals.GameContent.Load<Dictionary<string, LightDataModel>>(LightsAssetPath);
		}

		/// <summary>
		/// Removes the provided lights from the current location and from the light data asset they belong to.
		/// </summary>
		/// <param name="ids">The IDs of the lights to remove, as an enumerable collection.</param>
		private static void RemoveLights(IEnumerable<string> ids)
		{
			foreach (string id in ids)
				RemoveLight(id);
		}

		/// <summary>
		/// Removes the light with the given ID from the map and from the light data asset it belongs to.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		internal static bool RemoveLight(string id)
		{
			if (!Lights.ContainsKey(id))
				return false;

			try
			{
				LightDataModel lightData = Lights[id];
				Game1.currentLightSources?.Remove(lightData.LightSource.Id);
				Lights.Remove(id);
				return true;
			}
			catch (Exception e)
			{
				Log.Error($"Failed to remove light with provided ID \"{id}\": {e}");
				return false;
			}
		}

		/// <summary>
		/// Gets all lights for the current location and adds them to <see cref="Game1.currentLightSources"/>.
		/// </summary>
		internal static void AddLightsToCurrentLocation()
		{
			Dictionary<string, LightDataModel> lightsInLocation = (Dictionary<string, LightDataModel>)Lights.Where(kvp => Game1.currentLocation.Equals(kvp.Value.GameLocation));

			foreach ((string id, LightDataModel model) in lightsInLocation)
			{
				Game1.currentLightSources.Add(id, model.LightSource);
			}
		}
		#endregion

		#region Event Hooks

		/// <summary>
		/// Adds Lighting event hooks.
		/// </summary>
		internal static void AddEventHooks()
		{
			Globals.EventHelper.Content.AssetRequested += Lighting_AssetRequested;
			Globals.EventHelper.Content.AssetsInvalidated += Lighting_AssetInvalidated;
			Globals.EventHelper.GameLoop.SaveLoaded += (_, _) => Init();
			Globals.EventHelper.Player.Warped += (_, _) => AddLightsToCurrentLocation();
		}

		/// <summary>
		/// Provides the base lighting asset when requested. Includes the Saturday hangout light by default.
		/// </summary>
		private static void Lighting_AssetRequested(object sender, AssetRequestedEventArgs e)
		{
			if (e.NameWithoutLocale.IsEquivalentTo(LightsAssetPath))
				e.LoadFrom(() => new Dictionary<string, LightDataModel>(), AssetLoadPriority.Low);
		}

		/// <summary>
		/// Clears any existing lights, reloads the asset, and reapplies any appropriate lights.
		/// </summary>
		private static void Lighting_AssetInvalidated(object sender, AssetsInvalidatedEventArgs e)
		{
			if (!e.NamesWithoutLocale.Contains(LightsAssetName))
				return;

			RemoveLights(Lights.Keys);
			Lights = Globals.GameContent.Load<Dictionary<string, LightDataModel>>(LightsAssetPath);
			AddLightsToCurrentLocation();
		}

		#endregion
	}
}

/// <summary>
/// Data model for Content Patcher integration.
/// </summary>
internal class LightDataModel
{
	public string Id;
	public string Color;
	public string Location;
	public Vector2 Position;
	public float Intensity;
	internal Color BlendColor;
	internal GameLocation GameLocation;
	internal LightSource LightSource;

	public LightDataModel(string id, string color, string location, Vector2 position, float intensity)
	{
		Id = id;
		Color = color;
		Location = location;
		Position = position;
		Intensity = intensity;
		
		Color convertedColor = Utility.StringToColor(Color) ?? Microsoft.Xna.Framework.Color.Black;
		BlendColor = new Color(255 - convertedColor.R, 255 - convertedColor.G, 255 - convertedColor.B);
		GameLocation = Game1.getLocationFromName(location);
		LightSource = new LightSource(Id, 4, Position * 64f, Intensity, BlendColor);
	}

	public override string ToString()
	{
		return $"{Id}: {{Color: {Color} | Location: {Location} | Position: ({Position.X}, {Position.Y}) | Intensity: {Intensity}}}";
	}
}
