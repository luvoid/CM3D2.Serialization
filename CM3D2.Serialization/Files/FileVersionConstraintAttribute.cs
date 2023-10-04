using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CM3D2.Serialization.Files
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class FileVersionConstraintAttribute : Attribute
	{
		/// <summary>
		/// Minimum (inclusive)
		/// </summary>
		public readonly int Min = int.MinValue;

		/// <summary>
		/// Maximum (exclusive)
		/// </summary>
		public readonly int Max = int.MaxValue;

		/// <inheritdoc cref="FileVersionConstraintAttribute(int, int)"/>
		public FileVersionConstraintAttribute(int min)
		{
			Min = min;
		}

		/// <param name="min">Minimum (inclusive)</param>
		/// <param name="max">Maximum (exclusive)</param>
		public FileVersionConstraintAttribute(int min, int max)
		{
			Min = min;
			Max = max;
		}

		/// <inheritdoc cref="FileVersionConstraintAttribute(int, int)"/>
		public FileVersionConstraintAttribute(FileVersions min)
		{
			Min = (int)min;
		}

		/// <inheritdoc cref="FileVersionConstraintAttribute(int, int)"/>
		public FileVersionConstraintAttribute(FileVersions min, FileVersions max)
		{
			Min = (int)min;
			Max = (int)max;
		}
	}
}
