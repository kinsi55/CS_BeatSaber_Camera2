using System;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace Camera2.Utils {
	class RectConverter : JsonConverter<Rect> {
		public override void WriteJson(JsonWriter writer, Rect rect, JsonSerializer serializer) {
			var x = new JObject();

			x.Add("x", new JRaw(rect.xMin.ToString("0.0##", CultureInfo.InvariantCulture)));
			x.Add("y", new JRaw(rect.yMin.ToString("0.0##", CultureInfo.InvariantCulture)));
			x.Add("width", new JRaw(rect.width.ToString("0.0##", CultureInfo.InvariantCulture)));
			x.Add("height", new JRaw(rect.height.ToString("0.0##", CultureInfo.InvariantCulture)));

			x.WriteTo(writer);
		}

		public override Rect ReadJson(JsonReader reader, Type objectType, Rect existingValue, bool hasExistingValue, JsonSerializer serializer) {
			dynamic o = JObject.Load(reader);

			return new Rect((float)o.x, (float)o.y, (float)o.width, (float)o.height);
		}
	}
	
	class Vector3Converter : JsonConverter<Vector3> {
		public override void WriteJson(JsonWriter writer, Vector3 vec, JsonSerializer serializer) {
			var x = new JObject();

			x.Add("x", new JRaw(vec.x.ToString("0.0##", CultureInfo.InvariantCulture)));
			x.Add("y", new JRaw(vec.y.ToString("0.0##", CultureInfo.InvariantCulture)));
			x.Add("z", new JRaw(vec.z.ToString("0.0##", CultureInfo.InvariantCulture)));

			x.WriteTo(writer);
		}

		public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer) {
			dynamic o = JObject.Load(reader);

			return new Vector3((float)o.x, (float)o.y, (float)o.z);
		}
	}
}
