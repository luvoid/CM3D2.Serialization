using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CM3D2.Serialization.SourceGenerators.Extensions
{
	internal static class CollectionExtensions
	{
		private static int InternalIndexOf<T>(IEnumerable<T> list, T item, IEqualityComparer<T> equalityComparer)
		{
			equalityComparer ??= EqualityComparer<T>.Default;

			int index = 0;
			foreach (var element in list)
			{
				if (equalityComparer.Equals(item, element))
					return index;
				index++;
			}
			return -1;
		}
		/// <summary>
		/// Find the index of the element with a structure that matches <paramref name="item"/>.
		/// </summary>
		private static int InternalIndexOf<TElement, TFind>(IEnumerable<TElement> list, TFind item, IEqualityComparer equalityComparer)
			where TElement : IStructuralEquatable
			where TFind : IStructuralEquatable
		{
			equalityComparer ??= StructuralComparisons.StructuralEqualityComparer;

			int index = 0;
			foreach (var element in list)
			{
				if (item.Equals(element, equalityComparer))
					return index;
				index++;
			}
			return -1;
		}
		private static int InternalIndexOf<TElement, TFind, TEqualityComparer>(IEnumerable<TElement> list, TFind item, TEqualityComparer equalityComparer)
			where TElement : IEquatableUsingComparer<TFind, TEqualityComparer>
			where TEqualityComparer : IEqualityComparer<TFind>
		{
			int index = 0;
			foreach (var element in list)
			{
				if (element.Equals(item, equalityComparer))
					return index;
				index++;
			}
			return -1;
		}

		public static int IndexOf<T>(this IReadOnlyList<T> list, T item, IEqualityComparer<T> equalityComparer)
		{
			return InternalIndexOf(list, item, equalityComparer);
		}
		/// <summary>
		/// Find the index of the element with a structure that matches <paramref name="item"/>.
		/// </summary>
		public static int IndexOf<TElement, TFind>(this IReadOnlyList<TElement> list, TFind item, IEqualityComparer equalityComparer)
			where TElement : IStructuralEquatable
			where TFind : IStructuralEquatable
		{
			return InternalIndexOf(list, item, equalityComparer);
		}
		public static int IndexOf<TElement, TFind, TEqualityComparer>(this IReadOnlyList<TElement> list, TFind item, TEqualityComparer equalityComparer)
			where TElement : IEquatableUsingComparer<TFind, TEqualityComparer>
			where TEqualityComparer : IEqualityComparer<TFind>
		{
			return InternalIndexOf(list, item, equalityComparer);
		}

		public static bool Contains<T>(this IEnumerable<T> list, T item, IEqualityComparer<T> equalityComparer)
		{
			return InternalIndexOf(list, item, equalityComparer) != -1;
		}
		/// <summary>
		/// Returns <see langword="true"/> if there is any element with a structure that matches <paramref name="item"/>.
		/// </summary>
		public static bool Contains<TElement, TFind>(this IEnumerable<TElement> list, TFind item, IEqualityComparer equalityComparer)
			where TElement : IStructuralEquatable
			where TFind : IStructuralEquatable
		{
			return InternalIndexOf(list, item, equalityComparer) != -1;
		}
		public static bool Contains<TElement, TFind, TEqualityComparer>(this IEnumerable<TElement> list, TFind item, TEqualityComparer equalityComparer)
			where TElement : IEquatableUsingComparer<TFind, TEqualityComparer>
			where TEqualityComparer : IEqualityComparer<TFind>
		{
			return InternalIndexOf(list, item, equalityComparer) != -1;
		}


		public static T Last<T>(this IReadOnlyList<T> list)
			=> NthLast(list, -1);

		/// <param name="offset">Should be a negative value, where -1 is the last element.</param>
		public static T NthLast<T>(this IReadOnlyList<T> list, int offset)
		{
			return list[list.Count + offset];
		}

		public static void RemoveLast<T>(this IList<T> list)
			=> RemoveNthLast(list, -1);

		/// <param name="offset">Should be a negative value, where -1 is the last element.</param>
		public static void RemoveNthLast<T>(this IList<T> list, int offset)
		{
			list.RemoveAt(list.Count + offset);
		}

		public static bool TryPeek<T>(this Stack<T> stack, out T item)
		{
			if (stack.Count > 0)
			{
				item = stack.Peek();
				return true;
			}
			else
			{
				item = default;
				return false;
			}
		}

		public static IEnumerable<T> PopAll<T>(this Stack<T> stack)
		{
			while (stack.Count > 0)
			{
				yield return stack.Pop();
			}
		}
	}
}
