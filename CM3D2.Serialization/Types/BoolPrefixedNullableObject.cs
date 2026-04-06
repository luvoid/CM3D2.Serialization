using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CM3D2.Serialization.Types
{
    public struct BoolPrefixedNullableObject<T> : ICM3D2Serializable
        where T : class, ICM3D2Serializable
    {
        private bool hasValue;

        private T value;

        void ICM3D2Serializable.WriteWith(ICM3D2Writer writer)
        {
            writer.Write(hasValue);
            if (hasValue)
            {
                writer.Write(value);
            }
        }

        void ICM3D2Serializable.ReadWith(ICM3D2Reader reader)
        {
            reader.Read(out hasValue);
            if (hasValue)
            {
                reader.Read(out value);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="BoolPrefixedNullableObject{T}"/> struct has a value.
        /// </summary>
        public bool HasValue => hasValue;

        /// <summary>
        /// Gets the value of the current <see cref="BoolPrefixedNullableObject{T}"/>.
        /// </summary>
        public T Value
        {
            get
            {
                if (!HasValue)
                {
                    return null;
                }

                return value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the System.Nullable`1 structure to the specified value.
        /// </summary>
        public BoolPrefixedNullableObject(T value)
        {
            this.value = value;
            hasValue = true;
        }

        /// <summary>
        /// Retrieves the value of the current <see cref="BoolPrefixedNullableObject{T}"/> struct, or the struct's default value.
        /// </summary>
        public T GetValueOrDefault()
        {
            return value;
        }

        /// <summary>
        /// Retrieves the value of the current <see cref="BoolPrefixedNullableObject{T}"/> struct, or the specified default value.
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
        /// | true   | -if- The BoolPrefixedNullableObject.HasValue property is false, and the other parameter is null. 
        /// |        |      That is, two null values are equal by definition.
        /// |        | -or- The BoolPrefixedNullableObject.HasValue property is true, and the value returned by the BoolPrefixedNullableObject.Value property 
        /// |        |      is equal to the other parameter.
        /// |--------|--------------------------
        /// | false  | -if- The BoolPrefixedNullableObject.HasValue property for the current BoolPrefixedNullableObject structure is true, 
        /// |        |      and the other parameter is null.
        /// |        | -or- The BoolPrefixedNullableObject.HasValue property for the current BoolPrefixedNullableObject structure is false, 
        /// |        |      and the other parameter is not null.
        /// |        | -or- The BoolPrefixedNullableObject.HasValue property for the current BoolPrefixedNullableObject structure is true, 
        /// |        |      and the value returned by the BoolPrefixedNullableObject.Value property is not equal to the other parameter.
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
        /// Retrieves the hash code of <see cref="BoolPrefixedNullableObject{T}.value"/>.
        /// </summary>
        /// <remarks>
        /// Returns zero if <see cref="BoolPrefixedNullableObject{T}.hasValue"/> is false.
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
        /// Returns the text representation of the value of the current <see cref="BoolPrefixedNullableObject{T}"/>.
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
        /// Creates a new <see cref="BoolPrefixedNullableObject{T}"/> initialized to a specified value.
        /// </summary>
        public static implicit operator BoolPrefixedNullableObject<T>(T value)
        {
            return new BoolPrefixedNullableObject<T>(value);
        }

        /// <summary>
        /// Returns the value of a specified <see cref="BoolPrefixedNullableObject{T}"/> value.
        /// </summary>
        public static explicit operator T(BoolPrefixedNullableObject<T> value)
        {
            return value.Value;
        }
    }
}
