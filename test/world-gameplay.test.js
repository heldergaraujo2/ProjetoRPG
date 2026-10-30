import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';

const root = new URL('../', import.meta.url);
const read = path => readFileSync(new URL(path, root), 'utf8');
const manifest = JSON.parse(read('content/manifest.json'));
const map = JSON.parse(read('content/worlds/coast.json'));
const navigation = JSON.parse(read('content/worlds/coast-navigation.json'));
const worldService = read('server/World/WorldSessionService.cs');
const mapService = read('server/World/WorldMapCatalog.cs');
const worldEndpoints = read('server/World/WorldEndpoints.cs');
const socketEndpoint = read('server/Network/WebSocketEndpoint.cs');
const migrationUp = read('server/Persistence/Migrations/0002_world_sessions.up.sql');
const clientWorld = read('client/Scripts/WorldGameplay.cs');
const clientSocket = read('client/Scripts/WorldRealtimeClient.cs');
const lobby = read('client/Scripts/CharacterLobby.cs');
const bootstrap = read('client/project.godot');

function surfaceHeight(x, z, referenceY, maximumDifference) {
  let best = null;
  for (const triangle of navigation.triangles) {
    const [a, b, c] = [triangle.a, triangle.b, triangle.c];
    const denominator = (b[2] - c[2]) * (a[0] - c[0]) + (c[0] - b[0]) * (a[2] - c[2]);
    if (Math.abs(denominator) < 1e-10) continue;
    const u = ((b[2] - c[2]) * (x - c[0]) + (c[0] - b[0]) * (z - c[2])) / denominator;
    const v = ((c[2] - a[2]) * (x - c[0]) + (a[0] - c[0]) * (z - c[2])) / denominator;
    const w = 1 - u - v;
    if (u < -0.001 || v < -0.001 || w < -0.001) continue;
    const height = u * a[1] + v * b[1] + w * c[1];
    const difference = Math.abs(height - referenceY);
    if (difference <= maximumDifference && (best === null || difference < best.difference))
      best = { height, difference };
  }
  return best?.height ?? null;
}

test('Coast catalog pins the imported level and authoritative navigation surface to the audited source', () => {
  assert.deepEqual(manifest.entries.map(entry => [entry.id, entry.kind]), [
    ['world.coast', 'world_map'],
    ['world.coast.navigation', 'world_navigation']
  ]);
  assert.equal(map.id, 'world.coast');
  assert.equal(map.enabled, true);
  assert.equal(map.navigationId, 'world.coast.navigation');
  assert.equal(navigation.id, map.navigationId);
  assert.equal(map.sourceCommit, '73bea81341846341f302dfa184e33d31733c3fa8');
  assert.equal(navigation.sourceCommit, map.sourceCommit);
  assert.equal(navigation.triangles.length, 1849);
  assert.ok(navigation.triangles.every(({ a, b, c }) => [a, b, c].every(point => point.length === 3 && point.every(Number.isFinite))));

  const spawn = map.spawns.find(item => item.id === 'coast.initial');
  assert.ok(spawn, 'server has the selected Coast spawn');
  assert.equal(surfaceHeight(spawn.position.x, spawn.position.z, spawn.position.y, 0.55), spawn.position.y);
  for (let index = 0; index < 8; index++) {
    const angle = index * Math.PI * 2 / 8;
    const x = spawn.position.x + Math.cos(angle) * map.agentRadius;
    const z = spawn.position.z + Math.sin(angle) * map.agentRadius;
    assert.equal(surfaceHeight(x, z, spawn.position.y, 0.4), spawn.position.y,
      `spawn agent-radius sample ${index} is inside the imported navigation surface`);
  }
});

