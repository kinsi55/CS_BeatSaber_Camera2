using Camera2.HarmonyPatches;
using Camera2.Interfaces;
using Camera2.Utils;
using UnityEngine;

namespace Camera2.Configuration {
	class Settings_Multiplayer : CameraSubSettings {
		public bool followSpectatorPlattform = true;
	}
}

namespace Camera2.Middlewares {
	class Multiplayer : CamMiddleware, IMHandler {
		Transformer originLmao = null;

		new public bool Pre() {
			/*
			 * TODO: This should *eventually* (™) allow to set the origin of this camera to another player
			 * which is not us, which would allow to have third and firstperson cameras which work in the
			 * context of another player
			 */
			var x = HookMultiplayerSpectatorController.instance;

			if(!SceneUtil.isInMultiplayer || !SceneUtil.isInSong || !x || !settings.Multiplayer.followSpectatorPlattform) {
				if(originLmao != null) {
					originLmao.position = Vector3.zero;
					originLmao.rotation = Quaternion.identity;
				}

				return true;
			}

			if(originLmao == null) {
				originLmao = settings.cam.transformchain.AddOrGet("MultiplayerOrigin", TransformerOrders.PlayerOrigin);
			}

			if(x.currentSpot != null) {
				originLmao.position = x.currentSpot.transform.position;
				originLmao.rotation = x.currentSpot.transform.rotation;
			}

			return true;
		}
	}
}
