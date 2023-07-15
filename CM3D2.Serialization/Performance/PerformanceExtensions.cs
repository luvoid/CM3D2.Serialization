using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CM3D2.Serialization.Performance
{
	public static class PerformanceExtensions
	{
		public static void PopulateList<T>(this IList<T> list, int count)
			where T : new()
		{
			while (list.Count < count)
			{
				list.Add(new T());
			}
		}
	}
}
