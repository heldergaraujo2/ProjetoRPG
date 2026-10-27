const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const game = JSON.parse(fs.readFileSync(path.join(__dirname, '../public/data/game-data.json'), 'utf8'));
const meshes = JSON.parse(fs.readFileSync(path.join(__dirname, '../public/assets/mesh-manifest.json'), 'utf8'));

test('classes and creature templates expose combat stats', () => {
  assert.ok(Object.keys(game.classes).length >= 3);
  for (const cls of Object.values(game.classes)) {
    for (const stat of ['hp', 'damage', 'range', 'cooldown', 'speed']) assert.ok(cls[stat] > 0, `${cls.name} has ${stat}`);
  }
  for (const mob of Object.values(game.creatures)) {
    for (const stat of ['hp', 'damage', 'range', 'cooldown', 'speed', 'xp']) assert.ok(mob[stat] > 0, `${mob.name} has ${stat}`);
  }
});

test('maps have valid dimensions, spawns, portals and creature references', () => {
  for (const [id, map] of Object.entries(game.maps)) {
    assert.ok(map.width > 0 && map.height > 0, `${id} has dimensions`);
    assert.ok(map.spawn.x >= 0 && map.spawn.x <= map.width && map.spawn.y >= 0 && map.spawn.y <= map.height, `${id} has an in-bounds spawn`);
    for (const spawn of map.spawns) {
      assert.ok(game.creatures[spawn.kind], `${id} references known creature ${spawn.kind}`);
      assert.ok(spawn.x >= 0 && spawn.x <= map.width && spawn.y >= 0 && spawn.y <= map.height, `${spawn.id} is in bounds`);
    }
    for (const portal of map.portals) {
      assert.ok(game.maps[portal.to], `${portal.id} has a known destination`);
      assert.ok(portal.at.x >= 0 && portal.at.x <= game.maps[portal.to].width);
      assert.ok(portal.at.y >= 0 && portal.at.y <= game.maps[portal.to].height);
    }
  }
});

test('all placeholder actor keys have a mesh replacement slot', () => {
  for (const id of Object.keys(game.classes)) assert.ok(meshes.placeholders[`player.${id}`]);
  for (const id of Object.keys(game.creatures)) assert.ok(meshes.placeholders[`mob.${id}`]);
  assert.ok(meshes.placeholders['npc.guide']);
});
