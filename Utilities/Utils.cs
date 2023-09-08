using System;

namespace SunberryVillage.Utilities
{
	internal class Utils
	{
		private static readonly Random Random = new();

		// ended up not using this method at all, but it's so fuckin cool i dont wanna get rid of it
		// i was using it to generate light IDs......
		internal static string GenerateRandomString(int length)
		{
			string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

			char[] stringChars = new char[length];

			for (int i = 0; i < stringChars.Length; i++)
			{
				stringChars[i] = validChars[Random.Next(validChars.Length)];
			}

			return new string(stringChars);
		}
	}
}
