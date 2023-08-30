using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable UnusedMember.Global

namespace SunberryVillage.Utilities;

internal static class Extensions
{
	private static readonly Random Random = new();

	/// <summary>
	/// Given a list, selects a random element from the list and returns that element. If <paramref name="removeElement"/> is true, will remove the selected element from the list.
	/// Given a null or empty list, will return the default value for the given type.
	/// </summary>
	/// <typeparam name="T">Any type.</typeparam>
	/// <param name="list">The list from which to remove an element.</param>
	/// <param name="removeElement">Whether or not the element should be removed from the list.</param>
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
	/// Given a list and a quantity, selects that number of elements at random from the list and returns a separate list containing only the selected elements.<br />
	/// If <paramref name="removeElements" /> is true, will remove the selected elements from the original list.<br />
	/// Given a null or empty list, will return an empty list.<br />
	/// Given a quantity greater than the length of the list, will return a copy of the original list (and clear the original list if <paramref name="removeElements"/> is true).
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