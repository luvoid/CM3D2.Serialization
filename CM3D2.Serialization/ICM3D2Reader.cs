using System;
using System.ComponentModel;
using System.Text;

namespace CM3D2.Serialization
{
	public interface ICM3D2Reader : IDisposable
	{
		T Peek<T>() where T : unmanaged;
		string PeekString(Encoding encoding = null);

		/// <param name="encoding"> Defaults to <see cref="Encoding.UTF8"/> </param>
		void Read(out string str, Encoding encoding = null);
		void Read<T>(out T val) where T : unmanaged;
		void Read<T>(out T obj, object _ = null) where T : ICM3D2Serializable, new();
	}
}