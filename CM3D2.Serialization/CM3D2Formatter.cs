using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace CM3D2.Serialization
{
	public class CM3D2Formatter : IFormatter
	{
		public ISurrogateSelector SurrogateSelector { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public SerializationBinder Binder { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public StreamingContext Context { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		private Stream m_SerializationStream;


		public void Serialize(Stream serializationStream, object obj)
		{
			m_SerializationStream = serializationStream;
			Write(obj);
		}

		public void Serialize<T>(Stream serializationStream, T val)
			where T : unmanaged
		{
			m_SerializationStream = serializationStream;
			Write(val);
		}

		public object Deserialize(Stream serializationStream)
		{
			m_SerializationStream = serializationStream;
			throw new NotImplementedException();
		}

		public T Deserialize<T>(Stream serializationStream)
			where T : unmanaged
		{
			m_SerializationStream = serializationStream;
			return Read<T>();
		}

		public T Deserialize<T>(Stream serializationStream, T _ = null)
			where T : class
		{
			m_SerializationStream = serializationStream;
			return Read<T>();
		}

		internal void Write(object obj)
		{
			Type type = obj.GetType();
			if (obj is string str)
			{
				WriteString(str);
			}
			else if (obj is ICM3D2Serializable serializable)
			{
				serializable.WriteWith(this);
			}
			else
			{
				throw new NotImplementedException(obj.ToString());
			}
		}
		internal void Write<T>(T val)
			where T : unmanaged
		{
			if (typeof(T) == typeof(bool))
			{
				m_SerializationStream.WriteByte(1); // bools should only ever be 1 byte in length
			}
			else
			{
				byte[] bytes = ToBytes(val);
				m_SerializationStream.Write(bytes, 0, bytes.Length);
			}
		}

		internal unsafe T Read<T>()
			where T : unmanaged
		{
			if (typeof(T) == typeof(bool))
			{
				return (T)(object)ReadBool();
			}
			int size = sizeof(T);
			byte[] bytes = new byte[size];
			m_SerializationStream.Read(bytes, 0, bytes.Length);
			return FromBytes<T>(bytes);
		}

		internal T Read<T>(T _ = null)
			where T : class
		{
			if (typeof(T) == typeof(string))
			{
				return (T)(object)ReadString();
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		protected unsafe virtual byte[] ToBytes(in object structure)
		{
			Type type = structure.GetType();
			int size = Marshal.SizeOf(type);

			byte[] bytes = new byte[size];
			fixed (byte* ptr = bytes)
			{
				Marshal.StructureToPtr(structure, (IntPtr)ptr, true);
			}

			return bytes;
		}
		
		protected unsafe T FromBytes<T>(in byte[] bytes)
			where T : struct
		{
			object obj;
			fixed (byte* ptr = bytes)
			{
				obj = Marshal.PtrToStructure((IntPtr)ptr, typeof(T));
			}
			return (T)obj;
		}

		protected void WriteBool()
		{
			m_SerializationStream.WriteByte(1); // bools should only ever be 1 byte in length
		}

		protected bool ReadBool()
		{
			int value = m_SerializationStream.ReadByte();
			if (value == -1) throw new EndOfStreamException();
			return value != 0;
		}

		/// <param name="encoding">Defaults to <see cref="Encoding.ASCII"/></param>
		protected void WriteString(in string str, Encoding encoding = null)
		{
			if (encoding == null) encoding = Encoding.ASCII;
			byte[] bytes = encoding.GetBytes(str);
			m_SerializationStream.WriteByte((byte)bytes.Length);
			m_SerializationStream.Write(bytes, 0, bytes.Length);
		}

		/// <param name="encoding">Defaults to <see cref="Encoding.ASCII"/></param>
		protected string ReadString(Encoding encoding = null)
		{
			if (encoding == null) encoding = Encoding.ASCII;
			int size = m_SerializationStream.ReadByte();
			byte[] bytes = new byte[size];
			m_SerializationStream.Read(bytes, 0, bytes.Length);
			return encoding.GetString(bytes);
		}
	}
}
