import assert from 'node:assert/strict';
import { createHash } from 'node:crypto';
import { readFile } from 'node:fs/promises';
import { existsSync } from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { test } from 'node:test';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const clientDirectory = path.join(root, 'client');
const assetDirectory = path.join(clientDirectory, 'Assets', 'Characters', 'KayKitAdventurers');
const expectedSceneResources = new Map([
  ['Scenes/CharacterActor.tscn', ['res://Scripts/CharacterActor.cs', 'res://Assets/Characters/KayKitAdventurers/Barbarian.glb']],
  ['Scenes/CharacterDemo.tscn', [
    'res://Scripts/DemoCamera.cs',
    'res://Scenes/CharacterActor.tscn',
    'res://Assets/Characters/KayKitAdventurers/Barbarian.glb',
    'res://Assets/Characters/KayKitAdventurers/Mage.glb'
  ]]
]);
const expectedAssets = new Map([
  ['Barbarian.glb', 'cefc311a0e10c7858b6141f5ada7e33268727564fb8ac1347aab97d000669cc6'],
  ['Mage.glb', 'cf898585da33fab50c724d31605fb931eb2912e6d2280092141e98ca81ad507d']
]);
const expectedAnimations = [
  '1H_Melee_Attack_Chop', '1H_Melee_Attack_Slice_Diagonal', '1H_Melee_Attack_Slice_Horizontal', '1H_Melee_Attack_Stab',
  '1H_Ranged_Aiming', '1H_Ranged_Reload', '1H_Ranged_Shoot', '1H_Ranged_Shooting',
  '2H_Melee_Attack_Chop', '2H_Melee_Attack_Slice', '2H_Melee_Attack_Spin', '2H_Melee_Attack_Spinning',
  '2H_Melee_Attack_Stab', '2H_Melee_Idle', '2H_Ranged_Aiming', '2H_Ranged_Reload', '2H_Ranged_Shoot', '2H_Ranged_Shooting',
  'Block', 'Block_Attack', 'Block_Hit', 'Blocking', 'Cheer', 'Death_A', 'Death_A_Pose', 'Death_B', 'Death_B_Pose',
  'Dodge_Backward', 'Dodge_Forward', 'Dodge_Left', 'Dodge_Right', 'Dualwield_Melee_Attack_Chop',
  'Dualwield_Melee_Attack_Slice', 'Dualwield_Melee_Attack_Stab', 'Hit_A', 'Hit_B', 'Idle', 'Interact',
  'Jump_Full_Long', 'Jump_Full_Short', 'Jump_Idle', 'Jump_Land', 'Jump_Start', 'Lie_Down', 'Lie_Idle', 'Lie_Pose',
  'Lie_StandUp', 'PickUp', 'Running_A', 'Running_B', 'Running_Strafe_Left', 'Running_Strafe_Right',
  'Sit_Chair_Down', 'Sit_Chair_Idle', 'Sit_Chair_Pose', 'Sit_Chair_StandUp', 'Sit_Floor_Down', 'Sit_Floor_Idle',
  'Sit_Floor_Pose', 'Sit_Floor_StandUp', 'Spellcast_Long', 'Spellcast_Raise', 'Spellcast_Shoot', 'Spellcasting',
  'T-Pose', 'Throw', 'Unarmed_Idle', 'Unarmed_Melee_Attack_Kick', 'Unarmed_Melee_Attack_Punch_A',
  'Unarmed_Melee_Attack_Punch_B', 'Unarmed_Pose', 'Use_Item', 'Walking_A', 'Walking_B', 'Walking_Backwards', 'Walking_C'
];

