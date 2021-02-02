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
	[HarmonyPatch(typeof(MainSystemInit), "Init")]
	class GlobalFPSCap {
		public static void Postfix() {
			if(UnityEngine.XR.XRDevice.isPresent) {
				Application.targetFrameRate = -1;
				return;
			}

			var Kapp = Math.Max(Screen.currentResolution.refreshRate, 120);

			if(CamManager.cams?.Count > 0)
				Kapp = CamManager.cams.Values.Where(x => x.gameObject.activeInHierarchy).Max(x => x.settings.FPSLimiter.limit);
			
			Application.targetFrameRate = Kapp;
		}
	}
}
