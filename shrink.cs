/*!
	Copyright (C) 2010-2013 Kody Brown (kody@bricksoft.com).
	
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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Bricksoft.PowerCode;

namespace Bricksoft.DosToys
{
	public class shrink
	{
		static bool DEBUG = false;

		static string app;
		static int appLen;
		static string appPadding;

		static bool defaultCenter = false;

		public static int Main( string[] arguments )
		{
			int widthMaximum = Console.LargestWindowWidth - 4;
			int widthMinimum = 8;
			int heightMaximum = Console.LargestWindowHeight - 1;
			int heightMinimum = 1;

			string file;
			Settings settings;
			int value;
			StringBuilder error = new StringBuilder();

			int? width = new int?();
			int? height = new int?();
			bool? center = new bool?();
			bool writeToConfig = false;
			bool skipWidth = false;
			bool skipHeight = false;
			bool skipCenter = false;

			app = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
			appLen = Math.Max((app + ".exe").Length, 9);
			appPadding = new string(' ', appLen);

			file = Assembly.GetEntryAssembly().Location + ".settings";
			settings = new Settings(file);
			settings.read();

			for (int i = 0; i < arguments.Length; i++) {
				string a = arguments[i].Trim();

				if (int.TryParse(a, out value)) {
					value = Math.Max(value, 4);
					if (!width.HasValue && !skipWidth) {
						if (value > widthMaximum) {
							Console.WriteLine("{0," + appLen + "} | the maximum width allowed for the current screen is {1} columns. using maximum instead.", "** error", widthMaximum);
							value = widthMaximum;
						} else if (value < widthMinimum) {
							Console.WriteLine("{0," + appLen + "} | the minimum width allowed is {1} columns. using minimum instead.", "** error", widthMinimum);
							value = widthMinimum;
						}
						width = value;
					} else if (!height.HasValue && !skipHeight) {
						if (value > heightMaximum) {
							Console.WriteLine("{0," + appLen + "} | the maximum height allowed for the current screen is {1} rows. using maximum instead.", "** error", heightMaximum);
							value = heightMaximum;
						} else if (value < heightMinimum) {
							Console.WriteLine("{0," + appLen + "} | the minimum height allowed is {1} rows. using minimum instead.", "** error", heightMinimum);
							value = heightMinimum;
						}
						height = value;
					} else {
						DisplayError(settings, "unknown argument value.");
						return 2;
					}
				} else {
					if (a.Equals("-")) {
						// This separator allows the user to skip (not set) the width parameter.
						// For example: 'shrink.exe - 60' will only change the height and not the width,
						// much like: 'shrink.exe 100' will only change the width. (a trailing '-' here will be ignored.)
						if (!width.HasValue) {
							skipWidth = true;
						} else if (!height.HasValue) {
							skipHeight = true;
						} else {
							skipCenter = true;
						}
						continue;
					}

					while (a.StartsWith("-") || a.StartsWith("/")) {
						a = a.TrimStart('-').TrimStart('/');
					}

					if (a.Equals("?") || a.StartsWith("h", StringComparison.CurrentCultureIgnoreCase)) {
						DisplayCopyright();
						DisplayHelp(settings);
						DisplayConfig(settings);
						return 0;

					} else if (a.Equals("debug", StringComparison.CurrentCultureIgnoreCase)) {
						DEBUG = true;

					} else if (a.Equals("clear", StringComparison.CurrentCultureIgnoreCase)) {
						settings.clear();
						settings.write();

					} else if (a.Equals("config", StringComparison.CurrentCultureIgnoreCase)) {
						writeToConfig = true;
					} else if (a.Equals("!config", StringComparison.CurrentCultureIgnoreCase)) {
						writeToConfig = false;
					} else if (a.Equals("center", StringComparison.CurrentCultureIgnoreCase)) {
						center = true;
					} else if (a.Equals("!center", StringComparison.CurrentCultureIgnoreCase)) {
						center = false;

					} else if (a.Equals("edit", StringComparison.CurrentCultureIgnoreCase)) {
						LaunchUrl("notepad.exe", file);
					} else if (a.Equals("email", StringComparison.CurrentCultureIgnoreCase)) {
						LaunchUrl("mailto:Kody Brown <kody@bricksoft.com>");
					} else if (a.Equals("web", StringComparison.CurrentCultureIgnoreCase)) {
						LaunchUrl("http://bricksoft.com");
					} else if (a.Equals("src", StringComparison.CurrentCultureIgnoreCase) || a.Equals("source", StringComparison.CurrentCultureIgnoreCase)) {
						LaunchUrl("http://github.com/kodybrown/" + app);
					} else if (a.Equals("license", StringComparison.CurrentCultureIgnoreCase)) {
						LaunchUrl("http://opensource.org/licenses/MIT");

					} else {
						DisplayError(settings, "unknown argument.");
						return 1;
					}
				}
			}

			DisplayAppName();

			// If '--config' was specified without any other arguments, 
			// it will only output the current values from config.
			if (writeToConfig && !width.HasValue && !height.HasValue && !center.HasValue) {
				DisplayCopyright();
				DisplayConfig(settings);
				return 0;
			}

			// Write config values before they are (possibly) overwritten below.
			if (writeToConfig) {
				if (width.HasValue) {
					settings.attr<int>("width", width.Value);
				}
				if (height.HasValue) {
					settings.attr<int>("height", height.Value);
				}
				if (center.HasValue) {
					settings.attr<bool>("center", center.Value);
				}
				settings.write();
			}

			// If a value was not specified: use the value from config,
			// otherwise use its current value.
			if (!width.HasValue) {
				width = settings.contains("width") ? settings.attr<int>("width") : Console.WindowWidth;
			}
			if (!height.HasValue) {
				height = settings.contains("height") ? settings.attr<int>("height") : Console.WindowHeight;
			}
			// If center was not specified, use the value from config,
			// or default it to defaultCenter.
			if (!center.HasValue) {
				center = settings.contains("center") ? settings.attr<bool>("center") : defaultCenter;
			}

			//
			// Update the console.
			//
			try {
				Console.WindowWidth = Math.Min(widthMaximum, Math.Max(widthMinimum, width.Value));
				Console.BufferWidth = Console.WindowWidth;
			} catch (Exception ex) {
				error.AppendLine(string.Format("{0," + appLen + "} | Could not set the width.", "** error")).AppendLine(ex.Message);
			}

			try {
				Console.WindowHeight = Math.Min(heightMaximum, Math.Max(heightMinimum, height.Value));
			} catch (Exception ex) {
				error.AppendLine(string.Format("{0," + appLen + "} | Could not set the height.", "** error")).AppendLine(ex.Message);
			}

			if (!skipCenter && center.HasValue && center.Value) {
				try {
					ConsoleUtils.CenterWindow();
				} catch (Exception ex) {
					error.AppendLine(string.Format("{0," + appLen + "} | Could not center the window.", "** error")).AppendLine(ex.Message);
				}
			}

			// Ensure the whole console window is on the screen.
			try {
				IntPtr hWin;
				ConsoleUtils.Rect rc;
				Screen scr;
				int x = 0, y = 0;

				hWin = ConsoleUtils.GetConsoleWindow();
				ConsoleUtils.GetWindowRect(hWin, out rc);
				scr = Screen.FromPoint(new Point(rc.left, rc.top));

				if (rc.right > scr.WorkingArea.Right) {
					//x = rc.left - (scr.WorkingArea.Right - rc.right);
					x = rc.left + (scr.WorkingArea.Right - rc.right);
				} else {
					x = rc.left;
				}
				if (rc.bottom > scr.WorkingArea.Bottom) {
					y = rc.top + (scr.WorkingArea.Bottom - rc.bottom);
				} else {
					y = rc.top;
				}

				ConsoleUtils.MoveWindow(x, y);
			} catch (Exception ex) {
				error.AppendLine(string.Format("{0," + appLen + "} | Could not ensure the whole window is visible.", "** error")).AppendLine(ex.Message);
			}

			if (error.Length > 0) {
				Console.Write(error.ToString());
			}

			if (DEBUG) {
				Console.Write("press any key to continue: ");
				Console.ReadKey(true);
				Console.CursorLeft = 0;
				Console.Write("                            ");
				Console.CursorLeft = 0;
			}

			return 0;
		}

		private static void LaunchUrl(string file) { LaunchUrl(file, null); }

		private static void LaunchUrl(string file, string args) {
			ProcessStartInfo info = new ProcessStartInfo();

			info.Verb = "open";
			info.FileName = file;

			if (args != null && args.Length > 0) {
				info.Arguments = args;
			}

			Process.Start(info);
		}

		private static void DisplayAppName()
		{
			Console.WriteLine("{0,-" + appLen + "} | created by kody@bricksoft.com", app + ".exe"); // (--email)
		}

		private static void DisplayCopyright()
		{
			Console.WriteLine("{0} | http://github.com/kodybrown/" + app + " (--src)", appPadding);
			Console.WriteLine("{0} | released under the mit license (--license)", appPadding);
			Console.WriteLine("{0} | display usage information (--help)", appPadding);
		}

		private static void DisplayHelp( Settings settings )
		{
			Console.WriteLine("\nUSAGE:");
			Console.WriteLine("  {0}.exe [--config][--clear] width height center", app);
			Console.WriteLine();
			//Console.WriteLine(" OPTIONS:");
			Console.WriteLine("    width      sets the width of the console window.");
			Console.WriteLine("    height     sets the height of the console window.");
			Console.WriteLine("    center     centers the console window. use !center to prevent centering the window (overrides config).");
			Console.WriteLine();
			Console.WriteLine("    if a value is not specified the config value will be used. to override config, use - in place of the value to not change it from the current.");
			Console.WriteLine();
			Console.WriteLine("    --clear    clears the values in config.");
			Console.WriteLine("    --config   when used with other arguments, those values are applied then saved to config.");
			Console.WriteLine("               when used without other arguments, only displays the config values.");
			Console.WriteLine();
			Console.WriteLine("    the position of the --config, --clear, and center options do not matter. it is assumed however, that width always comes before height.");

			DisplayExamples();
		}

		private static void DisplayExamples()
		{
			int w = 27;
			Console.WriteLine("\nEXAMPLES:");
			Console.WriteLine("  {0,-" + w + "} displays all config values.", app + ".exe --config");
			Console.WriteLine();
			Console.WriteLine("  {0,-" + w + "} sets the console window, height, and centers the window.", app + ".exe 130 40 center");
			Console.WriteLine("  {0,-" + w + "} sets the console height and centers the window.", app + ".exe - 40 center");
			Console.WriteLine("  {0,-" + w + "} sets the console width.", app + ".exe 130");
			Console.WriteLine("  {0,-" + w + "} sets the console height.", app + ".exe - 40");
			Console.WriteLine("  {0,-" + w + "} centers the console window.", app + ".exe - - center");
			Console.WriteLine("  {0,-" + w + "} centers the console window.", app + ".exe center");
			Console.WriteLine();
			Console.WriteLine("  if --config is applied to any other options, they are applied, then saved to config.");
		}

		private static void DisplayConfig( Settings settings )
		{
			Console.WriteLine("\nCURRENT WINDOW:");
			Console.WriteLine("  width  = {0,3}", Console.WindowWidth);
			Console.WriteLine("  height = {0,3}", Console.WindowHeight);
			Console.WriteLine("\nSAVED CONFIG:");
			Console.WriteLine("  width  = {0}", settings.contains("width") ? settings.attr<int>("width").ToString() : "not set");
			Console.WriteLine("  height = {0}", settings.contains("height") ? settings.attr<int>("height").ToString() : "not set");
			Console.WriteLine("  center = {0}", settings.contains("center") ? settings.attr<bool>("center").ToString().ToLower() : defaultCenter.ToString().ToLower());
		}

		private static void DisplayError( Settings settings, string message )
		{
			DisplayCopyright();
			Console.WriteLine();
			Console.WriteLine("{0," + appLen + "} | {1}", "** error", message);
			DisplayHelp(settings);
			DisplayConfig(settings);
		}
	}
}
