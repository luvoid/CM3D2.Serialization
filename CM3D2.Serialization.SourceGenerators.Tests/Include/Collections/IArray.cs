using System;
using System.Collections;
using System.Collections.Generic;

namespace CM3D2.Serialization.Collections
{
	public interface IArray : IArrayCollection
	{
		/// <summary>
		///     Gets or sets the element at the specified index. 
		/// </summary>
		/// <param name="index">
		///     The zero-based index of the element to get or set.
		/// </param>
		/// <returns>
		///     The element at the specified index.
		/// </returns>
		/// <exception cref="System.ArgumentOutOfRangeException">
		///     index is not a valid index in the <see cref="IArray"/>.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		///     The property is set and the <see cref="IArray"/> is read-only.
		/// </exception>
		object this[int index] { get; set; }

		/// <summary>
		///     Copies all the elements of the current one-dimensional <see cref="IArray"/> to the specified
		///     one-dimensional <see cref="IArray"/> starting at the specified destination <see cref="IArray"/>
		///     index. The index is specified as a 64-bit integer.
		/// </summary>
		/// <param name="array">
		///     The one-dimensional <see cref="IArray"/> that is the destination of the elements copied
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
		void CopyTo(IArray array, long index);

		/// <summary>
		///     Gets the value at the specified position in the one-dimensional <see cref="IArray"/>.
		///     The index is specified as a 32-bit integer.
		/// </summary>
		/// <param name="index">
		///     A 32-bit integer that represents the position of the <see cref="IArray"/> element to
		///     get.
		/// </param>
		/// <returns>
		///     The value at the specified position in the one-dimensional <see cref="IArray"/>.
		/// </returns>
		/// <exception cref="System.ArgumentException">
		///     The current <see cref="IArray"/> does not have exactly one dimension.
		/// </exception>
		/// <exception cref="System.IndexOutOfRangeException">
		///     index is outside the range of valid indexes for the current <see cref="IArray"/>.
		/// </exception>
		object GetValue(int index);

		/// <summary>
		///     Gets the value at the specified position in the one-dimensional <see cref="IArray"/>.
		///     The index is specified as a 64-bit integer.
		/// </summary>
		/// <param name="index">
		///     A 64-bit integer that represents the position of the <see cref="IArray"/> element to
		///     get.
		/// </param>
		/// <returns>
		///     The value at the specified position in the one-dimensional <see cref="IArray"/>.
		/// </returns>
		/// <exception cref="System.ArgumentException">
		///     The current <see cref="IArray"/> does not have exactly one dimension.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		///     index is outside the range of valid indexes for the current <see cref="IArray"/>.
		/// </exception>
		object GetValue(long index);

		/// <summary>
		///     Sets a value to the element at the specified position in the one-dimensional
		///     <see cref="IArray"/>. The index is specified as a 32-bit integer.
		/// </summary>
		/// <param name="value">
		///     The new value for the specified element.
		/// </param>
		/// <param name="index">
		///     A 32-bit integer that represents the position of the <see cref="IArray"/> element to
		///     set.
		/// </param>
		/// <exception cref="System.ArgumentException">
		///     The current <see cref="IArray"/> does not have exactly one dimension.
		/// </exception>
		/// <exception cref="System.InvalidCastException">
		///     value cannot be cast to the element type of the current <see cref="IArray"/>.
		/// </exception>
		/// <exception cref="System.IndexOutOfRangeException">
		///     index is outside the range of valid indexes for the current <see cref="IArray"/>.
		/// </exception>
		void SetValue(object value, int index);

		/// <summary>
		///     Sets a value to the element at the specified position in the one-dimensional
		///     <see cref="IArray"/>. The index is specified as a 64-bit integer.
		/// </summary>
		/// <param name="value">
		///     The new value for the specified element.
		/// </param>
		/// <param name="index">
		///     A 64-bit integer that represents the position of the <see cref="IArray"/> element to
		///     set.
		/// </param>
		/// <exception cref="System.ArgumentException">
		///     The current <see cref="IArray"/> does not have exactly one dimension.
		/// </exception>
		/// <exception cref="System.InvalidCastException">
		///     value cannot be cast to the element type of the current <see cref="IArray"/>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		///     index is outside the range of valid indexes for the current <see cref="IArray"/>.
		/// </exception>
		void SetValue(object value, long index);
	}
}