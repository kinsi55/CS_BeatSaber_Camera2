using System;
using System.Runtime.InteropServices;

namespace Camera2.Utils {
	class WinAPI {
		public enum WindowsCursor {
			IDC_APPSTARTING = 32650,
			IDC_ARROW = 32512,
			IDC_CROSS = 32515,
			IDC_HAND = 32649,
			IDC_HELP = 32651,
			IDC_IBEAM = 32513,
			IDC_NO = 32648,
			IDC_SIZEALL = 32646,
			IDC_SIZENESW = 32643,
			IDC_SIZENS = 32645,
			IDC_SIZENWSE = 32642,
			IDC_SIZEWE = 32644,
			IDC_UPARROW = 32516,
			IDC_WAIT = 32514
		}

		public static void SetCursor(WindowsCursor cursor) {
			SetCursor(LoadCursor(IntPtr.Zero, (int)cursor));
		}

		[DllImport("user32.dll", EntryPoint = "SetCursor")]
		private static extern IntPtr SetCursor(IntPtr hCursor);

		[DllImport("user32.dll", EntryPoint = "LoadCursor")]
		private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);



		[DllImport("user32.dll")]
		public static extern int MessageBox(IntPtr hWnd, String text, String caption, int options);
	}
}
