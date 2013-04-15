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
using System.Collections.Generic;
using System.IO;

namespace Bricksoft.DosToys {
	public class Settings {
		private Dictionary<string, object> data;

		public object this[string key] {
			get {
				if (data.ContainsKey(key)) {
					return data[key];
				} else {
					return null;
				}
			}
			set {
				if (data.ContainsKey(key)) {
					data[key] = value;
				} else {
					data.Add(key, value);
				}
			}
		}

		public Settings() {
			data = new Dictionary<string, object>();
		}

		public static bool LoadSettings(string file, out Settings settings) {
			string[] lines;
			string l;
			string[] ar;

			settings = new Settings();

			if (!File.Exists(file)) {
				return false;
			}

			lines = File.ReadAllLines(file);

			for (int i = 0; i < lines.Length; i++) {
				l = lines[i].Trim();
				if (l.Length == 0 && l.StartsWith(";") && l.StartsWith("#")) {
					continue;
				}

				ar = l.Split(new char[] { '=' }, 2);
				if (ar.Length != 2) {
					continue;
				}

				settings.add(ar[0].Trim(), ar[1].Trim());
			}

			return true;
		}

		public void add(string key, object value) {
			if (data.ContainsKey(key)) {
				data[key] = value;
			} else {
				data.Add(key, value);
			}
		}

		public bool containsKey(string key) {
			return data.ContainsKey(key);
		}

		public bool getValue(string name, bool defaultValue) {
			object value;

			if (name == null || name.Length == 0) {
				throw new InvalidOperationException("name is required");
			}

			if (data.ContainsKey(name)) {
				value = data[name];
				if (value == null) {
					return defaultValue;
				}
				if (value.GetType() == typeof(bool)) {
					return (bool)value;
				} else {
					string t;
					t = value.ToString();
					return (t.StartsWith("t", StringComparison.CurrentCultureIgnoreCase) || t == "1");
				}
			}
			return defaultValue;
		}

		public string getValue(string name, string defaultValue) {
			object value;

			if (name == null || name.Length == 0) {
				throw new InvalidOperationException("name is required");
			}

			if (data.ContainsKey(name)) {
				value = data[name];
				if (value == null) {
					return defaultValue;
				}
				return value.ToString();
			} else {
				return defaultValue;
			}
		}

		public int getValue(string name, int defaultValue) {
			object value;

			if (name == null || name.Length == 0) {
				throw new InvalidOperationException("name is required");
			}

			if (data.ContainsKey(name)) {
				value = data[name];
				if (value == null) {
					return defaultValue;
				}
				if (value.GetType() == typeof(int)) {
					return (int)value;
				} else {
					int x;
					if (int.TryParse(value.ToString(), out x)) {
						return x;
					}
				}
			}
			return defaultValue;
		}
	}
}
