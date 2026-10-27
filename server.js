const http = require('node:http');
const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const { WebSocketServer, WebSocket } = require('ws');

const ROOT = __dirname;
const PUBLIC = path.join(ROOT, 'public');
const GAME = JSON.parse(fs.readFileSync(path.join(PUBLIC, 'data/game-data.json'), 'utf8'));
const PORT = Number(process.env.PORT || 3000);
const players = new Map();
const maps = new Map();
const mime = { '.html': 'text/html; charset=utf-8', '.css': 'text/css; charset=utf-8', '.js': 'text/javascript; charset=utf-8', '.json': 'application/json; charset=utf-8', '.svg': 'image/svg+xml' };

function newMapState(mapId) {
  const def = GAME.maps[mapId];
  const mobs = new Map();
  for (const spawn of def.spawns) {
    const template = GAME.creatures[spawn.kind];
    mobs.set(spawn.id, { id: spawn.id, kind: spawn.kind, name: template.name, x: spawn.x, y: spawn.y, homeX: spawn.x, homeY: spawn.y,
      hp: template.hp, maxHp: template.hp, alive: true, respawnAt: 0, attackAt: 0, targetId: null, wanderAt: 0, wanderX: spawn.x, wanderY: spawn.y });
  }
  return { mobs };
}
for (const id of Object.keys(GAME.maps)) maps.set(id, newMapState(id));

function safeName(value) {
  const name = String(value || 'Aventureiro').replace(/[<>\u0000-\u001f]/g, '').trim().slice(0, 18);
  return name || 'Aventureiro';
}
function send(ws, data) {
  if (ws.readyState === WebSocket.OPEN) ws.send(JSON.stringify(data));
}
function system(message, mapId, recipient) {
  const payload = { type: 'notice', message, at: Date.now() };
  for (const p of players.values()) if (p.mapId === mapId && (!recipient || p.id === recipient)) send(p.ws, payload);
}
function distance(a, b) { return Math.hypot(a.x - b.x, a.y - b.y); }
function levelFor(xp) { return Math.floor(Math.sqrt(xp / 100)) + 1; }

const server = http.createServer((req, res) => {
  const url = new URL(req.url, `http://${req.headers.host || 'localhost'}`);
  let pathname = decodeURIComponent(url.pathname);
  if (pathname === '/') pathname = '/index.html';
  const file = path.resolve(PUBLIC, `.${pathname}`);
  if (!file.startsWith(PUBLIC + path.sep) && file !== PUBLIC) { res.writeHead(403); return res.end('Forbidden'); }
  fs.readFile(file, (err, contents) => {
    if (err) { res.writeHead(404); return res.end('Not found'); }
    res.writeHead(200, { 'Content-Type': mime[path.extname(file)] || 'application/octet-stream', 'Cache-Control': 'no-cache' });
    res.end(contents);
  });
});
const wss = new WebSocketServer({ server, maxPayload: 4096 });

