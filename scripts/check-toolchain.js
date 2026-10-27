#!/usr/bin/env node
'use strict';

const { spawnSync } = require('node:child_process');

const checks = [
  { name: 'Godot 4 .NET/Mono executable', commands: [['godot', '--version'], ['godot4', '--version']], required: true, accept: output => /(^|\s|v)4\.\d/.test(output) && /mono/i.test(output) },
  { name: '.NET SDK', commands: [['dotnet', '--version']], required: true },
  { name: 'PostgreSQL local (psql)', commands: [['psql', '--version']], required: false },
  { name: 'PostgreSQL local (server)', commands: [['postgres', '--version']], required: false },
  { name: 'Docker (opcional para banco em container)', commands: [['docker', '--version']], required: false }
];

function run(args) {
  const result = spawnSync(args[0], args.slice(1), { encoding: 'utf8', timeout: 10000, windowsHide: true });
  if (result.error || result.status !== 0) return null;
  return (result.stdout || result.stderr || '').trim().split(/\r?\n/, 1)[0];
}

console.log(`Node.js ${process.version} | npm ${process.env.npm_config_user_agent?.match(/npm\/(\S+)/)?.[1] || 'use npm --version'}`);
const nodeOk = Number(process.versions.node.split('.')[0]) === 22 &&
  (Number(process.versions.node.split('.')[1]) > 22 ||
   (Number(process.versions.node.split('.')[1]) === 22 && Number(process.versions.node.split('.')[2]) >= 3));
if (!nodeOk) console.log('  ERRO: o protótipo exige Node >=22.22.3 <23 (veja .nvmrc).');

let missingRequired = 0;
for (const check of checks) {
  let version = null;
  for (const args of check.commands) {
    const candidate = run(args);
    if (candidate && (!check.accept || check.accept(candidate))) { version = candidate; break; }
  }
  const state = version ? `OK — ${version}` : (check.required ? 'AUSENTE (bloqueia validação da stack alvo)' : 'não instalado');
  console.log(`${check.required ? '[obrigatório]' : '[opcional]'} ${check.name}: ${state}`);
  if (!version && check.required) missingRequired++;
}

const runtimeResult = spawnSync('dotnet', ['--list-runtimes'], { encoding: 'utf8', timeout: 10000, windowsHide: true });
const hasNet10Runtime = !runtimeResult.error && runtimeResult.status === 0 && /Microsoft\.NETCore\.App\s+10\./.test(runtimeResult.stdout);
console.log(`[obrigatório para servidor] .NET 10 runtime: ${hasNet10Runtime ? 'OK' : 'AUSENTE'}`);
if (!hasNet10Runtime) missingRequired++;

if (!nodeOk || missingRequired) {
  console.error('\nToolchain incompleta. Nenhum build Godot/.NET foi executado por este diagnóstico.');
  process.exitCode = 1;
} else {
  console.log('\nToolchains detectadas. Ainda execute builds/testes reais e verifique compatibilidade Godot .NET antes de concluir a Fase 0.');
}
