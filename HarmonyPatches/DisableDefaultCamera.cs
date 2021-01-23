using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch(typeof(SmoothCameraController), "ActivateSmoothCameraIfNeeded")]
	class DisableSmoothCamera {
		static bool Prefix() {
			return false;
		}
	}

	[HarmonyPatch(typeof(SmoothCamera), "OnEnable")]
	class BlockSmoothCamera {
		static bool Prefix(MainCamera ____mainCamera) {
			return false;
		}
	}
}
