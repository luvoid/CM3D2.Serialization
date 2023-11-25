using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;

namespace CM3D2.Serialization.Collections
{
	/// <summary>
	/// Basis for creating a class that implements the <see cref="IArray"/> interface.
	/// </summary>
	public abstract class ArrayBase : ArrayCollectionBase, IArray, IList
	{
		/// <summary>
		///     Gets or sets the element at the specified index.
		/// </summary>
		/// <param name="index">
		///     The index of the element to get or set.
		/// </param>
		/// <returns>
		///     The element at the specified index.
		/// </returns>
		/// <exception cref="System.ArgumentOutOfRangeException">
		///     index is less than zero.-or-index is equal to or greater than System.Collections.ICollection.Count.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		///     The current <see cref="IArray"/> does not have exactly one dimension.
		/// </exception>
		public object this[int index]
		{
			get => GetValue(index);
			set => SetValue(value, index);
		}

		public abstract object GetValue(int index);
		
		public abstract object GetValue(long index);
		
		public abstract void SetValue(object value, int index);
		
		public abstract void SetValue(object value, long index);

		/// <summary>
		///     Implements System.Collections.IList.Add(System.Object). Throws a System.NotSupportedException
		///     in all cases.
		/// </summary>
		/// <returns>
		///     An exception is always thrown.
		/// </returns>
		/// <exception cref="System.NotSupportedException">
		///     In all cases.
		/// </exception>
		int IList.Add(object value)
		{
			throw new NotSupportedException("This method is not supported on a fixed size collection");
		}

		/// <summary>
		///     Determines whether an element is in the <see cref="IArray"/>.
		/// </summary>
		/// <param name="value">
		///     The object to locate in the <see cref="IArray"/>. The element to locate can be null for
		///     reference types.
		/// </param>
		/// <returns>
		///     true if value is found in the <see cref="IArray"/>; otherwise, false.
		/// </returns>
		/// <exception cref="System.RankException">
		///     The current <see cref="IArray"/> is multidimensional.
		/// </exception>
		bool IList.Contains(object value)
		{
			return (this as IList).IndexOf(value) >= 0;
		}

		/// <summary>
		///     Sets all elements in the <see cref="IArray"/> to zero, to false, or to null, depending
		///     on the element type.
		/// </summary>
		/// <exception cref="System.NotSupportedException">
		///     The <see cref="IArray"/> is read-only.
		/// </exception>
		void IList.Clear() => Clear();

		/// <summary>
		///     Searches for the specified object and returns the index of the first occurrence
		///     within the current one-dimensional instance.
		/// </summary>
		/// <param name="value">
		///     The object to locate in the current <see cref="IArray"/>.
		/// </param>
		/// <returns>
		///     The index of the first occurrence of value within the entire <see cref="IArray"/>, if
		///     found; otherwise, the lower bound of the <see cref="IArray"/> minus 1.
		/// </returns>
		/// <exception cref="System.RankException">
		///     The current <see cref="IArray"/> is multidimensional.
		/// </exception>
		int IList.IndexOf(object value) => IndexOf(value);

		/// <summary>
		///     Implements System.Collections.IList.Insert(System.Int32,System.Object). Throws
		///     a System.NotSupportedException in all cases.
		/// </summary>
		/// <exception cref="System.NotSupportedException">
		///     In all cases.
		/// </exception>
		void IList.Insert(int index, object value)
		{
			throw new NotSupportedException("This method is not supported on a fixed size collection");
		}

		/// <summary>
		///     Implements System.Collections.IList.Remove(System.Object). Throws a System.NotSupportedException
		///     in all cases.
		/// </summary>
		/// <exception cref="System.NotSupportedException">
		///     In all cases.
		/// </exception>
		void IList.Remove(object value)
		{
			throw new NotSupportedException("This method is not supported on a fixed size collection");
		}

		/// <summary>
		///     Implements System.Collections.IList.RemoveAt(System.Int32). Throws a System.NotSupportedException
		///     in all cases.
		/// </summary>
		/// <exception cref="System.NotSupportedException">
		///     In all cases.
		/// </exception>
		void IList.RemoveAt(int index)
		{
			throw new NotSupportedException("This method is not supported on a fixed size collection");
		}

		public abstract void CopyTo(IArray array, int index);

		public abstract void CopyTo(IArray array, long index);

		protected abstract int IndexOf(object obj);

		protected abstract void Clear();
	}
}
