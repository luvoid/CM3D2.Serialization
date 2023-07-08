using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CM3D2.Serialization.Collections.Generics
{
	public interface IArray<T> : IArrayCollection, IEnumerable<T>
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
		///     index is not a valid index in the <see cref="IArray{T}"/>.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		///     The property is set and the <see cref="IArray{T}"/> is read-only.
		/// </exception>
		T this[int index] { get; set; }

		/// <summary>
		///     Copies all the elements of the specified one-dimensional <see cref="Array"/> to the current
		///     one-dimensional <see cref="IArray{T}"/> starting at the specified destination <see cref="IArray{T}"/>
		///     index. The index is specified as a 32-bit integer.
		/// </summary>
		/// <param name="array">
		///     The one-dimensional <see cref="Array"/> that is the source of the elements copied
		///     to the current <see cref="IArray{T}"/>.
		/// </param>
		/// <param name="index">
		///     A 32-bit integer that represents the index in array at which copying begins.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		///     array is null.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		///     index is outside the range of valid indexes for array.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		///     array is multidimensional.-or-The number of elements in the source <see cref="IArray{T}"/>
		///     is greater than the available space from index to the end of the destination
		///     array.
		/// </exception>
		/// <exception cref="System.RankException">
		///     The source <see cref="Array"/> is multidimensional.
		/// </exception>
		void CopyFrom(T[] array, int toIndex);

		/// <summary>
		///     Copies all the elements of the specified one-dimensional <see cref="T[]"/> to the current
		///     one-dimensional <see cref="IArray{T}"/> starting at the specified destination <see cref="IArray{T}"/>
		///     index. The index is specified as a 64-bit integer.
		/// </summary>
		/// <param name="array">
		///     The one-dimensional <see cref="Array"/> that is the source of the elements copied
		///     to the current <see cref="IArray{T}"/>.
		/// </param>
		/// <param name="toIndex">
		///     A 64-bit integer that represents the index in array at which copying begins.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		///     array is null.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		///     index is outside the range of valid indexes for array.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		///     array is multidimensional.-or-The number of elements in the source <see cref="IArray{T}"/>
		///     is greater than the available space from index to the end of the destination
		///     array.
		/// </exception>
		/// <exception cref="System.RankException">
		///     The source <see cref="IArray{T}"/> is multidimensional.
		/// </exception>
		void CopyFrom(T[] array, long toIndex);

		/// <summary>
		///     Copies the elements of the System.Collections.Generic.ICollection`1 to an System.Array,
		///     starting at a particular System.Array index.
		/// </summary>
		/// <param name="array">
		///     The one-dimensional System.Array that is the destination of the elements copied
		///     from System.Collections.Generic.ICollection`1. The System.Array must have zero-based
		///     indexing.
		/// </param>
		/// <param name="arrayIndex">
		///     The zero-based index in array at which copying begins.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		///     array is null.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		///     arrayIndex is less than 0.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		///     The number of elements in the source System.Collections.Generic.ICollection`1
		///     is greater than the available space from arrayIndex to the end of the destination
		///     array.
		/// </exception>
		void CopyTo(T[] array, int arrayIndex);


		/// <summary>
		///     Copies all the elements of the current one-dimensional <see cref="IArray{T}"/> to the specified
		///     one-dimensional <see cref="IArray{T}"/> starting at the specified destination <see cref="IArray{T}"/>
		///     index. The index is specified as a 32-bit integer.
		/// </summary>
		/// <param name="array">
		///     The one-dimensional <see cref="IArray{T}"/> that is the destination of the elements copied
		///     from the current <see cref="IArray{T}"/>.
		/// </param>
		/// <param name="index">
		///     A 32-bit integer that represents the index in array at which copying begins.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		///     array is null.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		///     index is outside the range of valid indexes for array.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		///     array is multidimensional.-or-The number of elements in the source <see cref="IArray{T}"/>
		///     is greater than the available space from index to the end of the destination
		///     array.
		/// </exception>
		/// <exception cref="System.ArrayTypeMismatchException">
		///     The type of the source <see cref="IArray{T}"/> cannot be cast automatically to the type
		///     of the destination array.
		/// </exception>
		/// <exception cref="System.RankException">
		///     The source <see cref="IArray{T}"/> is multidimensional.
		/// </exception>
		/// <exception cref="System.InvalidCastException">
		///     At least one element in the source <see cref="IArray{T}"/> cannot be cast to the type of
		///     destination array.
		/// </exception>
		void CopyTo(IArray<T> array, int index);

		/// <summary>
		///     Copies all the elements of the current one-dimensional <see cref="IArray{T}"/> to the specified
		///     one-dimensional <see cref="IArray{T}"/> starting at the specified destination <see cref="IArray{T}"/>
		///     index. The index is specified as a 64-bit integer.
		/// </summary>
		/// <param name="array">
		///     The one-dimensional <see cref="IArray{T}"/> that is the destination of the elements copied
		///     from the current <see cref="IArray{T}"/>.
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
		///     array is multidimensional.-or-The number of elements in the source <see cref="IArray{T}"/>
		///     is greater than the available space from index to the end of the destination
		///     array.
		/// </exception>
		/// <exception cref="System.ArrayTypeMismatchException">
		///     The type of the source <see cref="IArray{T}"/> cannot be cast automatically to the type
		///     of the destination array.
		/// </exception>
		/// <exception cref="System.RankException">
		///     The source <see cref="IArray{T}"/> is multidimensional.
		/// </exception>
		/// <exception cref="System.InvalidCastException">
		///     At least one element in the source <see cref="IArray{T}"/> cannot be cast to the type of
		///     destination array.
		/// </exception>
		void CopyTo(IArray<T> array, long index);

		/// <summary>
		///     Gets the value at the specified position in the one-dimensional <see cref="IArray{T}"/>.
		///     The index is specified as a 32-bit integer.
		/// </summary>
		/// <param name="index">
		///     A 32-bit integer that represents the position of the <see cref="IArray{T}"/> element to
		///     get.
		/// </param>
		/// <returns>
		///     The value at the specified position in the one-dimensional <see cref="IArray{T}"/>.
		/// </returns>
		/// <exception cref="System.ArgumentException">
		///     The current <see cref="IArray{T}"/> does not have exactly one dimension.
		/// </exception>
		/// <exception cref="System.IndexOutOfRangeException">
		///     index is outside the range of valid indexes for the current <see cref="IArray{T}"/>.
		/// </exception>
		T GetValue(int index);

		/// <summary>
		///     Gets the value at the specified position in the one-dimensional <see cref="IArray{T}"/>.
		///     The index is specified as a 64-bit integer.
		/// </summary>
		/// <param name="index">
		///     A 64-bit integer that represents the position of the <see cref="IArray{T}"/> element to
		///     get.
		/// </param>
		/// <returns>
		///     The value at the specified position in the one-dimensional <see cref="IArray{T}"/>.
		/// </returns>
		/// <exception cref="System.ArgumentException">
		///     The current <see cref="IArray{T}"/> does not have exactly one dimension.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		///     index is outside the range of valid indexes for the current <see cref="IArray{T}"/>.
		/// </exception>
		T GetValue(long index);

		/// <summary>
		///     Sets a value to the element at the specified position in the one-dimensional
		///     <see cref="IArray{T}"/>. The index is specified as a 32-bit integer.
		/// </summary>
		/// <param name="value">
		///     The new value for the specified element.
		/// </param>
		/// <param name="index">
		///     A 32-bit integer that represents the position of the <see cref="IArray{T}"/> element to
		///     set.
		/// </param>
		/// <exception cref="System.ArgumentException">
		///     The current <see cref="IArray{T}"/> does not have exactly one dimension.
		/// </exception>
		/// <exception cref="System.InvalidCastException">
		///     value cannot be cast to the element type of the current <see cref="IArray{T}"/>.
		/// </exception>
		/// <exception cref="System.IndexOutOfRangeException">
		///     index is outside the range of valid indexes for the current <see cref="IArray{T}"/>.
		/// </exception>
		void SetValue(T value, int index);

		/// <summary>
		///     Sets a value to the element at the specified position in the one-dimensional
		///     <see cref="IArray{T}"/>. The index is specified as a 64-bit integer.
		/// </summary>
		/// <param name="value">
		///     The new value for the specified element.
		/// </param>
		/// <param name="index">
		///     A 64-bit integer that represents the position of the <see cref="IArray{T}"/> element to
		///     set.
		/// </param>
		/// <exception cref="System.ArgumentException">
		///     The current <see cref="IArray{T}"/> does not have exactly one dimension.
		/// </exception>
		/// <exception cref="System.InvalidCastException">
		///     value cannot be cast to the element type of the current <see cref="IArray{T}"/>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		///     index is outside the range of valid indexes for the current <see cref="IArray{T}"/>.
		/// </exception>
		void SetValue(T value, long index);
	}
}
