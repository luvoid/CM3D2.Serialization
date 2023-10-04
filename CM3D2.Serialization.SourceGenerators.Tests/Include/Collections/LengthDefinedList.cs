using System;
using System.Collections.Generic;
using System.Drawing;

namespace CM3D2.Serialization.Collections
{
	/// <summary>
	/// A list whose length must be set/validated before reading and writing.
	/// </summary>
	/// <remarks>
	/// If <typeparamref name="T"/> is an unmanaged struct, 
	/// consider using a <see cref="LengthPrefixedArray{T}"/> instead.
	/// </remarks>
	public sealed class LengthDefinedList<T> : List<T>, ICM3D2SerializableInstance, ILengthDefinedCollection
		where T : ICM3D2Serializable, new()
	{
		int _definedLength = -1;

		public LengthDefinedList()
			: this(0)
		{ }

		private LengthDefinedList(int size)
			: base(size)
		{ }

		public LengthDefinedList(IList<T> list)
			: base(list)
		{ }

		void ICM3D2SerializableInstance.WriteWith(ICM3D2Writer writer)
		{
			if (_definedLength != this.Count) 
				throw new InvalidOperationException($"The length of {nameof(LengthDefinedList<T>)} has changed since the last set/validation, or was never set/validated.");

			foreach (T element in this)
			{
				element.WriteWith(writer);
			}
		}

		void ICM3D2SerializableInstance.ReadWith(ICM3D2Reader reader)
		{
			if (_definedLength != this.Count) 
				throw new InvalidOperationException($"The length of {nameof(LengthDefinedList<T>)}<{typeof(T).Name}> has changed since the last set/validation, or was never set/validated.");

			for (int i = 0; i < this.Count; i++)
			{
				T element = new();
				element.ReadWith(reader);
				this[i] = element;
			}
		}


		public void SetLength(int length)
		{
			if (length < this.Count)
			{
				int removeCount = this.Count - length;
				this.RemoveRange(this.Count - removeCount - 1, removeCount);
			}

			while (this.Count < length)
			{
				this.Add(default);
			}

			_definedLength = length;
		}

		public void ValidateLength(int length, string nameofCollection = null, string nameofDefinition = null)
		{
			nameofCollection ??= nameof(LengthDefinedList<T>);
			nameofDefinition ??= nameof(length);

			if (length != this.Count)
			{
				throw new InvalidOperationException(
					$"{nameofCollection}.{nameof(this.Count)} must be equal to {nameofDefinition} ({length})");
			}

			_definedLength = length;
		}
	}
}
