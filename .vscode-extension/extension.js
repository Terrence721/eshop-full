const { spawn } = require("child_process");
const fs = require("fs");
const path = require("path");
const vscode = require("vscode");

const watcherPath = path.join(__dirname, "watcher.js");
const logPath = path.join(__dirname, "activate.log");

function log(line) {
  try {
    fs.appendFileSync(logPath, `${new Date().toISOString()} ${line}\n`);
  } catch {
    // Logging is diagnostic-only; never let it break activate().
  }
}

function activate() {
  // __dirname here is VS Code's global extensions folder once this
  // extension is actually installed (e.g. ~/.vscode/extensions/...), not
  // this project's own folder - confirmed the hard way: computing repoRoot
  // from __dirname made every yarn call fail with "No project found in
  // .../.vscode/extensions". The real repo path has to come from the
  // actual open workspace instead.
  const folder = vscode.workspace.workspaceFolders?.[0];
  const repoRoot = folder ? folder.uri.fsPath : undefined;
  log(`activate() called, repoRoot=${repoRoot ?? "undefined"}`);

  if (!repoRoot) {
    return;
  }

  // A detached watcher that outlives this extension host and polls its PID
  // directly via the OS is the actual cleanup trigger. deactivate() was
  // tried first and dropped - confirmed unreliable on a full VS Code quit
  // (sometimes never called at all; confirmed via microsoft/vscode#70665
  // and #47881) and even when it is called, it can be killed mid-execution
  // before it finishes (microsoft/vscode#161147). A detached process isn't
  // vulnerable to either failure mode, since it isn't part of the
  // extension host's own process tree.
  const watcher = spawn(
    "node",
    [watcherPath, String(process.pid), repoRoot],
    { detached: true, stdio: "ignore", windowsHide: true },
  );
  watcher.unref();
}

module.exports = { activate };
