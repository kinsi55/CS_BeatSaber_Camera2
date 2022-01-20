using Camera2.Configuration;
using Camera2.Interfaces;
using Camera2.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Camera2.Behaviours {
	class Settings_PostProcessing : CameraSubSettings {
		public float transparencyThreshold = 0f;

		private bool _forceDepthTexture = false;
		public bool forceDepthTexture {
			get => _forceDepthTexture;
			set {
				_forceDepthTexture = value;
				settings.cam.UpdateDepthTextureActive();
			}
		}

		public bool ShouldSerializechromaticAberrationAmount() => chromaticAberrationAmount != 0;
		public float chromaticAberrationAmount = 0f;
	}

	class CamPostProcessor : MonoBehaviour {
		private static readonly int Threshold = Shader.PropertyToID("_Threshold");
		private static readonly int HasDepth = Shader.PropertyToID("_HasDepth");
		private static readonly int MainTex = Shader.PropertyToID("_MainTex");
		private static readonly int Width = Shader.PropertyToID("_Width");
		private static readonly int ChromaticAberration = Shader.PropertyToID("_ChromaticAberration");

		protected Cam2 cam;
		protected CameraSettings settings => cam.settings;

		public void Init(Cam2 cam) {
			this.cam = cam;

			//cam.UCamera.GetComponent<MainEffectController>().afterImageEffectEvent += CamPostProcessor_afterImageEffectEvent;
		}

		//private void CamPostProcessor_afterImageEffectEvent(RenderTexture obj) {
		//	if(enabled && Plugin.ShaderMat_LuminanceKey) {
		//		Plugin.ShaderMat_LuminanceKey.SetFloat("_Threshold", settings.PostProcessing.transparencyThreshold);// settings.antiAliasing > 1 ? settings.PostProcessing.transparencyThreshold : 0);
		//		Plugin.ShaderMat_LuminanceKey.SetFloat("_HasDepth", cam.UCamera.depthTextureMode != DepthTextureMode.None ? 1 : 0);
		//		Graphics.Blit(obj, obj, Plugin.ShaderMat_LuminanceKey);
		//		if(settings.PostProcessing.chromaticAberrationAmount > 0) {
		//			Plugin.ShaderMat_CA.SetFloat("_ChromaticAberration", settings.PostProcessing.chromaticAberrationAmount / 1000);
		//			Graphics.Blit(obj, obj, Plugin.ShaderMat_CA);
		//		}
		//	}

		//	cam.PostprocessCompleted();
		//}

		void OnRenderImage(RenderTexture src, RenderTexture dest) {
			if(enabled && Plugin.ShaderMat_LuminanceKey) {
				Plugin.ShaderMat_LuminanceKey.SetFloat(Threshold, settings.PostProcessing.transparencyThreshold);
				Plugin.ShaderMat_LuminanceKey.SetFloat(HasDepth, cam.UCamera.depthTextureMode != DepthTextureMode.None ? 1 : 0);

				RenderTexture tmp = null;

				if(cam.isCurrentlySelectedInSettings) {
					tmp = RenderTexture.GetTemporary(dest.descriptor);
					Graphics.Blit(src, tmp, Plugin.ShaderMat_LuminanceKey);

					Plugin.ShaderMat_Outline.SetTexture(MainTex, dest);
					Plugin.ShaderMat_Outline.SetFloat(Width, settings.renderScale * 10);
					Graphics.Blit(tmp, dest, Plugin.ShaderMat_Outline);
					RenderTexture.ReleaseTemporary(tmp);
				} else {
					Graphics.Blit(src, dest, Plugin.ShaderMat_LuminanceKey);
				}

				if(settings.PostProcessing.chromaticAberrationAmount > 0) {
					Plugin.ShaderMat_CA.SetFloat(ChromaticAberration, settings.PostProcessing.chromaticAberrationAmount / 1000);
					Graphics.Blit(dest, dest, Plugin.ShaderMat_CA);
				}
			} else {
				Graphics.Blit(src, dest);
			}

			cam.PostprocessCompleted();
		}
	}
}
