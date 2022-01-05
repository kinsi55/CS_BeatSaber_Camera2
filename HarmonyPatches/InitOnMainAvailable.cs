using Camera2.Managers;
using HarmonyLib;
using UnityEngine;
using System.Linq;

namespace Camera2.HarmonyPatches {
	[HarmonyPatch(typeof(FirstPersonFlyingController), nameof(FirstPersonFlyingController.Awake))]
	static class InitOnMainAvailable {
		static bool isInited = false;
		public static bool useDepthTexture { get; private set; }
		static void Postfix(Camera ____camera) {
			useDepthTexture = ____camera.depthTextureMode != DepthTextureMode.None;

			if(!isInited) {
				if(CamManager.baseCullingMask == 0)
					CamManager.baseCullingMask = Camera.main.cullingMask;

				isInited = true;

				Plugin.Log.Notice("MainCamera is ready, Initializing...");

				CamManager.Init();
			} else {
				foreach(var cam in CamManager.cams.Values)
					cam.UpdateDepthTextureActive();
			}

			if(!HookFPFCToggle.foundSiraToggle)
				HookFPFCToggle.SetFPFCActive(____camera.transform, ____camera.isActiveAndEnabled);
		}
	}
}
