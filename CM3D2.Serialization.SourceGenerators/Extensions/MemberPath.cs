using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace CM3D2.Serialization.SourceGenerators.Extensions
{
	internal struct MemberPath : IImmutableList<ISymbol>, IEquatable<MemberPath>, IStructuralEquatable
	{
		public static readonly MemberPath Empty = new(ImmutableArray<ISymbol>.Empty);

		public static MemberPath FromExpression(ExpressionSyntax expression, SemanticModel semanticModel)
		{
			return FromExpression(expression, semanticModel, out _);
		}

		public static MemberPath FromExpression(ExpressionSyntax expression, SemanticModel semanticModel, out ExpressionSyntax leadingExpression)
		{
			Stack<ISymbol> memberStack = new();
			while (expression is MemberAccessExpressionSyntax or ElementAccessExpressionSyntax)
			{
				if (expression is MemberAccessExpressionSyntax memberAccess)
				{
					memberAccess.TryGetSpeculativeSymbol(semanticModel, out ISymbol memberSymbol);
					memberStack.Push(memberSymbol);
					expression = memberAccess.Expression;
				}
				else if (expression is ElementAccessExpressionSyntax elementAccess)
				{
					elementAccess.TryGetSpeculativeSymbol(semanticModel, out ISymbol indexerSymbol);
					memberStack.Push(indexerSymbol);
					expression = elementAccess.Expression;
				}
			}

			if (expression.TryGetSpeculativeSymbol(semanticModel, out ISymbol rootSymbol))
			{
				memberStack.Push(rootSymbol);
				leadingExpression = null;
			}
			else
			{
				leadingExpression = expression;
			}

			if (memberStack.Count == 0)
			{
				return MemberPath.Empty;
			}

			return new MemberPath(memberStack);
		}


		public static explicit operator MemberPath(ImmutableArray<ISymbol> array)
		{
			return new MemberPath(array);
		}


		private ImmutableArray<ISymbol> array;

		public MemberPath(ImmutableArray<ISymbol> fields)
		{
			array = fields;
		}

		public MemberPath(IEnumerable<ISymbol> fields)
			: this(fields.ToImmutableArray())
		{ }

		public override string ToString()
		{
			StringBuilder sb = new();
			foreach (var field in this)
			{
				if (sb.Length > 0)
					sb.Append(".");
				sb.Append(field?.Name ?? "*");
			}
			return sb.ToString();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(MemberPath left, MemberPath right)
		{
			return left.Equals(right);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(MemberPath left, MemberPath right)
		{
			return !left.Equals(right);
		}



		public int Count => ((IReadOnlyCollection<ISymbol>)array).Count;

		public ISymbol this[int index] => ((IReadOnlyList<ISymbol>)array)[index];

		public override bool Equals(object other)
		{
			if (other is MemberPath otherMemberPath)
			{
				return Equals(otherMemberPath);
			}
			return false;
		}

		public bool Equals(MemberPath other)
		{
			return array.Equals(other.array);
		}

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			object otherArray = other;
			if (other is MemberPath otherFieldPath)
			{
				otherArray = otherFieldPath.array;
			}
			return ((IStructuralEquatable)array).Equals(otherArray, comparer);
		}

		public override int GetHashCode()
		{
			return array.GetHashCode();
		}

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
		{
			return ((IStructuralEquatable)array).GetHashCode(comparer);
		}

		public MemberPath Clear()
		{
			return (MemberPath)array.Clear();
		}

		public int IndexOf(ISymbol item, int index, int count, IEqualityComparer<ISymbol> equalityComparer)
		{
			return array.IndexOf(item, index, count, equalityComparer);
		}

		public int LastIndexOf(ISymbol item, int index, int count, IEqualityComparer<ISymbol> equalityComparer)
		{
			return array.LastIndexOf(item, index, count, equalityComparer);
		}

		public MemberPath Add(ISymbol value)
		{
			return (MemberPath)array.Add(value);
		}

		public MemberPath AddRange(IEnumerable<ISymbol> items)
		{
			return (MemberPath)array.AddRange(items);
		}

		public MemberPath Insert(int index, ISymbol element)
		{
			return (MemberPath)array.Insert(index, element);
		}

		public MemberPath InsertRange(int index, IEnumerable<ISymbol> items)
		{
			return (MemberPath)array.InsertRange(index, items);
		}

		public MemberPath Remove(ISymbol value, IEqualityComparer<ISymbol> equalityComparer)
		{
			return (MemberPath)array.Remove(value, equalityComparer);
		}

		public MemberPath RemoveAll(Predicate<ISymbol> match)
		{
			return (MemberPath)array.RemoveAll(match);
		}

		public MemberPath RemoveRange(IEnumerable<ISymbol> items, IEqualityComparer<ISymbol> equalityComparer)
		{
			return (MemberPath)array.RemoveRange(items, equalityComparer);
		}

		public MemberPath RemoveRange(int index, int count)
		{
			return (MemberPath)array.RemoveRange(index, count);
		}

		public MemberPath RemoveAt(int index)
		{
			return (MemberPath)array.RemoveAt(index);
		}

		public MemberPath SetItem(int index, ISymbol value)
		{
			return (MemberPath)array.SetItem(index, value);
		}

		public MemberPath Replace(ISymbol oldValue, ISymbol newValue, IEqualityComparer<ISymbol> equalityComparer)
		{
			return (MemberPath)array.Replace(oldValue, newValue, equalityComparer);
		}

		public IEnumerator<ISymbol> GetEnumerator()
		{
			return ((IEnumerable<ISymbol>)array).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)array).GetEnumerator();
		}

		IImmutableList<ISymbol> IImmutableList<ISymbol>.Clear()
		{
			return ((IImmutableList<ISymbol>)array).Clear();
		}

		IImmutableList<ISymbol> IImmutableList<ISymbol>.Add(ISymbol value)
		{
			return ((IImmutableList<ISymbol>)array).Add(value);
		}

		IImmutableList<ISymbol> IImmutableList<ISymbol>.AddRange(IEnumerable<ISymbol> items)
		{
			return ((IImmutableList<ISymbol>)array).AddRange(items);
		}

		IImmutableList<ISymbol> IImmutableList<ISymbol>.Insert(int index, ISymbol element)
		{
			return ((IImmutableList<ISymbol>)array).Insert(index, element);
		}

		IImmutableList<ISymbol> IImmutableList<ISymbol>.InsertRange(int index, IEnumerable<ISymbol> items)
		{
			return ((IImmutableList<ISymbol>)array).InsertRange(index, items);
		}

		IImmutableList<ISymbol> IImmutableList<ISymbol>.Remove(ISymbol value, IEqualityComparer<ISymbol> equalityComparer)
		{
			return ((IImmutableList<ISymbol>)array).Remove(value, equalityComparer);
		}

		IImmutableList<ISymbol> IImmutableList<ISymbol>.RemoveAll(Predicate<ISymbol> match)
		{
			return ((IImmutableList<ISymbol>)array).RemoveAll(match);
		}

		IImmutableList<ISymbol> IImmutableList<ISymbol>.RemoveRange(IEnumerable<ISymbol> items, IEqualityComparer<ISymbol> equalityComparer)
		{
			return ((IImmutableList<ISymbol>)array).RemoveRange(items, equalityComparer);
		}

		IImmutableList<ISymbol> IImmutableList<ISymbol>.RemoveRange(int index, int count)
		{
			return ((IImmutableList<ISymbol>)array).RemoveRange(index, count);
		}

		IImmutableList<ISymbol> IImmutableList<ISymbol>.RemoveAt(int index)
		{
			return ((IImmutableList<ISymbol>)array).RemoveAt(index);
		}

		IImmutableList<ISymbol> IImmutableList<ISymbol>.SetItem(int index, ISymbol value)
		{
			return ((IImmutableList<ISymbol>)array).SetItem(index, value);
		}

		IImmutableList<ISymbol> IImmutableList<ISymbol>.Replace(ISymbol oldValue, ISymbol newValue, IEqualityComparer<ISymbol> equalityComparer)
		{
			return ((IImmutableList<ISymbol>)array).Replace(oldValue, newValue, equalityComparer);
		}
	}
}
