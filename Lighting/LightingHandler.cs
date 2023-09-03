using Microsoft.Xna.Framework;
using StardewValley;
using SunberryVillage.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SunberryVillage.Lighting
{
	// storing these for later while i completely fucking overhaul this goddamn code
	//private const float DefaultRadius = 4.8f;
	//private static readonly Vector2 DefaultPosition = new Vector2(59f, 85.5f) * 64f; // the center of the fountain is (58, 86) - shifting it up and to the right a bit to better cover everyone

	// Overcomplicated this but if i need to add more lights in the future, it will be pretty trivial, so that's nice
	// Consider converting to CP asset for ease of use by max and other sbv resident authors
	// you know, if you want to overcomplicate things even further...

	internal static class LightingHandler
	{
		internal static Dictionary<string, LightDataModel> Lights = new();				// contains lights defined in asset "SunberryTeam.SBV/Lights"
		internal static readonly Dictionary<string, LightDataModel> TempLights = new();	// contains lights defined via console command - these DO NOT persist through save/load

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
					GameLocation loc = Game1.currentLocation;
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

		internal static bool CreateDefaultLight(out string id, out string error)
		{
			error = "";
			id = "";

			string tempId = Utils.GenerateRandomString(6);
			GameLocation loc = Game1.currentLocation;
			Vector2 pos = Game1.player.getTileLocation();
			float intensity = 1f;

			LightDataModel lightData = new(loc, pos, intensity);
			if (!TempLights.TryAdd(tempId, lightData))
			{
				error = $"Unable to add light with parameters {{\"{tempId}\", \"{pos.X}\", \"{pos.Y}\", \"{intensity}\"}}.";
			}

			if (error != "")
				return false;

			id = tempId;
			return true;
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

			foreach (var light in allLights.Where(kvp => kvp.Value.Location == Game1.currentLocation))
			{
				Game1.currentLightSources.Add(light.Value.LightSource);
			}
		}
	}

	internal class LightDataModel
	{
		public GameLocation Location;
		public Vector2 Position;
		public float Intensity;
		internal LightSource LightSource;

		public LightDataModel(string location, Vector2 position, float intensity) : this(Game1.getLocationFromName(location), position, intensity)
		{ }

		public LightDataModel(GameLocation location, Vector2 position, float intensity)
		{
			Location = location;
			Position = position;
			Intensity = intensity;
			LightSource = new(4, Position * 64f, Intensity);
		}

		public override string ToString()
		{
			return $"{{{Location.Name} | ({Position.X}, {Position.Y}) | {Intensity}}}";
		}
	}
}
