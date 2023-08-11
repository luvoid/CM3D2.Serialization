using CM3D2.Serialization.Collections.Generic;
using System;
using System.Collections.Generic;

namespace CM3D2.Serialization.Collections
{
	/// <summary>
	/// An array whose length is defined at creation and before deserialization.
	/// </summary>
	public sealed class LengthDefinedArray<T> : UnmanagedArray<T>, ICM3D2SerializableInstance
		where T : unmanaged
	{
		bool _isLengthDefined = true;
		bool _isLengthValidated = false;

		public LengthDefinedArray()
			: this(0)
		{
			_isLengthDefined = false;
		}

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
			if (!_isLengthValidated) throw new InvalidOperationException($"The length of {nameof(LengthDefinedArray<T>)} was never validated.");
			this.WriteArrayWith(writer);
		}

		unsafe void ICM3D2SerializableInstance.ReadWith(ICM3D2Reader reader)
		{
			if (!_isLengthDefined) throw new InvalidOperationException($"The length of {nameof(LengthDefinedArray<T>)}<{typeof(T).Name}> was never defined.");
			this.ReadArrayWith(reader);
		}

		/// <summary>
		/// Call this function before reading
		/// </summary>
		public void SetLength(int length)
		{
			_isLengthDefined = true;
			var newArray = new T[length];
			Array.ConstrainedCopy(m_Array, 0, newArray, 0, Math.Min(length, m_Array.Length));
			m_Array = newArray;
		}

		/// <summary>
		/// Call this function before writing
		/// </summary>
		/// <exception cref="InvalidOperationException">If the array's length is not equal to <paramref name="length"/></exception>
		public void ValidateLength(int length, string nameofArray = null, string nameofDefinition = null)
		{
			nameofArray ??= nameof(LengthDefinedArray<T>);
			nameofDefinition ??= nameof(length);

			if (length != this.Length)
			{
				throw new InvalidOperationException(
					$"{nameofArray}.{nameof(this.Length)} must be equal to {nameofDefinition} ({length})");
			}
			_isLengthValidated = true;
		}

		public override object Clone() => new LengthDefinedArray<T>(m_Array.Clone() as T[]);
	}
}