wss.on('connection', (ws) => {
  const id = crypto.randomUUID().slice(0, 8);
  let player = null;
  ws.on('message', (raw) => {
    let msg;
    try { msg = JSON.parse(raw.toString()); } catch { return; }
    if (!player) {
      if (msg.type !== 'join') return;
      const classId = GAME.classes[msg.classId] ? msg.classId : 'guardiao';
      // The prototype has one server-owned initial region. Never let an untrusted
      // client choose a map/spawn; the production server will load this from the
      // authenticated, persisted character instead.
      const mapId = 'campina';
      const cls = GAME.classes[classId];
      const spawn = GAME.maps[mapId].spawn;
      player = { id, ws, name: safeName(msg.name), classId, mapId, x: spawn.x, y: spawn.y, aimX: 1, aimY: 0,
        hp: cls.hp, maxHp: cls.hp, xp: 0, level: 1, inputX: 0, inputY: 0, attackAt: 0, alive: true, respawnAt: 0, lastChatAt: 0, lastInputAt: Date.now() };
      players.set(id, player);
      send(ws, { type: 'welcome', id, player: publicPlayer(player), map: mapPayload(mapId) });
      system(`${player.name} chegou ao mundo.`, mapId);
      return;
    }
    if (msg.type === 'input') {
      const x = Number(msg.x), y = Number(msg.y);
      if (Number.isFinite(x) && Number.isFinite(y)) {
        const mag = Math.hypot(x, y) || 1;
        player.inputX = Math.max(-1, Math.min(1, x / Math.max(1, mag)));
        player.inputY = Math.max(-1, Math.min(1, y / Math.max(1, mag)));
        player.lastInputAt = Date.now();
      }
      const ax = Number(msg.aimX), ay = Number(msg.aimY), al = Math.hypot(ax, ay);
      if (Number.isFinite(al) && al > 0.01) { player.aimX = ax / al; player.aimY = ay / al; }
    } else if (msg.type === 'attack') {
      attemptAttack(player, String(msg.targetId || ''));
    } else if (msg.type === 'interact') {
      interact(player, String(msg.targetId || ''));
    } else if (msg.type === 'chat') {
      const now = Date.now();
      const text = String(msg.text || '').replace(/[<>\u0000-\u001f]/g, '').trim().slice(0, 120);
      if (text && now - player.lastChatAt > 900) {
        player.lastChatAt = now;
        for (const p of players.values()) if (p.mapId === player.mapId) send(p.ws, { type: 'chat', id: player.id, name: player.name, text });
      }
    }
  });
  ws.on('close', () => {
    if (player) { players.delete(player.id); system(`${player.name} saiu do mundo.`, player.mapId); }
  });
});

