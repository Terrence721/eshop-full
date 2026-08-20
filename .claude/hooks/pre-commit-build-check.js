const { execFileSync } = require('child_process');

const REPO_ROOT = 'c:/Users/Terre/source/repos/eShop-full';

function allow() {
  console.log(JSON.stringify({ hookSpecificOutput: { hookEventName: 'PreToolUse', permissionDecision: 'allow' } }));
  process.exit(0);
}

function deny(reason) {
  console.log(JSON.stringify({
    hookSpecificOutput: { hookEventName: 'PreToolUse', permissionDecision: 'deny', permissionDecisionReason: reason },
  }));
  process.exit(0);
}

let input;
try {
  input = JSON.parse(require('fs').readFileSync(0, 'utf8'));
} catch {
  allow();
}

const command = (input.tool_input && input.tool_input.command) || '';
if (!/^\s*git\s+commit\b/.test(command)) {
  allow();
}

try {
  execFileSync('dotnet', ['build', 'eShop.Web.slnf', '-v', 'quiet'], { cwd: REPO_ROOT });
  allow();
} catch {
  deny('dotnet build eShop.Web.slnf failed - fix the build before committing (run it manually to see the errors).');
}
