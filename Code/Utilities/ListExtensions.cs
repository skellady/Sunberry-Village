using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable UnusedMember.Global

namespace SunberryVillage.Utilities;

internal static class ListExtensions
{
	private static readonly Random Random = new();

	/// <summary>
	/// Given a list, selects a random element from the list and returns that element. If <paramref name="removeElement"/> is true, will remove the selected element from the list.
	/// Given a null or empty list, will return the default value for the given type.
	/// </summary>
	/// <typeparam name="T">Any type.</typeparam>
	/// <param name="list">The list from which to remove an element.</param>
	/// <param name="random">The specific Random instance to use, or a default one if <c>null</c>.</param>
	/// <param name="removeElement">Whether or not to remove the selected element from the list.</param>
	/// <returns>The element removed from the list.</returns>
	internal static T GetRandomElementFromList<T>(this IList<T> list, Random random = null, bool removeElement = false)
	{
		if (list is null || !list.Any())
			return default;

		// use supplied random param if it exists, otherwise default random instance
		int index = random?.Next(list.Count) ?? Random.Next(list.Count);

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
	/// <typeparam name="T">The type of the contents of the list.</typeparam>
	/// <param name="list">The list from which to remove the elements.</param>
	/// <param name="quantity">The number of elements to select from the list.</param>
	/// <param name="random">The specific Random instance to use, or a default one if <c>null</c>.</param>
	/// <param name="removeElements">Whether or not to remove the selected element or elements from the list.</param>
	/// <returns>A list consisting of the element or elements selected, or an empty list if unable to make a selection.</returns>
	internal static IList<T> GetRandomElementsFromList<T>(this IList<T> list, int quantity, Random random = null, bool removeElements = false)
	{
		List<T> elementList = new();

		if (list is null || !list.Any())
			return elementList;

		if (quantity > list.Count)
		{
			elementList.AddRange(list);
			elementList.Shuffle();

			if (removeElements)
				list.Clear();

			return elementList;
		}

		while (quantity > 0)
		{
			quantity--;
			T element = GetRandomElementFromList(list, random, removeElements);
			elementList.Add(element);
		}

		return elementList;
	}

	/// <summary>
	/// Given a list, will shuffle the contents of the list so that they are in a random order.
	/// </summary>
	/// <typeparam name="T">Any type.</typeparam>
	/// <param name="list">The list to shuffle.</param>
	/// <param name="random">The specific Random instance to use, or a default one if <c>null</c></param>
	/// <returns>The list instance in randomly shuffled order.</returns>
	internal static IList<T> Shuffle<T>(this IList<T> list, Random random = null)
	{
		Random rand = random ?? Random;

		int n = list.Count;  
		while (n > 1) {  
			n--;  
			int k = rand.Next(n + 1);
			list.Swap(n, k);
		}

		return list;
	}

	/// <summary>
	/// Swaps the contents of the two provided indices for the given list.
	/// </summary>
	/// <typeparam name="T">Any type.</typeparam>
	/// <param name="list">The list for which to swap the two elements.</param>
	/// <param name="index1">The index of the first element in the swap.</param>
	/// <param name="index2">The index of the second element in the swap.</param>
	public static void Swap<T>(this IList<T> list, int index1, int index2)
	{
		(list[index1], list[index2]) = (list[index2], list[index1]);
	}
}