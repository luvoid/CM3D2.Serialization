using CM3D2.Serialization.Collections.Generics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace CM3D2.Serialization.Collections
{
	public class LengthPrefixedArray<T> : ArrayBase<T>, ICM3D2Serializable
		where T : unmanaged
	{
		private T[] m_Array;

		public LengthPrefixedArray(int size)
		{
			m_Array = new T[size];
		}

		protected LengthPrefixedArray(T[] array)
		{
			m_Array = array;
		}

		void ICM3D2Serializable.WriteWith(ICM3D2Writer writer)
		{
			writer.Write(m_Array.Length);

			// TODO casting to byte[] and writing that is likely much faster.
			foreach (T element in m_Array)
			{
				writer.Write(element);
			}
		}

		unsafe void ICM3D2Serializable.ReadWith(ICM3D2Reader reader)
		{
			reader.Read(out int length);
			m_Array = new T[length];

			// TODO casting to byte[] and reading that is likely much faster.
			for (int i = 0; i < length; i++)
			{
				reader.Read(out m_Array[i]);
			}
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

		public override object Clone() => new LengthPrefixedArray<T>(m_Array.Clone() as T[]);

		public override void Initialize()
		{
			if (m_Array == null) m_Array = new T[0];
			m_Array.Initialize();
		}
	}
}
