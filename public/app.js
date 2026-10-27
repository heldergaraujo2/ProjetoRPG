const canvas = document.querySelector('#world');
const ctx = canvas.getContext('2d', { alpha: false, desynchronized: true });
const $ = (selector) => document.querySelector(selector);
const ui = {
  login: $('#login'), hud: $('#hud'), name: $('#player-name'), classes: $('#class-options'), play: $('#play-button'),
  status: $('#connection-status'), region: $('#region-name'), subtitle: $('#region-subtitle'), online: $('#online-count'),
  hp: $('#hp-bar'), hpNumbers: $('#hp-numbers'), xp: $('#xp-bar'), xpNumbers: $('#xp-numbers'),
  playerName: $('#hud-player-name'), playerClass: $('#hud-class'), avatar: $('#avatar-gem'), target: $('#target-card'),
  targetName: $('#target-name'), targetType: $('#target-type'), targetHp: $('#target-hp'), targetIcon: $('#target-icon'),
  notices: $('#notice-stack'), feed: $('#chat-feed'), chatForm: $('#chat-form'), chat: $('#chat-input'),
  dialogue: $('#dialogue'), dialogueName: $('#dialogue-name'), dialogueText: $('#dialogue-text'),
  help: $('#help-popover')
};
const symbols = { guardiao: '⬟', cacador: '➤', arcanista: '✦' };
const data = await fetch('/data/game-data.json').then(r => r.json());
const meshManifest = await fetch('/assets/mesh-manifest.json').then(r => r.json());
let socket = null, playerId = null, currentMap = null, selectedClass = 'guardiao', selectedTarget = null;
let localPlayer = null, entities = new Map(), mobState = new Map(), keys = new Set(), camera = { x: 0, y: 0 };
let lastFrame = performance.now(), sendInputAt = 0, attackPressed = false, interactPressed = false, chatFocused = false;
let dpr = 1, viewWidth = innerWidth, viewHeight = innerHeight, shakeUntil = 0, renderFrame = 0;
const renderActors = new Map();

function setCanvasSize() {
  dpr = Math.min(window.devicePixelRatio || 1, 1.5);
  viewWidth = innerWidth; viewHeight = innerHeight;
  canvas.width = Math.floor(viewWidth * dpr); canvas.height = Math.floor(viewHeight * dpr);
  ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
}
window.addEventListener('resize', setCanvasSize);
setCanvasSize();

function renderClassChoices() {
  ui.classes.innerHTML = Object.entries(data.classes).map(([id, cls]) => `
    <button class="class-option ${id === selectedClass ? 'selected' : ''}" data-class="${id}" type="button">
      <span class="class-symbol" style="color:${cls.color}">${symbols[id]}</span>
      <span class="class-name">${cls.name}<small>${cls.role}</small></span>
      <span class="class-desc">${cls.description}</span>
      <span class="class-stats"><span>❤ ${cls.hp} vida</span><span>⚔ ${cls.damage} dano</span><span>◉ ${cls.range} alcance</span></span>
    </button>`).join('');
  ui.classes.querySelectorAll('[data-class]').forEach(button => button.addEventListener('click', () => {
    selectedClass = button.dataset.class;
    renderClassChoices();
  }));
}
renderClassChoices();

