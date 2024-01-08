using System;

namespace SunberryVillage.Utilities
{
	internal class Utils
	{
		private static readonly Random Random = new();

		/// <summary>
		/// Generates a string of random alphanumeric characters of the specified <paramref name="length"/>.
		/// </summary>
		/// <param name="length">The length of the string to generate.</param>
		/// <returns>A string of random characters of the specified <paramref name="length"/>.</returns>
		internal static string GenerateRandomString(int length)
		{
			const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

			char[] stringChars = new char[length];

			for (int i = 0; i < stringChars.Length; i++)
			{
				stringChars[i] = validChars[Random.Next(validChars.Length)];
			}

			return new string(stringChars);
		}

		internal static string GetTranslationWithPlaceholder(string key) => Globals.TranslationHelper.Get(key).UsePlaceholder(true);
	}
}
