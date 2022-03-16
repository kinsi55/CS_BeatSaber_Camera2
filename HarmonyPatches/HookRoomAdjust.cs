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

		static void Postfix(Vector3SO ____roomCenter, FloatSO ____roomRotation, MethodBase __originalMethod) {
			if(____roomCenter == null) {
				position = Vector3.zero;
			} else {
				position = ____roomCenter;
			}

			if(____roomRotation == null) {
				eulerAngles = Vector3.zero;
			} else {
				eulerAngles = new Vector3(0, ____roomRotation, 0);
			}
			rotation = Quaternion.Euler(eulerAngles);

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
