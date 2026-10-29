import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';

const client = readFileSync(new URL('../client/Scripts/AccountGateway.cs', import.meta.url), 'utf8');
const transport = readFileSync(new URL('../client/Scripts/AccountApiClient.cs', import.meta.url), 'utf8');
const bootstrap = readFileSync(new URL('../client/Scripts/Bootstrap.cs', import.meta.url), 'utf8');
const project = readFileSync(new URL('../client/project.godot', import.meta.url), 'utf8');
const endpoints = readFileSync(new URL('../server/Accounts/AccountEndpoints.cs', import.meta.url), 'utf8');

test('Bootstrap remains the main scene and starts the real account gateway', () => {
  assert.match(project, /run\/main_scene="res:\/\/Scenes\/Bootstrap\.tscn"/);
  assert.match(bootstrap, /new AccountGateway\(\)/);
  assert.match(bootstrap, /CLIENT_ACCOUNT_FLOW_READY/);
});

test('client account routes match the server endpoints and bearer header contract', () => {
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
  assert.match(client, /accessToken/);
  assert.match(client, /expiresAt/);
});

test('credentials and bearer token are transient; classless character creation is explicit', () => {
  assert.match(client, /_password\.Secret = true/);
  assert.match(client, /classId = \(string\?\)null/);
  assert.doesNotMatch(client, /FileAccess|ConfigFile|PlayerPrefs|GD\.Print.*(?:password|accessToken|bearer)/i);
  assert.match(client, /nenhum estado de mundo foi criado/);
  assert.match(client, /não simula multiplayer/);
});

test('network errors retain retry paths and character lists can be refreshed', () => {
  assert.match(client, /Atualizar lista/);
  assert.match(client, /Não foi possível alcançar o servidor/);
  assert.match(client, /A sessão expirou ou foi revogada/);
});
