import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';

const client = readFileSync(new URL('../client/Scripts/AccountGateway.cs', import.meta.url), 'utf8');
const lobby = readFileSync(new URL('../client/Scripts/CharacterLobby.cs', import.meta.url), 'utf8');
const summary = readFileSync(new URL('../client/Scripts/CharacterSummary.cs', import.meta.url), 'utf8');
const scene = readFileSync(new URL('../client/Scenes/CharacterLobby.tscn', import.meta.url), 'utf8');
const environment = readFileSync(new URL('../client/Scenes/CharacterLobbyEnvironment.tres', import.meta.url), 'utf8');
const endpoints = readFileSync(new URL('../server/Accounts/AccountEndpoints.cs', import.meta.url), 'utf8');
const worldEndpoints = readFileSync(new URL('../server/World/WorldEndpoints.cs', import.meta.url), 'utf8');
const serverProgram = readFileSync(new URL('../server/Program.cs', import.meta.url), 'utf8');
const bootstrap = readFileSync(new URL('../client/Scripts/Bootstrap.cs', import.meta.url), 'utf8');
const project = readFileSync(new URL('../client/project.godot', import.meta.url), 'utf8');

test('the dedicated lobby is reached after the real session response while Bootstrap remains unchanged', () => {
  assert.match(project, /run\/main_scene="res:\/\/Scenes\/Bootstrap\.tscn"/);
  assert.match(bootstrap, /new AccountGateway\(\)/);
  assert.match(client, /response\.StatusCode != \(long\)HttpStatusCode\.Created/);
  assert.match(client, /OpenLobby\(\)/);
  assert.match(client, /ResourceLoader\.Load<PackedScene>\("res:\/\/Scenes\/CharacterLobby\.tscn"\)/);
  assert.match(scene, /CharacterLobby\.cs/);
});

