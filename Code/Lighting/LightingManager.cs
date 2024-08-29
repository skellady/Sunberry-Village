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
		internal static readonly Dictionary<string, LightDataModel> TempLights = new(); // contains lights defined via console command - these DO NOT persist through save/load

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
		/// Attempts to map the given <paramref name="args"/> to a <see cref="LightDataModel"/>. Returns whether or not it was successful, and if unsuccessful, the error message will be stored in <paramref name="error"/>.
		/// </summary>
		/// <param name="args">The parameters of the light to create.</param>
		/// <param name="error">Empty string if successful, or the error message if unsuccessful.</param>
		/// <returns><c>True</c> if the light is successfully created and added, <c>False</c> otherwise.</returns>
		internal static bool TryAddOrUpdateTempLight(string[] args, out string error)
		{
			error = "";

			if (args.Length != 4)
			{
				error = "Incorrect number of arguments supplied.";
			}
			else
			{
				string id = args[0];

				if (!float.TryParse(args[1], out float xPos) || !float.TryParse(args[2], out float yPos) || !float.TryParse(args[3], out float intensity))
				{
					error = $"Unable to map provided arguments {{{string.Join(", ", args.Select(s => "\"" + s + "\""))}}} to expected parameters {{id (string), xPos (float), yPos (float), intensity (float)}}.";
				}
				else
				{
					string loc = Game1.currentLocation.Name;
					LightDataModel lightData = new(loc, new Vector2(xPos, yPos), intensity);

					RemoveLight(id);

					if (!TempLights.TryAdd(id, lightData))
					{
						error = $"Unable to add light with parameters {{{string.Join(", ", args.Select(s => "\"" + s + "\""))}}}.";
					}
				}
			}

			if (error == "")
				return true;

			error += " Please modify the provided arguments and try again.";
			return false;
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
			if (TempLights.ContainsKey(id))
			{
				LightDataModel lightData = TempLights[id];
				Game1.currentLightSources?.Remove(lightData.LightSource);
				TempLights.Remove(id);
				return true;
			}
			else if (Lights.ContainsKey(id))
			{
				LightDataModel lightData = Lights[id];
				Game1.currentLightSources?.Remove(lightData.LightSource);
				Lights.Remove(id);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Gets all lights for the current location and adds them to <see cref="Game1.currentLightSources"/>.
		/// </summary>
		internal static void AddLightsToCurrentLocation()
		{
			var lightsInLocation = Lights.Union(TempLights).Where(kvp => kvp.Value.GameLocation is not null && kvp.Value.GameLocation.Equals(Game1.currentLocation)).ToList();

			foreach (var light in lightsInLocation)
			{
				Game1.currentLightSources.Add(light.Value.LightSource);
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
				e.LoadFrom(() => new Dictionary<string, LightDataModel>
				{
					["sophie.SBVSaturdayHangoutLight"] = new LightDataModel(
						location: "Custom_SBV_SunberryVillage",
						position: new Vector2(59f, 85.5f),
						intensity: 4.8f)
				}, AssetLoadPriority.Low);
		}

		/// <summary>
		/// Clears any existing lights, reloads the asset, and reapplies any appropriate lights.
		/// </summary>
		private static void Lighting_AssetInvalidated(object sender, AssetsInvalidatedEventArgs e)
		{
			if (e.NamesWithoutLocale.Contains(LightsAssetName))
			{
				RemoveLights(Lights.Keys);
				Lights = Globals.GameContent.Load<Dictionary<string, LightDataModel>>(LightsAssetPath);
				AddLightsToCurrentLocation();
			}
		}

		#endregion
	}
}

/// <summary>
/// Data model for Content Patcher integration.
/// </summary>
internal class LightDataModel
{
	public string Location;
	public Vector2 Position;
	public float Intensity;
	internal GameLocation GameLocation;
	internal LightSource LightSource;

	public LightDataModel(string location, Vector2 position, float intensity)
	{
		this.Location = location;
		this.Position = position;
		this.Intensity = intensity;

		GameLocation = Game1.getLocationFromName(location);
		LightSource = new LightSource(4, position * 64f, intensity);
	}

	public override string ToString()
	{
		return $"{{Location: {Location} | Position: ({Position.X}, {Position.Y}) | Intensity: {Intensity}}}";
	}
}
