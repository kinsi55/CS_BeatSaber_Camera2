using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Camera2.Configuration {
	[JsonObject(MemberSerialization.OptIn)]
	class CameraBoundsConfig {
		static readonly IFormatProvider h = CultureInfo.InvariantCulture.NumberFormat;

		static void ParseInto(ref float min, ref float max, string val) {
			min = float.NegativeInfinity;
			max = float.PositiveInfinity;

			if(val == null || val.Length == 0)
				return;

			var spl = val.Split(':');

			if(!float.TryParse(spl[0], NumberStyles.Float, h, out min))
				max = float.PositiveInfinity;

			if(spl.Length == 1 || !float.TryParse(spl[1], NumberStyles.Float, h, out max))
				max = float.PositiveInfinity;
		}

		[JsonProperty] public string pos_x { get => string.Format(h, "{0}:{1}", pos_x_min, pos_x_max); set => ParseInto(ref pos_x_min, ref pos_x_max, value); }
		[JsonProperty] public string pos_y { get => string.Format(h, "{0}:{1}", pos_y_min, pos_y_max); set => ParseInto(ref pos_y_min, ref pos_y_max, value); }
		[JsonProperty] public string pos_z { get => string.Format(h, "{0}:{1}", pos_z_min, pos_z_max); set => ParseInto(ref pos_z_min, ref pos_z_max, value); }
		[JsonProperty] public string rot_x { get => string.Format(h, "{0}:{1}", rot_x_min, rot_x_max); set => ParseInto(ref rot_x_min, ref rot_x_max, value); }
		[JsonProperty] public string rot_y { get => string.Format(h, "{0}:{1}", rot_y_min, rot_y_max); set => ParseInto(ref rot_y_min, ref rot_y_max, value); }
		[JsonProperty] public string rot_z { get => string.Format(h, "{0}:{1}", rot_z_min, rot_z_max); set => ParseInto(ref rot_z_min, ref rot_z_max, value); }

		public float rot_x_min = float.NegativeInfinity;
		public float rot_x_max = float.PositiveInfinity;

		public float rot_y_min = float.NegativeInfinity;
		public float rot_y_max = float.PositiveInfinity;

		public float rot_z_min = float.NegativeInfinity;
		public float rot_z_max = float.PositiveInfinity;


		public float pos_x_min = float.NegativeInfinity;
		public float pos_x_max = float.PositiveInfinity;

		public float pos_y_min = float.NegativeInfinity;
		public float pos_y_max = float.PositiveInfinity;

		public float pos_z_min = float.NegativeInfinity;
		public float pos_z_max = float.PositiveInfinity;
	}
}
