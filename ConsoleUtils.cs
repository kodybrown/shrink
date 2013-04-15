//
// Copyright (C) 2006-2013 Kody Brown (kody@bricksoft.com).
// 
// MIT License:
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to
// deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
// sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Bricksoft.PowerCode
{
	public static class ConsoleUtils
	{
		public static string CenterWindow()
		{
			IntPtr hWin;
			RECT rc;
			Screen scr;
			int x;
			int y;

			// Center the window on the screen.
			try {
				hWin = GetConsoleWindow();
			} catch (Exception ex) {
				return "Error : GetConsoleWindow()\n\n" + ex.Message;
			}

			try {
				GetWindowRect(hWin, out rc);
			} catch (Exception ex) {
				return "Error : GetWindowRect()\n\n" + ex.Message;
			}

			scr = Screen.FromPoint(new Point(rc.left, rc.top));

			x = scr.WorkingArea.Left + (scr.WorkingArea.Width - (rc.right - rc.left)) / 2;
			y = scr.WorkingArea.Top + (scr.WorkingArea.Height - (rc.bottom - rc.top)) / 2;

			try {
				MoveWindow(hWin, x, y, rc.right - rc.left, rc.bottom - rc.top, false);
			} catch (Exception ex) {
				return "Error : MoveWindow()\n\n" + ex.Message;
			}

			return string.Empty;
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
