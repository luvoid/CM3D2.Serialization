using CM3D2.Serialization.Collections.Generic;
using CM3D2.Serialization.Types;
using System;
using System.Collections.Generic;

namespace CM3D2.Serialization.Collections
{
	/// <summary>
	/// An array whose length must be set/validated before reading and writing.
	/// </summary>
	public sealed class LengthDefinedArray<[UnmanagedSerializable] T> : UnmanagedArray<T>, ICM3D2SerializableInstance, ILengthDefinedCollection
		where T : unmanaged
	{
		[NonSerialized]
		bool _isLengthDefined = false;

		public LengthDefinedArray()
			: this(0)
		{ }

		private LengthDefinedArray(int size)
			: base(size)
		{ }

		public LengthDefinedArray(IList<T> list)
			: base(list)
		{ }

		private LengthDefinedArray(T[] array)
			: base(array)
		{ }

		void ICM3D2SerializableInstance.WriteWith(ICM3D2Writer writer)
		{
			if (!_isLengthDefined)
				throw new InvalidOperationException($"The length of {nameof(LengthDefinedArray<T>)} has changed since the last set/validation, or was never set/validated.");
			this.WriteArrayWith(writer);
		}

		unsafe void ICM3D2SerializableInstance.ReadWith(ICM3D2Reader reader)
		{
			if (!_isLengthDefined) throw new InvalidOperationException($"The length of {nameof(LengthDefinedArray<T>)}<{typeof(T).Name}> was never set/validated.");
			this.ReadArrayWith(reader);
		}

		public void SetLength(int length)
		{
			var newArray = new T[length];
			Array.ConstrainedCopy(m_Array, 0, newArray, 0, Math.Min(length, m_Array.Length));
			m_Array = newArray;

			_isLengthDefined = true;
		}

		public void ValidateLength(int length, string nameofArray = null, string nameofDefinition = null)
		{
			nameofArray ??= nameof(LengthDefinedArray<T>);
			nameofDefinition ??= nameof(length);

			if (length != this.Length)
			{
				throw new InvalidOperationException(
					$"{nameofArray}.{nameof(this.Length)} must be equal to {nameofDefinition} ({length})");
			}

			_isLengthDefined = true;
		}

		public override object Clone() => new LengthDefinedArray<T>(m_Array.Clone() as T[]);
	}
}
