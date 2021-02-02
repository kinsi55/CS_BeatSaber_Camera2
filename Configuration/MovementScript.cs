using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Camera2.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.ComponentModel;

namespace Camera2.Configuration {
	class MovementScript {
		//public enum PositionType {
		//	Absolute,
		//	Relative
		//}
		public enum MoveType {
			Linear,
			Eased
		}

		public class Frame {
			//[JsonConverter(typeof(StringEnumConverter))]
			//public PositionType posType = PositionType.Absolute;
			[JsonConverter(typeof(StringEnumConverter)), DefaultValue(MoveType.Linear)]
			public MoveType transition = MoveType.Linear;
			[JsonConverter(typeof(Vector3Converter))]
			public Vector3 position = Vector3.zero;

			[JsonIgnore]
			public Quaternion rotation = Quaternion.identity;
			[JsonConverter(typeof(Vector3Converter)), JsonProperty("rotation")]
			public Vector3 rotationEuler {
				get { return rotation.eulerAngles; }
				set { rotation = Quaternion.Euler(value); }
			}


			[DefaultValue(0f)]
			public float FOV = 0f;
			public float duration = 0f;
			public float holdTime = 0f;

			[JsonIgnore]
			public float startTime = 0f;
			[JsonIgnore]
			public float transitionEndTime = 0f;
			[JsonIgnore]
			public float endTime = 0f;
		}

		[JsonProperty("syncToSong")]
		public bool syncToSong { get; private set; } = false;

		[JsonProperty("loop")]
		public bool loop { get; private set; } = true;

		public List<Frame> frames { get; private set; } = new List<Frame>();

		[JsonIgnore]
		public float scriptDuration { get; private set; } = 0f;

		private void PopulateTimes() {
			var time = 0f;
			foreach(var frame in frames) {
				frame.startTime = time;
				time = frame.transitionEndTime = 
					time + frame.duration;

				time = frame.endTime = 
					time + frame.holdTime;
			}
			scriptDuration = time;
		}

		public static MovementScript Load(string name) {
			var scriptPath = ConfigUtil.GetMovementScriptPath(name);
			if(!File.Exists(scriptPath))
				return null;

			var script = new MovementScript();

			var scriptContent = File.ReadAllText(scriptPath);
			// Not a Noodle movement script
			if(!scriptContent.Contains("Movements")) {
				JsonConvert.PopulateObject(scriptContent, script, JsonHelpers.leanDeserializeSettings);
			} else {
				// Camera Plus movement script, we need to convert it...
				dynamic camPlusScript = JObject.Parse(scriptContent.ToLower());

				script.syncToSong = camPlusScript.activeinpausemenu != "true";

				foreach(dynamic movement in camPlusScript.movements) {
					script.frames.Add(new Frame() {
						position = new Vector3((float)movement.startpos.x, (float)movement.startpos.y, (float)movement.startpos.z),
						rotationEuler = new Vector3((float)movement.startrot.x, (float)movement.startrot.y, (float)movement.startrot.z),
						FOV = (float)(movement.startpos.fov ?? 0f)
					});

					script.frames.Add(new Frame() {
						position = new Vector3((float)movement.endpos.x, (float)movement.endpos.y, (float)movement.endpos.z),
						rotationEuler = new Vector3((float)movement.endrot.x, (float)movement.endrot.y, (float)movement.endrot.z),
						duration = movement.duration,
						holdTime = movement.delay,
						FOV = (float)(movement.endpos.fov ?? 0f),
						transition = movement.easetransition == "true" ? MoveType.Eased : MoveType.Linear
					});
				}

				File.Move(scriptPath, $"{scriptPath}.cameraPlusFormat");
				File.WriteAllText(scriptPath, JsonConvert.SerializeObject(script, Formatting.Indented, new JsonSerializerSettings() {
					DefaultValueHandling = DefaultValueHandling.Ignore
				}));
			}

			//if(frames[0].posType == PositionType.Relative) {
			//	Plugin.Log.Warn("The first frame in a Movement script cannot have a relative position, not loaded");
			//	return null;
			//}

			script.PopulateTimes();

			return script;
		}
	}
}
