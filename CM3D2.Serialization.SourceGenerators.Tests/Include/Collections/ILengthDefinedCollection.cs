using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CM3D2.Serialization.Collections
{
	public interface ILengthDefinedCollection : ICollection, ICM3D2SerializableInstance
	{
		/// <summary>
		/// Call this function before reading
		/// </summary>
		public void SetLength(int length);

		/// <summary>
		/// Call this function before writing
		/// </summary>
		/// <exception cref="InvalidOperationException">If the collection's length is not equal to <paramref name="length"/></exception>
		public void ValidateLength(int length, string nameofCollection = null, string nameofDefinition = null);
	}
}
