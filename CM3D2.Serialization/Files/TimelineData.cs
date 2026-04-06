using CM3D2.Serialization.Collections;
using CM3D2.Serialization.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CM3D2.Serialization.Files
{
    // AWReadBinaryData
    [Serializable]
    public partial class TimelineData : ICM3D2Serializable, ISummarizable
    {
        public readonly string prefix = "BaseData";
        public int totalFrames;
        public int frameRate;

        [DeepSerialized]
        public List<Track> tracks = new();

        public readonly string suffix = "Finish";

        public abstract class Track : ICM3D2Serializable
        {
            public abstract string typeName { get; }
            public int trackId;
            public int totalFrames;
            public string objectTreePath;

            void ICM3D2Serializable.ReadWith(ICM3D2Reader reader)
            {
                reader.Read(out string foundTypeName);
                if (foundTypeName != typeName) throw new FormatException($"Expected typeName \"{typeName}\" but instead found \"{foundTypeName}\"");

                reader.Read(out trackId);
                reader.Read(out totalFrames);
                reader.Read(out objectTreePath);

                ReadDataWith(reader);
            }

            void ICM3D2Serializable.WriteWith(ICM3D2Writer writer)
            {
                writer.Write(typeName);
                writer.Write(trackId);

                totalFrames = GetTotalFrames();
                writer.Write(totalFrames);
                
                writer.Write(objectTreePath);

                WriteDataWith(writer);
            }

            protected abstract int GetTotalFrames();
            protected abstract void ReadDataWith(ICM3D2Reader reader);
            protected abstract void WriteDataWith(ICM3D2Writer writer);
        }

        public class TranslationTrack : Track
        {
            public const string TypeName = "Translation";
            public override string typeName => TypeName;

            [LengthDefinedBy(nameof(totalFrames))]
            public LengthDefinedArray<Float3> positionArray = new();

            protected override int GetTotalFrames() => positionArray.Length;

            protected override void ReadDataWith(ICM3D2Reader reader)
            {
                positionArray.SetLength(totalFrames);
                reader.Read(ref positionArray);
            }

            protected override void WriteDataWith(ICM3D2Writer writer)
            {
                positionArray.ValidateLength(totalFrames);
                writer.Write(positionArray);
            }
        }

        public class RotationTrack : Track
        {
            public const string TypeName = "Rotation";
            public override string typeName => TypeName;

            [LengthDefinedBy(nameof(totalFrames))]
            public LengthDefinedArray<Float4> quaternionArray = new();

            protected override int GetTotalFrames() => quaternionArray.Length;

            protected override void ReadDataWith(ICM3D2Reader reader)
            {
                quaternionArray.SetLength(totalFrames);
                reader.Read(ref quaternionArray);
            }

            protected override void WriteDataWith(ICM3D2Writer writer)
            {
                quaternionArray.ValidateLength(totalFrames);
                writer.Write(quaternionArray);
            }
        }

        public class PropertyTrack : Track
        {
            public const string TypeName = "Property";
            public override string typeName => TypeName;

            public ValueType valueType;
            public string componentName;
            public string propertyName;

            public LengthPrefixedArray<int> intValueArray = new();
            public LengthPrefixedArray<float> floatValueArray = new();
            public LengthPrefixedArray<Float4> colorValueArray = new();
            public LengthPrefixedArray<Float3> vector3ValueArray = new();

            public LengthPrefixedArray<IntPair> indexArray = new();

            public enum ValueType
            {
                Integer,
                Long,
                Float,
                Double,
                Vector2,
                Vector3,
                Color,
                Rect,
                MorphChannels,
                String,
            }

            protected override int GetTotalFrames() => indexArray.Length;

            protected override void ReadDataWith(ICM3D2Reader reader)
            {
                reader.Read(out valueType);
                reader.Read(out componentName);
                reader.Read(out propertyName);

                switch (valueType)
                {
                    case ValueType.Integer:
                        reader.Read(out intValueArray);
                        break;
                    case ValueType.Float:
                        reader.Read(out floatValueArray);
                        break;
                    case ValueType.Color:
                        reader.Read(out colorValueArray);
                        break;
                    case ValueType.Vector3:
                        reader.Read(out vector3ValueArray);
                        break;
                    default:
                        throw new FormatException($"Unsupported property track value type \"{valueType}\"");
                }
                
                reader.Read(out indexArray);
            }

            protected override void WriteDataWith(ICM3D2Writer writer)
            {
                writer.Write(valueType);
                writer.Write(componentName);
                writer.Write(propertyName);

                switch (valueType)
                {
                    case ValueType.Integer:
                        writer.Write(intValueArray);
                        break;
                    case ValueType.Float:
                        writer.Write(floatValueArray);
                        break;
                    case ValueType.Color:
                        writer.Write(colorValueArray);
                        break;
                    case ValueType.Vector3:
                        writer.Write(vector3ValueArray);
                        break;
                    default:
                        throw new FormatException($"Unsupported property track value type \"{valueType}\"");
                }

                writer.Write(indexArray);
            }
        }

        public partial class EventTrack : Track
        {
            public const string TypeName = "Event";
            public override string typeName => TypeName;

            public LengthPrefixedList<MethodData> methodDataArray = new();

            [AutoCM3D2Serializable]
            public partial class MethodData : ICM3D2Serializable
            {
                public int startFrame;
                public string componentName;
                public string methodName;
                public BoolPrefixedNullableObject<LengthPrefixedList<ParameterData>> parameters;
            }

            public class ParameterData : ICM3D2Serializable
            {
                public ValueType valueType;

                public bool boolValue;
                public int intValue;
                public float floatValue;
                public Float2 vector2Value;
                public Float3 vector3Value;
                public Float4 vector4Value;
                public Float4 colorValue;
                public Float4 rectValue;
                public string stringValue;
                public string objectValue;
                public LengthPrefixedList<ParameterData> arrayValue;
                
                public enum ValueType
                {
                    Integer,
                    Long,
                    Float,
                    Double,
                    Vector2,
                    Vector3,
                    Vector4,
                    Color,
                    Rect,
                    String,
                    Char,
                    Object,
                    Array,
                    Boolean,
                }

                void ICM3D2Serializable.ReadWith(ICM3D2Reader reader)
                {
                    reader.Read(out valueType);

                    switch (valueType)
                    {
                        case ValueType.Boolean:
                            reader.Read(out boolValue);
                            break;
                        case ValueType.Integer:
                        case ValueType.Long:
                            reader.Read(out intValue);
                            break;
                        case ValueType.Float:
                        case ValueType.Double:
                            reader.Read(out floatValue);
                            break;
                        case ValueType.Vector2:
                            reader.Read(out vector2Value);
                            break;
                        case ValueType.Vector3:
                            reader.Read(out vector3Value);
                            break;
                        case ValueType.Vector4:
                            reader.Read(out vector4Value);
                            break;
                        case ValueType.Color:
                            reader.Read(out colorValue);
                            break;
                        case ValueType.Rect:
                            reader.Read(out rectValue);
                            break;
                        case ValueType.String:
                        case ValueType.Char:
                            reader.Read(out stringValue);
                            break;
                        case ValueType.Object:
                            reader.Read(out objectValue);
                            break;
                        case ValueType.Array:
                            reader.Read(out arrayValue);
                            break;
                    }
                }
                void ICM3D2Serializable.WriteWith(ICM3D2Writer writer)
                {
                    writer.Write(valueType);

                    switch (valueType)
                    {
                        case ValueType.Boolean:
                            writer.Write(boolValue);
                            break;
                        case ValueType.Integer:
                        case ValueType.Long:
                            writer.Write(intValue);
                            break;
                        case ValueType.Float:
                        case ValueType.Double:
                            writer.Write(floatValue);
                            break;
                        case ValueType.Vector2:
                            writer.Write(vector2Value);
                            break;
                        case ValueType.Vector3:
                            writer.Write(vector3Value);
                            break;
                        case ValueType.Vector4:
                            writer.Write(vector4Value);
                            break;
                        case ValueType.Color:
                            writer.Write(colorValue);
                            break;
                        case ValueType.Rect:
                            writer.Write(rectValue);
                            break;
                        case ValueType.String:
                        case ValueType.Char:
                            writer.Write(stringValue);
                            break;
                        case ValueType.Object:
                            writer.Write(objectValue);
                            break;
                        case ValueType.Array:
                            writer.Write(arrayValue);
                            break;
                    }
                }

            }

            protected override int GetTotalFrames() => methodDataArray.Max(x => x.startFrame);

            protected override void ReadDataWith(ICM3D2Reader reader)
            {
                reader.Read(out methodDataArray);
            }

            protected override void WriteDataWith(ICM3D2Writer writer)
            {
                writer.Write(methodDataArray);
            }
        }

        void ICM3D2Serializable.ReadWith(ICM3D2Reader reader)
        {
            reader.Read(out string foundPrefix);
            if (foundPrefix != prefix) throw new FormatException($"Expected prefix \"{prefix}\" but instead found \"{foundPrefix}\"");

            reader.Read(out totalFrames);
            reader.Read(out frameRate);

            string typeName;
            while ((typeName = reader.PeekString()) != suffix)
            {
                Track track = null;
                switch (typeName)
                {
                    case TranslationTrack.TypeName:
                        reader.Read(out TranslationTrack translationTrack);
                        track = translationTrack;
                        break;

#pragma warning disable CM3D2Serialization031 // Field Read / Write out of Order
                    case RotationTrack.TypeName:
                        reader.Read(out RotationTrack rotationTrack);
                        track = rotationTrack;
                        break;

                    case PropertyTrack.TypeName:
                        reader.Read(out PropertyTrack propertyTrack);
                        track = propertyTrack;
                        break;

                    case EventTrack.TypeName:
                        reader.Read(out EventTrack eventTrack);
                        track = eventTrack;
                        break;
#pragma warning restore CM3D2Serialization031 // Field Read / Write out of Order

                    default:
                        throw new FormatException($"Unknown track typeName \"{typeName}\"");
                }
                tracks.Add(track);
            }

            reader.Read(out string foundSuffix);
            if (foundSuffix != suffix) throw new FormatException($"Expected suffix \"{suffix}\" but instead found \"{foundSuffix}\"");
        }

        void ICM3D2Serializable.WriteWith(ICM3D2Writer writer)
        {
            throw new NotImplementedException();
        }

        string ISummarizable.Summarize()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append($"{{ {totalFrames}frames@{frameRate}fps Track[{tracks.Count}]");
            if (tracks.Count > 0)
            {
                //stringBuilder.Append($" {{Channel[{tracks[0].channels.Count}]");
                //if (tracks[0].channels.Count > 0)
                //{
                //    stringBuilder.Append($"{{Keyframe[{tracks[0].channels[0].keyframes.Length}]");
                //
                //    if (tracks[0].channels.Count > 1) stringBuilder.Append(",...");
                //    stringBuilder.Append("}");
                //}
                //
                //if (tracks.Count > 1) stringBuilder.Append(", ...");
                //stringBuilder.Append("}");
            }
            stringBuilder.Append(" }");

            return stringBuilder.ToString();
        }
    }
}


