using CM3D2.Serialization.Collections.Generic;
using System.Collections.Generic;

namespace CM3D2.Serialization.Collections
{
	/// <summary>
	/// An array whose length is defined by an integer serialized at the beginning of the array.
	/// </summary>
	public sealed class LengthPrefixedArray<T> : UnmanagedArray<T>, ICM3D2Serializable
		where T : unmanaged
	{
		public LengthPrefixedArray()
			: this(0)
		{ }

		public LengthPrefixedArray(int size)
			: base(size)
		{ }

		public LengthPrefixedArray(IList<T> list)
			: base(list)
		{ }

		private LengthPrefixedArray(T[] array)
			: base(array)
		{ }
		
		void ICM3D2Serializable.WriteWith(ICM3D2Writer writer)
		{
			writer.Write(m_Array.Length);
			this.WriteArrayWith(writer);
		}

		void ICM3D2Serializable.ReadWith(ICM3D2Reader reader)
		{
			reader.Read(out int length);
			m_Array = new T[length];
			this.ReadArrayWith(reader);
		}

		public override object Clone() => new LengthPrefixedArray<T>(m_Array.Clone() as T[]);
	}
}
