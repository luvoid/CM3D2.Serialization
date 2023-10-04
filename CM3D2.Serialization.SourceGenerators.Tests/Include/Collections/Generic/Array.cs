using System;
using System.Collections;
using System.Collections.Generic;

namespace CM3D2.Serialization.Collections.Generic
{
	/// <summary>
	/// An implementation of <see cref="IArray{T}"/> and <see cref="ArrayBase{T}"/>
	/// using a wrapped array.
	/// </summary>
	public abstract class Array<T> : ArrayBase<T>
	{
		protected T[] m_Array;

		public Array(int size)
		{
			m_Array = new T[size];
		}

		public Array(IList<T> list)
		{
			m_Array = new T[list.Count];
			list.CopyTo(m_Array, 0);
		}

		protected Array(T[] array)
		{
			m_Array = array;
		}

		/// <summary>
		/// Avoid use execpt in performace critical scenarios.
		/// </summary>
		public void UnsafeSetArray(T[] array)
		{
			m_Array = array;
		}

		/// <summary>
		/// Avoid use execpt in performace critical scenarios.
		/// </summary>
		public void UnsafeEnumerable(T[] array)
		{
			m_Array = array;
		}


		public override int Length => m_Array.Length;
		public override long LongLength => m_Array.LongLength;

		public override void CopyFrom(T[] array, int toIndex)
		{
			array.CopyTo(m_Array, toIndex);
		}

		public override void CopyFrom(T[] array, long toIndex)
		{
			array.CopyTo(m_Array, toIndex);
		}

		public override void CopyTo(IArray<T> array, int index)
		{
			array.CopyFrom(m_Array, index);
		}

		public override void CopyTo(IArray<T> array, long index)
		{
			array.CopyFrom(m_Array, index);
		}

		public override T GetValue(int index) => m_Array[index];
		public override T GetValue(long index) => m_Array[index];
		public override void SetValue(T value, int index) => m_Array[index] = value;
		public override void SetValue(T value, long index) => m_Array[index] = value;

		protected override IEnumerator<T> GetGenericEnumerator() => (m_Array as IEnumerable<T>).GetEnumerator();

		protected override int IndexOf(T obj) => (m_Array as IList).IndexOf(obj);

		protected override void Clear() => (m_Array as IList).Clear();

		public override void CopyTo(Array array, int index) => m_Array.CopyTo(array, index);

		public override void CopyTo(Array array, long index) => m_Array.CopyTo(array, index);

		public override void Initialize()
		{
			if (m_Array == null) m_Array = new T[0];
			m_Array.Initialize();
		}
	}
}
