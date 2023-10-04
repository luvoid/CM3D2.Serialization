using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CM3D2.Serialization.SourceGenerators.Extensions
{
    internal interface IEquatableUsingComparer<T, TEqualityComparer>
        where TEqualityComparer : IEqualityComparer<T>
	{
        public bool Equals(T other, TEqualityComparer equalityComparer);
    }
}
