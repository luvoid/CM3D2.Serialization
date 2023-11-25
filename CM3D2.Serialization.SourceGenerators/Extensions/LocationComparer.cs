using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace CM3D2.Serialization.SourceGenerators.Extensions
{
	internal class LocationComparer : IComparer<Location>
	{
		public static LocationComparer Default = new(false);
		public static LocationComparer DepthFirst = new(true);

		private bool useEnd;
		private LocationComparer(bool useEnd)
		{
			this.useEnd = useEnd;
		}

		public int Compare(Location x, Location y)
		{
			int result;
			if (!useEnd)
			{
				result = x.SourceSpan.Start.CompareTo(y.SourceSpan.Start);
				if (result == 0)
				{
					result = -x.SourceSpan.End.CompareTo(y.SourceSpan.End);
				}
			}
			else
			{
				result = x.SourceSpan.End.CompareTo(y.SourceSpan.End);
				if (result == 0)
				{
					result = -x.SourceSpan.Start.CompareTo(y.SourceSpan.Start);
				}
			}
			return result;
		}
	}
}
