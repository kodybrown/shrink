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
using System.IO;
using System.Reflection;
using Bricksoft.PowerCode;

namespace Bricksoft.DosToys
{
	public class shrink
	{
		private static string appName = "";
		private static Settings settings;

		public static int Main( string[] arguments )
		{
			int width = -1;
			int height = -1;
			int largestWidth = Console.LargestWindowWidth - 4;
			int largestHeight = Console.LargestWindowHeight - 1;
			int x;
			bool center = false;
			string config;

			bool writeToConfig = false;
			bool widthSet = false;
			bool heightSet = false;
			bool centerSet = false;

			int configWidth = -1;
			int configHeight = -1;
			bool configCenter = false;

			appName = Assembly.GetEntryAssembly().Location;
			config = Path.Combine(Path.GetDirectoryName(appName), Path.GetFileNameWithoutExtension(appName)) + ".config";
			settings = new Settings(config);

			if (settings.read()) {
				configWidth = Math.Min(settings.attr<int>("width"), largestWidth);
				configHeight = Math.Min(settings.attr<int>("height"), largestHeight);
				configCenter = settings.attr<bool>("center");
			}

			for (int i = 0; i < arguments.Length; i++) {
				string a = arguments[i].Trim();
				if (int.TryParse(a, out x)) {
					x = Math.Max(x, 4);
					if (width == -1) {
						width = Math.Min(x, largestWidth);
						widthSet = true;
					} else if (height == -1) {
						height = Math.Min(x, largestHeight);
						heightSet = true;
					} else {
						Console.WriteLine("unknown argument value.");
						return 2;
					}
				} else {
					while (a.StartsWith("-") || a.StartsWith("/")) {
						a = a.TrimStart('-').TrimStart('/');
					}
					if (a.Equals("config", StringComparison.CurrentCultureIgnoreCase)) {
						writeToConfig = true;
					} else if (a.Equals("!config", StringComparison.CurrentCultureIgnoreCase)) {
						writeToConfig = false;
					} else if (a.Equals("center", StringComparison.CurrentCultureIgnoreCase)) {
						center = true;
						centerSet = true;
					} else if (a.Equals("!center", StringComparison.CurrentCultureIgnoreCase)) {
						center = false;
						centerSet = true;
					} else {
						Console.WriteLine("unknown argument.");
						return 1;
					}
				}
			}

			if (writeToConfig && !widthSet && !heightSet && !centerSet) {
				Console.WriteLine("width  = " + configWidth);
				Console.WriteLine("height = " + configHeight);
				Console.WriteLine("center = " + configCenter);
				return 0;
			}

			if (!widthSet) {
				if (configWidth > 0) {
					width = configWidth;
				} else {
					width = 80;
				}
			}
			if (!heightSet) {
				if (configHeight > 0) {
					height = configHeight;
				} else {
					height = 25;
				}
			}

			Console.WindowWidth = width;
			Console.WindowHeight = height;

			Console.BufferWidth = width;

			if (center) {
				ConsoleUtils.CenterWindow();
			}

			if (writeToConfig) {
				if (widthSet) {
					settings.attr<int>("width", width);
				}
				if (heightSet) {
					settings.attr<int>("height", height);
				}
				if (centerSet) {
					settings.attr<bool>("center", center);
				}
				settings.write();
			}

			return 0;
		}
	}
}
