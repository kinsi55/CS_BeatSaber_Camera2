// Exercise the production sender's lifecycle with stand-ins for Unity and the
// native plugin. Actual GPU sharing, colour and orientation require the game.
using System;
using Camera2.Spout;
using UnityEngine;
using UnityEngine.Rendering;

static class SpoutSenderSmoke {
	static void Check(bool condition, string message) {
		if(!condition) throw new Exception(message);
	}

	static void Main() {
		// Scene activation is independent of Spout/hiding. Of three hidden Spout
		// cameras, only those included in the scene render after leaving settings.
		var sceneMembership = new[] { true, false, false };
		var activeCameras = 0;
		foreach(var inScene in sceneMembership)
			if(CameraOutput.ShouldRender(inScene, false)) activeCameras++;
		Check(activeCameras == 1, "Two excluded hidden Spout cameras must not keep rendering");
		Check(!CameraOutput.ShouldRender(false, false), "Excluded cameras must remain inactive");
		Check(CameraOutput.ShouldRender(false, true), "Settings selection still permits editing excluded cameras");
		Check(CameraOutput.ShouldShowDesktop(true, true, false, false), "Scene camera retains desktop preview");
		Check(!CameraOutput.ShouldShowDesktop(true, true, true, true), "Hide desktop must apply even when selected");
		Check(CameraOutput.ShouldRender(true, false), "Scene-enabled cameras must render");
		Check(!CameraOutput.ShouldShowDesktop(true, true, false, true), "Hidden scene-enabled camera must not appear on desktop");

		CameraOutput.GetRenderSize(true, SpoutResolution.HD1080, 689, 387, 1.5f, out var rw, out var rh);
		Check(rw == 2880 && rh == 1620, "Fixed 1080p with 1.5 render scale must supersample internally");
		CameraOutput.GetOutputSize(SpoutResolution.HD1080, rw, rh, out var ow, out var oh);
		Check(ow == 1920 && oh == 1080, "Render scale must not change fixed Spout output size");
		CameraOutput.GetRenderSize(false, SpoutResolution.HD1080, 640, 360, 1.5f, out rw, out rh);
		Check(rw == 960 && rh == 540, "Spout off must retain normal desktop render sizing");
		CameraOutput.GetRenderSize(true, SpoutResolution.MatchCamera, 640, 360, 1.5f, out rw, out rh);
		CameraOutput.GetOutputSize(SpoutResolution.MatchCamera, rw, rh, out ow, out oh);
		Check(ow == 960 && oh == 540, "Match camera must preserve existing render-scale behaviour");
		CameraOutput.GetRenderSize(true, SpoutResolution.QHD1440, 0, 0, .5f, out rw, out rh);
		Check(rw == 1280 && rh == 720, "Fixed output must ignore desktop rectangle size");
		CameraOutput.GetRenderSize(false, SpoutResolution.MatchCamera, 0, 0, 1, out rw, out rh);
		Check(rw == 1 && rh == 1, "Minimized/zero viewport must not allocate a zero-size texture");

		var source = new RenderTexture(1920, 1080);
		var previous = new RenderTexture(1, 1);
		RenderTexture.active = previous;
		var sender = new SpoutSender();
		sender.Send(source, "Camera2 - FP");
		sender.Send(source, "Camera2 - FP");
		Check(SpoutPlugin.Created == 1 && Graphics.Copies == 2, "Steady frames must reuse the sender");
		Check(RenderTexture.active == previous && RenderTexture.Outstanding == 0, "Restore active target and return temporary textures");

		sender.Send(source, "Camera2 - Renamed");
		Check(SpoutPlugin.Created == 2 && SpoutPlugin.Disposed == 1, "Rename must replace the sender");
		sender.Send(new RenderTexture(2560, 1440), "Camera2 - Renamed");
		Check(SpoutPlugin.Created == 3 && SpoutPlugin.Disposed == 2, "Resize must replace the sender");
		sender.Dispose(); sender.Dispose();
		Check(SpoutPlugin.Disposed == 3 && UnityEngine.Object.Destroyed == 3, "Disposal must be complete and idempotent");

		SpoutPlugin.Ready = false;
		var pending = new SpoutSender();
		var copies = Graphics.Copies;
		pending.Send(source, "Camera2 - Pending");
		Check(Graphics.Copies == copies && RenderTexture.Outstanding == 0, "No copying before render-thread activation");
		SpoutPlugin.Ready = true;
		pending.Send(source, "Camera2 - Pending");
		Check(Graphics.Copies == copies + 1, "Sender should recover after deferred activation");
		pending.Dispose();

		var failing = new SpoutSender();
		Graphics.ThrowOnCopy = true;
		failing.Send(source, "Camera2 - Failing");
		Check(RenderTexture.active == previous && RenderTexture.Outstanding == 0, "Copy failure must still return the temporary texture");
		var created = SpoutPlugin.Created;
		Graphics.ThrowOnCopy = false;
		failing.Send(source, "Camera2 - Failing");
		Check(SpoutPlugin.Created == created, "Do not repeatedly retry a failed sender");
		failing.Dispose();

		var unsupported = new SpoutSender();
		SystemInfo.graphicsDeviceType = GraphicsDeviceType.Vulkan;
		unsupported.Send(source, "Camera2 - Unsupported");
		unsupported.Send(source, "Camera2 - Unsupported");
		Check(SpoutPlugin.Created == created, "Unsupported backend must not create native resources");
		SystemInfo.graphicsDeviceType = GraphicsDeviceType.Direct3D11;

		var retry = new SpoutSender();
		retry.Send(source, "Camera2 - Failing");
		Check(SpoutPlugin.Created == created + 1, "A new sender must allow retry after toggling");
		SpoutPlugin.ThrowOnDispose = true;
		retry.Dispose(); retry.Dispose();
		Check(RenderTexture.Outstanding == 0, "Native disposal failure must not escape or retain temporary textures");
		SpoutPlugin.ThrowOnDispose = false;

		var fixedOutput = new SpoutSender();
		created = SpoutPlugin.Created;
		fixedOutput.Send(new RenderTexture(2880, 1620), "Camera2 - Fixed", 1920, 1080);
		Check(SpoutPlugin.Width == 1920 && SpoutPlugin.Height == 1080, "Sender dimensions must be the selected output dimensions");
		fixedOutput.Send(new RenderTexture(960, 540), "Camera2 - Fixed", 1920, 1080);
		Check(SpoutPlugin.Created == created + 1, "Changing internal render scale must not recreate fixed-size sender");
		Check(Graphics.LastCopyWidth == 1920 && Graphics.LastCopyHeight == 1080, "Conversion target must use output pixels");
		fixedOutput.Send(source, "Camera2 - Fixed", 2560, 1440);
		Check(SpoutPlugin.Created == created + 2 && SpoutPlugin.Width == 2560, "Changing output resolution must recreate sender");
		fixedOutput.Dispose();
		Console.WriteLine("PASS: FPFC scene exclusion, desktop hiding, output/internal resolutions, steady output, rename, resize, disposal, deferred activation, failure cleanup, unsupported backend, retry");
	}
}

