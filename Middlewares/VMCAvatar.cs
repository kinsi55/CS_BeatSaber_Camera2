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

				var parsedAddress = IPAddress.Parse(stuff[0]);
				var b = parsedAddress.GetAddressBytes();

				if(!IPAddress.IsLoopback(parsedAddress) && b[0] != 10 && (b[0] != 192 || b[1] != 168) && (b[0] != 172 || (b[1] < 16 || b[1] > 31))) {
					Plugin.Log.Warn($"Tried to set public IP address ({value}) for camera {settings.cam.name} as the VMC destination. As this is almost certainly not intended it was prevented");
					return;
				}

				address.Address = parsedAddress;
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
