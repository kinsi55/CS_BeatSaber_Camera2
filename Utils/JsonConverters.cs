using System;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Camera2.Utils {
	class RectConverter : JsonConverter<Rect> {
		public override void WriteJson(JsonWriter writer, Rect rect, JsonSerializer serializer) {
			JObject.FromObject(new {
				x = rect.xMin,
				y = rect.yMin,
				width = rect.width,
				height = rect.height
			}).WriteTo(writer);
		}

		public override Rect ReadJson(JsonReader reader, Type objectType, Rect existingValue, bool hasExistingValue, JsonSerializer serializer) {
			dynamic o = JObject.Load(reader);

			return new Rect((float)o.x, (float)o.y, (float)o.width, (float)o.height);
		}
	}
	
	class Vector3Converter : JsonConverter<Vector3> {
		public override void WriteJson(JsonWriter writer, Vector3 vec, JsonSerializer serializer) {
			JObject.FromObject(new {
				x = vec.x,
				y = vec.y,
				z = vec.z
			}).WriteTo(writer);
		}

		public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer) {
			dynamic o = JObject.Load(reader);

			return new Vector3((float)o.x, (float)o.y, (float)o.z);
		}
	}

	class QuaternionConverter : JsonConverter<Quaternion> {
		public override void WriteJson(JsonWriter writer, Quaternion quat, JsonSerializer serializer) {
			JObject.FromObject(new {
				x = quat.x,
				y = quat.y,
				z = quat.z,
				w = quat.w
			}).WriteTo(writer);
		}

		public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue, bool hasExistingValue, JsonSerializer serializer) {
			dynamic o = JObject.Load(reader);

			return new Quaternion((float)o.x, (float)o.y, (float)o.z, (float)o.w);
		}
	}
}
