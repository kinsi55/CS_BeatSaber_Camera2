using Camera2.Managers;
using HarmonyLib;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch(typeof(MainSystemInit), nameof(MainSystemInit.Init))]
	static class GlobalFPSCap {
		static void Postfix() => ApplyFPSCap();

		static bool isOculus = false;
		static bool isOculusUserPresent = false;

		public static void Init() {
			/*
			 * On VRMode Oculus, when you take off the headset the game ends up in an uncapped FPS state,
			 * this makes sure to apply an FPS cap when the headset is taken off
			 */
			if(!OVRPlugin.initialized)
				return;

			isOculus = true;

			Task.Run(delegate () {
				for(; ; ) {
					var newPresentState = OVRPlugin.userPresent;

					if(newPresentState != isOculusUserPresent) {
#if DEBUG
						Plugin.Log.Info(newPresentState ? "HMD mounted - Removing FPS cap" : "HMD unmounted - Applying FPS cap");
#endif
						ApplyFPSCap(newPresentState);

						isOculusUserPresent = newPresentState;
					}

					System.Threading.Thread.Sleep(isOculusUserPresent ? 2000 : 500);
				}
			});
		}

		public static void ApplyFPSCap() {
			ApplyFPSCap(UnityEngine.XR.XRDevice.isPresent || UnityEngine.XR.XRDevice.refreshRate != 0);
		}

		static void ApplyFPSCap(bool isHmdPresent) {
			QualitySettings.vSyncCount = 0;

			if(isHmdPresent && (!isOculus || isOculusUserPresent)) {
				Application.targetFrameRate = -1;
			} else {
				var Kapp = 30;

				if(CamManager.cams?.Count > 0) {
					QualitySettings.vSyncCount = 1;
					var srr = Screen.currentResolution.refreshRate;

					foreach(var cam in CamManager.cams.Values.Where(x => x.gameObject.activeInHierarchy)) {
						if(cam.settings.FPSLimiter.fpsLimit <= 0 || cam.settings.FPSLimiter.fpsLimit == srr) {
							if(Kapp < srr)
								Kapp = srr;
						} else if(Kapp < cam.settings.FPSLimiter.fpsLimit) {
							Kapp = cam.settings.FPSLimiter.fpsLimit;
							QualitySettings.vSyncCount = 0;
						}
					}
				}

				Application.targetFrameRate = Kapp;
			}
		}
	}
}
