using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;
using Camera2.Behaviours;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch(typeof(SmoothCameraController), "Start")]
	class InitOnMainAvailable {
		static bool isInited = false;
		static void Prefix() {
			if(isInited) return;
			isInited = true;
			CamManager.Init();
		}

	}
}
