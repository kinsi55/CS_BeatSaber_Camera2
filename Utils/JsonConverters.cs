using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using UnityEngine;
using static Camera2.Configuration.CameraSettings;

namespace Camera2.Utils
{
    class RectConverter : JsonConverter<Rect> {
		public override void WriteJson(JsonWriter writer, Rect rect, JsonSerializer serializer) {
			writer.WriteStartObject();
			writer.WritePropertyName("x");
			writer.WriteValue(JsonHelpers.prepareFloat(rect.xMin));
			writer.WritePropertyName("y");
			writer.WriteValue(JsonHelpers.prepareFloat(rect.yMin));
			writer.WritePropertyName("width");
			writer.WriteValue(JsonHelpers.prepareFloat(rect.width));
			writer.WritePropertyName("height");
			writer.WriteValue(JsonHelpers.prepareFloat(rect.height));
			writer.WriteEndObject();
		}

		public override Rect ReadJson(JsonReader reader, Type objectType, Rect existingValue, bool hasExistingValue, JsonSerializer serializer) {
			var rect = new Rect();
			do {
				reader.Read();
				if(reader.TokenType == JsonToken.PropertyName) {
					string property = reader.Value.ToString();
					if(property == "x")
						rect.x = (float)reader.ReadAsDecimal().GetValueOrDefault();
					else if(property == "y")
						rect.y = (float)reader.ReadAsDecimal().GetValueOrDefault();
					else if(property == "width")
						rect.width = (float)reader.ReadAsDecimal().GetValueOrDefault();
					else if(property == "height")
						rect.height = (float)reader.ReadAsDecimal().GetValueOrDefault();
				}
			}
			while(reader.TokenType != JsonToken.EndObject);
			return rect;
		}
	}

	class ScreenRectConverter : JsonConverter<ScreenRect> {
		public override void WriteJson(JsonWriter writer, ScreenRect rect, JsonSerializer serializer) {
			writer.WriteStartObject();
			writer.WritePropertyName("x");
			writer.WriteValue(JsonHelpers.prepareFloat(rect.x));
			writer.WritePropertyName("y");
			writer.WriteValue(JsonHelpers.prepareFloat(rect.y));
			writer.WritePropertyName("width");
			writer.WriteValue(JsonHelpers.prepareFloat(rect.width));
			writer.WritePropertyName("height");
			writer.WriteValue(JsonHelpers.prepareFloat(rect.height));
			writer.WritePropertyName("locked");
			writer.WriteValue(rect.locked);
			writer.WriteEndObject();
		}

		public override ScreenRect ReadJson(JsonReader reader, Type objectType, ScreenRect existingValue, bool hasExistingValue, JsonSerializer serializer) {
			var sr = new ScreenRect(0, 0, 1, 1, false);
			do {
				reader.Read();
				if(reader.TokenType == JsonToken.PropertyName) {
					string property = reader.Value.ToString();
					if(property == "x")
						sr.x = (float)reader.ReadAsDecimal().GetValueOrDefault();
					else if(property == "y")
						sr.y = (float)reader.ReadAsDecimal().GetValueOrDefault();
					else if(property == "width")
						sr.width = (float)reader.ReadAsDecimal().GetValueOrDefault();
					else if(property == "height")
						sr.height = (float)reader.ReadAsDecimal().GetValueOrDefault();
					else if(property == "locked")
						sr.locked = reader.ReadAsBoolean().GetValueOrDefault();
				}
			}
			while(reader.TokenType != JsonToken.EndObject);
			return sr;
		}
	}

	class Vector3Converter : JsonConverter<Vector3> {
		public override void WriteJson(JsonWriter writer, Vector3 vec, JsonSerializer serializer) {
			writer.WriteStartObject();
			writer.WritePropertyName("x");
			writer.WriteValue(JsonHelpers.prepareFloat(vec.x));
			writer.WritePropertyName("y");
			writer.WriteValue(JsonHelpers.prepareFloat(vec.y));
			writer.WritePropertyName("z");
			writer.WriteValue(JsonHelpers.prepareFloat(vec.z));
			writer.WriteEndObject();
		}

		public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer) {
			Vector3 vec = new Vector3();
			do {
				reader.Read();
				if(reader.TokenType == JsonToken.PropertyName) {
					string property = reader.Value.ToString();
					if(property == "x")
						vec.x = (float)reader.ReadAsDecimal().GetValueOrDefault();
					else if(property == "y")
						vec.y = (float)reader.ReadAsDecimal().GetValueOrDefault();
					else if(property == "z")
						vec.z = (float)reader.ReadAsDecimal().GetValueOrDefault();
				}
			}
			while(reader.TokenType != JsonToken.EndObject);
			return vec;
		}
	}

	class StringEnumConverterMigrateFromBool : StringEnumConverter {
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			if(reader.TokenType == JsonToken.Boolean)
				return (bool)reader.Value ? 1 : 0;
			return base.ReadJson(reader, objectType, existingValue, serializer);
		}
	}

	/*class DictionaryConverter<K, V> : JsonConverter<Dictionary<K, V>> where K : IConvertible where V : IConvertible {
		public override void WriteJson(JsonWriter writer, Dictionary<K, V> value, JsonSerializer serializer) {
			writer.WriteStartObject();

			foreach(KeyValuePair<K, V> pair in value) {
				writer.WritePropertyName(Enum.GetName(typeof(K), pair.Key));
				writer.WriteValue(Enum.GetName(typeof(V), pair.Value));
			}

			writer.WriteEndObject();
		}

		public override Dictionary<K, V> ReadJson(JsonReader reader, Type objectType, Dictionary<K, V> existingValue, bool hasExistingValue, JsonSerializer serializer) {
			var jObject = JObject.Load(reader);
			var outVal = new Dictionary<K, V>();

			foreach(var x in jObject) {
				try {
					outVal.Add(
						(K)Enum.Parse(typeof(K), x.Key, true),
						(V)Enum.Parse(typeof(V), x.Value.ToObject<string>(), true)
					);
				} catch { }
			}

			return outVal;
		}
	}*/

	static class JsonHelpers {
		public static JRaw limitFloatResolution(float val) {
			return new JRaw(val.ToString("0.0###", CultureInfo.InvariantCulture));
		}

		public static float prepareFloat(float val)
        {
			return (float)Math.Round(val, 4);
        }

		public static readonly JsonSerializerSettings leanDeserializeSettings = new JsonSerializerSettings {
			NullValueHandling = NullValueHandling.Ignore,
			Error = (se, ev) => {
#if DEBUG
				Plugin.Log.Warn("Failed JSON deserialize:");
				Plugin.Log.Warn(ev.ErrorContext.Error);
#endif
				ev.ErrorContext.Handled = true;
			}
		};
	}
}
