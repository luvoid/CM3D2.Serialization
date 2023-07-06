using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;


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

		public unsafe void Read<T>(out T val)
			where T : unmanaged
		{
			if (typeof(T) == typeof(bool))
			{
				val = (T)(object)ReadBool();
				return;
			}

			int size = sizeof(T);
			byte[] bytes = new byte[size];
			m_Stream.Read(bytes, 0, bytes.Length);
			val = FromBytes<T>(bytes);
			return;
		}


		public void Read(out string str, Encoding encoding = null)
		{
			if (encoding == null) encoding = Encoding.UTF8;
			int size = m_Stream.ReadByte();
			byte[] bytes = new byte[size];
			m_Stream.Read(bytes, 0, bytes.Length);
			str = encoding.GetString(bytes);
		}

		public void Read<T>(out T obj, object _ = null)
			where T : ICM3D2Serializable, new()
		{
			obj = new T();
			obj.ReadWith(this);
		}

		public T Peek<T>()
			where T : unmanaged
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

		private void AssertCanPeak()
		{
			if (!m_Stream.CanSeek)
			{
				throw new InvalidOperationException("Cannot peek because the stream does not support seeking");
			}
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
			object obj;
			fixed (byte* ptr = bytes)
			{
				obj = Marshal.PtrToStructure((IntPtr)ptr, typeof(T));
			}
			return (T)obj;
		}
	}
}
