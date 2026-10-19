// dotask: 1
// description: Publish the .NET Framework application, optionally staging installer files.
// options:
//   - {name: configuration, alias: c, choices: [Debug, Release], default: Release}
//   - {name: stage-dir, type: path, description: Fresh installer staging directory.}
//   - {name: app-version, description: Explicit version supplied by the installer creator.}
// requires: [{kind: setting, value: project}, {kind: tool, value: dotnet}]
// end-dotask
using DoTask;

public static class Target
{
  public static async Task Main()
  {
    var context = BuildContext.Current;
    if (!context.IsWindows) throw new TaskException("shrink publishing requires Windows.");
    var project = context.Config.GetPath("project");
    var configuration = context.Parameters.Get<string>("configuration");
    List<string> properties = [$"-p:Configuration={configuration}"];
    if (context.Parameters.Contains("app-version")) properties.Add($"-p:Version={context.Parameters.Get<string>("app-version")}");
    // Framework publishing uses the installed CLR, without a runtime identifier
    // or a bundled modern .NET runtime. The user policy owns all output paths.
    await context.RunAsync("dotnet", ["publish", project, .. properties]);
    var evaluation = await context.RunAsync(new ProcessDefinition {
      Executable = "dotnet",
      Arguments = ["msbuild", project, .. properties, "-getProperty:PublishDir"],
      CaptureOutput = true
    });
    var published = evaluation.StandardOutput.Trim();
    if (!Directory.Exists(published)) throw new TaskException("Evaluated PublishDir does not exist.");
    if (context.Parameters.Contains("stage-dir")) {
      var stage = context.Parameters.GetPath("stage-dir");
      Directory.CreateDirectory(Path.Combine(stage, "defaults"));
      foreach (var file in new[] { "shrink.exe", "shrink.exe.config" }) File.Copy(Path.Combine(published, file), Path.Combine(stage, file));
      File.Copy(context.Path("README.md"), Path.Combine(stage, "README.md"));
      File.Copy(context.Path("LICENSE.txt"), Path.Combine(stage, "LICENSE.txt"));
      // Defaults must never sit beside the executable: local discovery would
      // shadow the editable Roaming settings installed outside immutable builds.
      File.Copy(context.Path("packaging/shrink.settings"), Path.Combine(stage, "defaults", "shrink.settings"));
    }
    Console.WriteLine(published);
  }
}
