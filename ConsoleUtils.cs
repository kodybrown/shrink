/*!
	Copyright (C) 2006-2013 Kody Brown (kody@bricksoft.com).
	
	MIT License:
	
	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to
	deal in the Software without restriction, including without limitation the
	rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
	sell copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:
	
	The above copyright notice and this permission notice shall be included in
	all copies or substantial portions of the Software.
	
	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
	FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
	DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Bricksoft.PowerCode
{
	public class ConsoleUtils
	{
		public struct Rect { public int left, top, right, bottom; }

		/// <summary>
		/// Gets the console window's handle.
		/// </summary>
		/// <returns></returns>
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr GetConsoleWindow();

		/// <summary>
		/// Gets the console window's size and position.
		/// </summary>
		/// <param name="hWnd"></param>
		/// <param name="rc"></param>
		/// <returns></returns>
		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool GetWindowRect( IntPtr hWnd, out Rect rc );

		/// <summary>
		/// Moves the window to the specified location and size.
		/// </summary>
		/// <param name="hWnd"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="w"></param>
		/// <param name="h"></param>
		/// <param name="repaint"></param>
		/// <returns></returns>
		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool MoveWindow( IntPtr hWnd, int left, int top, int width, int height, bool repaint );

		/// <summary>
		/// Moves the window to the specified location and size.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="top"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		public static void MoveWindow( int left, int top, int width, int height ) { MoveWindow(GetConsoleWindow(), left, top, width, height, true); }

		/// <summary>
		/// Moves the window to the specified location.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="top"></param>
		public static void MoveWindow( int left, int top )
		{
			IntPtr hWin;
			Rect rc;

			hWin = GetConsoleWindow();
			GetWindowRect(hWin, out rc);

			MoveWindow(GetConsoleWindow(), left, top, rc.right - rc.left, rc.bottom - rc.top, true);
		}

		/// <summary>
		/// Centers the console window on its current screen.
		/// </summary>
		/// <returns></returns>
		public static void CenterWindow()
		{
			IntPtr hWin;
			Rect rc;
			Screen scr;
			int x, y;

			hWin = GetConsoleWindow();
			GetWindowRect(hWin, out rc);

			scr = Screen.FromPoint(new Point(rc.left, rc.top));

			x = scr.WorkingArea.Left + (scr.WorkingArea.Width - (rc.right - rc.left)) / 2;
			y = scr.WorkingArea.Top + (scr.WorkingArea.Height - (rc.bottom - rc.top)) / 2;

			MoveWindow(hWin, x, y, rc.right - rc.left, rc.bottom - rc.top, true);
		}

		public enum WindowPosition
		{
			NotSet = 0,
			BottomLeft = 1,
			Bottom = 2,
			BottomRight = 3,
			Left = 4,
			Center = 5,
			Right = 6,
			TopLeft = 7,
			Top = 8,
			TopRight = 9,
			Maximized = 10,
			Minimized = 11,
			Restore = 12
		}

		public static void MoveWindow( WindowPosition p )
		{
			if (p == WindowPosition.NotSet) {
				return;
			} else if (p == WindowPosition.Minimized) {
				// TODO
				throw new NotImplementedException();
			} else if (p == WindowPosition.Restore) {
				// TODO
				throw new NotImplementedException();
			}

			IntPtr hWin;
			Rect rc;
			Screen scr;
			int maxColWidth = Console.LargestWindowWidth - 4,
				maxColHeight = Console.LargestWindowHeight - 1;

			hWin = GetConsoleWindow();
			GetWindowRect(hWin, out rc);
			scr = Screen.FromPoint(new Point(rc.left, rc.top));

			int cols = Console.WindowWidth, rows = Console.WindowHeight;
			int x = rc.left, y = rc.top;

			// Figure out the window size.
			if (p == WindowPosition.Center) {
				cols = Console.WindowWidth;
			} else if (p == WindowPosition.Top || p == WindowPosition.Bottom || p == WindowPosition.Maximized) {
				cols = maxColWidth;
			} else if (p == WindowPosition.TopRight || p == WindowPosition.Right || p == WindowPosition.BottomRight
					|| p == WindowPosition.TopLeft || p == WindowPosition.Left || p == WindowPosition.BottomLeft) {
				cols = (int)Math.Round(maxColWidth / 2F, 0);
			}

			if (p == WindowPosition.Center) {
				rows = Console.WindowHeight;
			} else if (p == WindowPosition.Left || p == WindowPosition.Right || p == WindowPosition.Maximized) {
				rows = maxColHeight;
			} else if (p == WindowPosition.TopLeft || p == WindowPosition.Top || p == WindowPosition.TopRight
					|| p == WindowPosition.BottomLeft || p == WindowPosition.Bottom || p == WindowPosition.BottomRight) {
				rows = (int)Math.Round(maxColHeight / 2F, 0);
			}

			// Set the window size..
			if (cols > Console.BufferWidth) {
				Console.BufferWidth = cols;
			}
			Console.WindowWidth = cols;
			Console.BufferWidth = cols;

			if (rows > Console.BufferHeight) {
				Console.BufferHeight = rows;
			}
			Console.WindowHeight = rows;

			if (p == WindowPosition.Maximized || p == WindowPosition.Center) {
				ConsoleUtils.CenterWindow();
			} else {
				// Figure out the window location.
				if (p == WindowPosition.Top || p == WindowPosition.Bottom
						|| p == WindowPosition.TopLeft || p == WindowPosition.Left || p == WindowPosition.BottomLeft) {
					x = scr.WorkingArea.Left;
				} else if (p == WindowPosition.TopRight || p == WindowPosition.Right || p == WindowPosition.BottomRight) {
					x = (int)Math.Round(scr.WorkingArea.Width / 2F, 0);
				}

				if (p == WindowPosition.Left || p == WindowPosition.Right
						|| p == WindowPosition.TopLeft || p == WindowPosition.Top || p == WindowPosition.TopRight) {
					y = scr.WorkingArea.Top;
				} else if (p == WindowPosition.BottomLeft || p == WindowPosition.Bottom || p == WindowPosition.BottomRight) {
					y = (int)Math.Round(scr.WorkingArea.Height / 2F, 0);
				}

				// Set the window location..
				MoveWindow(x, y);
			}
		}


		// http://blogs.microsoft.co.il/blogs/pavely/archive/2009/07/23/changing-console-fonts.aspx

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct ConsoleFont
		{
			public uint Index;
			public short SizeX, SizeY;
		}

		[DllImport("kernel32")]
		public static extern bool SetConsoleIcon( IntPtr hIcon );

		public static bool SetConsoleIcon( Icon icon ) { return SetConsoleIcon(icon.Handle); }

		[DllImport("kernel32")]
		private extern static bool SetConsoleFont( IntPtr hOutput, uint index );

		private enum StdHandle
		{
			OutputHandle = -11
		}

		[DllImport("kernel32")]
		private static extern IntPtr GetStdHandle( StdHandle index );

		public static bool SetConsoleFont( uint index ) { return SetConsoleFont(GetStdHandle(StdHandle.OutputHandle), index); }

		[DllImport("kernel32")]
		private static extern bool GetConsoleFontInfo( IntPtr hOutput, [MarshalAs(UnmanagedType.Bool)]bool bMaximize, uint count, [MarshalAs(UnmanagedType.LPArray), Out] ConsoleFont[] fonts );

		[DllImport("kernel32")]
		private static extern uint GetNumberOfConsoleFonts();

		public static uint ConsoleFontsCount { get { return GetNumberOfConsoleFonts(); } }

		public static ConsoleFont[] ConsoleFonts
		{
			get
			{
				ConsoleFont[] fonts = new ConsoleFont[GetNumberOfConsoleFonts()];
				if (fonts.Length > 0) {
					GetConsoleFontInfo(GetStdHandle(StdHandle.OutputHandle), false, (uint)fonts.Length, fonts);
				}
				return fonts;
			}
		}

		/*
			Example usage:

			var fonts = ConsoleUtils.ConsoleFonts;
			for (int f = 0; f < fonts.Length; f++) {
				Console.WriteLine("{0}: X={1}, Y={2}", fonts[f].Index, fonts[f].SizeX, fonts[f].SizeY);
			}
			ConsoleUtils.SetConsoleFont(10);
			ConsoleUtils.SetConsoleIcon(SystemIcons.Information);

		 */

	}
}
