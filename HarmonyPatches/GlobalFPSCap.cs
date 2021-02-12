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
			if(UnityEngine.XR.XRDevice.isPresent || UnityEngine.XR.XRDevice.refreshRate != 0) {
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
