using Camera2.Configuration;
using Camera2.Interfaces;
using Camera2.Managers;
using Camera2.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace Camera2.Behaviours {
	class Settings_Shader {
		public string assetBundlePath = "";
		public string shaderName = "";

		public readonly Dictionary<string, float> properties = new Dictionary<string, float>();
	}

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

		public Settings_Shader[] shaders = new Settings_Shader[0];
	}

	class CamPostProcessor : MonoBehaviour {
		private static readonly int Threshold = Shader.PropertyToID("_Threshold");
		private static readonly int HasDepth = Shader.PropertyToID("_HasDepth");
		private static readonly int Width = Shader.PropertyToID("_Width");

		protected Cam2 cam;
		protected CameraSettings settings => cam.settings;

		public void Init(Cam2 cam) {
			this.cam = cam;
		}

		void OnDisable() {
			
		}

		void OnRenderImage(RenderTexture _src, RenderTexture dest) {
			if(enabled && Plugin.ShaderMat_LuminanceKey) {
				Plugin.ShaderMat_LuminanceKey.SetFloat(Threshold, settings.PostProcessing.transparencyThreshold);
				Plugin.ShaderMat_LuminanceKey.SetFloat(HasDepth, cam.UCamera.depthTextureMode != DepthTextureMode.None ? 1 : 0);

				RenderTexture main = _src;

				void Apply(Material mat) {
					RenderTexture temp = RenderTexture.GetTemporary(main.descriptor);
					Graphics.Blit(main, temp, mat);
					if(main != _src)
						RenderTexture.ReleaseTemporary(main);

					main = temp;
				}

				foreach(var shader in settings.PostProcessing.shaders) {
					var loadedShader = ShaderManager.GetOrLoadShader(shader.assetBundlePath, shader.shaderName);
					var shaderMat = loadedShader.shaderMat;

					foreach(var prop in shader.properties) {
						if(!loadedShader.propIds.TryGetValue(prop.Key, out var propId))
							continue;

						shaderMat.SetFloat(propId, prop.Value);
					}

					Apply(shaderMat);
				}

				Apply(Plugin.ShaderMat_LuminanceKey);

				if(cam.isCurrentlySelectedInSettings)
					Apply(Plugin.ShaderMat_Outline);

				//if(settings.PostProcessing.chromaticAberrationAmount > 0) {
				//	Plugin.ShaderMat_CA.SetFloat(ChromaticAberration, settings.PostProcessing.chromaticAberrationAmount / 1000);
				//	Graphics.Blit(dest, dest, Plugin.ShaderMat_CA);
				//}

				Graphics.Blit(main, dest);
				if(main != _src)
					RenderTexture.ReleaseTemporary(main);
			} else {
				Graphics.Blit(_src, dest);
			}

			cam.PostprocessCompleted();
		}
	}

	//static class UnityMotionBlur {
	//	static readonly int SID_MainTex_TexelSize = Shader.PropertyToID("_MainTex_TexelSize");
	//	static readonly int SID_CameraMotionVectorsTexture_TexelSize = Shader.PropertyToID("_CameraMotionVectorsTexture_TexelSize");
	//	static readonly int SID_VelocityTex_TexelSize = Shader.PropertyToID("_VelocityTex_TexelSize");
	//	static readonly int SID_NeighborMaxTex_TexelSize = Shader.PropertyToID("_NeighborMaxTex_TexelSize");
	//	static readonly int SID_VelocityScale = Shader.PropertyToID("_VelocityScale");
	//	static readonly int SID_TileMaxLoop = Shader.PropertyToID("_TileMaxLoop");
	//	static readonly int SID_TileMaxOffs = Shader.PropertyToID("_TileMaxOffs");
	//	static readonly int SID_MaxBlurRadius = Shader.PropertyToID("_MaxBlurRadius");
	//	static readonly int SID_RcpMaxBlurRadius = Shader.PropertyToID("_RcpMaxBlurRadius");
	//	static readonly int SID_LoopCount = Shader.PropertyToID("_LoopCount");

	//	enum Pass : int {
	//		VelocitySetup,
	//		TileMax1,
	//		TileMax2,
	//		TileMaxV,
	//		NeighborMax,
	//		Reconstruction
	//	}

	//	static float settings_shutterAngle = 270f;
	//	static float settings_sampleCount = 10;

	//	public static void Apply(RenderTexture src, RenderTexture dest, Camera camera) {
	//		Plugin.ShaderMat_UnityMotionBlur.SetFloatArray(SID_MainTex_TexelSize, new [] { src.texelSize.x, src.texelSize.y });
	//		Plugin.ShaderMat_UnityMotionBlur.SetFloatArray(SID_CameraMotionVectorsTexture_TexelSize, new [] { camera.velocity.x, src.texelSize.y });
	//		Plugin.ShaderMat_UnityMotionBlur.SetFloatArray(SID_VelocityTex_TexelSize, new [] { camera.velocity.x, camera.velocity.y, camera.velocity.z });
	//		Plugin.ShaderMat_UnityMotionBlur.SetFloatArray(SID_NeighborMaxTex_TexelSize, new [] { src.texelSize.x, src.texelSize.y });


	//		const float kMaxBlurRadius = 5f;
	//		var vectorRTFormat = RenderTextureFormat.RGHalf;
	//		var packedRTFormat = RenderTextureFormat.ARGB2101010;

	//		// Calculate the maximum blur radius in pixels.
	//		int maxBlurPixels = (int)(kMaxBlurRadius * src.height / 100);

	//		// Calculate the TileMax size.
	//		// It should be a multiple of 8 and larger than maxBlur.
	//		int tileSize = ((maxBlurPixels - 1) / 8 + 1) * 8;

	//		// Pass 1 - Velocity/depth packing
	//		var velocityScale = settings_shutterAngle / 360f; //settings.shutterAngle
	//		Plugin.ShaderMat_UnityMotionBlur.SetFloat(SID_VelocityScale, velocityScale);
	//		Plugin.ShaderMat_UnityMotionBlur.SetFloat(SID_MaxBlurRadius, maxBlurPixels);
	//		Plugin.ShaderMat_UnityMotionBlur.SetFloat(SID_RcpMaxBlurRadius, 1f / maxBlurPixels);

	//		var vbuffer = RenderTexture.GetTemporary(src.width, src.height, 0, packedRTFormat);
	//		Graphics.Blit(vbuffer, vbuffer, Plugin.ShaderMat_UnityMotionBlur, (int)Pass.VelocitySetup);

	//		// Pass 2 - First TileMax filter (1/2 downsize)
	//		var tile2 = RenderTexture.GetTemporary(src.width / 2, src.height / 2, 0, vectorRTFormat);
	//		Graphics.Blit(vbuffer, tile2, Plugin.ShaderMat_UnityMotionBlur, (int)Pass.TileMax1);

	//		// Pass 3 - Second TileMax filter (1/2 downsize)
	//		var tile4 = RenderTexture.GetTemporary(src.width / 4, src.height / 4, 0, vectorRTFormat);
	//		Graphics.Blit(tile2, tile4, Plugin.ShaderMat_UnityMotionBlur, (int)Pass.TileMax2);
	//		RenderTexture.ReleaseTemporary(tile2);

	//		// Pass 4 - Third TileMax filter (1/2 downsize)
	//		var tile8 = RenderTexture.GetTemporary(src.width / 8, src.height / 8, 0, vectorRTFormat);
	//		Graphics.Blit(tile4, tile8, Plugin.ShaderMat_UnityMotionBlur, (int)Pass.TileMax2);
	//		RenderTexture.ReleaseTemporary(tile4);

	//		// Pass 5 - Fourth TileMax filter (reduce to tileSize)
	//		var tileMaxOffs = Vector2.one * (tileSize / 8f - 1f) * -0.5f;
	//		Plugin.ShaderMat_UnityMotionBlur.SetVector(SID_TileMaxOffs, tileMaxOffs);
	//		Plugin.ShaderMat_UnityMotionBlur.SetFloat(SID_TileMaxLoop, (int)(tileSize / 8f));

	//		var tile = RenderTexture.GetTemporary(src.width / tileSize, src.height / tileSize, 0, vectorRTFormat);
	//		Graphics.Blit(tile8, tile, Plugin.ShaderMat_UnityMotionBlur, (int)Pass.TileMaxV);
	//		RenderTexture.ReleaseTemporary(tile8);

	//		// Pass 6 - NeighborMax filter
	//		int neighborMaxWidth = src.width / tileSize;
	//		int neighborMaxHeight = src.height / tileSize;
	//		var neighborMax = RenderTexture.GetTemporary(neighborMaxWidth, neighborMaxHeight, 0, vectorRTFormat);
	//		Graphics.Blit(tile, neighborMax, Plugin.ShaderMat_UnityMotionBlur, (int)Pass.NeighborMax);
	//		RenderTexture.ReleaseTemporary(tile);

	//		// Pass 7 - Reconstruction pass
	//		Plugin.ShaderMat_UnityMotionBlur.SetFloat(SID_LoopCount, Mathf.Clamp(settings_sampleCount / 2, 1, 64));
	//		Graphics.Blit(src, dest, Plugin.ShaderMat_UnityMotionBlur, (int)Pass.Reconstruction);

	//		RenderTexture.ReleaseTemporary(vbuffer);
	//		RenderTexture.ReleaseTemporary(neighborMax);
	//	}
	//}
}
