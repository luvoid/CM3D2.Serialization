﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CM3D2.Serialization.Tests
{
	public class CommonTests
	{
		protected MemoryStream m_Stream = new DebugMemoryStream();
		protected CM3D2Serializer m_Serializer = new CM3D2Serializer();

		protected byte[] ReadBytes(int count = 4096, Stream stream = null)
		{
			if (stream == null) stream = m_Stream;
			var bytes = new byte[count];
			int readCount = stream.Read(bytes, 0, count);
			Array.Resize(ref bytes, readCount);
			return bytes;
		}
	}
}
