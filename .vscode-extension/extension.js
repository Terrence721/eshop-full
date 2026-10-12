const { spawnSync } = require("child_process");
const path = require("path");

const repoRoot = path.resolve(__dirname, "..");

function activate() {
  // Nothing to do on activate - this extension exists only to get a
  // deactivate() callback from the VS Code extension host.
}

function deactivate() {
  // Runs on a clean window close, extension-host restart, or full VS Code
  // quit - NOT on a force-killed process (Task Manager, OS crash), which
  // VS Code gives no hook for at all. spawnSync (not spawn/exec) keeps this
  // synchronous so it actually finishes inside VS Code's short shutdown
  // window, instead of being abandoned mid-flight like a fire-and-forget
  // async call would be.
  // A single command string, not a command + args array, avoids Node's
  // shell:true + args-array combination (args aren't escaped when a shell
  // is involved - flagged by Node itself as DEP0190). No untrusted input
  // reaches this call either way, but there's no reason to leave the warning.
  spawnSync("yarn clean:build-servers", {
    cwd: repoRoot,
    shell: true,
    stdio: "ignore",
    timeout: 15000,
  });
}

module.exports = { activate, deactivate };
