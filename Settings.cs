/*!
	Copyright (C) 2003-2013 Kody Brown (kody@bricksoft.com).
	
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
using System.Collections.Generic;
using System.IO;

namespace Bricksoft.PowerCode
{
	public class Settings
	{
		private Dictionary<string, object> data;
		private string file;

		//public object this[string key]
		//{
		//	get
		//	{
		//		if (data.ContainsKey(key)) {
		//			return data[key];
		//		} else {
		//			return null;
		//		}
		//	}
		//	set
		//	{
		//		if (data.ContainsKey(key)) {
		//			data[key] = value;
		//		} else {
		//			data.Add(key, value);
		//		}
		//	}
		//}

		/// <summary>
		/// Creates a new instance of the class.
		/// </summary>
		public Settings()
		{
			data = new Dictionary<string, object>();
		}

		/// <summary>
		/// Creates a new instance of the class.
		/// </summary>
		public Settings( string file )
			: this()
		{
			this.file = file;
		}

		public void clear() { data.Clear(); }

		public bool read() { return read(file); }

		public bool read( string file )
		{
			string[] ar;
			string l, value, name;
			short shortVal;
			int intVal;
			long longVal;
			ulong ulongVal;
			DateTime dtVal;

			//this.file = file;
			data.Clear();

			if (!File.Exists(file)) {
				return false;
			}

			using (StreamReader reader = File.OpenText(file)) {
				while (!reader.EndOfStream) {
					l = reader.ReadLine();
					if (l == null || l.Length == 0 || l.StartsWith(";") || l.StartsWith("#") || !l.Contains("=")) {
						continue;
					}

					ar = l.Split(new char[] { '=' }, 2);
					if (ar.Length != 2) {
						continue;
					}

					name = ar[0].Trim();
					value = ar[1];

					if (value.Equals("true", StringComparison.InvariantCultureIgnoreCase)) {
						this.attr(name, true);
					} else if (value.Equals("false", StringComparison.InvariantCultureIgnoreCase)) {
						this.attr(name, false);
						//} else if (value.StartsWith("\"") && value.EndsWith("\"")) {
						//	// string enclosed in double-quotes
						//	value = value.Substring(1, value.Length - 2);
						//	while (value.IndexOf("@\\r") > -1 || value.IndexOf("@\\n") > -1) {
						//		value = value.Replace("@\\r", "\r").Replace("@\\n", "\n");
						//	}
						//	this.attr(name, value);
					} else if (value.StartsWith("[\"") && value.EndsWith("\"]")) {
						// TODO string[]
						this.attr(name, value);
					} else if (short.TryParse(value, out shortVal)) {
						this.attr(name, shortVal);
					} else if (int.TryParse(value, out intVal)) {
						this.attr(name, intVal);
					} else if (long.TryParse(value, out longVal)) {
						this.attr(name, longVal);
					} else if (ulong.TryParse(value, out ulongVal)) {
						this.attr(name, ulongVal);
					} else if (DateTime.TryParse(value, out dtVal)) {
						this.attr(name, dtVal);
					} else {
						while (value.IndexOf("@\\r") > -1 || value.IndexOf("@\\n") > -1) {
							value = value.Replace("@\\r", "\r").Replace("@\\n", "\n");
						}
						this.attr(name, value);
					}
				}

				reader.Close();
			}

			return true;
		}

		public bool write() { return write(file); }

		public bool write( string file )
		{
			string value;

			//this.file = file;

			if (File.Exists(file)) {
				File.SetAttributes(file, FileAttributes.Normal);
				File.Delete(file);
			}

			using (StreamWriter w = File.CreateText(file)) {
				foreach (KeyValuePair<string, object> p in this.data) {
					if (p.Value != null) {
						if (p.Value is bool) {
							w.WriteLine(p.Key + "=" + p.Value.ToString().ToLower());
						} else if (p.Value is DateTime) {
							w.WriteLine(p.Key + "=" + p.Value.ToString());
						} else if (p.Value is string) {
							value = p.Value.ToString();
							while (value.IndexOfAny(new char[] { '\r', '\n' }) > -1) {
								value = value.Replace("\r", "@\\r").Replace("\n", "@\\n");
							}
							w.WriteLine(p.Key + "=" + value);
						} else {
							w.WriteLine(p.Key + "=" + p.Value.ToString());
						}
					}
				}
				w.Close();
			}

			return true;
		}

		/// <summary>
		/// Returns whether the specified <paramref name="key"/> exists in the settings.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool contains( string key ) { return data.ContainsKey(key); }

		/// <summary>
		/// Returns whether the specified <paramref name="key"/> exists in the settings.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="comparison"></param>
		/// <returns></returns>
		public bool contains( string key, StringComparison comparison )
		{
			foreach (KeyValuePair<string, object> entry in data) {
				if (entry.Key.Equals(key, comparison)) {
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Removes the specified <paramref name="key"/> from the settings and returns it.
		/// If the item was not found, null is returned.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public object remove( string key )
		{
			object value;

			if (data.ContainsKey(key)) {
				value = data[key];
				data.Remove(key);
				return value;
			}

			return null;
		}

		/// <summary>
		/// Gets the value of <paramref name="name"/> from the settings.
		/// Returns it as type T.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		public T attr<T>( string name )
		{
			if (name == null || name.Length == 0) {
				throw new InvalidOperationException("name is required");
			}

			if (data.ContainsKey(name)) {
				if (typeof(T) == typeof(bool) || typeof(T).IsSubclassOf(typeof(bool))) {
					if ((object)data[name] != null) {
						return (T)(object)(data[name].ToString().StartsWith("t", StringComparison.CurrentCultureIgnoreCase));
					}
				} else if (typeof(T) == typeof(DateTime) || typeof(T).IsSubclassOf(typeof(DateTime))) {
					DateTime dt;
					if ((object)data[name] != null && DateTime.TryParse(data[name].ToString(), out dt)) {
						return (T)(object)dt;
					}
				} else if (typeof(T) == typeof(short) || typeof(T).IsSubclassOf(typeof(short))) {
					short i;
					if ((object)data[name] != null && short.TryParse(data[name].ToString(), out i)) {
						return (T)(object)i;
					}
				} else if (typeof(T) == typeof(int) || typeof(T).IsSubclassOf(typeof(int))) {
					int i;
					if ((object)data[name] != null && int.TryParse(data[name].ToString(), out i)) {
						return (T)(object)i;
					}
				} else if (typeof(T) == typeof(long) || typeof(T).IsSubclassOf(typeof(long))) {
					long i;
					if ((object)data[name] != null && long.TryParse(data[name].ToString(), out i)) {
						return (T)(object)i;
					}
				} else if (typeof(T) == typeof(ulong) || typeof(T).IsSubclassOf(typeof(ulong))) {
					ulong i;
					if ((object)data[name] != null && ulong.TryParse(data[name].ToString(), out i)) {
						return (T)(object)i;
					}
				} else if (typeof(T) == typeof(string) || typeof(T).IsSubclassOf(typeof(string))) {
					if ((object)data[name] != null) {
						return (T)(object)(data[name]).ToString();
					}
				} else {
					throw new InvalidOperationException("unknown or unsupported data type");
				}
			}

			return default(T);
		}

		/// <summary>
		/// Sets the value of <paramref name="name"/> in the settings to the <paramref name="value"/> specified.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public T attr<T>( string name, T value )
		{
			if (name == null || name.Length == 0) {
				throw new InvalidOperationException("name is required");
			}

			if (data.ContainsKey(name)) {
				data[name] = value;
			} else {
				data.Add(name, value);
			}

			return value;
		}
	}
}
