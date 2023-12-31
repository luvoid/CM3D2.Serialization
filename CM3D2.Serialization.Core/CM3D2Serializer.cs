﻿using System;
using System.IO;
using System.Runtime.Serialization;

namespace CM3D2.Serialization
{
	public class CM3D2Serializer : IFormatter
	{
		public ISurrogateSelector SurrogateSelector { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public SerializationBinder Binder { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public StreamingContext Context { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public void Serialize(Stream serializationStream, object obj)
		{
			using (var writer = new CM3D2Writer(serializationStream))
			{
				if (obj is string str)
				{
					writer.Write(str);
				}
				else if (obj is ICM3D2Serializable serializable)
				{
					serializable.WriteWith(writer);
				}
				else
				{
					throw new NotImplementedException();
				}
			}
		}

		public void Serialize<T>(Stream serializationStream, T val)
			where T : unmanaged
		{
			using (var writer = new CM3D2Writer(serializationStream))
			{
				writer.Write(val);
			}
		}

		public object Deserialize(Stream serializationStream)
		{
			using (var reader = new CM3D2Reader(serializationStream))
			{
				//m_SerializationStream = serializationStream;
				throw new NotImplementedException();
			}
		}

		public T Deserialize<T>(Stream serializationStream)
			where T : struct
		{
			Type type = typeof(T);
			if (!typeof(ICM3D2Serializable).IsAssignableFrom(type) && !typeof(T).IsUnmanaged()) 
				throw new ArgumentException($"The type {type.Name} must be an unmanaged struct, or implement {typeof(ICM3D2Serializable)}.");

			return DeserializeUnmanaged<T>(serializationStream);
		}

		public T Deserialize<T>(Stream serializationStream, T obj = null)
			where T : class
		{
			return DeserializeManaged<T>(serializationStream, obj);
		}

		private T DeserializeManaged<T>(Stream serializationStream, T obj = null)
			where T : class
		{
			using (var reader = new CM3D2Reader(serializationStream))
			{
				if (typeof(T) == typeof(string))
				{
					reader.Read(out string str);
					return str as T;
				}

				// else

				if (obj == null)
				{
					try
					{
						obj = Activator.CreateInstance<T>();
					}
					catch (MissingMethodException)
					{
						obj = FormatterServices.GetUninitializedObject(typeof(T)) as T;
					}
				}

				if (typeof(ICM3D2Serializable).IsAssignableFrom(typeof(T)))
				{
					
					(obj as ICM3D2Serializable).ReadWith(reader);
					return obj;
				}
				else
				{
					throw new NotImplementedException();
				}
			}
		}
		
		private T DeserializeUnmanaged<T>(Stream serializationStream)
			where T : struct
		{
			using (var reader = new CM3D2Reader(serializationStream))
			{
				reader.Read(out T val);
				return val;
			}
		}
	}
}