function connect() {
  if (socket && socket.readyState <= WebSocket.OPEN) socket.close();
  ui.play.disabled = true; ui.play.textContent = 'CONECTANDO…'; ui.status.classList.add('hidden');
  const protocol = location.protocol === 'https:' ? 'wss:' : 'ws:';
  socket = new WebSocket(`${protocol}//${location.host}`);
  socket.addEventListener('open', () => socket.send(JSON.stringify({ type: 'join', name: ui.name.value, classId: selectedClass })));
  socket.addEventListener('message', event => {
    let msg; try { msg = JSON.parse(event.data); } catch { return; }
    handleMessage(msg);
  });
  socket.addEventListener('close', () => {
    if (playerId) { ui.status.classList.remove('hidden'); setTimeout(connect, 1800); }
    else { ui.play.disabled = false; ui.play.innerHTML = 'TENTAR NOVAMENTE <span>→</span>'; }
  });
  socket.addEventListener('error', () => { if (!playerId) { ui.play.disabled = false; ui.play.innerHTML = 'TENTAR NOVAMENTE <span>→</span>'; } });
}
function handleMessage(msg) {
  if (msg.type === 'welcome') {
    playerId = msg.id;
    initMap(msg.map);
    localPlayer = { ...msg.player };
    entities.set(playerId, { ...msg.player, targetX: msg.player.x, targetY: msg.player.y });
    ui.login.classList.add('hidden'); ui.hud.classList.remove('hidden'); ui.status.classList.add('hidden');
    ui.playerName.textContent = msg.player.name; updateHud(msg.player);
    pushChat('notice', 'Sistema', `Você chegou a ${currentMap.name}.`);
    return;
  }
  if (msg.type === 'map') {
    initMap(msg.map); selectedTarget = null; ui.target.classList.add('hidden');
    camera.x = localPlayer?.x || 0; camera.y = localPlayer?.y || 0;
    pushChat('notice', 'Sistema', `Nova região: ${currentMap.name}.`);
    return;
  }
  if (msg.type === 'snapshot') {
    const activeIds = new Set();
    for (const p of msg.players) {
      activeIds.add(p.id);
      const existing = entities.get(p.id);
      if (!existing) entities.set(p.id, { ...p, targetX: p.x, targetY: p.y });
      else Object.assign(existing, { ...p, targetX: p.x, targetY: p.y });
      if (p.id === playerId) { localPlayer = { ...p }; updateHud(p); }
    }
    for (const id of entities.keys()) if (!activeIds.has(id)) entities.delete(id);
    mobState.clear();
    for (const mob of msg.mobs) mobState.set(mob.id, { ...mob });
    updateOnline(msg.players.length);
    if (selectedTarget && mobState.has(selectedTarget)) updateTarget(mobState.get(selectedTarget));
    else if (selectedTarget && !mobState.has(selectedTarget)) { selectedTarget = null; ui.target.classList.add('hidden'); }
    return;
  }
  if (msg.type === 'notice') { pushChat('notice', 'Mundo', msg.message); return; }
  if (msg.type === 'chat') { pushChat('', msg.name, msg.text); return; }
  if (msg.type === 'dialogue') { showDialogue(msg.name, msg.text); return; }
}
function initMap(map) {
  currentMap = map;
  ui.region.textContent = map.name; ui.subtitle.textContent = map.subtitle;
  mobState.clear(); for (const mob of map.mobs || []) mobState.set(mob.id, mob);
}
function updateOnline(count) { ui.online.textContent = String(count); }
function updateHud(player) {
  if (!player) return;
  const cls = data.classes[player.classId];
  const hp = Math.max(0, player.hp), hpMax = player.maxHp || cls.hp;
  ui.hp.style.width = `${Math.max(0, hp / hpMax * 100)}%`;
  ui.hpNumbers.textContent = `${hp} / ${hpMax}`;
  const nextXp = Math.pow(player.level, 2) * 100;
  const previousXp = Math.pow(player.level - 1, 2) * 100;
  ui.xp.style.width = `${Math.max(0, Math.min(100, (player.xp - previousXp) / Math.max(1, nextXp - previousXp) * 100))}%`;
  ui.xpNumbers.textContent = `${player.xp} / ${nextXp} XP`;
  ui.playerClass.textContent = `${cls.name} · Nv. ${player.level}`;
  ui.avatar.textContent = symbols[player.classId]; ui.avatar.style.color = cls.color; ui.avatar.style.borderColor = `${cls.color}66`;
  ui.avatar.style.background = `${cls.color}12`;
}
function pushChat(kind, name, text) {
  const line = document.createElement('div'); line.className = `chat-line ${kind}`;
  const strong = document.createElement('b'); strong.textContent = `${name}:`;
  const span = document.createElement('span'); span.textContent = ` ${text}`;
  line.append(strong, span); ui.feed.appendChild(line);
  while (ui.feed.children.length > 7) ui.feed.firstElementChild.remove();
  setTimeout(() => line.remove(), 14000);
}
function notify(text) {
  const node = document.createElement('div'); node.className = 'notice'; node.textContent = text;
  ui.notices.appendChild(node); setTimeout(() => node.remove(), 3300);
}
function showDialogue(name, text) {
  ui.dialogueName.textContent = name; ui.dialogueText.textContent = text;
  ui.dialogue.classList.remove('hidden');
}
function send(data) { if (socket?.readyState === WebSocket.OPEN) socket.send(JSON.stringify(data)); }
function updateTarget(mob) {
  const template = data.creatures[mob.kind];
  if (!template) return;
  ui.target.classList.remove('hidden'); ui.targetName.textContent = mob.name;
  ui.targetType.textContent = `${template.name} · criatura selvagem`;
  ui.targetIcon.textContent = mob.kind === 'wisp' ? '✦' : mob.kind === 'golem' ? '⬢' : '●';
  ui.targetIcon.style.color = template.color;
  ui.targetHp.style.width = `${Math.max(0, mob.hp / mob.maxHp * 100)}%`;
}
function playerWorld() { return localPlayer || entities.get(playerId); }
function screenToWorld(sx, sy) { return { x: sx + camera.x - viewWidth / 2, y: sy + camera.y - viewHeight / 2 }; }
function closestEntity(world, list, maxDistance = 80) {
  let best = null, bestD = maxDistance;
  for (const item of list) { const d = Math.hypot(item.x - world.x, item.y - world.y); if (d < bestD) { bestD = d; best = item; } }
  return best;
}
canvas.addEventListener('pointerdown', event => {
  if (!currentMap || ui.login && !ui.login.classList.contains('hidden')) return;
  const world = screenToWorld(event.clientX, event.clientY);
  const aliveMobs = [...mobState.values()].filter(m => m.alive);
  const mob = closestEntity(world, aliveMobs, 52);
  if (mob) { selectedTarget = mob.id; updateTarget(mob); return; }
  const npc = closestEntity(world, currentMap.npcs || [], 48);
  if (npc) { selectedTarget = npc.id; notify(`${npc.name} — pressione E para interagir`); return; }
  const portal = closestEntity(world, currentMap.portals || [], 60);
  if (portal) { selectedTarget = portal.id; notify(`${portal.name} — aproxime-se para viajar`); return; }
  selectedTarget = null; ui.target.classList.add('hidden');
});

