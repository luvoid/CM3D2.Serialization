using System;
using System.IO;
using System.Text;
using CM3D2.Serialization.Types;

namespace CM3D2.Serialization
{
    public interface ICM3D2Reader : IDisposable
	{
		/// <summary>
		///		Check the value of the next byte to be read from the stream,
		///		without advancing the stream position.
		/// </summary>
		/// <returns>
		///		Returns -1 if at the end-of-stream,
		///		otherwise returns the value that is about to be read from the stream.
		/// </returns>
		/// <exception cref="InvalidOperationException">
		///		If the stream does not support seeking
		///	</exception>
		int PeekByte();

		/// <summary>
		///		Peek at the value without advancing the stream position.
		/// </summary>
		/// <remarks>
		///		This method may still raise <see cref="EndOfStreamException"/>.
		///		Use <see cref="PeekByte"/> to avoid exceptions at end-of-stream.
		/// </remarks>
		/// <exception cref="InvalidOperationException">
		///		If the stream does not support seeking
		///	</exception>
		/// <exception cref="EndOfStreamException"></exception>
		T Peek<T>() where T : struct;
		string PeekString(Encoding encoding = null);

		/// <summary>
		///		Reads a string
		/// </summary>
		/// <param name="encoding">
		///		Defaults to <see cref="Encoding.UTF8"/>
		/// </param>
		/// <exception cref="EndOfStreamException"></exception>
		void Read(out string str, Encoding encoding = null);

		/// <summary>
		///		Reads a primitive or unmanaged struct.
		/// </summary>
		/// <remarks>
		///		If the struct implements <see cref="ICM3D2Serializable"/>, 
		///		the interface method will take precedance.
		/// </remarks>
		/// <exception cref="ArgumentException">
		///		If the type <typeparamref name="T"/> cannot be read.
		///	</exception>
		/// <exception cref="EndOfStreamException"></exception>
		void Read<T>(out T val) where T : struct;

		/// <summary>
		///		Reads a primitive or unmanaged struct.
		/// </summary>
		/// <remarks>
		///		This method will always attempt to read a value.
		///		See <see cref="Omittable{T}"/> or <see cref="BoolPrefixedNullable{T}"/>
		///		if special handling of nullable-like values is desired.
		/// </remarks>
		/// <exception cref="ArgumentException">
		///		If the type <typeparamref name="T"/> cannot be read.
		///	</exception>
		/// <exception cref="EndOfStreamException"></exception>
		void Read<T>(out T? val) where T : struct;

		/// <summary>
		/// Reads a struct or class that implements the <see cref="ICM3D2Serializable"/> interface.
		/// </summary>
		/// <exception cref="EndOfStreamException"></exception>
		void Read<T>(out T obj, object _ = null) where T : ICM3D2Serializable;

		/// <summary>
		/// Reads a struct or class that implements the <see cref="ICM3D2SerializableInstance"/> interface.
		/// </summary>
		/// <remarks>The <paramref name="obj"/> should never be null.</remarks>
		/// <exception cref="EndOfStreamException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public void Read<T>(ref T obj, object _ = null, object __ = null) where T : ICM3D2SerializableInstance;

		void DebugLog(string note);
	}
}