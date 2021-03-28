using Camera2.Managers;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch(typeof(MainSystemInit), nameof(MainSystemInit.Init))]
	class GlobalFPSCap {
		public static void Postfix() {
			ApplyFPSCap(UnityEngine.XR.XRDevice.isPresent || UnityEngine.XR.XRDevice.refreshRate != 0);
		}

		public static void Init() {
			/*
			 * On VRMode Oculus, when you take off the headset the game ends up in an uncapped FPS state,
			 * this makes sure to apply an FPS cap when the headset is taken off
			 */
			if(!OVRPlugin.initialized)
				return;

			Task.Run(delegate () {
				bool lastPresentState = false;

				for(; ; ) {
					var newPresentState = OVRPlugin.userPresent;

					if(newPresentState != lastPresentState) {
#if DEBUG
						Plugin.Log.Info(newPresentState ? "HMD mounted - Removing FPS cap" : "HMD unmounted - Applying FPS cap");
#endif
						ApplyFPSCap(newPresentState);

						lastPresentState = newPresentState;
					}

					System.Threading.Thread.Sleep(lastPresentState ? 2000 : 500);
				}
			});
		}

		public static void ApplyFPSCap(bool isHmdPresent) {
			if(isHmdPresent) {
				Application.targetFrameRate = -1;
			} else {
				var Kapp = 30;

				if(CamManager.cams?.Count > 0) {
					foreach(var cam in CamManager.cams.Values.Where(x => x.gameObject.activeInHierarchy)) {
						if(cam.settings.FPSLimiter.fpsLimit <= 0) {
							Kapp = Screen.currentResolution.refreshRate;
							break;
						} else if(Kapp < cam.settings.FPSLimiter.fpsLimit) {
							Kapp = cam.settings.FPSLimiter.fpsLimit;
						}
					}
				}

				Application.targetFrameRate = Kapp;
			}
		}
	}
}
