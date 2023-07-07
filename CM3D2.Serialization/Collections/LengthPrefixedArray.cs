using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace CM3D2.Serialization.Collections
{
	public class LengthPrefixedArray<T> : ICloneable, IList<T>, ICM3D2Serializable
		where T : unmanaged
	{
		private T[] m_Array;

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
			reader.Read(out uint length);
			m_Array = new T[(long)length];

			// TODO casting to byte[] and reading that is likely much faster.
			for (int i = 0; i < length; i++)
			{
				reader.Read(out m_Array[i]);
			}
		}


		public int Length => m_Array.Length;
		public long LongLength => m_Array.LongLength;

		/// <summary> Gets the zero-based rank (number of dimensions) of the System.Array. </summary>
		/// <returns> The zero-based rank (number of dimensions) of the System.Array. </returns>
		public int Rank => m_Array.Rank;
		
		/// <summary> Gets an object that can be used to synchronize access to the System.Array. </summary>
		/// <returns> An object that can be used to synchronize access to the System.Array. </returns>
		public object SyncRoot => m_Array.SyncRoot;
		
		/// <summary> Gets a value indicating whether the System.Array has a fixed size. </summary>
		/// <returns> This property is always true for all arrays. </returns>
		public bool IsFixedSize => m_Array.IsSynchronized;

		/// <summary> Gets a value indicating whether access to the System.Array is synchronized (thread safe). </summary>
		/// <returns> This property is always false for all arrays. </returns>
		public bool IsSynchronized => m_Array.IsSynchronized;


		public T this[int index] { get => m_Array[index]; set => m_Array[index] = value; }

		int ICollection<T>.Count => ((ICollection<T>)m_Array).Count;
		   
		/// <summary>
		/// Gets a value indicating whether the System.Array is read-only.
		/// </summary>
		public bool IsReadOnly => m_Array.IsReadOnly;

		void ICollection<T>.Add(T item) 
			=> ((ICollection<T>)m_Array).Add(item);

		void ICollection<T>.Clear() 
			=> ((ICollection<T>)m_Array).Clear();

		public object Clone() 
			=> m_Array.Clone();

		public bool Contains(T item) 
			=> m_Array.Contains(item);

		public void CopyTo(T[] array, int arrayIndex)
			=> m_Array.CopyTo(array, arrayIndex);

		int IList<T>.IndexOf(T item)
			=> ((IList<T>)m_Array).IndexOf(item);

		void IList<T>.Insert(int index, T item)
			=> ((IList<T>)m_Array).Insert(index, item);

		bool ICollection<T>.Remove(T item)
			=> ((ICollection<T>)m_Array).Remove(item);

		void IList<T>.RemoveAt(int index)
			=> ((IList<T>)m_Array).RemoveAt(index);

		public IEnumerator GetEnumerator()
			=> m_Array.GetEnumerator();

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
			=> ((IEnumerable<T>)m_Array).GetEnumerator();
	}
}
