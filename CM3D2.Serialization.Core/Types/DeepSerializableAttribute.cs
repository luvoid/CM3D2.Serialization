using System;

namespace CM3D2.Serialization.Types
{
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class DeepSerializableAttribute : Attribute
    { }
}
