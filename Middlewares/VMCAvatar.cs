using Camera2.Interfaces;
using Camera2.VMC;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Net;
using UnityEngine;

namespace Camera2.Configuration {
	enum VMCMode {
		Disabled,
		Sender,
		//Receiver
	}

	class Settings_VMCAvatar : CameraSubSettings {
		[JsonConverter(typeof(StringEnumConverter))]
		public VMCMode mode = VMCMode.Disabled;

		public string destination {
			get => address.ToString();
			set {
				string[] stuff = value.Split(':');

				address.Address = IPAddress.Parse(stuff[0]);
				address.Port = stuff.Length == 2 ? ushort.Parse(stuff[1]) : 39540;
			}
		}

		[JsonIgnore]
		public IPEndPoint address = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 39540);
	}
}

namespace Camera2.Middlewares {
	class VMCAvatar : CamMiddleware, IMHandler {
		static OscClient sender;

		float prevFov;
		Vector3 prevPos;
		Quaternion prevRot;

		new public void Post() {
			if(cam.settings.VMCProtocol.mode == Configuration.VMCMode.Disabled)
				return;

			if(prevFov == cam.settings.FOV && prevPos == cam.transformchain.position && prevRot == cam.transformchain.rotation )
				return;

			try {
				sender ??= new OscClient(cam.settings.VMCProtocol.address);

				sender.SendCamPos(cam);
			} catch { } finally {
				prevFov = cam.settings.FOV;
				prevPos = cam.transformchain.position;
				prevRot = cam.transformchain.rotation;
			}
		}
	}
}
