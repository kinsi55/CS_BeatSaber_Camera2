using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Camera2.Spout {
	internal sealed class SpoutSender : IDisposable {
		private IntPtr sender;
		private Texture2D sharedTexture;
		private string channel;
		private int width, height;
		private bool failed;

		internal void Send(RenderTexture source, string name, int outputWidth = 0, int outputHeight = 0) {
			if(failed || source == null || source.width < 1 || source.height < 1)
				return;

			try {
				if(!SpoutPlugin.Loaded || SystemInfo.graphicsDeviceType != GraphicsDeviceType.Direct3D11)
					throw new NotSupportedException("Spout2 requires the Windows x64 Direct3D11 graphics backend (including D3D11 through Proton).");

				var targetWidth = outputWidth > 0 ? outputWidth : source.width;
				var targetHeight = outputHeight > 0 ? outputHeight : source.height;
				if(sender != IntPtr.Zero && (channel != name || width != targetWidth || height != targetHeight))
					Dispose();

				if(sender == IntPtr.Zero) {
					sender = SpoutPlugin.CreateSender(name, targetWidth, targetHeight);
					if(sender == IntPtr.Zero)
						throw new InvalidOperationException("The native Spout plugin is not initialized.");
					channel = name;
					width = targetWidth;
					height = targetHeight;
				}

				SpoutPlugin.IssueEvent(sender);
				if(sharedTexture == null) {
					var pointer = SpoutPlugin.GetTexturePointer(sender);
					// Activation happens asynchronously on the render thread.
					if(pointer == IntPtr.Zero)
						return;

					sharedTexture = Texture2D.CreateExternalTexture(
						SpoutPlugin.GetTextureWidth(sender), SpoutPlugin.GetTextureHeight(sender),
						TextureFormat.ARGB32, false, false, pointer);
					sharedTexture.hideFlags = HideFlags.DontSave;
					Plugin.Log.Info($"Spout2 output: {channel} ({width}x{height})");
				}

				// External Texture2D objects cannot be blit destinations. Use one pooled,
				// depthless conversion target, then copy to the shared GPU texture.
				// Keep alpha from Camera2's transparency effect; Spout uses inverted Y.
				var previous = RenderTexture.active;
				var temporary = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1);
				try {
					Graphics.Blit(source, temporary, new Vector2(1, -1), new Vector2(0, 1));
					Graphics.CopyTexture(temporary, sharedTexture);
				} finally {
					RenderTexture.active = previous;
					RenderTexture.ReleaseTemporary(temporary);
				}
			} catch(Exception ex) {
				failed = true;
				Plugin.Log.Error($"Spout2 output failed for '{name}'. Toggle Spout2 off and on to retry.");
				Plugin.Log.Error(ex);
				Dispose();
			}
		}

		public void Dispose() {
			var oldSender = sender;
			sender = IntPtr.Zero;
			try {
				if(oldSender != IntPtr.Zero)
					SpoutPlugin.IssueEvent(oldSender, dispose: true);
			} catch(Exception ex) {
				// A native plugin failure must not interrupt the camera's normal rendering.
				Plugin.Log.Error("Could not dispose Spout2 sender.");
				Plugin.Log.Error(ex);
			} finally {
				if(sharedTexture != null) {
					UnityEngine.Object.Destroy(sharedTexture);
					sharedTexture = null;
				}
			}
		}
	}
}
