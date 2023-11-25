using System.Collections;
using System.IO;

namespace CM3D2.Serialization.SourceGenerators.Tests
{
	internal static class TextWriterExtensions
	{
		public static void WriteAll(this TextWriter textWriter, IEnumerable enumerable)
		{
			foreach (var item in enumerable)
			{
				textWriter.WriteLine(item);
			}
		}
	}
}
