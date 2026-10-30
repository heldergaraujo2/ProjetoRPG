import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';

const client = readFileSync(new URL('../client/Scripts/AccountGateway.cs', import.meta.url), 'utf8');
const lobby = readFileSync(new URL('../client/Scripts/CharacterLobby.cs', import.meta.url), 'utf8');
const transport = readFileSync(new URL('../client/Scripts/AccountApiClient.cs', import.meta.url), 'utf8');
const bootstrap = readFileSync(new URL('../client/Scripts/Bootstrap.cs', import.meta.url), 'utf8');
const project = readFileSync(new URL('../client/project.godot', import.meta.url), 'utf8');
const clientProject = readFileSync(new URL('../client/ProjetoRpg.Client.csproj', import.meta.url), 'utf8');
const endpoints = readFileSync(new URL('../server/Accounts/AccountEndpoints.cs', import.meta.url), 'utf8');
const worldEndpoints = readFileSync(new URL('../server/World/WorldEndpoints.cs', import.meta.url), 'utf8');

test('Bootstrap remains the main scene and starts the real account gateway', () => {
  assert.match(project, /run\/main_scene="res:\/\/Scenes\/Bootstrap\.tscn"/);
  assert.match(bootstrap, /new AccountGateway\(\)/);
  assert.match(bootstrap, /CLIENT_ACCOUNT_FLOW_READY/);
});

test('client account routes match the server endpoints and bearer header contract', () => {
  assert.match(clientProject, /Godot\.NET\.Sdk\/4\.7\.2/);
  assert.match(clientProject, /<TargetFramework>net8\.0<\/TargetFramework>/);
  for (const route of [
    '/api/v1/accounts/register',
    '/api/v1/sessions',
    '/api/v1/characters',
    '/api/v1/sessions/current'
  ]) {
    assert.ok(client.includes(route), `client route missing: ${route}`);
    assert.ok(endpoints.includes(route), `server route missing: ${route}`);
  }

  assert.match(transport, /Authorization: Bearer \{bearerToken\}/);
  assert.match(transport, /uri\.IsLoopback/);
  assert.match(transport, /private readonly HttpRequest _request/);
  assert.match(transport, /Godot\.HttpClient\.Method method/);
  assert.match(client, /HttpClient\.Method\.Post/);
  assert.match(client, /HttpRequest\.Result\.Success/);
  assert.doesNotMatch(client, /\b(?:HTTPRequest|HTTPClient)\b/);
  assert.match(client, /accessToken/);
  assert.match(client, /expiresAt/);
});

test('credentials are transient and world entry is enabled only after a valid selection', () => {
  assert.match(client, /_password\.Secret = true/);
  assert.match(client, /classId = \(string\?\)null/);
  assert.doesNotMatch(client, /FileAccess|ConfigFile|PlayerPrefs|GD\.Print.*(?:password|accessToken|bearer)/i);
  assert.match(lobby, /Entrar no mundo/);
  assert.match(lobby, /EnterWorldRequested/);
  assert.match(lobby, /_enterWorldButton\.Disabled = _busy \|\| !CanEnterWorld\(\)/);
  assert.match(client, /TryReadWorldSnapshot/);
  assert.match(client, /WorldGameplay\.tscn/);
  assert.ok(worldEndpoints.includes('/api/v1/world/sessions'));
});

test('server validation and authentication error codes have UI messages', () => {
  for (const code of ['invalid_credentials', 'authentication_required', 'invalid_character', 'name_unavailable', 'class_unavailable', 'character_not_found', 'world_unavailable']) {
    assert.ok(endpoints.includes(code) || worldEndpoints.includes(code) || client.includes(`"${code}"`), `missing mapped API error: ${code}`);
    assert.ok(client.includes(`"${code}"`), `client does not map API error: ${code}`);
  }
});

test('network errors retain retry paths and character lists can be refreshed', () => {
  assert.match(lobby, /Atualizar lista/);
  assert.match(client, /Não foi possível alcançar o servidor/);
  assert.match(client, /Falha ao consultar a lista/);
  assert.match(client, /SetLoadFailure/);
  assert.match(client, /A sessão expirou ou foi revogada/);
});