namespace Camera2 {
	static class Plugin { internal static readonly TestLog Log = new TestLog(); }
	sealed class TestLog { public void Info(string _) {} public void Error(object _) {} }
}
namespace Camera2.Spout {
	static class SpoutPlugin {
		internal static bool Loaded = true, Ready = true, ThrowOnDispose;
		internal static int Created, Disposed;
		static int width, height;
		internal static int Width => width;
		internal static int Height => height;
		internal static IntPtr CreateSender(string _, int w, int h) {
			width = w; height = h; return new IntPtr(++Created);
		}
		internal static IntPtr GetTexturePointer(IntPtr _) => Ready ? new IntPtr(1) : IntPtr.Zero;
		internal static int GetTextureWidth(IntPtr _) => width;
		internal static int GetTextureHeight(IntPtr _) => height;
		internal static void IssueEvent(IntPtr _, bool dispose = false) {
			if(dispose) {
				Disposed++;
				if(ThrowOnDispose) throw new InvalidOperationException("Injected native failure");
			}
		}
	}
}
namespace UnityEngine.Rendering { enum GraphicsDeviceType { Direct3D11, Vulkan } }
namespace UnityEngine {
	class Object { internal static int Destroyed; public static void Destroy(Object _) { Destroyed++; } }
	class Texture : Object { public int width, height; }
	enum HideFlags { DontSave }
	enum TextureFormat { ARGB32 }
	enum RenderTextureFormat { ARGB32 }
	enum RenderTextureReadWrite { Default }
	class Texture2D : Texture {
		public HideFlags hideFlags;
		public static Texture2D CreateExternalTexture(int w, int h, TextureFormat f, bool m, bool l, IntPtr p) => new Texture2D { width = w, height = h };
	}
	class RenderTexture : Texture {
		internal static int Outstanding;
		public static RenderTexture active;
		public RenderTexture(int w, int h) { width = w; height = h; }
		public static RenderTexture GetTemporary(int w, int h, int d, RenderTextureFormat f, RenderTextureReadWrite r, int a) {
			Outstanding++; return new RenderTexture(w, h);
		}
		public static void ReleaseTemporary(RenderTexture _) { Outstanding--; }
	}
	struct Vector2 { public Vector2(float x, float y) {} }
	static class SystemInfo { public static GraphicsDeviceType graphicsDeviceType = GraphicsDeviceType.Direct3D11; }
	static class Graphics {
		internal static int Copies;
		internal static int LastCopyWidth, LastCopyHeight;
		internal static bool ThrowOnCopy;
		public static void Blit(Texture s, RenderTexture d, Vector2 scale, Vector2 offset) { RenderTexture.active = d; }
		public static void CopyTexture(Texture s, Texture d) {
			if(ThrowOnCopy) throw new InvalidOperationException("Injected copy failure");
			LastCopyWidth = s.width; LastCopyHeight = s.height;
			Copies++;
		}
	}
}