function interact() {
  if (!currentMap || !localPlayer) return;
  const all = [...(currentMap.npcs || []), ...(currentMap.portals || [])];
  const nearby = closestEntity(localPlayer, all, 130);
  if (nearby) { send({ type: 'interact', targetId: nearby.id }); return; }
  if (selectedTarget) send({ type: 'interact', targetId: selectedTarget });
  else notify('Não há ninguém por perto para interagir.');
}
function attack() {
  if (!currentMap || !localPlayer || !localPlayer.alive) return;
  let mob = selectedTarget && mobState.get(selectedTarget);
  if (!mob?.alive) mob = closestEntity(localPlayer, [...mobState.values()].filter(m => m.alive), data.classes[selectedClass].range + 30);
  if (!mob) { notify('Nenhuma criatura ao alcance.'); return; }
  selectedTarget = mob.id; updateTarget(mob);
  const cls = data.classes[selectedClass];
  const d = Math.hypot(mob.x - localPlayer.x, mob.y - localPlayer.y);
  if (d > cls.range + 30) { notify('Aproxime-se da criatura para atacar.'); return; }
  send({ type: 'attack', targetId: mob.id });
  shakeUntil = performance.now() + 90;
}
function keyDown(event) {
  if (event.code === 'Enter' && document.activeElement !== ui.chat) {
    event.preventDefault(); ui.chat.focus(); chatFocused = true; return;
  }
  if (document.activeElement === ui.chat) {
    if (event.code === 'Escape') { ui.chat.blur(); chatFocused = false; }
    return;
  }
  if (['ArrowUp','ArrowDown','ArrowLeft','ArrowRight','Space'].includes(event.code)) event.preventDefault();
  keys.add(event.code);
  if (event.code === 'Space' && !event.repeat) attackPressed = true;
  if (event.code === 'KeyE' && !event.repeat) interactPressed = true;
  if (event.code === 'Escape') ui.dialogue.classList.add('hidden');
}
window.addEventListener('keydown', keyDown);
window.addEventListener('keyup', e => keys.delete(e.code));
window.addEventListener('blur', () => keys.clear());
ui.play.addEventListener('click', connect);
ui.name.addEventListener('keydown', event => { if (event.key === 'Enter') connect(); });
ui.chatForm.addEventListener('submit', event => {
  event.preventDefault(); const text = ui.chat.value.trim();
  if (text) send({ type: 'chat', text }); ui.chat.value = ''; ui.chat.blur(); chatFocused = false;
});
ui.chat.addEventListener('focus', () => { chatFocused = true; });
ui.chat.addEventListener('blur', () => { chatFocused = false; });
$('#clear-target').addEventListener('click', () => { selectedTarget = null; ui.target.classList.add('hidden'); });
$('#dialogue-close').addEventListener('click', () => ui.dialogue.classList.add('hidden'));
$('#menu-button').addEventListener('click', () => ui.help.classList.toggle('hidden'));
$('#help-close').addEventListener('click', () => ui.help.classList.add('hidden'));