test('a missing or invalid lobby scene clears local session before exposing the authentication panel', () => {
  const login = client.slice(client.indexOf('private async void OnLoginPressed'), client.indexOf('private void OpenLobby'));
  assert.match(login, /try\s*\{\s*OpenLobby\(\)/);
  assert.match(login, /catch \(Exception exception\) when \(exception is InvalidOperationException or InvalidCastException\)/);
  const failedOpen = login.slice(login.indexOf('catch ('), login.indexOf('_authenticationPanel.Visible = false'));
  assert.match(failedOpen, /ClearLocalSession\(\)/);
  assert.ok(failedOpen.indexOf('ClearLocalSession()') < failedOpen.indexOf('return;'));
  assert.ok(login.indexOf('OpenLobby()') < login.indexOf('_authenticationPanel.Visible = false'));
});

test('the lobby DTO contains only fields actually returned by the character contract', () => {
  assert.match(summary, /CharacterSummary\(string CharacterId, string Name, string\? ClassId\)/);
  assert.doesNotMatch(summary, /Level|Attribute|Strength|Dexterity|Appearance/i);
  assert.match(endpoints, /MapGet\("\/api\/v1\/characters"/);
  assert.match(endpoints, /MapPost\("\/api\/v1\/characters"/);
  assert.match(client, /JsonSerializer\.Deserialize<List<CharacterSummary>>/);
  assert.match(client, /seenCharacterIds\.Add\(id\)/);
  assert.match(client, /IsValidCharacterName/);
  assert.match(client, /IsValidClassId/);
});

test('character creation uses the real endpoint, confirms 201, and refreshes only after server confirmation', () => {
  const create = client.slice(client.indexOf('private async void OnCreateRequested'), client.indexOf('private async void OnLogoutRequested'));
  assert.match(create, /"\/api\/v1\/characters"/);
  assert.match(create, /classId = \(string\?\)null/);
  assert.match(create, /HttpStatusCode\.Created/);
  assert.match(create, /TryReadCharacter\(/);
  assert.match(create, /await RefreshCharactersAsync\(\)/);
  assert.match(create, /_preferredCharacterId = createdCharacter\.CharacterId/);
  assert.ok(create.indexOf('HttpStatusCode.Created') < create.indexOf('TryReadCharacter('));
  assert.ok(create.indexOf('TryReadCharacter(') < create.indexOf('_preferredCharacterId = createdCharacter.CharacterId'));
  assert.ok(create.indexOf('_preferredCharacterId = createdCharacter.CharacterId') < create.indexOf('await RefreshCharactersAsync()'));
});

test('the account lobby creates one isolated idle 3D preview card for every server-returned character', () => {
  assert.match(lobby, /foreach \(var character in characters\)\s*AddCharacterCard\(character\)/);
  assert.match(lobby, /new GridContainer/);
  assert.match(lobby, /new SubViewportContainer/);
  assert.match(lobby, /RenderTargetUpdateMode = SubViewport\.UpdateMode\.WhenVisible/);
  assert.match(lobby, /CreateCharacterViewport\(CharacterSummary character\)/);
  assert.match(lobby, /actor\.PlayerControlled = false/);
  assert.match(lobby, /actor\.ModelScene = model/);
  assert.match(lobby, /ResolvePreviewModel\(character\.ClassId\)/);
  assert.match(lobby, /RefreshCardSelection\(\)/);
  assert.match(lobby, /BorderColor = selected \?/);
  assert.match(lobby, /PRÉVIA GENÉRICA|prévia genérica/);
  assert.match(lobby, /A figura é uma prévia genérica, não aparência persistida/);
  assert.match(environment, /background_color/);
  assert.match(readFileSync(new URL('../client/Scripts/CharacterActor.cs', import.meta.url), 'utf8'), /PlayAnimation\("Idle"\)/);
  assert.doesNotMatch(lobby, /CharacterDemo\.tscn/);
});

test('loading, empty, failure, retry, logout, and account switching states are represented', () => {
  for (const text of ['SetLoading(bool loading)', 'SetLoadFailure(string message)', 'SetStatus(string message, bool isError = false)', 'RefreshRequested', 'CreateRequested', 'LogoutRequested', 'EnterWorldRequested'])
    assert.ok(lobby.includes(text), `missing lobby state/hook: ${text}`);
  assert.match(lobby, /Esta conta ainda não tem personagens/);
  assert.match(client, /Falha de conexão ao carregar personagens/);
  assert.match(client, /A sessão expirou ou foi revogada pelo servidor/);
  const refresh = client.slice(client.indexOf('private async Task RefreshCharactersAsync'), client.indexOf('private void ExpireSession'));
  assert.match(refresh, /catch \(Exception exception\)/);
  assert.match(refresh, /SetLoading\(false\)/);
  assert.match(refresh, /SetLoadFailure\(/);
  assert.match(client, /ClearLocalSession\(\)/);
  assert.match(client, /HttpClient\.Method\.Delete, "\/api\/v1\/sessions\/current"/);
  const clearSession = client.slice(client.indexOf('private void ClearLocalSession'), client.indexOf('private async Task RunBusyAsync'));
  assert.ok(clearSession.indexOf('_accessToken = null') < clearSession.indexOf('_lobby.Visible = false'));
  assert.ok(clearSession.indexOf('_lobby.Visible = false') < clearSession.indexOf('_lobby.QueueFree()'));
  assert.match(clearSession, /_username\.Clear\(\)/);
  assert.match(clearSession, /_password\.Clear\(\)/);
  assert.match(clearSession, /_authenticationPanel\.Visible = true/);
});

test('logout clears local account state even when remote revocation throws', () => {
  const logout = client.slice(client.indexOf('private async Task LogoutAsync'), client.indexOf('private async Task RefreshCharactersAsync'));
  assert.match(logout, /HttpClient\.Method\.Delete, "\/api\/v1\/sessions\/current"/);
  assert.match(logout, /catch \(Exception exception\)/);
  assert.ok(logout.indexOf('catch (Exception exception)') < logout.indexOf('ClearLocalSession()'));
  assert.ok(logout.indexOf('ClearLocalSession()') < logout.indexOf('SetStatus(serverRevoked'));
});

test('world entry is gated by a real selected character and backed by authenticated server routes', () => {
  assert.match(lobby, /Entrar no mundo/);
  assert.match(lobby, /_enterWorldButton\.Disabled = _busy \|\| !CanEnterWorld\(\)/);
  assert.match(lobby, /characters\.Any\(character => string\.Equals\(character\.CharacterId, selectedId/);
  assert.match(lobby, /EnterWorldRequested\?\.Invoke/);
  assert.match(client, /TryReadWorldSnapshot/);
  assert.match(worldEndpoints, /MapPost\("\/api\/v1\/world\/sessions"/);
  assert.match(worldEndpoints, /MapDelete\("\/api\/v1\/world\/sessions\//);
  assert.match(serverProgram, /app\.MapWorldEndpoints\(\)/);
  assert.doesNotMatch(endpoints, /Map(?:Post|Get|Put|Patch)\("\/api\/v1\/characters\/[^\"]+\/enter/);
});
