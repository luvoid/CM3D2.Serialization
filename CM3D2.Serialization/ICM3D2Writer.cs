using System;
using System.ComponentModel;
using System.Text;

namespace CM3D2.Serialization
{
	public interface ICM3D2Writer : IDisposable
	{
		/// <summary>
		/// Writes a struct or class that implements the <see cref="ICM3D2Serializable"/> interface.
		/// </summary>
		void Write(ICM3D2Serializable obj);

		/// <summary>
		/// Writes a struct or class that implements the <see cref="ICM3D2SerializableInstance"/> interface.
		/// </summary>
		void Write(ICM3D2SerializableInstance obj);

		/// <summary>
		///		Writes a string.
		/// </summary>
		/// <param name="encoding">
		///		Defaults to <see cref="Encoding.UTF8"/>.
		/// </param>
		void Write(string str, Encoding encoding = null);

		/// <summary>
		///		Writes a primitive or unmanaged struct.
		/// </summary>
		/// <remarks>
		///		If the struct implements <see cref="ICM3D2Serializable"/>, 
		///		the interface method will take precedance.
		/// </remarks>
		/// <exception cref="ArgumentException">
		///		If the type <typeparamref name="T"/> cannot be written.
		///	</exception>
		void Write<T>(T val) where T : struct;

		/// <summary>
		///		Writes a primitive or unmanaged struct.
		/// </summary>
		/// <remarks>
		///		This method will always attempt to write a value.
		///		See <see cref="Omittable{T}"/> or <see cref="BoolPrefixedNullable{T}"/>
		///		if special handling of nullable-like values is desired.
		/// </remarks>
		/// <exception cref="ArgumentException">
		///		If the type <typeparamref name="T"/> cannot be written.
		///	</exception>
		/// <exception cref="InvalidOperationException">
		///		If <paramref name="val"/> has no value.
		///	</exception>
		void Write<T>(T? val) where T : struct;

		void DebugLog(string note);
	}
}