function hash(x, y, seed) {
  let n = Math.imul(x ^ seed, 0x45d9f3b); n = Math.imul(n ^ (n >>> 16), 0x45d9f3b);
  n = Math.imul(n ^ y, 0x45d9f3b); return ((n ^ (n >>> 16)) >>> 0) / 4294967295;
}
function drawGround() {
  const theme = currentMap.theme;
  const base = theme === 'ember' ? '#443c35' : '#718a59';
  const minor = theme === 'ember' ? '#65564a' : '#819967';
  ctx.fillStyle = base; ctx.fillRect(0, 0, viewWidth, viewHeight);
  const step = 82;
  const startX = Math.floor((camera.x - viewWidth / 2) / step) - 1;
  const endX = Math.ceil((camera.x + viewWidth / 2) / step) + 1;
  const startY = Math.floor((camera.y - viewHeight / 2) / step) - 1;
  const endY = Math.ceil((camera.y + viewHeight / 2) / step) + 1;
  const sx = Math.floor(camera.x - viewWidth / 2), sy = Math.floor(camera.y - viewHeight / 2);
  for (let gx = startX; gx <= endX; gx++) for (let gy = startY; gy <= endY; gy++) {
    const value = hash(gx, gy, theme === 'ember' ? 713 : 113);
    const x = gx * step - sx, y = gy * step - sy;
    if (value > .46) {
      ctx.fillStyle = value > .82 ? (theme === 'ember' ? '#665547' : '#8da16b') : minor;
      ctx.globalAlpha = value > .82 ? .22 : .12;
      ctx.beginPath(); ctx.ellipse(x + hash(gx, gy, 7) * step, y + hash(gy, gx, 9) * step, 14 + value * 16, 5 + value * 5, value * 3, 0, Math.PI * 2); ctx.fill();
    }
    if (value < .24 && theme !== 'ember') {
      ctx.globalAlpha = .22; ctx.strokeStyle = '#d4d69c'; ctx.lineWidth = 1;
      const ox = x + hash(gx, gy, 3) * step, oy = y + hash(gy, gx, 2) * step;
      ctx.beginPath(); ctx.moveTo(ox, oy); ctx.lineTo(ox + 3, oy - 5); ctx.moveTo(ox + 3, oy); ctx.lineTo(ox + 7, oy - 4); ctx.stroke();
    }
  }
  ctx.globalAlpha = 1;
  drawLandmarks();
}
function drawLandmarks() {
  if (!currentMap) return;
  for (const landmark of currentMap.landmarks || []) {
    const sx = landmark.x - camera.x + viewWidth / 2, sy = landmark.y - camera.y + viewHeight / 2;
    if (sx < -500 || sx > viewWidth + 500 || sy < -500 || sy > viewHeight + 500) continue;
    if (landmark.type === 'lake' || landmark.type === 'lava') {
      ctx.fillStyle = landmark.color; ctx.globalAlpha = .82;
      ctx.beginPath(); ctx.ellipse(sx, sy, landmark.w / 2, landmark.h / 2, -.2, 0, Math.PI * 2); ctx.fill();
      ctx.globalAlpha = .35; ctx.strokeStyle = landmark.type === 'lava' ? '#ffb56a' : '#b9e1c6'; ctx.lineWidth = 5; ctx.stroke(); ctx.globalAlpha = 1;
      continue;
    }
    const count = landmark.count || 1;
    for (let i = 0; i < count; i++) {
      const a = i * 2.39996, r = 24 + Math.sqrt(i) * 27;
      const x = sx + Math.cos(a) * r, y = sy + Math.sin(a) * r * .68;
      if (x < -50 || x > viewWidth + 50 || y < -50 || y > viewHeight + 50) continue;
      if (landmark.type === 'grove') drawTree(x, y, i);
      else drawRuin(x, y, i, currentMap.theme === 'ember');
    }
  }
}
function drawTree(x, y, index) {
  const radius = 14 + (hash(index, Math.floor(x), 33) * 10);
  ctx.fillStyle = 'rgba(14,28,19,.22)'; ctx.beginPath(); ctx.ellipse(x + 5, y + 9, radius * 1.15, radius * .58, 0, 0, Math.PI * 2); ctx.fill();
  ctx.fillStyle = '#5b4934'; ctx.fillRect(x - 3, y - 3, 6, 17);
  ctx.fillStyle = index % 3 === 0 ? '#45684b' : '#52764c'; ctx.beginPath(); ctx.arc(x, y - 11, radius, 0, Math.PI * 2); ctx.fill();
  ctx.fillStyle = '#75935a'; ctx.globalAlpha = .65; ctx.beginPath(); ctx.arc(x - radius * .3, y - 14, radius * .54, 0, Math.PI * 2); ctx.fill(); ctx.globalAlpha = 1;
}
function drawRuin(x, y, index, dark) {
  ctx.fillStyle = 'rgba(0,0,0,.18)'; ctx.fillRect(x - 15, y + 4, 34, 23);
  ctx.fillStyle = dark ? '#554c46' : '#837d68';
  ctx.save(); ctx.translate(x, y); ctx.rotate((index % 2 ? 1 : -1) * .07);
  ctx.fillRect(-17, -22, 29, 25); ctx.fillRect(-14, -29, 8, 12); ctx.fillRect(0, -34, 9, 17);
  ctx.fillStyle = dark ? '#77604d' : '#aaa083'; ctx.fillRect(-12, -18, 16, 3); ctx.restore();
}
function drawPortal(portal, time) {
  const x = portal.x - camera.x + viewWidth / 2, y = portal.y - camera.y + viewHeight / 2;
  if (x < -100 || x > viewWidth + 100 || y < -100 || y > viewHeight + 100) return;
  const pulse = Math.sin(time / 380) * 3;
  ctx.save(); ctx.translate(x, y);
  ctx.globalAlpha = .16; ctx.fillStyle = '#eac877'; ctx.beginPath(); ctx.arc(0, 0, 48 + pulse, 0, Math.PI * 2); ctx.fill(); ctx.globalAlpha = .8;
  ctx.strokeStyle = '#f0cf84'; ctx.lineWidth = 3; ctx.beginPath(); ctx.ellipse(0, 0, 25 + pulse * .3, 39 + pulse, 0, 0, Math.PI * 2); ctx.stroke();
  ctx.globalAlpha = .5; ctx.fillStyle = '#dbbd76'; ctx.beginPath(); ctx.ellipse(0, 0, 16, 29, 0, 0, Math.PI * 2); ctx.fill(); ctx.restore();
  drawLabel(portal.name, x, y - 47, '#f0d38f');
}
function drawLabel(text, x, y, color = '#edf0e7') {
  ctx.font = '11px DM Sans, sans-serif'; const width = ctx.measureText(text).width + 14;
  ctx.fillStyle = 'rgba(12,18,14,.75)'; ctx.beginPath(); ctx.roundRect(x - width / 2, y - 12, width, 20, 5); ctx.fill();
  ctx.fillStyle = color; ctx.textAlign = 'center'; ctx.fillText(text, x, y + 2);
}
function drawNpc(npc, time) {
  const x = npc.x - camera.x + viewWidth / 2, y = npc.y - camera.y + viewHeight / 2;
  if (x < -70 || x > viewWidth + 70 || y < -70 || y > viewHeight + 70) return;
  const color = npc.color || '#e4c17d';
  ctx.fillStyle = 'rgba(0,0,0,.25)'; ctx.beginPath(); ctx.ellipse(x, y + 15, 12, 6, 0, 0, Math.PI * 2); ctx.fill();
  ctx.fillStyle = color; ctx.beginPath(); ctx.moveTo(x, y - 20); ctx.lineTo(x + 13, y - 5); ctx.lineTo(x + 9, y + 12); ctx.lineTo(x - 9, y + 12); ctx.lineTo(x - 13, y - 5); ctx.closePath(); ctx.fill();
  ctx.fillStyle = '#f0d6a5'; ctx.beginPath(); ctx.arc(x, y - 20, 7, 0, Math.PI * 2); ctx.fill();
  ctx.fillStyle = '#f5d37e'; ctx.globalAlpha = .75 + Math.sin(time / 330) * .2; ctx.font = '14px sans-serif'; ctx.textAlign = 'center'; ctx.fillText('✦', x, y - 39); ctx.globalAlpha = 1;
  drawLabel(npc.name, x, y - 52);
}
function actorOnScreen(actor) {
  const x = actor.x - camera.x + viewWidth / 2, y = actor.y - camera.y + viewHeight / 2;
  return { x, y };
}
function drawMob(mob, time) {
  if (!mob.alive) return;
  const template = data.creatures[mob.kind]; if (!template) return;
  const pos = actorOnScreen(mob), { x, y } = pos;
  if (x < -70 || x > viewWidth + 70 || y < -70 || y > viewHeight + 70) return;
  const size = mob.kind === 'golem' ? 16 : mob.kind === 'wisp' ? 12 : 13;
  ctx.fillStyle = 'rgba(0,0,0,.26)'; ctx.beginPath(); ctx.ellipse(x, y + 12, size * 1.15, size * .53, 0, 0, Math.PI * 2); ctx.fill();
  ctx.save(); ctx.translate(x, y + Math.sin(time / 280 + mob.x) * 1.7);
  ctx.fillStyle = template.color; ctx.strokeStyle = selectedTarget === mob.id ? '#fff1c9' : `${template.color}cc`; ctx.lineWidth = selectedTarget === mob.id ? 2 : 1;
  if (template.shape === 'circle') { ctx.beginPath(); ctx.arc(0, 0, size, 0, Math.PI * 2); ctx.fill(); ctx.stroke(); ctx.fillStyle = '#d7ffe6'; ctx.beginPath(); ctx.arc(-4, -2, 2, 0, Math.PI * 2); ctx.arc(4, -2, 2, 0, Math.PI * 2); ctx.fill(); }
  else if (template.shape === 'diamond') { ctx.rotate(Math.PI / 4); ctx.fillRect(-size * .72, -size * .72, size * 1.44, size * 1.44); ctx.strokeRect(-size * .72, -size * .72, size * 1.44, size * 1.44); }
  else { ctx.beginPath(); ctx.roundRect(-size, -size, size * 2, size * 2, 4); ctx.fill(); ctx.stroke(); ctx.fillStyle = '#452f27'; ctx.fillRect(-7, -4, 4, 4); ctx.fillRect(3, -4, 4, 4); }
  ctx.restore();
  if (selectedTarget === mob.id || mob.hp < mob.maxHp) {
    ctx.fillStyle = '#151714'; ctx.fillRect(x - 17, y - size - 12, 34, 4);
    ctx.fillStyle = '#d16e69'; ctx.fillRect(x - 17, y - size - 12, 34 * mob.hp / mob.maxHp, 4);
  }
  if (selectedTarget === mob.id) drawLabel(mob.name, x, y - size - 18, '#f4e5cc');
}
function drawPlayer(p, time) {
  if (!p.alive) return;
  const cls = data.classes[p.classId], pos = actorOnScreen(p), { x, y } = pos;
  if (x < -70 || x > viewWidth + 70 || y < -70 || y > viewHeight + 70) return;
  const isLocal = p.id === playerId;
  ctx.fillStyle = 'rgba(0,0,0,.3)'; ctx.beginPath(); ctx.ellipse(x, y + 14, 13, 6, 0, 0, Math.PI * 2); ctx.fill();
  ctx.save(); ctx.translate(x, y); ctx.fillStyle = cls.color; ctx.strokeStyle = isLocal ? '#f2e4b5' : '#1d2922'; ctx.lineWidth = isLocal ? 2 : 1.5;
  if (p.classId === 'guardiao') { ctx.beginPath(); ctx.roundRect(-9, -10, 18, 24, 6); ctx.fill(); ctx.stroke(); ctx.fillStyle = '#e6d5b1'; ctx.beginPath(); ctx.arc(0, -12, 7, 0, Math.PI * 2); ctx.fill(); }
  else if (p.classId === 'cacador') { ctx.beginPath(); ctx.moveTo(0, -23); ctx.lineTo(12, 11); ctx.lineTo(0, 15); ctx.lineTo(-12, 11); ctx.closePath(); ctx.fill(); ctx.stroke(); ctx.fillStyle = '#e9d9b6'; ctx.beginPath(); ctx.arc(0, -13, 6, 0, Math.PI * 2); ctx.fill(); }
  else { ctx.beginPath(); ctx.moveTo(0, -26); ctx.lineTo(12, -13); ctx.lineTo(0, 0); ctx.lineTo(-12, -13); ctx.closePath(); ctx.fill(); ctx.stroke(); ctx.fillStyle = '#eadcc4'; ctx.beginPath(); ctx.arc(0, -14, 6, 0, Math.PI * 2); ctx.fill(); }
  const ax = p.aimX || 1, ay = p.aimY || 0;
  ctx.strokeStyle = cls.color; ctx.globalAlpha = .75; ctx.lineWidth = 2; ctx.beginPath(); ctx.moveTo(ax * 9, ay * 9); ctx.lineTo(ax * 16, ay * 16); ctx.stroke(); ctx.globalAlpha = 1;
  if (isLocal) { ctx.strokeStyle = '#e8d08f66'; ctx.lineWidth = 1; ctx.beginPath(); ctx.ellipse(0, 3, 17, 11, 0, 0, Math.PI * 2); ctx.stroke(); }
  ctx.restore();
  const shortName = isLocal ? p.name : p.name;
  drawLabel(`${shortName} · Nv.${p.level}`, x, y - 38, cls.color);
  if (p.hp < p.maxHp) { ctx.fillStyle = '#111'; ctx.fillRect(x - 15, y + 19, 30, 3); ctx.fillStyle = '#73c77a'; ctx.fillRect(x - 15, y + 19, 30 * p.hp / p.maxHp, 3); }
}
function drawEmptyBackground() {
  ctx.fillStyle = '#1b2a21'; ctx.fillRect(0, 0, viewWidth, viewHeight);
  for (let i = 0; i < 25; i++) {
    const x = (i * 173 + renderFrame * .08) % (viewWidth + 50), y = (i * 277) % viewHeight;
    ctx.globalAlpha = .2; ctx.fillStyle = '#bbce92'; ctx.beginPath(); ctx.arc(x, y, 1.5 + i % 3, 0, Math.PI * 2); ctx.fill();
  }
  ctx.globalAlpha = 1;
}
function frame(now) {
  requestAnimationFrame(frame); renderFrame++;
  const dt = Math.min(.05, Math.max(0, (now - lastFrame) / 1000)); lastFrame = now;
  if (!currentMap || !localPlayer) { drawEmptyBackground(); return; }
  const moveX = (keys.has('KeyD') || keys.has('ArrowRight') ? 1 : 0) - (keys.has('KeyA') || keys.has('ArrowLeft') ? 1 : 0);
  const moveY = (keys.has('KeyS') || keys.has('ArrowDown') ? 1 : 0) - (keys.has('KeyW') || keys.has('ArrowUp') ? 1 : 0);
  const len = Math.hypot(moveX, moveY) || 1;
  if (localPlayer.alive) {
    localPlayer.x = Math.max(22, Math.min(currentMap.width - 22, localPlayer.x + moveX / len * data.classes[localPlayer.classId].speed * dt));
    localPlayer.y = Math.max(22, Math.min(currentMap.height - 22, localPlayer.y + moveY / len * data.classes[localPlayer.classId].speed * dt));
  }
  const nowMs = performance.now();
  if (nowMs - sendInputAt > 50) {
    let aimX = 1, aimY = 0;
    const target = selectedTarget && mobState.get(selectedTarget);
    if (target) { const dx = target.x - localPlayer.x, dy = target.y - localPlayer.y, d = Math.hypot(dx, dy) || 1; aimX = dx / d; aimY = dy / d; }
    send({ type: 'input', x: moveX / len * (moveX || moveY ? 1 : 0), y: moveY / len * (moveX || moveY ? 1 : 0), aimX, aimY });
    sendInputAt = nowMs;
  }
  if (attackPressed) { attackPressed = false; attack(); }
  if (interactPressed) { interactPressed = false; interact(); }
  const serverLocal = entities.get(playerId);
  if (serverLocal) {
    const correction = Math.min(1, dt * (Math.hypot(moveX, moveY) ? 2.1 : 6));
    localPlayer.x += (serverLocal.targetX - localPlayer.x) * correction;
    localPlayer.y += (serverLocal.targetY - localPlayer.y) * correction;
  }
  for (const [id, entity] of entities) {
    if (id === playerId) continue;
    entity.x += (entity.targetX - entity.x) * Math.min(1, dt * 10);
    entity.y += (entity.targetY - entity.y) * Math.min(1, dt * 10);
  }
  camera.x += (localPlayer.x - camera.x) * Math.min(1, dt * 7);
  camera.y += (localPlayer.y - camera.y) * Math.min(1, dt * 7);
  ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
  if (nowMs < shakeUntil) { ctx.translate((Math.random() - .5) * 3, (Math.random() - .5) * 3); }
  drawGround();
  for (const portal of currentMap.portals || []) drawPortal(portal, nowMs);
  for (const npc of currentMap.npcs || []) drawNpc(npc, nowMs);
  for (const mob of mobState.values()) drawMob(mob, nowMs);
  for (const [id, p] of entities) if (id !== playerId) drawPlayer(p, nowMs);
  drawPlayer(localPlayer, nowMs);
  if (!localPlayer.alive) drawLabel('Você foi derrotado · retornando…', viewWidth / 2, viewHeight / 2 - 48, '#f0c5a0');
  drawMinimap();
}
function drawMinimap() {
  if (!currentMap) return;
  const width = innerWidth < 700 ? 102 : 136, height = width * currentMap.height / currentMap.width;
  const x = viewWidth - width - 24, y = innerWidth < 700 ? 126 : 91;
  ctx.fillStyle = 'rgba(12,18,14,.75)'; ctx.beginPath(); ctx.roundRect(x, y, width, height, 7); ctx.fill();
  ctx.strokeStyle = '#e4e7d522'; ctx.lineWidth = 1; ctx.stroke();
  const sx = width / currentMap.width, sy = height / currentMap.height;
  for (const portal of currentMap.portals || []) { ctx.fillStyle = '#e9c779'; ctx.beginPath(); ctx.arc(x + portal.x * sx, y + portal.y * sy, 3, 0, Math.PI * 2); ctx.fill(); }
  for (const npc of currentMap.npcs || []) { ctx.fillStyle = '#fff0b6'; ctx.fillRect(x + npc.x * sx - 1.5, y + npc.y * sy - 1.5, 3, 3); }
  for (const mob of mobState.values()) if (mob.alive) { ctx.fillStyle = data.creatures[mob.kind].color; ctx.fillRect(x + mob.x * sx - 1, y + mob.y * sy - 1, 2, 2); }
  for (const p of entities.values()) if (p.id !== playerId) { ctx.fillStyle = '#c1ddff'; ctx.beginPath(); ctx.arc(x + p.x * sx, y + p.y * sy, 2, 0, Math.PI * 2); ctx.fill(); }
  ctx.fillStyle = '#f4e7bc'; ctx.beginPath(); ctx.arc(x + localPlayer.x * sx, y + localPlayer.y * sy, 3.2, 0, Math.PI * 2); ctx.fill();
}
requestAnimationFrame(frame);
