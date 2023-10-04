using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CM3D2.Serialization.Collections
{
	/// <summary>
	/// Basis for creating a class that implements the <see cref="IArrayCollection"/> interface.
	/// </summary>
	public abstract class ArrayCollectionBase : IArrayCollection, ICollection
	{
		public abstract int Length { get; }
		public abstract long LongLength { get; }

		/// <summary>
		///     Gets the number of elements contained in the <see cref="IArrayCollection"/>.
		/// </summary>
		/// <returns>
		///     The number of elements contained in the <see cref="IArrayCollection"/>.
		/// </returns>
		int ICollection.Count => Length;
		int IArrayCollection.Count => Length;

		/// <summary>
		///     Gets an object that can be used to synchronize access to the <see cref="IArrayCollection"/>.
		/// </summary>
		/// <returns>
		///     An object that can be used to synchronize access to the <see cref="IArrayCollection"/>.
		/// </returns>
		public virtual object SyncRoot => this;

		/// <summary>
		///     Gets a value indicating whether the <see cref="IArrayCollection"/> is read-only.
		/// </summary>
		/// <returns>
		///     This property is always false for all arrays.
		/// </returns>
		public virtual bool IsReadOnly => false;

		/// <summary>
		///     Gets a value indicating whether the <see cref="IArrayCollection"/> has a fixed size.
		/// </summary>
		/// <returns>
		///     This property is always true for all arrays.
		/// </returns>
		public bool IsFixedSize => true;

		/// <summary>
		///     Gets a value indicating whether access to the <see cref="IArrayCollection"/> is synchronized (thread safe).
		/// </summary>
		/// <returns>
		///     This property is always false for all arrays.
		/// </returns>
		public virtual bool IsSynchronized => false;


		/// <summary>
		///     Copies all the elements of the current one-dimensional <see cref="IArrayCollection"/> to the specified
		///     one-dimensional <see cref="Array"/> starting at the specified destination <see cref="Array"/>
		///     index. The index is specified as a 32-bit integer.
		/// </summary>
		/// <param name="array">
		///     The one-dimensional <see cref="Array"/> that is the destination of the elements copied
		///     from the current <see cref="IArrayCollection"/>.
		/// </param>
		/// <param name="index">
		///     A 32-bit integer that represents the index in array at which copying begins.
		/// </param>
		public abstract void CopyTo(Array array, int index);

		public abstract void CopyTo(Array array, long index);

		/// <summary>
		///     Creates a shallow copy of the <see cref="IArrayCollection"/>.
		/// </summary>
		/// <returns>
		///     A shallow copy of the <see cref="IArrayCollection"/>.
		/// </returns>
		public abstract object Clone();

		/// <summary>
		///     Returns an <see cref="IEnumerator"/> for the <see cref="IArray"/>.
		/// </summary>
		/// <returns>
		///     An <see cref="IEnumerator"/> for the <see cref="IArray"/>.
		/// </returns>
		protected abstract IEnumerator GetObjectEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetObjectEnumerator();

		public abstract void Initialize();
	}
}
