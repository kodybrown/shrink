//
// Copyright (C) 2010-2013 Kody Brown (kody@bricksoft.com).
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
using System.IO;
using System.Reflection;
using Bricksoft.PowerCode;

namespace Bricksoft.DosToys
{
	public class shrink
	{
		private static string appName = "";
		private static Settings settings;

		public static void Main( string[] args )
		{
			int width = -1;
			int height = -1;
			int largestWidth = Console.LargestWindowWidth - 4;
			int largestHeight = Console.LargestWindowHeight - 1;
			int x;
			bool center = false;
			string config;

			int tempWidth = -1;
			int tempHeight = -1;
			bool tempCenter = false;

			appName = Assembly.GetEntryAssembly().Location;
			config = Path.Combine(Path.GetDirectoryName(appName), Path.GetFileNameWithoutExtension(appName)) + ".config";

			if (Settings.LoadSettings(config, out settings)) {
				tempWidth = Math.Min(settings.getValue("width", -1), largestWidth);
				tempHeight = Math.Min(settings.getValue("height", -1), largestHeight);
				tempCenter = settings.getValue("center", false);
			}

			foreach (string a in args) {
				if (int.TryParse(a, out x)) {
					x = Math.Max(x, 4);
					if (width == -1) {
						width = Math.Min(x, largestWidth);
					} else if (height == -1) {
						height = Math.Min(x, largestHeight);
					} else {
						Console.WriteLine("unknown argument value.");
						return;
					}
				} else if (a == "-" || a.StartsWith("c", StringComparison.CurrentCultureIgnoreCase)) {
					center = true;
				} else {
					Console.WriteLine("unknown argument.");
					return;
				}
			}

			if (width == -1) {
				if (tempWidth > -1) {
					width = tempWidth;
				} else {
					width = 80;
				}
			}
			if (height == -1) {
				if (tempHeight > -1) {
					height = tempHeight;
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
		}
	}
}
