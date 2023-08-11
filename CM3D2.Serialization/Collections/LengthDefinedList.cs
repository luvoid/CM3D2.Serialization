using System;
using System.Collections.Generic;

namespace CM3D2.Serialization.Collections
{
	/// <summary>
	/// A list where that is prefixed by its length as an integer.
	/// </summary>
	/// <remarks>
	/// If <typeparamref name="T"/> is an unmanaged struct, 
	/// consider using a <see cref="LengthPrefixedArray{T}"/> instead.
	/// </remarks>
	public sealed class LengthDefinedList<T> : List<T>, ICM3D2SerializableInstance
		where T : ICM3D2Serializable, new()
	{
		bool _isLengthDefined = true;
		bool _isLengthValidated = true;

		public LengthDefinedList()
			: this(0)
		{
			_isLengthDefined = false;
		}

		private LengthDefinedList(int size)
			: base(size)
		{ }

		public LengthDefinedList(IList<T> list)
			: base(list)
		{ }

		void ICM3D2SerializableInstance.WriteWith(ICM3D2Writer writer)
		{
			if (!_isLengthValidated) throw new InvalidOperationException($"The length of {nameof(LengthDefinedList<T>)} was never validated.");

			foreach (T element in this)
			{
				element.WriteWith(writer);
			}
		}

		void ICM3D2SerializableInstance.ReadWith(ICM3D2Reader reader)
		{
			if (!_isLengthDefined) throw new InvalidOperationException($"The length of {nameof(LengthDefinedList<T>)}<{typeof(T).Name}> was never defined.");

			for (int i = 0; i < this.Count; i++)
			{
				T element = new();
				element.ReadWith(reader);
				this[i] = element;
			}
		}


		/// <summary>
		/// Call this function before reading
		/// </summary>
		public void SetLength(int length)
		{
			_isLengthDefined = true;

			if (length < this.Count)
			{
				int removeCount = this.Count - length;
				this.RemoveRange(this.Count - removeCount - 1, removeCount);
			}

			while (this.Count < length)
			{
				this.Add(default);
			}
		}

		/// <summary>
		/// Call this function before writing
		/// </summary>
		/// <exception cref="InvalidOperationException">If the list's count is not equal to <paramref name="length"/></exception>
		public void ValidateLength(int length, string nameofList = null, string nameofDefinition = null)
		{
			nameofList ??= nameof(LengthDefinedList<T>);
			nameofDefinition ??= nameof(length);

			if (length != this.Count)
			{
				throw new InvalidOperationException(
					$"{nameofList}.{nameof(this.Count)} must be equal to {nameofDefinition} ({length})");
			}
			_isLengthValidated = true;
		}
	}
}
