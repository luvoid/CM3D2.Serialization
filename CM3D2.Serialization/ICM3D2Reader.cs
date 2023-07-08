using System;
using System.IO;
using System.Text;

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
		T Peek<T>() where T : unmanaged;
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
		void Read<T>(out T val) where T : unmanaged;

		/// <summary>
		///		Reads a nullable primitive or unmanaged struct.
		///		<paramref name="val"/> is set to null if at end-of-stream.
		/// </summary>
		/// <remarks>
		///		If more bytes are in the stream, but not enough for the whole struct,
		///		an <see cref="EndOfStreamException"/> will still be raised.
		/// </remarks>
		/// <exception cref="ArgumentException">
		///		If the type <typeparamref name="T"/> cannot be read.
		///	</exception>
		/// <exception cref="InvalidOperationException">
		///		If the stream does not support seeking
		///	</exception>
		/// <exception cref="EndOfStreamException"></exception>
		void Read<T>(out T? val) where T : unmanaged;

		/// <summary>
		/// Reads a struct or class that implements the <see cref="ICM3D2Serializable"/> interface.
		/// </summary>
		/// <exception cref="EndOfStreamException"></exception>
		void Read<T>(out T obj, object _ = null) where T : ICM3D2Serializable;

		void DebugLog(string note);
	}
}