using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using CM3D2.Serialization.Types;

namespace CM3D2.Serialization
{
    internal class CM3D2Reader : ICM3D2Reader
	{
		Stream m_Stream;

		public CM3D2Reader(Stream stream)
		{
			m_Stream = stream;
		}
		
		void IDisposable.Dispose()
		{
			m_Stream = null;
		}

		public int PeekByte()
		{
			AssertCanPeak();
			long position = m_Stream.Position;
			int val = m_Stream.ReadByte();
			m_Stream.Position = position;
			return val;
		}

		public T Peek<T>()
			where T : struct
		{
			AssertCanPeak();
			long position = m_Stream.Position;
			Read(out T val);
			m_Stream.Position = position;
			return val;
		}

		public string PeekString(Encoding encoding = null)
		{
			AssertCanPeak();
			long position = m_Stream.Position;
			Read(out string str, encoding);
			m_Stream.Position = position;
			return str;
		}

		public void Read(out string str, Encoding encoding = null)
		{
			if (encoding == null) encoding = Encoding.UTF8;
			Read(out Int7Bit32 size);
			byte[] bytes = new byte[size];
			int read = m_Stream.Read(bytes, 0, bytes.Length);
			if (read != bytes.Length) throw new EndOfStreamException();
			str = encoding.GetString(bytes);
		}

		public unsafe void Read<T>(out T val)
			where T : struct
		{
			Type type = typeof(T);

			if (typeof(ICM3D2Serializable).IsAssignableFrom(type))
			{
				val = ReadInterface<T>();
			}

			else if (type == typeof(bool))
			{
				val = (T)(object)ReadBool();
			}

			else if (!type.IsUnmanaged())
			{
				throw new ArgumentException($"Cannot read struct {type.Name} because it is not an unmanaged type.");
			}

			else if (!type.IsBytesCastable())
			{
				throw new ArgumentException($"Cannot read struct {type.Name} with an invalid StructLayout");
			}

			else
			{
				int size = sizeof(T);
				byte[] bytes = new byte[size];
				int read = m_Stream.Read(bytes, 0, bytes.Length);
				if (read != bytes.Length) throw new EndOfStreamException();
				val = FromBytes<T>(bytes);
			}
		}

		public void Read<T>(out T? val)
			where T : struct
		{
			Read(out T readVal);
			val = readVal;
		}

		public void Read<T>(out T obj, object _ = null)
			where T : ICM3D2Serializable
		{
			obj = ReadInterface<T>();
		}

		public void Read<T>(ref T obj, object _ = null, object __ = null)
			where T : ICM3D2SerializableInstance
		{
			if (obj == null) throw new ArgumentNullException($"A instance is required to read the type {typeof(T).Name}");
			obj.ReadWith(this);
		}

		//internal void ReadBytesAs<[IsUnmanaged] T>()

		protected T ReadInterface<T>()
		{
			object obj;
			try
			{
				obj = Activator.CreateInstance<T>();
			}
			catch (MissingMethodException)
			{
				obj = (T)FormatterServices.GetSafeUninitializedObject(typeof(T));
			}
			(obj as ICM3D2Serializable).ReadWith(this);
			return (T)obj;
		}

		protected bool ReadBool()
		{
			int value = m_Stream.ReadByte();
			if (value == -1) throw new EndOfStreamException();
			return value != 0;
		}
		
		protected unsafe T FromBytes<T>(in byte[] bytes)
			where T : struct
		{
			Type type = typeof(T);
			Type marshalType;
			if (type.IsEnum)
			{
				marshalType = Enum.GetUnderlyingType(type);
			}
			else
			{
				marshalType = type;
			}

			object obj;
			fixed (byte* ptr = bytes)
			{
				obj = Marshal.PtrToStructure((IntPtr)ptr, marshalType);
			}

			return (T)obj;
		}

		private void AssertCanPeak()
		{
			if (!m_Stream.CanSeek)
			{
				throw new InvalidOperationException("Cannot peek because the stream does not support seeking");
			}
		}
		public void DebugLog(string note)
		{
#if DEBUG
			Console.Error.WriteLine($"{note} at stream position 0x{m_Stream.Position:X8}");
#endif
		}

		

	}
}
