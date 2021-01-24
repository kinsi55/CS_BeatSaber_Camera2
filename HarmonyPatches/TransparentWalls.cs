using UnityEngine;
using HarmonyLib;
using Camera2.Utils;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch(typeof(StretchableObstacle), "SetSizeAndColor")]
	public class TransparentWalls {
		static void Postfix(Transform ____obstacleCore) {
			____obstacleCore.gameObject.layer = (int)VisibilityLayers.Walls;
		}

		public static void MakeWallsOpaqueForMainCam() {
			Camera.main.cullingMask |= (int)VisibilityMasks.Walls;
		}
	}
}
