using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable UnusedMember.Global

namespace SunberryVillage.Utilities;

internal static class Extensions
{
	private static readonly Random Random = new();

	/// <summary>
	/// Given a list, removes a random element from the list and returns that element. Given a null or empty list, will return the default value for the given type.
	/// </summary>
	/// <typeparam name="T">Any type.</typeparam>
	/// <param name="list">The list from which to remove an element.</param>
	/// <returns>The element removed from the list.</returns>
	internal static T GetRandomElementFromList<T>(this List<T> list, bool removeElement = false)
	{
		if (list is null || !list.Any())
			return default;

		int index = Random.Next(list.Count);
		T element = list.ElementAt(index);

		if (removeElement)
			list.RemoveAt(index);

		return element;
	}

	/// <summary>
	/// Given a list and a quantity of elements to remove, removes that number of elements at random from the list and returns a separate list containing the removed elements.<br />
	/// Given a null or empty list, will return an empty list.<br />
	/// Given a quantity greater than the length of the list, will clear the original list and return a copy.
	/// </summary>
	/// <typeparam name="T">Any type.</typeparam>
	/// <param name="list">The list from which to remove the elements.</param>
	/// <param name="quantity">The element or elements removed from the list</param>
	/// <returns></returns>
	internal static List<T> GetRandomElementsFromList<T>(this List<T> list, int quantity, bool removeElements = false)
	{
		List<T> elementList = new();

		if (list is null || !list.Any())
			return elementList;

		if (quantity > list.Count)
		{
			elementList.AddRange(list);
			list.Clear();

			return elementList;
		}

		while (quantity > 0)
		{
			quantity--;
			T element = GetRandomElementFromList(list, removeElements);
			elementList.Add(element);
		}

		return elementList;
	}
}