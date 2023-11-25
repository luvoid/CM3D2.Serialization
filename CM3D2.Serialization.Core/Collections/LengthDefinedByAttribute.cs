using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CM3D2.Serialization.Collections
{
	public sealed class LengthDefinedByAttribute : Attribute
	{
		public readonly string MemberName;

		public LengthDefinedByAttribute(string memberName)
		{
			MemberName = memberName;
		}
	}
}
