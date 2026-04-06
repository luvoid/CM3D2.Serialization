using CM3D2.Serialization.Files;
using CM3D2.Serialization.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CM3D2.Serialization.Tests
{
    [TestClass]
    public class TimelineDataTests : FileTests<TimelineData>
    {
        const string k_ExampleTimelineDataPath = "Resources/timeline_data.bytes";

        protected override TimelineData CreateExample()
        {
            throw new NotImplementedException();
        }

        protected override string ExampleFilePath => k_ExampleTimelineDataPath;

        [TestMethod] public override void SerializeTest() { }// => DoSerializeTest();
        [TestMethod] public override void DeserializeTest() => DoDeserializeTest();
        [TestMethod] public override void SerializeAndDeserializeTest() { }// => DoSerializeAndDeserializeTest();
        [TestMethod] public override void UnchangedTest() { }// => DoUnchangedTest();


        [TestMethod]
        public void DeserializeToJSON()
        {
            string filePath = ExampleFilePath;

            TimelineData example;
            using (var file = new DebugFileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                example = m_Serializer.Deserialize<TimelineData>(file);
            }

            var options = new JsonSerializerOptions()
            {
                IncludeFields = true,
                WriteIndented = true,
                Converters = { new TrackConverter(), new ParameterDataConverter() },
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };

            var json = JsonSerializer.Serialize(example, options);
            Logger.LogMessage(json);
        }
    }

    public class TrackConverter : JsonConverter<TimelineData.Track>
    {
        public override TimelineData.Track Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, TimelineData.Track value, JsonSerializerOptions options)
        {
            if (value is TimelineData.EventTrack eventTrack)
            {
                writer.WriteRawValue(JsonSerializer.Serialize(eventTrack, options));
            }
            else if (value is TimelineData.PropertyTrack propertyTrack)
            {
                //writer.WriteRawValue(JsonSerializer.Serialize(propertyTrack, options));
                writer.WriteStartObject();
                writer.WriteString(nameof(propertyTrack.typeName), propertyTrack.typeName);
                writer.WriteNumber(nameof(propertyTrack.trackId), propertyTrack.trackId);
                writer.WriteNumber(nameof(propertyTrack.totalFrames), propertyTrack.totalFrames);
                writer.WriteString(nameof(propertyTrack.objectTreePath), propertyTrack.objectTreePath);

                writer.WriteString(nameof(propertyTrack.valueType), propertyTrack.valueType.ToString());
                writer.WriteString(nameof(propertyTrack.componentName), propertyTrack.componentName);
                writer.WriteString(nameof(propertyTrack.propertyName), propertyTrack.propertyName);

                void WriteProperty<T>(string fieldName, T fieldValue)
                {
                    writer.WritePropertyName(fieldName);
                    writer.WriteRawValue(JsonSerializer.Serialize(fieldValue, options));
                }

                switch (propertyTrack.valueType)
                {
                    case TimelineData.PropertyTrack.ValueType.Integer:
                        WriteProperty(nameof(propertyTrack.intValueArray), propertyTrack.intValueArray);
                        break;
                    case TimelineData.PropertyTrack.ValueType.Float:
                        WriteProperty(nameof(propertyTrack.floatValueArray), propertyTrack.floatValueArray);
                        break;
                    case TimelineData.PropertyTrack.ValueType.Color:
                        WriteProperty(nameof(propertyTrack.colorValueArray), propertyTrack.colorValueArray);
                        break;
                    case TimelineData.PropertyTrack.ValueType.Vector3:
                        WriteProperty(nameof(propertyTrack.vector3ValueArray), propertyTrack.vector3ValueArray);
                        break;
                    default:
                        WriteProperty("ERROR", $"Unsupported property-track value-type '{propertyTrack.valueType}'");
                        break;
                }

                WriteProperty(nameof(propertyTrack.vector3ValueArray),propertyTrack.indexArray);

                writer.WriteEndObject();
            }
            else if (value is TimelineData.RotationTrack rotationTrack)
            {
                writer.WriteRawValue(JsonSerializer.Serialize(rotationTrack, options));
            }
            else if (value is TimelineData.TranslationTrack translationTrack)
            {
                writer.WriteRawValue(JsonSerializer.Serialize(translationTrack, options));
            }
        }
    }

    public class ParameterDataConverter : JsonConverter<TimelineData.EventTrack.ParameterData>
    {
        public override TimelineData.EventTrack.ParameterData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, TimelineData.EventTrack.ParameterData value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString(nameof(value.valueType), value.valueType.ToString());

            void WriteProperty<T>(string fieldName, T fieldValue)
            {
                writer.WritePropertyName(fieldName);
                writer.WriteRawValue(JsonSerializer.Serialize(fieldValue, options));
            }

            switch (value.valueType)
            {
                case TimelineData.EventTrack.ParameterData.ValueType.Boolean:
                    WriteProperty(nameof(value.boolValue), value.boolValue);
                    break;
                case TimelineData.EventTrack.ParameterData.ValueType.Integer:
                case TimelineData.EventTrack.ParameterData.ValueType.Long:
                    WriteProperty(nameof(value.intValue), value.intValue);
                    break;
                case TimelineData.EventTrack.ParameterData.ValueType.Float:
                case TimelineData.EventTrack.ParameterData.ValueType.Double:
                    WriteProperty(nameof(value.floatValue), value.floatValue);
                    break;
                case TimelineData.EventTrack.ParameterData.ValueType.Vector2:
                    WriteProperty(nameof(value.vector2Value), value.vector2Value);
                    break;
                case TimelineData.EventTrack.ParameterData.ValueType.Vector3:
                    WriteProperty(nameof(value.vector3Value), value.vector3Value);
                    break;
                case TimelineData.EventTrack.ParameterData.ValueType.Vector4:
                    WriteProperty(nameof(value.vector4Value), value.vector4Value);
                    break;
                case TimelineData.EventTrack.ParameterData.ValueType.Color:
                    WriteProperty(nameof(value.colorValue), value.colorValue);
                    break;
                case TimelineData.EventTrack.ParameterData.ValueType.Rect:
                    WriteProperty(nameof(value.intValue), value.intValue);
                    break;
                case TimelineData.EventTrack.ParameterData.ValueType.String:
                case TimelineData.EventTrack.ParameterData.ValueType.Char:
                    WriteProperty(nameof(value.stringValue), value.stringValue);
                    break;
                case TimelineData.EventTrack.ParameterData.ValueType.Object:
                    WriteProperty(nameof(value.objectValue), value.objectValue);
                    break;
                case TimelineData.EventTrack.ParameterData.ValueType.Array:
                    WriteProperty(nameof(value.arrayValue), value.arrayValue);
                    break;
                default:
                    WriteProperty("ERROR", $"Unsupported parameter-data value-type '{value.valueType}'");
                    break;

            }

            writer.WriteEndObject();
        }
    }
}
