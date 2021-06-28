using System;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;
using static Camera2.Configuration.CameraSettings;

namespace Camera2.Utils {
	class RectConverter : JsonConverter<Rect> {
		public override void WriteJson(JsonWriter writer, Rect rect, JsonSerializer serializer) {
			var x = new JObject();

			x.Add("x", JsonHelpers.limitFloatResolution(rect.xMin));
			x.Add("y", JsonHelpers.limitFloatResolution(rect.yMin));
			x.Add("width", JsonHelpers.limitFloatResolution(rect.width));
			x.Add("height", JsonHelpers.limitFloatResolution(rect.height));

			x.WriteTo(writer);
		}

		public override Rect ReadJson(JsonReader reader, Type objectType, Rect existingValue, bool hasExistingValue, JsonSerializer serializer) {
			dynamic o = JObject.Load(reader);

			return new Rect((float)o.x, (float)o.y, (float)o.width, (float)o.height);
		}
	}

	class ScreenRectConverter : JsonConverter<ScreenRect> {
		public override void WriteJson(JsonWriter writer, ScreenRect rect, JsonSerializer serializer) {
			var x = new JObject();

			x.Add("x", JsonHelpers.limitFloatResolution(rect.x));
			x.Add("y", JsonHelpers.limitFloatResolution(rect.y));
			x.Add("width", JsonHelpers.limitFloatResolution(rect.width));
			x.Add("height", JsonHelpers.limitFloatResolution(rect.height));
			x.Add("locked", rect.locked);

			x.WriteTo(writer);
		}

		public override ScreenRect ReadJson(JsonReader reader, Type objectType, ScreenRect existingValue, bool hasExistingValue, JsonSerializer serializer) {
			JObject jo = JObject.Load(reader);
			dynamic o = jo;

			return new ScreenRect((float)o.x, (float)o.y, (float)o.width, (float)o.height, jo.GetValue("locked")?.Value<bool>() ?? false);
		}
	}

	class Vector3Converter : JsonConverter<Vector3> {
		public override void WriteJson(JsonWriter writer, Vector3 vec, JsonSerializer serializer) {
			var x = new JObject();

			x.Add("x", JsonHelpers.limitFloatResolution(vec.x));
			x.Add("y", JsonHelpers.limitFloatResolution(vec.y));
			x.Add("z", JsonHelpers.limitFloatResolution(vec.z));

			x.WriteTo(writer);
		}

		public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer) {
			dynamic o = JObject.Load(reader);

			return new Vector3((float)o.x, (float)o.y, (float)o.z);
		}
	}

	class StringEnumConverterMigrateFromBool : StringEnumConverter {
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			if(reader.TokenType == JsonToken.Boolean)
				return (bool)reader.Value ? 1 : 0;
			return base.ReadJson(reader, objectType, existingValue, serializer);
		}
	}

	class DictionaryConverter<K, V> : JsonConverter<Dictionary<K, V>> where K: IConvertible where V : IConvertible {
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
	}

	static class JsonHelpers {
		public static JRaw limitFloatResolution(float val) {
			return new JRaw(val.ToString("0.0###", CultureInfo.InvariantCulture));
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
