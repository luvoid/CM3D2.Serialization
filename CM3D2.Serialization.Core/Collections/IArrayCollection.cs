using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace CM3D2.Serialization.Collections
{
	public interface IArrayCollection : ICloneable, ICollection
	{
		[EditorBrowsable(EditorBrowsableState.Never), Obsolete("Use IArrayCollection.Length instead")]
		new int Count { get; }

		/// <summary>
		///     Gets a value indicating whether the System.Collections.IList is read-only.
		/// </summary>
		/// <returns>
		///     true if the System.Collections.IList is read-only; otherwise, false.
		/// </returns>
		bool IsReadOnly { get; }

		/// <summary>
		///     Gets a 32-bit integer that represents the total number of elements in all the
		///     dimensions of the array.
		/// </summary>
		/// <returns>
		///     A 32-bit integer that represents the total number of elements in all the dimensions
		///     of the array; zero if there are no elements in the array.
		/// </returns>
		int Length { get; }


		/// <summary>
		///     Gets a 64-bit integer that represents the total number of elements in all the
		///     dimensions of the <see cref="IArrayCollection"/>.
		/// </summary>
		/// <returns>
		///     A 64-bit integer that represents the total number of elements in all the dimensions
		///    of the <see cref="IArrayCollection"/>.
		/// </returns>
		long LongLength { get; }

		/// <summary>
		///     Copies all the elements of the current one-dimensional <see cref="IArray"/> to the specified
		///     one-dimensional <see cref="Array"/> starting at the specified destination <see cref="Array"/>
		///     index. The index is specified as a 64-bit integer.
		/// </summary>
		/// <param name="array">
		///     The one-dimensional <see cref="Array"/> that is the destination of the elements copied
		///     from the current <see cref="IArray"/>.
		/// </param>
		/// <param name="index">
		///     A 64-bit integer that represents the index in array at which copying begins.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		///     array is null.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		///     index is outside the range of valid indexes for array.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		///     array is multidimensional.-or-The number of elements in the source <see cref="IArray"/>
		///     is greater than the available space from index to the end of the destination
		///     array.
		/// </exception>
		/// <exception cref="System.ArrayTypeMismatchException">
		///     The type of the source <see cref="IArray"/> cannot be cast automatically to the type
		///     of the destination array.
		/// </exception>
		/// <exception cref="System.RankException">
		///     The source <see cref="IArray"/> is multidimensional.
		/// </exception>
		/// <exception cref="System.InvalidCastException">
		///     At least one element in the source <see cref="IArray"/> cannot be cast to the type of
		///     destination array.
		/// </exception>
		void CopyTo(Array array, long index);

		/// <summary>
		///     Initializes every element of the value-type <see cref="IArrayCollection"/> by calling the default
		///     constructor of the value type.
		/// </summary>
		void Initialize();
	}
}