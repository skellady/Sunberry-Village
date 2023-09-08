using Microsoft.Xna.Framework;
using StardewValley;
using System.Collections.Generic;
using System.Linq;

namespace SunberryVillage.Lighting
{
	internal static class LightingHandler
	{
		internal static Dictionary<string, LightDataModel> Lights = new();              // contains lights defined in asset "SunberryTeam.SBV/Lights"
		internal static readonly Dictionary<string, LightDataModel> TempLights = new(); // contains lights defined via console command - these DO NOT persist through save/load

		internal const string LightsAssetPath = "SunberryTeam.SBV/Lights";

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

					RemoveTempLight(id);

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

		internal static bool RemoveTempLight(string id)
		{
			if (!TempLights.ContainsKey(id))
				return false;

			LightDataModel lightData = TempLights[id];
			Game1.currentLightSources?.Remove(lightData.LightSource);
			TempLights.Remove(id);
			return true;
		}

		internal static void AddLightsToCurrentLocation()
		{
			IEnumerable<KeyValuePair<string, LightDataModel>> allLights = Lights.Union(TempLights);

			foreach (var light in allLights.Where(kvp => kvp.Value.GameLocation == Game1.currentLocation))
			{
				Game1.currentLightSources.Add(light.Value.LightSource);
			}
		}
	}

	internal class LightDataModel
	{
		public string Location;
		public Vector2 Position;
		public float Intensity;
		internal GameLocation GameLocation;
		internal LightSource LightSource;

		public LightDataModel(string Location, Vector2 Position, float Intensity)
		{
			this.Location = Location;
			this.Position = Position;
			this.Intensity = Intensity;

			GameLocation = Game1.getLocationFromName(Location);
			LightSource = new(4, Position * 64f, Intensity);
		}

		public override string ToString()
		{
			return $"{{Location: {Location} | Position: ({Position.X}, {Position.Y}) | Intensity: {Intensity}}}";
		}
	}
}
