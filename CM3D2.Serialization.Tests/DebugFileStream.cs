using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CM3D2.Serialization.Tests
{
	internal class DebugFileStream : FileStream
	{
		private int lastPercent = -1;

		private void LogProgress()
		{
			int percent = (int)((float)this.Position / this.Length * 100);
			if (percent != lastPercent)
			{
				System.Diagnostics.Debug.WriteLine($"Progress: {percent}%");
				System.Diagnostics.Debug.Flush();
				lastPercent = percent;
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			var result = base.Read(buffer, offset, count);
			LogProgress();
			return result;
		}

		public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			var result = base.ReadAsync(buffer, offset, count, cancellationToken);
			LogProgress();
			return result;
		}

		public override int ReadByte()
		{
			var result = base.ReadByte();
			LogProgress();
			return result;
		}

		public DebugFileStream(string path, FileMode mode, FileAccess access)
			: base(path, mode, access)
		{ }

		public DebugFileStream(string path, FileMode mode, FileAccess access, FileShare share)
			: base(path, mode, access, share)
		{ }

		public DebugFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize)
			: base(path, mode, access, share, bufferSize)
		{ }

		public DebugFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options)
			: base(path, mode, access, share, bufferSize, options)
		{ }

		public DebugFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync)
			: base(path, mode, access, share, bufferSize, useAsync)
		{ }

		public DebugFileStream(string path, FileMode mode, FileSystemRights rights, FileShare share, int bufferSize, FileOptions options, FileSecurity fileSecurity)
			: base(path, mode, rights, share, bufferSize, options, fileSecurity)
		{ }

		public DebugFileStream(string path, FileMode mode, FileSystemRights rights, FileShare share, int bufferSize, FileOptions options)
			: base(path, mode, rights, share, bufferSize, options)
		{ }
	}
}
