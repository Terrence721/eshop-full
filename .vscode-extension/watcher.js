// Runs detached from the VS Code extension host that spawned it, so it
// survives even when that process is killed abruptly on a full VS Code
// quit (the exact case deactivate() turned out not to reliably fire for -
// confirmed against real upstream VS Code issues #70665/#47881). Polls the
// extension host's own PID directly via the OS, not a VS Code lifecycle
// callback, and runs the cleanup the moment that PID is actually gone.
const { spawnSync } = require("child_process");

const watchedPid = Number(process.argv[2]);
const repoRoot = process.argv[3];
const POLL_INTERVAL_MS = 3000;

if (!Number.isInteger(watchedPid) || !repoRoot) {
  process.exit(1);
}

function isAlive(pid) {
  try {
    // Signal 0 does not actually send a signal - it's the standard
    // cross-platform (including Windows) way to check process existence.
    process.kill(pid, 0);
    return true;
  } catch {
    return false;
  }
}

function poll() {
  if (isAlive(watchedPid)) {
    setTimeout(poll, POLL_INTERVAL_MS);
    return;
  }

  spawnSync("yarn clean:build-servers", {
    cwd: repoRoot,
    shell: true,
    stdio: "ignore",
    timeout: 15000,
  });
  process.exit(0);
}

poll();