function publicPlayer(p) {
  return { id: p.id, name: p.name, classId: p.classId, x: p.x, y: p.y, hp: p.hp, maxHp: p.maxHp, xp: p.xp, level: p.level, alive: p.alive, aimX: p.aimX, aimY: p.aimY };
}
function mapPayload(mapId) {
  const def = GAME.maps[mapId];
  const state = maps.get(mapId);
  return { id: mapId, name: def.name, subtitle: def.subtitle, theme: def.theme, width: def.width, height: def.height, spawn: def.spawn, portals: def.portals, npcs: def.npcs,
    mobs: [...state.mobs.values()].map(({ id, kind, name, x, y, hp, maxHp, alive }) => ({ id, kind, name, x, y, hp, maxHp, alive })) };
}
function changeMap(p, portal) {
  const oldMap = p.mapId;
  p.mapId = portal.to;
  p.x = portal.at.x; p.y = portal.at.y;
  p.inputX = 0; p.inputY = 0;
  send(p.ws, { type: 'map', map: mapPayload(p.mapId) });
  system(`${p.name} chegou à região.`, p.mapId);
  system(`${p.name} partiu para outra região.`, oldMap);
}
function interact(p, targetId) {
  const map = GAME.maps[p.mapId];
  const npc = map.npcs.find(n => n.id === targetId);
  if (npc && Math.hypot(npc.x - p.x, npc.y - p.y) < 110) {
    send(p.ws, { type: 'dialogue', name: npc.name, text: npc.dialogue });
    return;
  }
  const portal = map.portals.find(item => item.id === targetId);
  if (portal && Math.hypot(portal.x - p.x, portal.y - p.y) < portal.radius + 30) changeMap(p, portal);
}
function attemptAttack(p, targetId) {
  const now = Date.now();
  const cls = GAME.classes[p.classId];
  if (!p.alive || now < p.attackAt) return;
  const mob = maps.get(p.mapId).mobs.get(targetId);
  if (!mob || !mob.alive || distance(p, mob) > cls.range + 30) return;
  p.attackAt = now + cls.cooldown;
  mob.hp = Math.max(0, mob.hp - cls.damage);
  if (mob.hp <= 0) {
    mob.alive = false;
    mob.respawnAt = now + 12000;
    mob.targetId = null;
    p.xp += GAME.creatures[mob.kind].xp;
    const nextLevel = levelFor(p.xp);
    if (nextLevel > p.level) {
      p.level = nextLevel;
      p.maxHp += 8;
      p.hp = Math.min(p.maxHp, p.hp + 22);
      system(`${p.name} alcançou o nível ${p.level}!`, p.mapId, p.id);
    }
  }
}
function tick(dt) {
  const now = Date.now();
  for (const p of players.values()) {
    if (!p.alive) {
      if (now >= p.respawnAt) { p.alive = true; p.hp = p.maxHp; p.x = GAME.maps[p.mapId].spawn.x; p.y = GAME.maps[p.mapId].spawn.y; }
      continue;
    }
    if (now - p.lastInputAt > 800) { p.inputX = 0; p.inputY = 0; }
    const speed = GAME.classes[p.classId].speed;
    p.x = Math.max(22, Math.min(GAME.maps[p.mapId].width - 22, p.x + p.inputX * speed * dt));
    p.y = Math.max(22, Math.min(GAME.maps[p.mapId].height - 22, p.y + p.inputY * speed * dt));
    const portal = GAME.maps[p.mapId].portals.find(item => Math.hypot(item.x - p.x, item.y - p.y) < item.radius * 0.65);
    if (portal) changeMap(p, portal);
  }
  for (const [mapId, state] of maps) {
    const def = GAME.maps[mapId];
    for (const mob of state.mobs.values()) {
      if (!mob.alive) {
        if (now >= mob.respawnAt) { mob.alive = true; mob.hp = mob.maxHp; mob.x = mob.homeX; mob.y = mob.homeY; }
        continue;
      }
      const stats = GAME.creatures[mob.kind];
      let target = mob.targetId && players.get(mob.targetId);
      if (!target || target.mapId !== mapId || !target.alive || distance(mob, target) > stats.aggro * 1.5) {
        target = null; mob.targetId = null;
        let best = stats.aggro;
        for (const candidate of players.values()) {
          if (candidate.mapId !== mapId || !candidate.alive) continue;
          const d = distance(mob, candidate);
          if (d < best) { best = d; target = candidate; }
        }
        if (target) mob.targetId = target.id;
      }
      if (target) {
        const d = distance(mob, target);
        if (d > stats.range * 0.8) {
          mob.x += (target.x - mob.x) / (d || 1) * stats.speed * dt;
          mob.y += (target.y - mob.y) / (d || 1) * stats.speed * dt;
        } else if (now >= mob.attackAt) {
          mob.attackAt = now + stats.cooldown;
          target.hp = Math.max(0, target.hp - stats.damage);
          if (target.hp <= 0) { target.alive = false; target.respawnAt = now + 3500; target.hp = 0; system(`${target.name} foi derrotado. Retornará em breve.`, mapId); }
        }
      } else {
        mob.targetId = null;
        if (now > mob.wanderAt) {
          mob.wanderAt = now + 2300 + Math.random() * 3500;
          const a = Math.random() * Math.PI * 2, r = 35 + Math.random() * 110;
          mob.wanderX = Math.max(30, Math.min(def.width - 30, mob.homeX + Math.cos(a) * r));
          mob.wanderY = Math.max(30, Math.min(def.height - 30, mob.homeY + Math.sin(a) * r));
        }
        const d = Math.hypot(mob.wanderX - mob.x, mob.wanderY - mob.y);
        if (d > 4) { mob.x += (mob.wanderX - mob.x) / d * stats.speed * 0.3 * dt; mob.y += (mob.wanderY - mob.y) / d * stats.speed * 0.3 * dt; }
      }
    }
  }
}
function snapshot() {
  const now = Date.now();
  for (const p of players.values()) {
    const nearbyPlayers = [];
    for (const q of players.values()) if (q.mapId === p.mapId) nearbyPlayers.push(publicPlayer(q));
    const mobs = [...maps.get(p.mapId).mobs.values()].map(({ id, kind, name, x, y, hp, maxHp, alive }) => ({ id, kind, name, x, y, hp, maxHp, alive }));
    send(p.ws, { type: 'snapshot', at: now, players: nearbyPlayers, mobs });
  }
}
let previous = Date.now();
setInterval(() => { const now = Date.now(); tick(Math.min(0.1, (now - previous) / 1000)); previous = now; }, 50);
setInterval(snapshot, 100);
server.listen(PORT, '0.0.0.0', () => console.log(`ProjetoRPG online em http://0.0.0.0:${PORT}`));
