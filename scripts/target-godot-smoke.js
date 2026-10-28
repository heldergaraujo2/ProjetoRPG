import { spawnSync } from 'node:child_process';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const clientPath = path.join(root, 'client');
const candidates = process.env.GODOT_BIN ? [process.env.GODOT_BIN, 'godot', 'godot4'] : ['godot', 'godot4'];
let executable;
let versionResult;
for (const candidate of candidates) {
  const result = spawnSync(candidate, ['--version'], { cwd: root, encoding: 'utf8', timeout: 10000 });
  if (!result.error && result.status === 0) {
    executable = candidate;
    versionResult = result;
    break;
  }
}
if (!executable) {
  console.error('TARGET_TEST_BLOCKED: Godot 4 .NET executable is unavailable. Install the official matching Mono build and rerun npm run test:target:client.');
  process.exit(2);
}

const version = `${versionResult.stdout}${versionResult.stderr}`.trim();
if (!/^4\./.test(version) || !/mono/i.test(version)) {
  console.error(`TARGET_TEST_BLOCKED: expected Godot 4 .NET/Mono build; found: ${version}`);
  process.exit(2);
}
const dotnet = spawnSync('dotnet', ['--version'], { cwd: root, encoding: 'utf8', timeout: 10000 });
if (dotnet.error || dotnet.status !== 0) {
  console.error('TARGET_TEST_BLOCKED: .NET SDK is unavailable. Install the SDK pinned in global.json before importing/building the C# Godot project.');
  process.exit(2);
}
console.log(`Using ${executable} ${version}; .NET SDK ${dotnet.stdout.trim()}`);

function run(args, label, timeout) {
  const result = spawnSync(executable, args, { cwd: root, encoding: 'utf8', timeout, maxBuffer: 8 * 1024 * 1024 });
  const output = `${result.stdout ?? ''}${result.stderr ?? ''}`;
  if (result.error || result.status !== 0) {
    console.error(`${label} failed (status=${result.status}, error=${result.error ?? 'none'}):\n${output}`);
    process.exit(1);
  }
  return output;
}

run(['--headless', '--editor', '--build-solutions', '--path', clientPath, '--quit'], 'Godot editor import/C# build', 180000);
const output = run(['--headless', '--path', clientPath, '--quit-after', '2'], 'Godot Bootstrap scene', 60000);
if (!output.includes('CLIENT_BOOT_OK')) {
  console.error(`Godot started but Bootstrap marker was absent; this is not a successful client smoke test.\n${output}`);
  process.exit(1);
}
console.log('GODOT_CLIENT_SMOKE_OK: headless editor import/build and Bootstrap scene startup.');
