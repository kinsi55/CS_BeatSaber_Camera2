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

		[HarmonyPatch(typeof(VRCenterAdjust), nameof(VRCenterAdjust.OnEnable))]
		[HarmonyPatch(typeof(VRCenterAdjust), nameof(VRCenterAdjust.Start))]
		[HarmonyPatch(typeof(VRCenterAdjust), nameof(VRCenterAdjust.SetRoomTransformOffset))]
		[HarmonyPatch(typeof(VRCenterAdjust), nameof(VRCenterAdjust.ResetRoom))]
		static void Postfix(SettingsManager ____settingsManager, MethodBase __originalMethod) {
			if(____settingsManager != null) {
				position = ____settingsManager.settings.room.center;
				eulerAngles = new Vector3(0, ____settingsManager.settings.room.rotation, 0);
			} else {
				position = Vector3.zero;
				eulerAngles = Vector3.zero;
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
	}
}
