using Microsoft.Xna.Framework;
using StardewValley;

namespace SunberryVillage.Lighting
{
	// Overcomplicated this but if i need to add more lights in the future, it will be pretty trivial, so that's nice
	// Consider converting to CP asset for ease of use by max and other sbv resident authors
	// you know, if you want to overcomplicate things even further...

	internal class LightingHandler
	{
		private static LightSource lightSource; // currently only supports adding a single LightSource instance at a time. May overhaul this later depending on max's needs.

		private const float DefaultRadius = 4.8f;
		private static readonly Vector2 DefaultPosition = new Vector2(59f, 85.5f) * 64f; // the center of the fountain is (58, 86) - shifting it up and to the right a bit to better cover everyone

		/// <summary>
		/// Adds light to Saturday hangout area. Uses the default values.
		/// </summary>
		internal static void AddLight() => AddLight(DefaultRadius, DefaultPosition);

		/// <summary>
		/// Adds light to Saturday hangout area. Uses the default position and the provided radius.
		/// </summary>
		/// <param name="radius">The radius of the light.</param>
		internal static void AddLight(float radius) => AddLight(radius, DefaultPosition);

		/// <summary>
		/// Adds light to Saturday hangout area. Uses the default radius and the provided position.
		/// </summary>
		/// <param name="position">The position at which to add the light.</param>
		internal static void AddLight(Vector2 position) => AddLight(DefaultRadius, position);

		/// <summary>
		/// Adds light to map with the specified radius and position.
		/// </summary>
		/// <param name="radius">The radius of the light.</param>
		/// <param name="position">The position at which to add the light.</param>
		internal static void AddLight(float radius, Vector2 position)
		{
			RemoveLight();

			lightSource = new(4, position, radius);
			Game1.currentLightSources.Add(lightSource);
		}

		/// <summary>
		/// If the light exists, remove it.
		/// </summary>
		internal static void RemoveLight()
		{
			if (lightSource is not null)
			{
				Game1.currentLightSources.Remove(lightSource);
				lightSource = null;
			}
		}
	}
}
