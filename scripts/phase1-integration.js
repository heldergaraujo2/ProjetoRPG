import { randomBytes, createHash } from 'node:crypto';
import { spawn, spawnSync } from 'node:child_process';
import { once } from 'node:events';
import { createServer } from 'node:net';
import { mkdtemp, rm, writeFile } from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import WebSocket from 'ws';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const serverProject = path.join(root, 'server', 'ProjetoRpg.Server.csproj');
const dotnetVersion = spawnSync('dotnet', ['--version'], { cwd: root, encoding: 'utf8' });
if (dotnetVersion.error || dotnetVersion.status !== 0) {
  console.error('PHASE1_TEST_BLOCKED: .NET SDK is unavailable.');
  process.exit(2);
}
const dockerVersion = spawnSync('docker', ['--version'], { cwd: root, encoding: 'utf8' });
if (dockerVersion.error || dockerVersion.status !== 0) {
  console.error('PHASE1_TEST_BLOCKED: Docker is unavailable; the test requires an isolated PostgreSQL container.');
  process.exit(2);
}
console.log(`Phase 1 integration test using .NET SDK ${dotnetVersion.stdout.trim()} and ${dockerVersion.stdout.trim()}.`);
const serverBuild = spawnSync('dotnet', ['build', serverProject, '--verbosity', 'quiet'],
  { cwd: root, encoding: 'utf8', timeout: 240000, maxBuffer: 8 * 1024 * 1024 });
if (serverBuild.status !== 0) {
    console.error(`PHASE1_BUILD_FAILED: server build/restore failed before integration tests.\n${serverBuild.stdout}\n${serverBuild.stderr}`);
  process.exit(1);
}

function requireCondition(condition, message) {
  if (!condition) throw new Error(message);
}

async function availablePort() {
  const probe = createServer();
  probe.listen(0, '127.0.0.1');
  await once(probe, 'listening');
  const { port } = probe.address();
  await new Promise((resolve, reject) => probe.close(error => error ? reject(error) : resolve()));
  return port;
}

function startProcess(command, args, env = process.env) {
  const child = spawn(command, args, {
    cwd: root,
    env,
    detached: process.platform !== 'win32',
    stdio: ['ignore', 'pipe', 'pipe']
  });
  let output = '';
  child.stdout.setEncoding('utf8').on('data', chunk => { output += chunk; });
  child.stderr.setEncoding('utf8').on('data', chunk => { output += chunk; });
  return { child, getOutput: () => output };
}

function waitForClose(child, timeoutMs) {
  return new Promise(resolve => {
    if (child.exitCode !== null || child.signalCode !== null) {
      resolve({ code: child.exitCode, signal: child.signalCode });
      return;
    }
    const onClose = (code, signal) => {
      clearTimeout(timeout);
      resolve({ code, signal });
    };
    const timeout = setTimeout(() => {
      child.off('close', onClose);
      resolve(null);
    }, timeoutMs);
    child.once('close', onClose);
  });
}

async function stopProcess(handle) {
  const { child } = handle;
  if (child.exitCode !== null || child.signalCode !== null) return;
  if (process.platform === 'win32' && child.pid) {
    spawnSync('taskkill', ['/PID', String(child.pid), '/T', '/F'], { encoding: 'utf8' });
  } else if (child.pid) {
    try { process.kill(-child.pid, 'SIGTERM'); } catch { child.kill('SIGTERM'); }
  }
  if (await waitForClose(child, 6000) === null) {
    if (process.platform === 'win32' && child.pid)
      spawnSync('taskkill', ['/PID', String(child.pid), '/T', '/F'], { encoding: 'utf8' });
    else if (child.pid) {
      try { process.kill(-child.pid, 'SIGKILL'); } catch { child.kill('SIGKILL'); }
    }
    await waitForClose(child, 2000);
  }
}

