using System;
using System.IO;

namespace CM3D2.Serialization.Types
{
    /// <summary>
    /// A value that can be completely omitted. If no value is present, that means nothing was read / will be written.
    /// </summary>
    /// <remarks>
    /// There will be nothing in the stream to indicate if the value is there or not.
    /// Ensure this is only ever written at the end of the stream.
    /// <br/><br/>
    /// Attempting to read an <see cref="Omittable{T}"/> while at the end-of-stream will not throw an exception.
    /// If more bytes are in the stream, but not enough for the whole struct,
    ///	an <see cref="EndOfStreamException"/> will still be raised.
    /// </remarks>
    public struct Omittable<[UnmanagedSerializable] T> : ICM3D2Serializable
        where T : struct
    {
        [NonSerialized]
        private bool hasValue;

        private T value;

        void ICM3D2Serializable.WriteWith(ICM3D2Writer writer)
        {
            if (hasValue)
            {
                writer.Write(value);
            }
        }

        void ICM3D2Serializable.ReadWith(ICM3D2Reader reader)
        {
            if (reader.PeekByte() > -1)
            {
                hasValue = true;
                reader.Read(out value);
            }
            else
            {
                hasValue = false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="Omittable{T}"/> struct has a value.
        /// </summary>
        public bool HasValue => hasValue;

        /// <summary>
        /// Gets the value of the current <see cref="Omittable{T}"/>.
        /// </summary>
        public T Value
        {
            get
            {
                if (!HasValue)
                {
                    throw new InvalidOperationException($"The {nameof(Omittable<T>)}<{typeof(T).Name}> has no value");
                }

                return value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the System.Nullable`1 structure to the specified value.
        /// </summary>
        public Omittable(T value)
        {
            this.value = value;
            hasValue = true;
        }

        /// <summary>
        /// Retrieves the value of the current <see cref="Omittable{T}"/> struct, or the struct's default value.
        /// </summary>
        public T GetValueOrDefault()
        {
            return value;
        }

        /// <summary>
        /// Retrieves the value of the current <see cref="Omittable{T}"/> struct, or the specified default value.
        /// </summary>
        public T GetValueOrDefault(T defaultValue)
        {
            if (!HasValue)
            {
                return defaultValue;
            }

            return value;
        }

        /// <summary>
        /// Indicates whether the current System.Nullable`1 object is equal to a specified object.
        /// </summary>
        /// <param name="other"></param>
        /// <remarks>
        /// This table describes how equality is defined for the compared values:
        /// <code>
        /// | Return | ValueDescription 
        /// |--------|--------------------------
        /// | true   | -if- The Omittable.HasValue property is false, and the other parameter is null. 
        /// |        |      That is, two null values are equal by definition.
        /// |        | -or- The Omittable.HasValue property is true, and the value returned by the Omittable.Value property 
        /// |        |      is equal to the other parameter.
        /// |--------|--------------------------
        /// | false  | -if- The Omittable.HasValue property for the current Omittable structure is true, 
        /// |        |      and the other parameter is null.
        /// |        | -or- The Omittable.HasValue property for the current Omittable structure is false, 
        /// |        |      and the other parameter is not null.
        /// |        | -or- The Omittable.HasValue property for the current Omittable structure is true, 
        /// |        |      and the value returned by the Omittable.Value property is not equal to the other parameter.
        /// </code>
        /// </remarks>
        /// <returns></returns>
        public override bool Equals(object other)
        {
            if (!HasValue)
            {
                return other == null;
            }

            if (other == null)
            {
                return false;
            }

            return value.Equals(other);
        }

        /// <summary>
        /// Retrieves the hash code of <see cref="Omittable{T}.value"/>.
        /// </summary>
        /// <remarks>
        /// Returns zero if <see cref="Omittable{T}.hasValue"/> is false.
        /// </remarks>
        public override int GetHashCode()
        {
            if (!HasValue)
            {
                return 0;
            }

            return value.GetHashCode();
        }

        /// <summary>
        /// Returns the text representation of the value of the current <see cref="Omittable{T}"/>.
        /// </summary>
        public override string ToString()
        {
            if (!HasValue)
            {
                return "";
            }

            return value.ToString();
        }

        /// <summary>
        /// Creates a new <see cref="Omittable{T}"/> initialized to a specified value.
        /// </summary>
        public static implicit operator Omittable<T>(T value)
        {
            return value;
        }

        /// <summary>
        /// Returns the value of a specified <see cref="Omittable{T}"/> value.
        /// </summary>
        public static explicit operator T(Omittable<T> value)
        {
            return value.Value;
        }
    }
}
