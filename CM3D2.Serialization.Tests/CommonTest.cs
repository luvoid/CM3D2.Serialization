using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CM3D2.Serialization.Tests
{
	public class CommonTest
	{
		protected MemoryStream m_Stream = new MemoryStream();
		protected CM3D2Formatter m_Formatter = new CM3D2Formatter();

		protected byte[] ReadBytes(int count = 4096, Stream stream = null)
		{
			if (stream == null) stream = m_Stream;
			var bytes = new byte[count];
			int readCount = stream.Read(bytes, 0, count);
			Array.Resize(ref bytes, readCount);
			return bytes;
		}

		protected void PrintBytes(byte[] bytes)
		{
			foreach (byte b in bytes)
			{
				Console.Write($"{b:X2} ");
			}
			Console.WriteLine();
		}
	}
}
