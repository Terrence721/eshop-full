import { execFileSync } from "node:child_process";

// dotnet build-server shutdown stops MSBuild/VB-C#-compiler/Razor build
// servers started by dotnet build/test, but after a long session of CLI
// builds a few MSBuild "node reuse" workers can still be left running -
// confirmed directly on this repo (2026-10-12): 7 of them plus a
// VBCSCompiler.exe survived until this exact call, and collided with VS
// Code's C# Dev Kit project-system startup, breaking Test Explorer across
// every test project (microsoft/vscode-dotnettools#3628 made more likely
// to hit, not caused by it). This script runs the shutdown, then verifies
// nothing matching that shape is still running instead of assuming it
// worked.

function runBuildServerShutdown() {
  try {
    const output = execFileSync("dotnet", ["build-server", "shutdown"], {
      encoding: "utf-8",
    });
    process.stdout.write(output);
  } catch (error) {
    console.error(`dotnet build-server shutdown failed: ${error.message}`);
  }
}

// Matched on the MSBuild.dll/VBCSCompiler shape specifically, not on "any
// dotnet.exe" - a genuinely running `dotnet run`/`dotnet watch` app is a
// dotnet.exe process too and isn't what this is looking for.
function findLingeringBuildServers() {
  if (process.platform === "win32") {
    const script =
      "Get-CimInstance Win32_Process | " +
      "Where-Object { $_.Name -eq 'VBCSCompiler.exe' -or ($_.Name -eq 'dotnet.exe' -and $_.CommandLine -match 'MSBuild\\.dll') } | " +
      "ForEach-Object { \"$($_.ProcessId) $($_.Name)\" }";
    try {
      const output = execFileSync(
        "powershell.exe",
        ["-NoProfile", "-Command", script],
        { encoding: "utf-8" },
      );
      return output
        .split(/\r?\n/)
        .map((line) => line.trim())
        .filter(Boolean);
    } catch {
      return [];
    }
  }

  try {
    const output = execFileSync("pgrep", ["-fl", "VBCSCompiler|MSBuild.dll"], {
      encoding: "utf-8",
    });
    return output.split("\n").filter(Boolean);
  } catch {
    // pgrep exits non-zero when nothing matches - that's the success case.
    return [];
  }
}

runBuildServerShutdown();

const lingering = findLingeringBuildServers();
if (lingering.length === 0) {
  console.log("No lingering MSBuild/VBCSCompiler build-server processes.");
} else {
  console.warn("Still running after shutdown:");
  for (const line of lingering) {
    console.warn(`  ${line}`);
  }
  process.exitCode = 1;
}
