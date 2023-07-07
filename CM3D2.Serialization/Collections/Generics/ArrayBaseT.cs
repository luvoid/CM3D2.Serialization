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

namespace CM3D2.Serialization.Collections.Generics
{
	/// <summary>
	/// Basis for creating a class that implements the <see cref="IArray{T}"/> interface.
	/// </summary>
	public abstract class ArrayBase<T> : ArrayCollectionBase, IArray<T>, IList<T>
	{
		int ICollection<T>.Count => throw new NotImplementedException();

		public virtual T this[int index] { get => GetValue(index); set => SetValue(value, index); }

		public virtual void CopyTo(T[] array, int arrayIndex) => CopyTo((Array)array, arrayIndex);
		public abstract void CopyTo(IArray<T> array, int index);
		public abstract void CopyTo(IArray<T> array, long index);
		public abstract T GetValue(int index);
		public abstract T GetValue(long index);
		public abstract void SetValue(T value, int index);
		public abstract void SetValue(T value, long index);

		IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetGenericEnumerator();
		protected sealed override IEnumerator GetObjectEnumerator() => GetGenericEnumerator();
		protected abstract IEnumerator<T> GetGenericEnumerator();


		protected abstract int IndexOf(T obj);
		protected abstract void Clear();

		int IList<T>.IndexOf(T item) => IndexOf(item);

		/// <summary>
		///     Implements System.Collections.IList.Insert(System.Int32,System.Object). Throws
		///     a System.NotSupportedException in all cases.
		/// </summary>
		/// <exception cref="System.NotSupportedException">
		///     In all cases.
		/// </exception>
		void IList<T>.Insert(int index, T value)
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
		void IList<T>.RemoveAt(int index)
		{
			throw new NotSupportedException("This method is not supported on a fixed size collection");
		}

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
		void ICollection<T>.Add(T item)
		{
			throw new NotSupportedException("This method is not supported on a fixed size collection");
		}

		/// <summary>
		///     Sets all elements in the <see cref="IArray"/> to zero, to false, or to null, depending
		///     on the element type.
		/// </summary>
		/// <exception cref="System.NotSupportedException">
		///     The <see cref="IArray"/> is read-only.
		/// </exception>
		void ICollection<T>.Clear() => Clear();

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
		bool ICollection<T>.Contains(T item)
		{
			return IndexOf(item) >= 0;
		}

		/// <summary>
		///     Implements System.Collections.IList.Remove(System.Object). Throws a System.NotSupportedException
		///     in all cases.
		/// </summary>
		/// <exception cref="System.NotSupportedException">
		///     In all cases.
		/// </exception>
		bool ICollection<T>.Remove(T item)
		{
			throw new NotSupportedException("This method is not supported on a fixed size collection");
		}
	}
}
