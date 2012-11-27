using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Bricksoft.PowerCode
{
	public static class ConsoleUtils
	{
		public static void CenterWindow()
		{
			IntPtr hWin;
			RECT rc;
			Screen scr;
			int x;
			int y;

			// Center the window on the screen.
			hWin = GetConsoleWindow();

			GetWindowRect(hWin, out rc);
			scr = Screen.FromPoint(new Point(rc.left, rc.top));

			x = scr.WorkingArea.Left + (scr.WorkingArea.Width - (rc.right - rc.left)) / 2;
			y = scr.WorkingArea.Top + (scr.WorkingArea.Height - (rc.bottom - rc.top)) / 2;

			MoveWindow(hWin, x, y, rc.right - rc.left, rc.bottom - rc.top, false);
		}

		#region P/Invoke declarations

		private struct RECT { public int left, top, right, bottom; }

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr GetConsoleWindow();

		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool GetWindowRect( IntPtr hWnd, out RECT rc );

		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool MoveWindow( IntPtr hWnd, int x, int y, int w, int h, bool repaint );

		#endregion
	}
}
