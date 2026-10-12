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
//
// Separately, C# Dev Kit's Test Explorer Server process
// (Microsoft.VisualStudio.Code.ServiceHost.exe) can hang mid-session
// without exiting - confirmed directly on this repo (2026-10-12): it
// silently stopped pushing test-discovery updates for over two hours while
// staying alive, so Test Explorer never learned a new test project
// existed. There's no graceful shutdown command for it like there is for
// MSBuild, so it's force-killed outright; VS Code's own
// Microsoft.VisualStudio.Code.ServiceController.exe transparently
// respawns a fresh one the next time it's needed (verified: killing the
// stuck host produced a brand new one with a different pipe name within a
// second).
//
// Microsoft.VisualStudio.Code.Server.exe and ServiceController.exe
// themselves are deliberately NOT in this list, even though they're part
// of the same process family - confirmed directly on this repo
// (2026-10-12) the hard way: killing them alongside ServiceHost broke
// every single project in Test Explorer ("Build failed") until a full VS
// Code window reload, because they're what respawns ServiceHost in the
// first place - kill the supervisor and there's nothing left to recover
// the thing it supervises.

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

// Matched by exact binary name (plus the BuildHost dll for the dotnet.exe
// case), not by a loose "csdevkit" string match - that string also shows up
// in the command lines of diagnostic commands used to investigate this very
// issue, which would make the match catch itself. Deliberately excludes
// Microsoft.VisualStudio.Code.Server.exe and ServiceController.exe - see
// the comment above.
function killCSharpDevKitServiceHub() {
  if (process.platform === "win32") {
    const script =
      "Get-CimInstance Win32_Process | " +
      "Where-Object { $_.Name -eq 'Microsoft.VisualStudio.Code.ServiceHost.exe' -or ($_.Name -eq 'dotnet.exe' -and $_.CommandLine -match 'ProjectSystem\\.Server\\.BuildHost\\.dll') } | " +
      "ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue; \"$($_.ProcessId) $($_.Name)\" }";
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
    const output = execFileSync(
      "pkill",
      ["-fl", "Microsoft.VisualStudio.Code.ServiceHost|ProjectSystem.Server.BuildHost"],
      { encoding: "utf-8" },
    );
    return output.split("\n").filter(Boolean);
  } catch {
    return [];
  }
}

runBuildServerShutdown();
const killedServiceHub = killCSharpDevKitServiceHub();
if (killedServiceHub.length > 0) {
  console.log("Killed lingering C# Dev Kit ServiceHub processes:");
  for (const line of killedServiceHub) {
    console.log(`  ${line}`);
  }
}

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