function runDotnet(args, env = process.env, noBuild = false) {
  const commandArgs = ['run', '--no-launch-profile', '--project', serverProject, '--verbosity', 'quiet'];
  if (noBuild) commandArgs.splice(1, 0, '--no-build');
  commandArgs.push('--', ...args);
  return spawnSync('dotnet', commandArgs, { cwd: root, env, encoding: 'utf8', timeout: 180000, maxBuffer: 8 * 1024 * 1024 });
}

function stopDockerContainer(name) {
  spawnSync('docker', ['stop', name], { cwd: root, encoding: 'utf8', timeout: 30000 });
  spawnSync('docker', ['rm', '--force', name], { cwd: root, encoding: 'utf8', timeout: 30000 });
}

async function waitForPostgres(containerName, username, databaseName) {
  const deadline = Date.now() + 120000;
  let last = '';
  while (Date.now() < deadline) {
    const result = spawnSync('docker', ['exec', containerName, 'pg_isready', '-U', username, '-d', databaseName],
      { cwd: root, encoding: 'utf8', timeout: 5000 });
    if (result.status === 0) return;
    last = result.stderr || result.stdout || result.error?.message || 'not ready';
    await new Promise(resolve => setTimeout(resolve, 500));
  }
  throw new Error(`PostgreSQL container did not become ready: ${last}`);
}

function psql(containerName, username, databaseName, sql) {
  const result = spawnSync('docker', ['exec', containerName, 'psql', '-X', '-v', 'ON_ERROR_STOP=1', '-U', username, '-d', databaseName, '-At', '-c', sql],
    { cwd: root, encoding: 'utf8', timeout: 15000 });
  if (result.status !== 0) throw new Error(`Disposable database assertion failed (exit ${result.status ?? 'unknown'}); SQL/output withheld to avoid leaking generated test credentials.`);
  return result.stdout.trim();
}

function startServer(port, extraEnv = {}) {
  const env = { ...process.env, ASPNETCORE_URLS: `http://127.0.0.1:${port}`, ...extraEnv };
  const handle = startProcess('dotnet', ['run', '--no-build', '--no-launch-profile', '--project', serverProject, '--verbosity', 'quiet'], env);
  return handle;
}

async function waitForHttp(url, handle, timeoutMs = 30000) {
  const deadline = Date.now() + timeoutMs;
  let lastError;
  while (Date.now() < deadline) {
    if (handle.child.exitCode !== null) throw new Error(`Server exited (${handle.child.exitCode}).\n${handle.getOutput()}`);
    try {
      const response = await fetch(url, { signal: AbortSignal.timeout(1500) });
      if (response.ok) return response;
    } catch (error) { lastError = error; }
    await new Promise(resolve => setTimeout(resolve, 200));
  }
  throw new Error(`Timed out waiting for ${url}: ${lastError ?? 'no response'}.\n${handle.getOutput()}`);
}

async function jsonRequest(url, method, body, token) {
  const headers = { 'content-type': 'application/json' };
  if (token) headers.authorization = `Bearer ${token}`;
  const response = await fetch(url, {
    method,
    headers,
    body: body === undefined ? undefined : JSON.stringify(body),
    signal: AbortSignal.timeout(10000)
  });
  return { response, text: await response.text() };
}

function expectStatus(result, status, label) {
  requireCondition(result.response.status === status,
    `${label}: expected HTTP ${status}, got ${result.response.status}: ${result.text}`);
}

function websocketProbe(url) {
  return new Promise((resolve, reject) => {
    const socket = new WebSocket(url);
    let gotAck = false;
    const timeout = setTimeout(() => finish(new Error('WebSocket protocol smoke timed out.')), 10000);
    function finish(error) {
      clearTimeout(timeout);
      if (socket.readyState === WebSocket.OPEN) socket.close();
      if (error) reject(error); else resolve();
    }
    socket.once('open', () => socket.send(JSON.stringify({ type: 'Hello', protocolVersion: 1, sequence: 1 })));
    socket.on('message', data => {
      let message;
      try { message = JSON.parse(data.toString()); } catch { finish(new Error('WebSocket returned invalid JSON.')); return; }
      if (message.type === 'HelloAck') {
        gotAck = true;
        socket.send(JSON.stringify({ type: 'Ping', protocolVersion: 1, sequence: 2 }));
      } else if (message.type === 'Pong' && message.sequence === 2 && gotAck) finish();
    });
    socket.once('error', finish);
  });
}

