using System;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch]
	static class HookRoomAdjust {
		public static MonoBehaviour instance { get; private set; }
		public static Vector3 position { get; private set; }
		public static Quaternion rotation { get; private set; }
		public static Vector3 eulerAngles { get; private set; }

		static void Postfix(VRCenterAdjust __instance, MethodBase __originalMethod) {
			if(__originalMethod.Name == "OnEnable")
				__instance.Start();

			var transform = __instance.transform;
			position = transform.position;
			rotation = transform.rotation;
			eulerAngles = transform.eulerAngles;

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

		static IEnumerable<MethodBase> TargetMethods() {
			yield return AccessTools.Method(typeof(VRCenterAdjust), nameof(VRCenterAdjust.OnEnable));
			yield return AccessTools.Method(typeof(VRCenterAdjust), nameof(VRCenterAdjust.Start));
			yield return AccessTools.Method(typeof(VRCenterAdjust), nameof(VRCenterAdjust.HandleRoomCenterDidChange));
			yield return AccessTools.Method(typeof(VRCenterAdjust), nameof(VRCenterAdjust.HandleRoomRotationDidChange));
			yield return AccessTools.Method(typeof(VRCenterAdjust), nameof(VRCenterAdjust.ResetRoom));
		}
	}
}
