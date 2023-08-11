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
		///		Writes a nullable primitive or unmanaged struct. 
		///		Nothing will be written if the value does not exist.
		/// </summary>
		/// <remarks>
		///		This method does not write anything that indicates if the
		///		value is there or not. Unless it is at the end of the stream,
		///		make sure there is some sort of external indicator.
		/// </remarks>
		/// <exception cref="ArgumentException">
		///		If the type <typeparamref name="T"/> cannot be written.
		///	</exception>
		void Write<T>(T? val) where T : struct;

		void DebugLog(string note);
	}
}