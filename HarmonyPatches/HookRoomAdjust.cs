using Camera2.Managers;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch]
	class HookRoomAdjust {
		public static MonoBehaviour instance { get; private set; }

		static void Postfix(MonoBehaviour __instance) {
			instance = __instance;

			foreach(var cam in CamManager.cams.Values) {
				if(cam.transform.parent != null)
					continue;

				cam.transform.position = instance.transform.position;
				cam.transform.rotation = instance.transform.rotation;
			}
		}

		[HarmonyTargetMethods]
		static IEnumerable<MethodBase> TargetMethods() {
			yield return AccessTools.Method(typeof(VRCenterAdjust), nameof(VRCenterAdjust.Start));
			yield return AccessTools.Method(typeof(VRCenterAdjust), nameof(VRCenterAdjust.HandleRoomCenterDidChange));
		}
	}
}
