import { spawnSync } from 'node:child_process';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const integration = path.join(root, 'scripts', 'phase1-integration.js');
const result = spawnSync(process.execPath, [integration], {
  cwd: root,
  env: process.env,
  stdio: 'inherit'
});
if (result.error) {
  console.error(`PHASE1_TEST_LAUNCH_FAILED: ${result.error.message}`);
  process.exit(1);
}
process.exit(result.status ?? 1);
