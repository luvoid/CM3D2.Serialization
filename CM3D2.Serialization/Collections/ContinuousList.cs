using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CM3D2.Serialization.Collections
{
	/// <summary>
	/// Elements are serialized sequentially.
	/// </summary>
	/// <remarks>
	/// If there is another element in the list, <c>true</c> is written,
	/// to indicate the list is continued, followed by the element data.
	/// This applies to the first element as well.
	/// <br/>
	/// After all the elements have been written, <c>false</c> is written to
	/// indicate the end of the list.
	/// </remarks>
	public class ContinuousList<T> : List<T>
	{ }
}
