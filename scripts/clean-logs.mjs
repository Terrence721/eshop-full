import { execFileSync } from "node:child_process";
import { realpathSync, rmSync } from "node:fs";
import { join } from "node:path";
import { fileURLToPath } from "node:url";

const repoRoot = join(fileURLToPath(new URL(".", import.meta.url)), "..");

function listUntrackedLogFiles(ignoredOnly) {
  const args = ["ls-files", "--others", "--exclude-standard"];
  if (ignoredOnly) args.push("--ignored");
  args.push("*.log");

  const output = execFileSync("git", args, {
    cwd: repoRoot,
    encoding: "utf-8",
  });
  return output.split("\n").filter(Boolean);
}

// --exclude-standard alone drops any .log file that already matches a
// .gitignore pattern - and this repo's .gitignore has a blanket "*.log"
// rule, so on its own that call would always return nothing at all.
// Pairing it with --ignored flips to the opposite, ignored-only set;
// neither call alone covers every .log file, so both are unioned.
//
// node_modules/ is filtered out separately even though it's gitignored
// too: the --ignored pass would otherwise also reach genuine .log files
// shipped inside installed npm packages, not just this repo's own noise.
// No .NET-side equivalent is needed - NuGet's package cache lives in
// ~/.nuget/packages, outside this repo entirely, same as Gradle's.
const candidates = [
  ...new Set([...listUntrackedLogFiles(false), ...listUntrackedLogFiles(true)]),
].filter((relativePath) => !relativePath.startsWith("node_modules/"));

// This repo's one real yarn workspace (Identity.WebApp) means a stray
// .log file inside it could in principle also surface via some other
// path once dependencies are installed - dedupe by real path, and skip
// anything already gone by the time we get to it, so "Removed X" only
// prints for a path this run actually removed.
const seen = new Set();
let removedCount = 0;

for (const relativePath of candidates) {
  const filePath = join(repoRoot, relativePath);
  let realPath;
  try {
    realPath = realpathSync(filePath);
  } catch {
    continue;
  }
  if (seen.has(realPath)) continue;
  seen.add(realPath);

  rmSync(filePath, { force: true });
  console.log(`Removed ${relativePath}`);
  removedCount++;
}

if (removedCount === 0) {
  console.log("No log files found.");
}