function parseGlbJson(buffer, fileName) {
  assert.equal(buffer.toString('ascii', 0, 4), 'glTF', `${fileName} has the GLB magic`);
  assert.equal(buffer.readUInt32LE(4), 2, `${fileName} is glTF binary version 2`);
  assert.equal(buffer.readUInt32LE(8), buffer.length, `${fileName} declares its full byte length`);
  const jsonLength = buffer.readUInt32LE(12);
  assert.equal(buffer.readUInt32LE(16), 0x4e4f534a, `${fileName} starts with a JSON chunk`);
  return JSON.parse(buffer.toString('utf8', 20, 20 + jsonLength).trim());
}

function parseSceneResources(sceneText, scenePath) {
  const resources = new Map();
  for (const line of sceneText.match(/^\[ext_resource[^\]]*\]$/gm) ?? []) {
    const resourcePath = line.match(/\bpath="([^"]+)"/)?.[1];
    const id = line.match(/\bid="([^"]+)"/)?.[1];
    assert.ok(resourcePath && id, `${scenePath} has a path and id for every external resource`);
    assert.ok(resourcePath.startsWith('res://'), `${scenePath} resource ${id} uses a project-relative res:// path`);
    const absolutePath = path.resolve(clientDirectory, ...resourcePath.slice('res://'.length).split('/'));
    assert.ok(absolutePath.startsWith(`${clientDirectory}${path.sep}`), `${scenePath} resource ${resourcePath} stays inside client`);
    assert.ok(existsSync(absolutePath), `${scenePath} resource ${id} exists: ${resourcePath}`);
    resources.set(id, resourcePath);
  }

  const referencedIds = [...sceneText.matchAll(/ExtResource\("([^"]+)"\)/g)].map(match => match[1]);
  for (const id of referencedIds) {
    assert.ok(resources.has(id), `${scenePath} refers to declared external resource ${id}`);
  }
  return [...resources.values()];
}

test('KayKit GLBs match audited hashes, contain their textured 41-joint rigs and the 76 audited clips', async () => {
  for (const [fileName, expectedHash] of expectedAssets) {
    const bytes = await readFile(path.join(assetDirectory, fileName));
    assert.equal(createHash('sha256').update(bytes).digest('hex'), expectedHash, `${fileName} SHA-256`);

    const gltf = parseGlbJson(bytes, fileName);
    assert.equal(gltf.asset.version, '2.0');
    const embeddedPngIndex = gltf.images?.findIndex(image => image.mimeType === 'image/png' && Number.isInteger(image.bufferView)) ?? -1;
    assert.notEqual(embeddedPngIndex, -1, `${fileName} embeds a PNG image`);
    assert.ok(gltf.textures?.some(texture => texture.source === embeddedPngIndex), `${fileName} references its embedded PNG from a texture`);
    assert.ok(gltf.materials?.length > 0, `${fileName} has at least one material`);
    assert.ok(gltf.meshes?.some(mesh => mesh.primitives?.some(primitive => Number.isInteger(primitive.material))), `${fileName} meshes reference materials`);
    assert.ok(gltf.skins?.some(skin => skin.name === 'Rig' && skin.joints.length === 41), `${fileName} has the inspected Rig with 41 joints`);
    const animationNames = gltf.animations.map(animation => animation.name);
    assert.deepEqual(animationNames, expectedAnimations, `${fileName} animation names and order`);
  }
});

