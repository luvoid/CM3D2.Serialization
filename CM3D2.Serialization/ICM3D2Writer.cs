using System;
using System.ComponentModel;
using System.Text;

namespace CM3D2.Serialization
{
	public interface ICM3D2Writer : IDisposable
	{
		void Write(ICM3D2Serializable obj);

		/// <param name="encoding">Defaults to <see cref="Encoding.UTF8"/></param>
		void Write(string str, Encoding encoding = null);
		void Write<T>(T val) where T : unmanaged;
	}
}