using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;

namespace CM3D2.Serialization.SourceGenerators.Extensions
{
	internal class WildcardSymbolEqualityComparer : IEqualityComparer<ISymbol>, IEqualityComparer
	{
		public static readonly WildcardSymbolEqualityComparer Default = new(SymbolEqualityComparer.Default);
		public static readonly WildcardSymbolEqualityComparer IncludeNullability = new(SymbolEqualityComparer.IncludeNullability);

		private readonly IEqualityComparer<ISymbol> equalityComparer;
		private WildcardSymbolEqualityComparer(IEqualityComparer<ISymbol> equalityComparer)
		{
			this.equalityComparer = equalityComparer;
		}

		public bool Equals(ISymbol x, ISymbol y)
		{
			if (x == null || y == null)
			{
				return true;
			}
			else
			{
				return equalityComparer.Equals(x, y);
			}
		}

		public int GetHashCode(ISymbol obj)
		{
			return equalityComparer.GetHashCode(obj);
		}

		bool IEqualityComparer.Equals(object x, object y)
		{
			if (x is ISymbol xSymbol && y is ISymbol ySymbol)
			{
				return Equals(xSymbol, ySymbol);
			}
			else
			{
				return Equals(x, y);
			}
		}

		int IEqualityComparer.GetHashCode(object obj)
		{
			if (obj is ISymbol symbol)
			{
				return GetHashCode(symbol);
			}
			else
			{
				return obj.GetHashCode();
			}
		}
	}
}
