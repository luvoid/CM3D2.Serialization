using System;
using System.Collections.Generic;
using System.Text;

namespace CM3D2.Serialization.Collections
{
	/// <summary>
	/// A list of strings whose length must be set/validated before reading and writing.
	/// </summary>
	public sealed class LengthDefinedStringList : List<string>, ICM3D2SerializableInstance, ILengthDefinedCollection
	{
		[NonSerialized]
		public Encoding Encoding = Encoding.UTF8;

		[NonSerialized]
		int _definedLength = -1;

		public LengthDefinedStringList()
			: this(0)
		{ }

		private LengthDefinedStringList(int size)
			: base(size)
		{ }

		public LengthDefinedStringList(IList<string> list)
			: base(list)
		{ }

		void ICM3D2SerializableInstance.WriteWith(ICM3D2Writer writer)
		{
			if (_definedLength != this.Count)
				throw new InvalidOperationException($"The length of {nameof(LengthDefinedStringList)} has changed since the last set/validation, or was never set/validated.");

			foreach (string element in this)
			{
				writer.Write(element, Encoding);
			}
		}

		void ICM3D2SerializableInstance.ReadWith(ICM3D2Reader reader)
		{
			if (_definedLength != this.Count)
				throw new InvalidOperationException($"The length of {nameof(LengthDefinedStringList)} has changed since the last set/validation, or was never set/validated.");

			for (int i = 0; i < this.Count; i++)
			{
				reader.Read(out string element, Encoding);
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
			nameofCollection ??= nameof(LengthDefinedStringList);
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
