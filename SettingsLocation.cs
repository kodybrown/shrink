using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Bricksoft.DosToys
{
	internal static class SettingsLocation
	{
		internal static string Resolve( ref string[] arguments )
		{
			string explicitPath = null;
			List<string> remaining = new List<string>();
			// Select the file before actions, so --config-file works in any position
			// and --clear can never write to the wrong configuration.
			for (int i = 0; i < arguments.Length; i++) {
				if (!arguments[i].Trim().TrimStart('-', '/').Equals("config-file", StringComparison.OrdinalIgnoreCase)) {
					remaining.Add(arguments[i]);
					continue;
				}
				if (explicitPath != null || ++i == arguments.Length || string.IsNullOrWhiteSpace(arguments[i]) || arguments[i].StartsWith("--")) {
					throw new InvalidDataException("Specify --config-file once, followed by a file path.");
				}
				explicitPath = Path.GetFullPath(arguments[i]);
			}
			arguments = remaining.ToArray();
			if (explicitPath != null) {
				if (!ExistingFile(explicitPath)) {
					throw new FileNotFoundException("Settings file not found: " + explicitPath);
				}
				return explicitPath;
			}

			string executable = Assembly.GetEntryAssembly().Location;
			string filename = Path.GetFileNameWithoutExtension(executable) + ".settings";
			foreach (string directory in new string[] { Environment.CurrentDirectory, Path.GetDirectoryName(executable) }) {
				string candidate = Path.Combine(directory, filename);
				if (ExistingFile(candidate)) {
					return candidate;
				}
			}

			// APPDATA names the roaming directory, including redirected profiles.
			// A child-process override also permits isolated integration tests.
			string roaming = Environment.GetEnvironmentVariable("APPDATA");
			if (string.IsNullOrWhiteSpace(roaming)) {
				roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			}
			if (string.IsNullOrWhiteSpace(roaming) || !Path.IsPathRooted(roaming) || Path.GetPathRoot(roaming).Length < 3) {
				throw new InvalidDataException("Cannot locate an absolute Roaming AppData directory.");
			}
			string fallback = Path.Combine(Path.Combine(roaming, "shrink"), "shrink.settings");
			ExistingFile(fallback);
			return fallback;
		}

		private static bool ExistingFile( string path )
		{
			try {
				if ((File.GetAttributes(path) & FileAttributes.Directory) != 0) {
					throw new InvalidDataException("Settings path is a directory: " + path);
				}
				return true;
			} catch (FileNotFoundException) {
				return false;
			} catch (DirectoryNotFoundException) {
				return false;
			}
		}
	}
}
