using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CM3D2.Serialization.Files
{
	/// <summary>
	/// Named version numbers used in files.
	/// <br/><br/>
	/// A <c>version</c> field should always be an <see cref="int"/>;
	/// these enums are mostly to be used as a named integer for readability.
	/// </summary>
	public enum FileVersions : int
	{
		CM3D2 = 1000,
		COM3D2 = 2000,
		COM3D2_1 = 2001,
	}
}
