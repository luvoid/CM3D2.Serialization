using System.Collections.Generic;

namespace CM3D2.Serialization.Collections.Generic
{
	/// <summary>
	/// An array whose length is defined at creation and before deserialization.
	/// </summary>
	public abstract class UnmanagedArray<T> : Array<T>
		where T : unmanaged
	{
		public UnmanagedArray(int size)
			: base(size)
		{ }

		public UnmanagedArray(IList<T> list)
			: base(list)
		{ }

		protected UnmanagedArray(T[] array)
			: base(array)
		{ }

		protected unsafe void WriteArrayWith(ICM3D2Writer writer)
		{
			// TODO casting to byte[] and writing that is likely much faster.
			foreach (T element in m_Array)
			{
				writer.Write(element);
			}
		}

		protected unsafe void ReadArrayWith(ICM3D2Reader reader)
		{
			// TODO casting to byte[] and reading that is likely much faster.
			for (int i = 0; i < m_Array.Length; i++)
			{
				reader.Read(out m_Array[i]);
			}
		}
	}
}
