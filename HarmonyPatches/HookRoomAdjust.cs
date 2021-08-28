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
	static class HookRoomAdjust {
		public static MonoBehaviour instance { get; private set; }
		public static Vector3 position { get; private set; }
		public static Quaternion rotation { get; private set; }
		public static Vector3 eulerAngles { get; private set; }

		static void Postfix(MonoBehaviour __instance, MethodBase __originalMethod) {
			position = __instance.transform.position;
			rotation = __instance.transform.rotation;
			eulerAngles = __instance.transform.eulerAngles;

#if DEBUG
			Plugin.Log.Warn("HookRoomAdjust.Postfix! " + __originalMethod.Name);
			Console.WriteLine("pos {0}, rot {1}", position, rotation);
#endif
		}

		public static void ApplyCustom(Vector3 position, Quaternion rotation) {
			HookRoomAdjust.position = position;
			HookRoomAdjust.rotation = rotation;
			HookRoomAdjust.eulerAngles = rotation.eulerAngles;

#if DEBUG
			Plugin.Log.Warn("HookRoomAdjust.ApplyCustom!");
			Console.WriteLine("pos {0}, rot {1}", position, rotation);
#endif
		}

		[HarmonyTargetMethods]
		static IEnumerable<MethodBase> TargetMethods() {
			yield return AccessTools.Method(typeof(VRCenterAdjust), nameof(VRCenterAdjust.OnEnable));
			yield return AccessTools.Method(typeof(VRCenterAdjust), nameof(VRCenterAdjust.Start));
			yield return AccessTools.Method(typeof(VRCenterAdjust), nameof(VRCenterAdjust.HandleRoomCenterDidChange));
			yield return AccessTools.Method(typeof(VRCenterAdjust), nameof(VRCenterAdjust.HandleRoomRotationDidChange));
			yield return AccessTools.Method(typeof(VRCenterAdjust), nameof(VRCenterAdjust.ResetRoom));
		}
	}
}