test('entry is authenticated, ownership-scoped, idempotent, and only server map/spawn data can choose initial position', () => {
  assert.match(worldEndpoints, /MapPost\("\/api\/v1\/world\/sessions"/);
  assert.match(worldEndpoints, /GetAuthenticatedAccountAsync/);
  assert.match(worldService, /WHERE account_id = @accountId AND character_id = @characterId FOR UPDATE/);
  assert.match(worldService, /UPDATE world_sessions SET closed_at = now\(\)/);
  assert.match(worldService, /SELECT world_session_id, map_id, spawn_id/);
  assert.match(worldService, /INSERT INTO world_sessions/);
  assert.match(worldService, /CoastMapId = "world\.coast"/);
  assert.match(worldService, /CoastSpawnId = "coast\.initial"/);
  assert.match(worldService, /var spawn = map\.GetSpawn\(CoastSpawnId\)/);
  assert.match(worldService, /clock_timestamp\(\) \+ @grace/);
  assert.match(migrationUp, /UNIQUE INDEX uq_world_sessions_active_character/);
  assert.match(migrationUp, /character_id uuid NOT NULL REFERENCES characters\(character_id\)/);
  assert.match(migrationUp, /account_id uuid NOT NULL REFERENCES accounts\(account_id\)/);
  assert.match(worldEndpoints, /return result\.Resumed\s*\? Results\.Ok/);
  assert.match(worldEndpoints, /Results\.NotFound\(new WorldApiError\("character_not_found"\)\)/);
});

test('server movement consumes bounded direction and server ticks, then samples collision-sized navmesh steps', () => {
  assert.match(worldService, /MoveIntentPayload intent/);
  assert.match(worldService, /float\.IsFinite\(intent\.DirectionX\)/);
  assert.match(worldService, /Math\.Abs\(intent\.DirectionX\) > 1\.001f/);
  assert.match(worldService, /currentTick <= lastTick/);
  assert.match(worldService, /Math\.Clamp\(currentTick - lastTick, 1, 2\)/);
  assert.match(worldService, /map\.TryMove\(current, intent\.DirectionX, intent\.DirectionZ, elapsedTicks/);
  assert.match(mapService, /CollisionRadius = 0\.32/);
  assert.match(mapService, /CollisionSamples = 8/);
  assert.match(mapService, /Math\.Ceiling\(distance \/ 0\.05\)/);
  assert.match(mapService, /TryGetWalkableHeight\(candidateX, candidateZ/);
  assert.match(worldService, /last_input_sequence = @sequence, last_input_tick = @tick/);
  assert.match(worldService, /WHERE ws\.world_session_id = @sessionId AND ws\.account_id = @accountId/);
  assert.doesNotMatch(read('shared/World/WorldContracts.cs'), /TargetPosition|FinalPosition|ClientPosition|RequestedSpeed/);
});

test('WebSocket ties an authenticated account to one world session, validates bearer on each movement, and preserves reconnect state', () => {
  assert.match(socketEndpoint, /GetAuthenticatedAccountAsync\(token/);
  assert.match(socketEndpoint, /Type == "WorldJoin"/);
  assert.match(socketEndpoint, /sessions\.AttachAsync\(accountId\.Value, join\.WorldSessionId/);
  assert.match(socketEndpoint, /registry\.TryAttach\(join\.WorldSessionId, connectionId\)/);
  assert.match(socketEndpoint, /Type == "MoveIntent"/);
  assert.match(socketEndpoint, /sessions\.MoveAsync\(ownerAccountId, currentSessionId, intent/);
  assert.match(socketEndpoint, /authentication_expired/);
  assert.match(socketEndpoint, /finally\s*\{\s*if \(attachedSessionId is \{ \} sessionId\)\s*registry\.Detach/);
  assert.match(worldService, /Task<WorldSessionSnapshot\?> AttachAsync/);
  assert.match(worldService, /Task LeaveAsync/);
  assert.match(worldService, /SessionGracePeriod = TimeSpan\.FromMinutes\(10\)/);
  assert.match(worldEndpoints, /MapDelete\("\/api\/v1\/world\/sessions\//);
});

test('Godot uses Bootstrap, loads the real Coast scene, animates a server-controlled selected actor, and returns to lobby', () => {
  assert.match(bootstrap, /run\/main_scene="res:\/\/Scenes\/Bootstrap\.tscn"/);
  assert.match(clientWorld, /CoastWorld\.tscn/);
  assert.match(clientWorld, /CharacterActor\.tscn/);
  assert.match(clientWorld, /ApplyAuthoritativeState/);
  assert.match(clientWorld, /Input\.IsKeyPressed\(Key\.W\)/);
  assert.match(clientWorld, /Input\.IsKeyPressed\(Key\.Right\)/);
  assert.match(clientWorld, /GameplayCamera/);
  assert.match(clientWorld, /ReturnRequested/);
  assert.match(clientWorld, /SetConnectionStatus/);
  assert.match(clientWorld, /Tentar reconectar/);
  assert.match(clientSocket, /SetRequestHeader\("Authorization", \$"Bearer \{accessToken\}"\)/);
  assert.match(clientSocket, /new MoveIntentPayload\(direction\.X, direction\.Y, sequence\)/);
  assert.doesNotMatch(clientSocket, /new MoveIntentPayload\([^)]*(?:Position|Speed)/);
  assert.match(lobby, /_enterWorldButton\.Disabled = _busy \|\| !CanEnterWorld\(\)/);
  assert.match(lobby, /EnterWorldRequested\?\.Invoke/);
  assert.match(read('client/Scripts/AccountGateway.cs'), /ReleaseWorldSessionAsync/);
});
