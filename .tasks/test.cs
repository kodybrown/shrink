// dotask: 1
// description: Build shrink and run its isolated settings and Windows console smoke tests.
// options:
//   - {name: configuration, alias: c, choices: [Debug, Release], default: Release}
// requires: [{kind: task, value: _/dotnet/build}, {kind: setting, value: project}]
// end-dotask
using DoTask;

public static class Target
{
  public static async Task Main()
  {
    var context = BuildContext.Current;
    var configuration = context.Parameters.Get<string>("configuration");
    await context.ExecTargetAsync("_/dotnet/build", new { Configuration = configuration });
    var evaluation = await context.RunAsync(new ProcessDefinition {
      Executable = "dotnet",
      Arguments = ["msbuild", context.Config.GetPath("project"), $"-p:Configuration={configuration}", "-getProperty:TargetPath"],
      CaptureOutput = true
    });
    await context.RunAsync("powershell.exe", ["-NoProfile", "-ExecutionPolicy", "Bypass", "-File", context.Path("tests/SmokeTests.ps1"), "-ExecutablePath", evaluation.StandardOutput.Trim()]);
  }
}