test('the isolated scene uses both real models without replacing the official start scene', async () => {
  const [project, demo, actorScene, actorScript, cameraScript] = await Promise.all([
    readFile(path.join(root, 'client', 'project.godot'), 'utf8'),
    readFile(path.join(root, 'client', 'Scenes', 'CharacterDemo.tscn'), 'utf8'),
    readFile(path.join(root, 'client', 'Scenes', 'CharacterActor.tscn'), 'utf8'),
    readFile(path.join(root, 'client', 'Scripts', 'CharacterActor.cs'), 'utf8'),
    readFile(path.join(root, 'client', 'Scripts', 'DemoCamera.cs'), 'utf8')
  ]);

  assert.match(project, /run\/main_scene="res:\/\/Scenes\/Bootstrap\.tscn"/);
  assert.match(demo, /CharacterActor\.tscn/);
  assert.match(demo, /Barbarian\.glb/);
  assert.match(demo, /Mage\.glb/);
  assert.match(demo, /PlayerControlled = false/);
  assert.match(demo, /MAGE — NPC LOCAL/);
  const playerBlock = demo.split('[node name="Player"')[1]?.split('\n[node ')[0] ?? '';
  const npcBlock = demo.split('[node name="NPC"')[1]?.split('\n[node ')[0] ?? '';
  assert.match(playerBlock, /VisualYawCorrectionDegrees = 180\.0/);
  assert.match(npcBlock, /VisualYawCorrectionDegrees = 0\.0/);
  assert.match(actorScene, /\[node name="CharacterActor" type="CharacterBody3D"\]/);
  assert.match(actorScene, /\[node name="CollisionShape3D" type="CollisionShape3D" parent="\."\]/);
  assert.match(actorScene, /VisualYawCorrectionDegrees = 180\.0/);
  assert.match(actorScript, /model\.RotateY\(Mathf\.DegToRad\(VisualYawCorrectionDegrees\)\)/);
  assert.match(actorScript, /MouseButton\.Left/);
  assert.match(actorScript, /Plane|flat y=0 plane/);
  assert.match(actorScript, /Vector3 offset = _destination - GlobalPosition/);
  assert.match(actorScript, /Vector3 direction = offset \/ distance/);
  assert.match(actorScript, /Velocity = new Vector3\(direction\.X \* speed, 0\.0f, direction\.Z \* speed\)/);
  assert.match(actorScript, /LookAt\(GlobalPosition \+ direction, Vector3\.Up\)/);
  assert.match(actorScript, /distance <= 0\.08f/);
  assert.match(actorScript, /Idle/);
  assert.match(actorScript, /Walking_A/);
  assert.match(actorScript, /Running_A/);
  assert.match(actorScript, /if \(!PlayerControlled \|\| @event is not InputEventMouseButton/);
  assert.match(actorScript, /required visual model is missing/i);
  assert.match(cameraScript, /class DemoCamera : Camera3D/);
});

test('required GLBs, source/license, and every external scene reference exist at their published paths', async () => {
  for (const [scenePath, expectedResources] of expectedSceneResources) {
    const sceneText = await readFile(path.join(clientDirectory, scenePath), 'utf8');
    assert.deepEqual(parseSceneResources(sceneText, scenePath), expectedResources, `${scenePath} external resources`);
  }

  for (const relativePath of [
    'Scenes/Bootstrap.tscn',
    'Assets/Characters/KayKitAdventurers/Barbarian.glb',
    'Assets/Characters/KayKitAdventurers/Mage.glb',
    'Assets/Characters/KayKitAdventurers/LICENSE.txt',
    'Assets/Characters/KayKitAdventurers/SOURCE.md'
  ]) {
    assert.ok(existsSync(path.join(clientDirectory, relativePath)), `client/${relativePath} exists`);
  }
});

test('KayKit CC0 license and provenance/hash/animation audit are retained beside the models', async () => {
  const [license, source] = await Promise.all([
    readFile(path.join(assetDirectory, 'LICENSE.txt'), 'utf8'),
    readFile(path.join(assetDirectory, 'SOURCE.md'), 'utf8')
  ]);
  assert.equal(createHash('sha256').update(license).digest('hex'), 'ae322141814056dda0deea7540d74c41d87aee1da319977cd1bd84ee5a923629');
  assert.match(license, /License: \(Creative Commons Zero, CC0\)/i);
  assert.match(license, /creativecommons\.org\/publicdomain\/zero\/1\.0\//i);
  assert.match(source, /Kay Lousberg/);
  assert.match(source, /672074b73ba276876a19e8816ecdc5241817ab47/);
  assert.match(source, /CC0 1\.0 Universal/);
  assert.match(source, /75 animações/);
  assert.match(source, /76 nomes/);
});
