// dotask: 1
// description: Create a self-extracting shrink installer using prebuilt doinstall tools.
// options:
//   - {name: doinstall-tools, type: path, description: Directory with matching native builder/runtime/launcher tools.}
//   - {name: app-version, description: Package version; otherwise evaluated MSBuild Version.}
//   - {name: output, type: path, description: Alternate package parent directory.}
//   - {name: build-stamp, description: Reproducible UTC YYDDD-HHMM stamp.}
// requires: [{kind: task, value: publish}, {kind: task, value: _/doinstall/create-installer}]
// end-dotask
use dotask_sdk::{BuildContext, Context, Result, Value, json, serde_json};
use std::path::PathBuf;

fn main() {
    dotask_sdk::run(task);
}
fn task(project: &BuildContext) -> Result<()> {
    if !cfg!(windows) || project.architecture() != "x64" {
        dotask_sdk::bail!("This installer recipe targets Windows x64.");
    }
    let mut parameters = json!({});
    if let Ok(output) = project.string("output") {
        parameters["output"] = json!(project.path(output));
    }
    let tools = if let Ok(path) = project.string("doinstall-tools") {
        Some(project.path(path))
    } else if let Ok(source) = project.setting_path("doinstall.source") {
        let metadata: Value = serde_json::from_str(&project.capture(
            project.command("cargo").current_dir(source).args([
                "metadata",
                "--format-version",
                "1",
                "--no-deps",
                "--locked",
            ]),
        )?)?;
        Some(
            PathBuf::from(
                metadata["target_directory"]
                    .as_str()
                    .context("Doinstall target_directory is missing")?,
            )
            .join("release"),
        )
    } else {
        None
    };
    if let Some(tools) = tools {
        for (key, filename) in [
            ("builder", "doinstall-builder.exe"),
            ("installer", "doinstall.exe"),
            ("launcher-template", "doinstall-launcher.exe"),
        ] {
            let path = tools.join(filename);
            if !path.is_file() {
                dotask_sdk::bail!("Missing prebuilt tool: {}", path.display());
            }
            parameters[key] = json!(path);
        }
    }
    // Read MSBuild's evaluated release version, including caller overrides.
    // The recipe forwards the same value to publishing, preventing mismatches.
    let version = if let Ok(value) = project.string("app-version") {
        value.to_owned()
    } else {
        project
            .capture(
                project
                    .command("dotnet")
                    .arg("msbuild")
                    .arg(project.setting_path("project")?)
                    .args(["-p:Configuration=Release", "-getProperty:Version"]),
            )?
            .trim()
            .to_owned()
    };
    parameters["app-version"] = json!(version);
    if let Ok(stamp) = project.string("build-stamp") {
        parameters["build-stamp"] = json!(stamp);
    }
    let artifact = project.create_installer("_/doinstall/create-installer", parameters)?;
    project.set_installer_result(&artifact)
}
