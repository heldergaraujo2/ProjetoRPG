import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync, statSync } from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { createHash } from 'node:crypto';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const coast = path.join(root, 'client/Assets/Worlds/JDungeon3D-Coast');
const scene = readFileSync(path.join(coast, 'CoastWorld.tscn'), 'utf8');
const modelScene = readFileSync(path.join(coast, 'CoastModel.tscn'), 'utf8');
const provenance = readFileSync(path.join(coast, 'README.md'), 'utf8');

const requiredNotices = [
  ['JDungeon3D-MIT.txt', '3c5d416e2aefb63cd2732485f82a0db64c406c6fa941b24733d575f4f6cfd6ad'],
  ['Coast-MIT.txt', '63aa8d425a1e2e0fbe9683fe72af63bfae3c6d8a4dddaf4287b80b0fa90a1cab'],
  ['PineTree-MIT.txt', '63aa8d425a1e2e0fbe9683fe72af63bfae3c6d8a4dddaf4287b80b0fa90a1cab'],
  ['CelticBarn-MIT.txt', '63aa8d425a1e2e0fbe9683fe72af63bfae3c6d8a4dddaf4287b80b0fa90a1cab'],
  ['CelticBlacksmith-MIT.txt', '63aa8d425a1e2e0fbe9683fe72af63bfae3c6d8a4dddaf4287b80b0fa90a1cab'],
  ['CelticHouse-MIT.txt', '63aa8d425a1e2e0fbe9683fe72af63bfae3c6d8a4dddaf4287b80b0fa90a1cab'],
  ['CelticTavern-MIT.txt', '63aa8d425a1e2e0fbe9683fe72af63bfae3c6d8a4dddaf4287b80b0fa90a1cab']
];

test('Coast integration pins exact upstream provenance and preserves original MIT notices', () => {
  assert.match(provenance, /73bea81341846341f302dfa184e33d31733c3fa8/);
  for (const [copiedName, expectedHash] of requiredNotices) {
    const copied = readFileSync(path.join(coast, 'licenses', copiedName));
    assert.equal(createHash('sha256').update(copied).digest('hex'), expectedHash, `${copiedName} must match its audited upstream notice`);
  }
});

test('Coast scenes resolve only selected assets and retain static map collision/navigation content', () => {
  const scenePaths = [...scene.matchAll(/path="res:\/\/([^"]+)"/g), ...modelScene.matchAll(/path="res:\/\/([^"]+)"/g)]
    .map(match => path.join(root, 'client', match[1]));
  for (const resourcePath of scenePaths) assert.doesNotThrow(() => statSync(resourcePath), `missing resource ${resourcePath}`);
  assert.match(scene, /\[node name="CoastWorld" type="Node3D"\]/);
  assert.match(scene, /type="NavigationMesh"/);
  assert.match(scene, /NavigationRegion3D/);
  assert.match(scene, /SpawnReference/);
  assert.match(scene, /position = Vector3\(0\.148617, 2\.25, 51\.4204\)/);
  assert.equal((scene.match(/PackedInt32Array\(/g) ?? []).length, 1849, 'preserve the source Coast navigation polygons');
  assert.equal((modelScene.match(/\[node name="StaticBody3D"/g) ?? []).length, 68);
  assert.equal((modelScene.match(/\[node name="CollisionShape3D"/g) ?? []).length, 68);
  assert.ok(statSync(path.join(coast, 'CoastModel.glb')).size > 2_000_000);
  assert.match(scene, /Water\.gdshader|coast_water/);
});

test('the imported map excludes JDungeon3D gameplay systems and does not replace Bootstrap', () => {
  for (const forbidden of ['Player.tscn', 'Player.gd', 'ComponentList.tscn', 'RespawnComponent', 'Mushroomdog.tscn', 'Kelp.tscn']) {
    assert.ok(!scene.includes(forbidden), `CoastWorld must not retain ${forbidden}`);
  }
  assert.doesNotMatch(modelScene, /script\s*=|Player|MovementComponent|Multiplayer/);
  const project = readFileSync(path.join(root, 'client/project.godot'), 'utf8');
  assert.match(project, /run\/main_scene="res:\/\/Scenes\/Bootstrap\.tscn"/);
});
