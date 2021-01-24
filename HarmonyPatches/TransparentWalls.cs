using Camera2.Utils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
