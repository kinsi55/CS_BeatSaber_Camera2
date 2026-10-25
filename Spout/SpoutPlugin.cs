using IPA.Utilities;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace Camera2.Spout {
	// Native ABI from KlakSpout e1e492fb147614a166941be1d65c68a717127f3d.
	// The private filename prevents overwriting another mod's KlakSpout plugin.
	internal static class SpoutPlugin {
		private const string Library = "Camera2Spout";
		internal static bool Loaded { get; private set; }

		[DllImport("kernel32", EntryPoint = "LoadLibraryW", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern IntPtr LoadLibrary(string path);

		internal static void Load() {
			if(Loaded || Environment.OSVersion.Platform != PlatformID.Win32NT || IntPtr.Size != 8)
				return;

			try {
				var directory = Path.Combine(UnityGame.InstallPath, "Beat Saber_Data", "Plugins", "x86_64");
				Directory.CreateDirectory(directory);
				var path = Path.Combine(directory, Library + ".dll");
				using(var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("Camera2.Spout.Native.Camera2Spout.dll"))
				using(var buffer = new MemoryStream()) {
					resource.CopyTo(buffer);
					var bytes = buffer.ToArray();
					if(!File.Exists(path) || !File.ReadAllBytes(path).SequenceEqual(bytes))
						File.WriteAllBytes(path, bytes);
				}

				if(LoadLibrary(path) == IntPtr.Zero)
					throw new Win32Exception(Marshal.GetLastWin32Error());

				Loaded = true;
				Plugin.Log.Info("Camera2 Spout2 native plugin loaded.");
			} catch(Exception ex) {
				Plugin.Log.Error("Could not load Spout2. Camera2 will continue without Spout output.");
				Plugin.Log.Error(ex);
			}
		}

		internal static void IssueEvent(IntPtr sender, bool dispose = false) {
			// Disposal must run on Unity's render thread, after queued texture copies.
			using(var commands = new CommandBuffer()) {
				commands.IssuePluginEventAndData(GetRenderEventFunc(), dispose ? 1 : 0, sender);
				Graphics.ExecuteCommandBuffer(commands);
			}
		}

		[DllImport(Library)]
		private static extern IntPtr GetRenderEventFunc();
		[DllImport(Library, CharSet = CharSet.Ansi)]
		internal static extern IntPtr CreateSender(string name, int width, int height);
		[DllImport(Library)]
		internal static extern IntPtr GetTexturePointer(IntPtr sender);
		[DllImport(Library)]
		internal static extern int GetTextureWidth(IntPtr sender);
		[DllImport(Library)]
		internal static extern int GetTextureHeight(IntPtr sender);
	}
}
