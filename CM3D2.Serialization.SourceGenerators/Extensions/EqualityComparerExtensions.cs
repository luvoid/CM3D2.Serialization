using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CM3D2.Serialization.SourceGenerators.Extensions
{

	internal static class EqualityComparerCastExtensions
	{
		public static IEqualityComparer AsObjectEqualityComparer<T>(this IEqualityComparer<T> genericEqualityComparer)
		{
			if (genericEqualityComparer is IEqualityComparer objectEqualityComparer)
			{
				return objectEqualityComparer;
			}
			else
			{
				return new EqualityComparerCast<T>(genericEqualityComparer);
			}
		}

		private class EqualityComparerCast<T> : IEqualityComparer<T>, IEqualityComparer
		{
			private readonly IEqualityComparer<T> wrappedEqualityComparer;
			public EqualityComparerCast(IEqualityComparer<T> equalityComparer)
			{
				wrappedEqualityComparer = equalityComparer;
			}
			public bool Equals(T x, T y)
			{
				return wrappedEqualityComparer.Equals(x, y);
			}

			public new bool Equals(object x, object y)
			{
				if (typeof(T).IsValueType && (x is not T || y is not T))
					return false;

				if (x is T or null && y is T or null)
				{
					return Equals(
						x is not null ? (T)x : default,
						y is not null ? (T)y : default
					);
				}

				return false;
			}

			public int GetHashCode(T obj)
			{
				return wrappedEqualityComparer.GetHashCode(obj);
			}

			public int GetHashCode(object obj)
			{
				if (typeof(T).IsValueType && obj is null)
				{
					return 0;
				}
				else if (obj is T or null)
				{
					return wrappedEqualityComparer.GetHashCode(
						obj is not null ? (T)obj : default
					);
				}
				else
				{
					return obj.GetHashCode();
				}
			}
		}
	}
}
