import { execFileSync } from "node:child_process";

// Releases file locks that MSBuild copy tasks hit when a previous dev session
// is still running (e.g. WebhookClient.exe holding eShop.ServiceDefaults.dll).
// Run this before `dotnet build` whenever an incremental rebuild starts
// tripping MSB3026/MSB3027. Pair it with `/nr:false` on the build command so
// MSBuild doesn't immediately re-acquire "node reuse" locks.
//
// Usage:  yarn unlock:build
//         yarn build          (runs this then `dotnet build eShop.slnx /nr:false`)

const SERVICE_PROCESSES = [
  "WebhookClient",
  "Webhooks.API",
  "Basket.API",
  "Identity.API",
  "OrderProcessor",
  "eShop.AppHost",
];

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

function killProcessesByExactName(names) {
  if (process.platform !== "win32") return [];
  const nameList = names.map((n) => `'${n}'`).join(",");
  const script =
    `$names = @(${nameList}); ` +
    "Get-Process -ErrorAction SilentlyContinue | " +
    "Where-Object { $names -contains $_.Name } | " +
    "ForEach-Object { Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue; \"$($_.Id) $($_.Name)\" }";
  try {
    const output = execFileSync("powershell.exe", ["-NoProfile", "-Command", script], {
      encoding: "utf-8",
    });
    return output.split(/\r?\n/).map((l) => l.trim()).filter(Boolean);
  } catch {
    return [];
  }
}

function killLingeringBuildServers() {
  if (process.platform !== "win32") return [];
  const script =
    "Get-CimInstance Win32_Process | " +
    "Where-Object { $_.Name -eq 'VBCSCompiler.exe' -or ($_.Name -eq 'dotnet.exe' -and $_.CommandLine -match 'MSBuild\\.dll') } | " +
    "ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue; \"$($_.ProcessId) $($_.Name)\" }";
  try {
    const output = execFileSync("powershell.exe", ["-NoProfile", "-Command", script], {
      encoding: "utf-8",
    });
    return output.split(/\r?\n/).map((l) => l.trim()).filter(Boolean);
  } catch {
    return [];
  }
}

runBuildServerShutdown();

const killedServices = killProcessesByExactName(SERVICE_PROCESSES);
if (killedServices.length) {
  console.log("Stopped running service processes:");
  for (const line of killedServices) console.log(`  ${line}`);
} else {
  console.log("No running service processes to stop.");
}

const killedServers = killLingeringBuildServers();
if (killedServers.length) {
  console.log("Stopped lingering MSBuild/VBCSCompiler processes:");
  for (const line of killedServers) console.log(`  ${line}`);
} else {
  console.log("No lingering MSBuild/VBCSCompiler processes.");
}

console.log("Build unlock complete.");