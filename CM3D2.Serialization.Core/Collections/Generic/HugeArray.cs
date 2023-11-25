using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CM3D2.Serialization.Collections.Generic
{
	public class HugeArray<T> : ArrayBase<T>
	{
		T[][] m_Arrays;

		[EditorBrowsable(EditorBrowsableState.Never), Obsolete("Use HugeArray<T>[long] instead")]
		public sealed override T this[int index]
		{ 
			get => base[index]; 
			set => base[index] = value;
		}

		public T this[long index]
		{
			get => GetValue(index);
			set => SetValue(value, index);
		}

		[EditorBrowsable(EditorBrowsableState.Never), Obsolete("Use LongLength instead")]
		public sealed override int Length => (int)LongLength;

		public override long LongLength => m_Arrays.Sum((a) => a.LongLength);

		private HugeArray(T[][] arrays)
		{
			m_Arrays = arrays;
		}

		public override void Initialize()
		{
			throw new NotImplementedException();
		}

		public override object Clone()
		{
			T[][] arrays = new T[m_Arrays.Length][];
			for (int i = 0; i < m_Arrays.Length; i++)
			{
				arrays[i] = m_Arrays[i].Clone() as T[];
			}
			return new HugeArray<T>(arrays);
		}

		void AssertOneDimension(Array array)
		{
			if (array != null && array.Rank != 1)
			{
				throw new ArgumentException("Cannot copy to multi-dimensional array");
			}
		}

		void AssertInCopyRange(Array array, long index)
		{
			if (LongLength > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException("index", $"Size of {this.GetType()} exceeds the max size of {array.GetType()}");
			}

			if (index + LongLength > array.Length)
			{
				throw new ArgumentOutOfRangeException($"Copying would execed the bounds of the array");
			}
		}

		public override void CopyFrom(T[] array, int toIndex) => CopyFrom(array, (long)toIndex);

		public override void CopyFrom(T[] array, long toIndex)
		{
			throw new NotImplementedException();
		}

		public override void CopyTo(IArray<T> array, int index) => CopyTo(array, (long)index);

		public override void CopyTo(IArray<T> array, long index)
		{
			throw new NotImplementedException();
		}

		public override void CopyTo(Array array, int index) => CopyTo(array, (long)index);

		public override void CopyTo(Array array, long index)
		{
			AssertOneDimension(array);
			AssertInCopyRange(array, index);

			if (m_Arrays.Length > 0)
			{
				m_Arrays[0].CopyTo(array, index);
			}
		}


		public const long MAX_INDEX = long.MaxValue >> 1;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="row"></param>
		/// <param name="col"></param>
		/// <returns>
		///		Returns true if the index is valid (for a max size HugeArray). 
		///		Returns false if the if the index is negative or greater than MAX_INDEX
		///	</returns>
		public static bool TranslateIndex(long index, out int row, out int col)
		{

			unchecked // Ignore arithmatic overflow errors. Invalid values are recognized by the returned bool.
			{
				// row == (index / (int.MaxValue + 1L))  FOR ALL NUMBERS >= 0 AND <= k_MaxIndex
				row = (int)(index >> 31);

				// col == (index % (int.MaxValue + 1L))  FOR ALL NUMBERS >= 0 AND <= k_MaxIndex
				col = (int)(index & int.MaxValue);
			}

			// 0 <= index <= MAX_INDEX
			return (0 <= index) && (index <= MAX_INDEX);
		}

		[EditorBrowsable(EditorBrowsableState.Never), Obsolete("Use GetValue(long) instead")]
		public sealed override T GetValue(int index) => GetValue((long)index);

		public override T GetValue(long index)
		{
			if (!TranslateIndex(index, out int x, out int y)) throw new IndexOutOfRangeException();
			return m_Arrays[x][y];
		}

		[EditorBrowsable(EditorBrowsableState.Never), Obsolete("Use SetValue(value, long) instead")]
		public override void SetValue(T value, int index) => SetValue(value, (long)index);

		public override void SetValue(T value, long index)
		{
			if (!TranslateIndex(index, out int x, out int y)) throw new IndexOutOfRangeException();
			m_Arrays[x][y] = value;
		}

		protected override void Clear()
		{
			throw new NotImplementedException();
		}

		protected override IEnumerator<T> GetGenericEnumerator()
		{
			throw new NotImplementedException();
		}

		protected override int IndexOf(T obj)
		{
			throw new NotImplementedException();
		}
	}
}