let containerName;
let postgresImageId = 'unknown';
let postgresVersion = 'unknown';
let runningServers = [];
try {
  const dbPort = await availablePort();
  const databaseName = 'projetorpg_test';
  const databaseUser = 'rpg_test';
  const databasePassword = randomBytes(24).toString('base64url');
  containerName = `projetorpg-pg-${process.pid}-${randomBytes(4).toString('hex')}`;
  const dockerRun = spawnSync('docker', [
    'run', '--detach', '--rm', '--name', containerName,
    '--publish', `127.0.0.1:${dbPort}:5432`,
    '--env', `POSTGRES_DB=${databaseName}`,
    '--env', `POSTGRES_USER=${databaseUser}`,
    '--env', `POSTGRES_PASSWORD=${databasePassword}`,
    process.env.PROJETORPG_TEST_POSTGRES_IMAGE ?? 'postgres:18-alpine'
  ], { cwd: root, encoding: 'utf8', timeout: 60000 });
  if (dockerRun.status !== 0) throw new Error(`Could not start the disposable PostgreSQL container: ${dockerRun.stderr || dockerRun.error?.message}`);
  await waitForPostgres(containerName, databaseUser, databaseName);
  const imageInspect = spawnSync('docker', ['inspect', '--format', '{{.Image}}', containerName], { cwd: root, encoding: 'utf8' });
  if (imageInspect.status === 0) postgresImageId = imageInspect.stdout.trim();
  const connectionString = `Host=127.0.0.1;Port=${dbPort};Database=${databaseName};Username=${databaseUser};Password=${databasePassword};Ssl Mode=Disable;Timeout=2;Command Timeout=5`;
  const dbEnv = { ...process.env, ConnectionStrings__GameDatabase: connectionString, ASPNETCORE_ENVIRONMENT: 'Development' };
  delete dbEnv.ConnectionStrings__MigrationDatabase;
  delete dbEnv['ConnectionStrings:MigrationDatabase'];
  delete dbEnv.GAME_CONTENT_DIRECTORY;

  const invalidConfigurationEnv = { ...process.env };
  delete invalidConfigurationEnv.ConnectionStrings__GameDatabase;
  delete invalidConfigurationEnv['ConnectionStrings:GameDatabase'];
  const invalidConfiguration = startProcess('dotnet', ['run', '--no-build', '--no-launch-profile', '--project', serverProject, '--verbosity', 'quiet'], invalidConfigurationEnv);
  const invalidConfigurationExit = await waitForClose(invalidConfiguration.child, 15000);
  if (invalidConfigurationExit === null) {
    await stopProcess(invalidConfiguration);
    throw new Error('Server did not reject missing database configuration.');
  }
  requireCondition(invalidConfigurationExit.code !== 0 && invalidConfiguration.getOutput().includes('CONFIGURATION_INVALID'),
    `Missing database configuration was not rejected safely.\n${invalidConfiguration.getOutput()}`);

  const unavailableDbPort = await availablePort();
  const unavailableDbEnv = {
    ...process.env,
    ConnectionStrings__GameDatabase: `Host=127.0.0.1;Port=${unavailableDbPort};Database=unavailable;Username=unavailable;Password=${randomBytes(16).toString('hex')};Ssl Mode=Disable;Timeout=1;Command Timeout=2`,
    ASPNETCORE_ENVIRONMENT: 'Development'
  };
  const unavailableServerPort = await availablePort();
  const unavailableServer = startServer(unavailableServerPort, unavailableDbEnv);
  runningServers.push(unavailableServer);
  const unavailableBase = `http://127.0.0.1:${unavailableServerPort}`;
  await waitForHttp(`${unavailableBase}/health/live`, unavailableServer);
  const unavailableDbHealth = await fetch(`${unavailableBase}/health/database`, { signal: AbortSignal.timeout(10000) });
  const unavailableReadiness = await fetch(`${unavailableBase}/health/ready`, { signal: AbortSignal.timeout(10000) });
  const unavailableBody = await unavailableReadiness.json();
  requireCondition(unavailableDbHealth.status === 503 && unavailableReadiness.status === 503 && unavailableBody.database === 'unavailable',
    `A live process with an unavailable database must not report readiness: ${JSON.stringify(unavailableBody)}`);
  expectStatus(await jsonRequest(`${unavailableBase}/api/v1/sessions`, 'POST', { username: 'missing', password: 'not-a-valid-password' }),
    503, 'authentication request with unavailable database');
  await stopProcess(unavailableServer);
  runningServers = runningServers.filter(server => server !== unavailableServer);

  const beforeMigrationPort = await availablePort();
  const beforeMigration = startServer(beforeMigrationPort, dbEnv);
  runningServers.push(beforeMigration);
  const beforeMigrationBase = `http://127.0.0.1:${beforeMigrationPort}`;
  await waitForHttp(`${beforeMigrationBase}/health/live`, beforeMigration);
  const pendingReady = await fetch(`${beforeMigrationBase}/health/ready`, { signal: AbortSignal.timeout(10000) });
  const pendingBody = await pendingReady.json();
  requireCondition(pendingReady.status === 503 && pendingBody.database === 'available' && pendingBody.schema === 'migration_pending',
    `Database connectivity must be distinguished from missing migrations: ${JSON.stringify(pendingBody)}`);
  await stopProcess(beforeMigration);
  runningServers = runningServers.filter(server => server !== beforeMigration);

  const firstMigration = runDotnet(['--db-migrate'], dbEnv);
  requireCondition(firstMigration.status === 0 && firstMigration.stdout.includes('DB_MIGRATIONS_APPLIED'),
    `Initial migration failed.\n${firstMigration.stdout}\n${firstMigration.stderr}`);
  const refusedRollback = runDotnet(['--db-rollback'], dbEnv, true);
  requireCondition(refusedRollback.status !== 0 && refusedRollback.stderr.includes('confirm-data-loss'),
    'Rollback without explicit data-loss confirmation must be refused.');
  const rollback = runDotnet(['--db-rollback', '--confirm-data-loss'], dbEnv, true);
  requireCondition(rollback.status === 0 && rollback.stdout.includes('DB_MIGRATION_ROLLED_BACK'),
    `Confirmed rollback failed.\n${rollback.stdout}\n${rollback.stderr}`);
  const pendingStatus = runDotnet(['--db-status'], dbEnv, true);
  requireCondition(pendingStatus.status === 0 && JSON.parse(pendingStatus.stdout).pending.includes(1),
    `Rollback did not return schema to the expected pending state: ${pendingStatus.stdout}`);
  const reapplied = runDotnet(['--db-migrate'], dbEnv, true);
  requireCondition(reapplied.status === 0 && reapplied.stdout.includes('DB_MIGRATIONS_APPLIED'),
    `Migration re-application failed.\n${reapplied.stdout}\n${reapplied.stderr}`);
  const migrationStatus = runDotnet(['--db-status'], dbEnv, true);
  requireCondition(migrationStatus.status === 0 && JSON.parse(migrationStatus.stdout).pending.length === 0,
    `Migration status is not current: ${migrationStatus.stdout}`);
  postgresVersion = psql(containerName, databaseUser, databaseName, 'SHOW server_version');

  for (const invalidCatalogCase of [
    { file: null, id: 'test.path', contentPath: '../outside.json', expected: 'Content path escapes the catalog directory' },
    { file: JSON.stringify({ id: 'different.id' }), id: 'test.expected', contentPath: 'entry.json', expected: 'Content file id must match manifest id' },
    { file: JSON.stringify({ id: 'test.duplicate' }), id: 'test.duplicate', contentPath: 'entry.json', duplicate: true, expected: 'Duplicate content id' },
    { file: JSON.stringify({ id: 'invalid' }), id: '../invalid', contentPath: 'entry.json', expected: 'Invalid stable content id' }
  ]) {
    const invalidCatalogDirectory = await mkdtemp(path.join(os.tmpdir(), 'projetorpg-invalid-catalog-'));
    if (invalidCatalogCase.file !== null) await writeFile(path.join(invalidCatalogDirectory, 'entry.json'), invalidCatalogCase.file);
    await writeFile(path.join(invalidCatalogDirectory, 'manifest.json'), JSON.stringify({
      schemaVersion: 1,
      contentVersion: 'invalid-fixture',
      entries: [
        { id: invalidCatalogCase.id, kind: 'class', path: invalidCatalogCase.contentPath },
        ...(invalidCatalogCase.duplicate ? [{ id: invalidCatalogCase.id, kind: 'class', path: invalidCatalogCase.contentPath }] : [])
      ]
    }));
    const invalidCatalogPort = await availablePort();
    const invalidCatalogServer = startServer(invalidCatalogPort, { ...dbEnv, GAME_CONTENT_DIRECTORY: invalidCatalogDirectory });
    const invalidCatalogExit = await waitForClose(invalidCatalogServer.child, 15000);
    if (invalidCatalogExit === null) {
      await stopProcess(invalidCatalogServer);
      await rm(invalidCatalogDirectory, { recursive: true, force: true });
      throw new Error('Server did not reject an invalid content catalog at startup.');
    }
    requireCondition(invalidCatalogExit.code !== 0 && invalidCatalogServer.getOutput().includes(invalidCatalogCase.expected),
      `Invalid catalog was not rejected (${invalidCatalogCase.expected}).\n${invalidCatalogServer.getOutput()}`);
    await rm(invalidCatalogDirectory, { recursive: true, force: true });
  }

  const port = await availablePort();
  const server = startServer(port, dbEnv);
  runningServers.push(server);
  const baseUrl = `http://127.0.0.1:${port}`;
  await waitForHttp(`${baseUrl}/health/live`, server);
  const ready = await fetch(`${baseUrl}/health/ready`);
  requireCondition(ready.status === 200, `Migrated database should make server ready, got ${ready.status}.`);
  const dbHealth = await fetch(`${baseUrl}/health/database`);
  requireCondition(dbHealth.status === 200, 'Database health must perform a real PostgreSQL query.');
  const contentHealth = await (await fetch(`${baseUrl}/health/content`)).json();
  requireCondition(contentHealth.contentVersion === '0.1.0' && contentHealth.entries === 0,
    `Unexpected production content catalog: ${JSON.stringify(contentHealth)}`);
  const tickHealth = await (await fetch(`${baseUrl}/health/tick`)).json();
  requireCondition(tickHealth.configuredRateHz === 20 && tickHealth.databaseConnectionConfigured === true,
    `Unexpected server heartbeat health: ${JSON.stringify(tickHealth)}`);
  await websocketProbe(`ws://127.0.0.1:${port}/ws`);

  const invalidRegistration = await jsonRequest(`${baseUrl}/api/v1/accounts/register`, 'POST', { username: 'x', password: 'short' });
  expectStatus(invalidRegistration, 400, 'invalid account input');
  const username = `Player${randomBytes(4).toString('hex')}`;
  const password = `Valid-Pass-${randomBytes(12).toString('base64url')}`;
  const registration = await jsonRequest(`${baseUrl}/api/v1/accounts/register`, 'POST', { username, password });
  const duplicateRegistration = await jsonRequest(`${baseUrl}/api/v1/accounts/register`, 'POST', { username: username.toLowerCase(), password });
  expectStatus(registration, 202, 'account registration');
  expectStatus(duplicateRegistration, 202, 'duplicate registration enumeration resistance');
  requireCondition(registration.text === duplicateRegistration.text,
    'Registration must return the same response for an available and already-registered username.');
  const accountCount = psql(containerName, databaseUser, databaseName,
    `SELECT count(*)::text FROM accounts WHERE username_normalized = '${username.toUpperCase()}'`);
  requireCondition(accountCount === '1', 'Case-normalized duplicate registration created more than one account.');

  const nonexistentLogin = await jsonRequest(`${baseUrl}/api/v1/sessions`, 'POST', { username: `${username}missing`, password });
  const wrongPasswordLogin = await jsonRequest(`${baseUrl}/api/v1/sessions`, 'POST', { username, password: `${password}wrong` });
  expectStatus(nonexistentLogin, 401, 'nonexistent account login');
  expectStatus(wrongPasswordLogin, 401, 'wrong password login');
  requireCondition(nonexistentLogin.text === wrongPasswordLogin.text && nonexistentLogin.text === '{"error":"invalid_credentials"}',
    'Invalid login responses must not disclose whether an account exists.');

  const login = await jsonRequest(`${baseUrl}/api/v1/sessions`, 'POST', { username: username.toLowerCase(), password });
  expectStatus(login, 201, 'valid login');
  const session = JSON.parse(login.text);
  requireCondition(typeof session.accessToken === 'string' && /^[A-Za-z0-9_-]{43}$/.test(session.accessToken),
    'Login must issue a high-entropy opaque bearer token.');
  const hashedPassword = psql(containerName, databaseUser, databaseName,
    `SELECT (password_hash <> '${password}' AND password_hash LIKE 'AQAAAA%')::text FROM accounts WHERE username_normalized = '${username.toUpperCase()}'`);
  requireCondition(hashedPassword === 'true', 'Stored credential is not an ASP.NET Identity V3 password hash.');
  const sessionHashHex = createHash('sha256').update(session.accessToken, 'ascii').digest('hex');
  const tokenStorage = psql(containerName, databaseUser, databaseName,
    `SELECT count(*)::text FROM account_sessions WHERE token_hash = decode('${sessionHashHex}', 'hex')`);
  requireCondition(tokenStorage === '1', 'Only the SHA-256 token hash should be persisted.');

  const createCharacter = await jsonRequest(`${baseUrl}/api/v1/characters`, 'POST',
    { name: 'HeroOne' }, session.accessToken);
  expectStatus(createCharacter, 201, 'character creation without assigning an unimplemented class');
  const character = JSON.parse(createCharacter.text);
  requireCondition(character.characterId && character.classId === null, 'Created unassigned character response is incomplete.');
  const noClass = await jsonRequest(`${baseUrl}/api/v1/characters`, 'POST',
    { name: 'NoClass', classId: 'fake.class' }, session.accessToken);
  expectStatus(noClass, 422, 'unavailable character class');

  const secondUsername = `Other${randomBytes(4).toString('hex')}`;
  const secondPassword = `Second-Pass-${randomBytes(12).toString('base64url')}`;
  expectStatus(await jsonRequest(`${baseUrl}/api/v1/accounts/register`, 'POST', { username: secondUsername, password: secondPassword }), 202, 'second account registration');
  const secondLogin = await jsonRequest(`${baseUrl}/api/v1/sessions`, 'POST', { username: secondUsername, password: secondPassword });
  expectStatus(secondLogin, 201, 'second account login');
  const secondToken = JSON.parse(secondLogin.text).accessToken;
  expectStatus(await jsonRequest(`${baseUrl}/api/v1/characters`, 'GET'), 401, 'unauthenticated character access');
  expectStatus(await jsonRequest(`${baseUrl}/api/v1/characters`, 'POST', { name: 'HeroOne' }, secondToken), 409,
    'character name uniqueness across accounts');
  const ownerList = await jsonRequest(`${baseUrl}/api/v1/characters`, 'GET', undefined, session.accessToken);
  expectStatus(ownerList, 200, 'owner character list');
  requireCondition(JSON.parse(ownerList.text).length === 1, 'Owner should see exactly their own character.');
  expectStatus(await jsonRequest(`${baseUrl}/api/v1/characters/${character.characterId}`, 'GET', undefined, secondToken), 404, 'other account cannot read character');
  expectStatus(await jsonRequest(`${baseUrl}/api/v1/characters/${character.characterId}`, 'PATCH', { name: 'StolenName' }, secondToken), 404, 'other account cannot alter character');
  const ownerRename = await jsonRequest(`${baseUrl}/api/v1/characters/${character.characterId}`, 'PATCH', { name: 'HeroRenamed' }, session.accessToken);
  expectStatus(ownerRename, 204, 'owner character rename');
  const ownerRead = await jsonRequest(`${baseUrl}/api/v1/characters/${character.characterId}`, 'GET', undefined, session.accessToken);
  requireCondition(JSON.parse(ownerRead.text).name === 'HeroRenamed', 'Owner rename was not persisted.');

  const accountId = psql(containerName, databaseUser, databaseName,
    `SELECT account_id::text FROM accounts WHERE username_normalized = '${username.toUpperCase()}'`);
  const expiryUpdate = psql(containerName, databaseUser, databaseName,
    `UPDATE account_sessions SET expires_at = now() - interval '1 second' WHERE token_hash = decode('${sessionHashHex}', 'hex') RETURNING account_id::text`);
  requireCondition(expiryUpdate === accountId, 'Could not set the first test session to expired.');
  expectStatus(await jsonRequest(`${baseUrl}/api/v1/characters`, 'GET', undefined, session.accessToken), 401, 'expired session');

  await jsonRequest(`${baseUrl}/api/v1/sessions/current`, 'DELETE', undefined, secondToken);
  expectStatus(await jsonRequest(`${baseUrl}/api/v1/characters`, 'GET', undefined, secondToken), 401, 'revoked session');

  const renewedLogin = await jsonRequest(`${baseUrl}/api/v1/sessions`, 'POST', { username, password });
  expectStatus(renewedLogin, 201, 'login before restart persistence test');
  const renewedToken = JSON.parse(renewedLogin.text).accessToken;
  await stopProcess(server);
  runningServers = runningServers.filter(item => item !== server);

  const restarted = startServer(port, dbEnv);
  runningServers.push(restarted);
  await waitForHttp(`${baseUrl}/health/ready`, restarted);
  const afterRestart = await jsonRequest(`${baseUrl}/api/v1/characters`, 'GET', undefined, renewedToken);
  expectStatus(afterRestart, 200, 'session and character persistence after server restart');
  requireCondition(JSON.parse(afterRestart.text).length === 1 && JSON.parse(afterRestart.text)[0].name === 'HeroRenamed',
    'Character state did not survive server restart.');

  const auditSummary = psql(containerName, databaseUser, databaseName,
    "SELECT string_agg(event_type, ',' ORDER BY event_id) FROM security_audit_events");
  for (const event of ['account_registered', 'login_succeeded', 'login_failed', 'session_revoked', 'character_created', 'character_renamed'])
    requireCondition(auditSummary.includes(event), `Expected audit event ${event} was not recorded.`);
  const auditColumns = psql(containerName, databaseUser, databaseName,
    "SELECT string_agg(column_name, ',' ORDER BY ordinal_position) FROM information_schema.columns WHERE table_name = 'security_audit_events'");
  requireCondition(!/password|token|username/i.test(auditColumns), 'Audit schema must not include password, bearer token or username fields.');
  requireCondition(!server.getOutput().includes(password) && !server.getOutput().includes(session.accessToken),
    'Server output leaked a password or access token.');

  await stopProcess(restarted);
  runningServers = runningServers.filter(item => item !== restarted);
  console.log(`PHASE1_INTEGRATION_OK: PostgreSQL ${postgresVersion} image ${postgresImageId}; migrations apply/rollback/reapply; readiness vs liveness; validated config; registration/login; generic credential errors; ASP.NET Identity password hash; hashed/revocable/expiring sessions; character ownership/persistence; audit events and WebSocket smoke.`);
} catch (error) {
  console.error(`PHASE1_INTEGRATION_FAILED: ${error.stack ?? error}`);
  process.exitCode = 1;
} finally {
  for (const server of runningServers) await stopProcess(server);
  if (containerName) stopDockerContainer(containerName);
}
