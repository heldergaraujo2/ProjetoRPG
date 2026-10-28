# CHECKPOINT — continuidade do ProjetoRPG

**Atualizado:** 2026-10-28
**Fase oficial atual:** 1 — Fundação do servidor
**Estado:** Fase 0 concluída em 2026-10-28 após builds limpos e testes reais de Shared, servidor e cliente no Windows. Próximo objetivo oficial: Fase 1 — Fundação do servidor, ainda incompleta.
**Branch ativa:** `arena/b2f4e6da-projetorpg` (base original `40e8badd43ca28c17662944824a3c25335e22182`; não trocada). PR #1 está aberta contra `main`.
**Checkpoint oficial importado:** `rpg-pack/CHECKPOINT.md` é histórico; este arquivo na raiz é a única fonte operacional.

## Causa confirmada da divergência

1. A branch desta sessão permaneceu no commit-base `40e8bad`; a primeira referência local de `origin/main` estava em `3941e64`.
2. `git fetch origin main` atualizou **somente a referência** para `origin/main` `9ac6d53ecf58cca77fa05295583123bb7ac0f000`. Não ocorreu checkout, merge, rebase ou mudança de branch.
3. A árvore desse `main` continha o README e pacote oficial `rpg-pack/`, mas não tinha `project.godot`, `.csproj`, `.sln` ou implementação Godot/.NET. A busca por branches remotas também não revelou outra branch publicada com esses projetos.
4. Logo, a causa observável é: o checkout Arena não tinha recebido a atualização documental de `main`, e a referência `main` encontrada também não continha o scaffold que o usuário diz ter testado localmente. **Não há evidência suficiente para determinar em que branch/commit estão os arquivos do PC do usuário ou o que foi exatamente testado lá.** Não se conclui que os testes externos falharam; eles seguem sem logs/versões/artifacts verificáveis nesta sessão.

## Fontes e documentos reconciliados

- `rpg-pack/` foi importado de `origin/main` como fonte documental oficial; `rpg-pack/CHECKPOINT.md` foi marcado como histórico.
- `README.md`, `docs/02_ARQUITETURA.md`, `docs/adr/ADR-001-stack.md`, `docs/WINDOWS_SETUP.md`, `docs/CHANGELOG.md` e este checkpoint foram atualizados para distinguir planejamento, implementação e evidência; o guia Windows contém clone, instalação, restore, execução e testes.
- Stack-alvo: Godot `4.7.2-stable` .NET / `Godot.NET.Sdk/4.7.2` com client/shared `net8.0` (targets oficiais GodotSharp/GodotTools); servidor ASP.NET Core/SDK+TFM `.NET 10.0.402` / `net10.0`; PostgreSQL produção, SQLite desenvolvimento e WebSocket/TCP inicial. Requer Godot Mono e SDK .NET 10.0.402. A documentação oficial Godot 4.7 diz que Godot traz runtime para rodar jogos e exige SDK .NET separado; 4.5+ permite SDK 8 ou posterior. Não é necessário instalar runtime 8 separado; o client `net8.0` requer targeting pack 8 no build (restore primeiro; SDK 8 oficial somente se necessário). O servidor usa runtime 10 do SDK. Integração permanece **sem build real**; fontes na ADR.
- O README, arquitetura, ADR e readmes preexistentes de client/server/infra foram recuperados de blobs Git inacessíveis e preservados em `docs/archive/`. Como existem snapshots próximos diferentes, não é possível garantir que sejam byte a byte a última versão não commitada do working tree; ver `docs/archive/RECOVERY.md`.

## Estado real por componente

### Já existia e foi preservado

- Protótipo cliente HTML/CSS/Canvas 2D em `public/` e servidor Node.js/WebSocket em `server.js`.
- Dados e testes de protótipo em `public/data/` e `test/`; pacote npm com `ws`.
- Referências visuais preexistentes, não convertidas em assets de produção.

### Scaffold criado nesta etapa — builds e smoke tests confirmados

- `global.json`, `ProjetoRpg.sln`, `shared/ProjetoRpg.Shared.csproj`, contratos JSON/versionamento e `tests/Shared.Smoke/`.
- `server/ProjetoRpg.Server.csproj`: ASP.NET Core, endpoints live/tick/content, catálogo JSON validado no startup, loop-clock que conta pulsos a 20 Hz, métricas counter/histogram e WebSocket `Hello`/`HelloAck` + `Ping`/`Pong`, com validação simples de versão e sequence.
- `client/ProjetoRpg.Client.csproj`, `client/project.godot`, cena/script Bootstrap e teste headless. A cena apenas confirma inicialização; não é gameplay/MMORPG.
- `content/manifest.json` e `content/schemas/manifest.schema.json`; `scripts/target-server-smoke.js`, `scripts/target-godot-smoke.js`, `scripts/check-target-stack.sh` e comandos npm separados.
- O catálogo inicial está vazio. O loader atual valida manualmente manifest, IDs únicos/estáveis, existência/path traversal e JSON dos arquivos referenciados; ainda não é validador JSON Schema genérico.

### Não implementado / não comprovado

- Contas, registro/login, hash de senha, sessões autenticadas, personagens, persistência, PostgreSQL/SQLite, migrações e audit log.
- Simulação de mundo: o serviço 20 Hz atual só conta ticks e emite métrica; não atualiza atores/movimento nem prova precisão de deadline.
- Bots/carga 0/100, movimentação autorizada, navegação/pathfinding, colisão, sincronização, combate, loot, inventário, economia, atributos, teleporte, quests ou interações.
- O cliente não implementa input. Mantêm-se os requisitos: clique esquerdo é o único input de movimento; clique direito solicita ataque validado pelo servidor; W/A/S/D/Espaço não são gameplay; I/P/M são atalhos de UI; atributos canônicos Fúria, Defesa, Mana, Agilidade e Sabedoria. Não foram encontrados/introduzidos sistemas paralelos FOR/AGI/VIT/ENE/CMD.

## Matriz oficial da Fase 0

- [x] Ler/importar a especificação oficial e registrar ADR/stack justificada.
- [x] Estruturar diretórios alvo, solução/projetos, content/schema e scripts locais de build/teste.
- [x] Criar runner local `scripts/check-target-stack.sh` para executar verificações em sequência.
- [x] Criar arquivos mínimos de cliente Godot C# e servidor ASP.NET Core.
- [x] Compilar limpo cliente e servidor com versões instaladas e compatíveis.
- [x] Executar smoke da cena Godot e integração HTTP/WebSocket do servidor.
- [x] Conferir os builds reais e registrar as evidências de execução no checkpoint.
- [x] Concluir a Fase 0 após os builds e testes iniciais passarem; Fase 1 permanece incompleta.

## Trabalho seguro iniciado em direção à Fase 1

Implementado apenas o esqueleto de serialização/versionamento, handshake/ping, clock de 20 Hz, métricas básicas e recusa de conteúdo inválido no startup. O código de teste tenta cobrir endpoints, medir a taxa média do clock numa janela de 1 s, ping-pong, versão incompatível e manifest inválido, e foi executado com sucesso no Windows em 2026-10-28, conforme a seção de validação local abaixo. Autenticação/hash, banco, migrations, contas/personagens, catálogos de gameplay, audit log, teste com 100 bots e aceite completo da Fase 1 permanecem pendentes. Isso é progresso parcial, não Fase 1 concluída.

## Testes e comandos — resultados históricos do sandbox Linux (2026-10-27)

Executado a partir da raiz `/home/user/ProjetoRPG`:

- `npm run check` → **passou**, syntax-check dos scripts + 4/4 testes do protótipo Node. Durante a primeira tentativa, `node --test` auto-descobriu scripts chamados `test-*.js`; eles foram renomeados para `target-*-smoke.js` e o comando final passou com 4/4.
- `npm run doctor` → **status 1 esperado**. Reportou Node `v22.22.3` / npm `10.9.8`; Godot Mono, SDK .NET e runtime .NET 10 ausentes; PostgreSQL/Docker ausentes.
- `npm run test:target:shared` → **status 127**, `dotnet: not found`; nenhum teste C# executou.
- `npm run test:target:server` → **status 2**, `TARGET_TEST_BLOCKED`; não houve build nem teste HTTP/WebSocket do novo ASP.NET Core.
- `npm run test:target:client` → **status 2**, `TARGET_TEST_BLOCKED`; Godot Mono não está instalado, não houve import/build/boot da cena.
- `npm run test:target` → **status 127**, a sequência parou ao tentar o teste compartilhado porque `dotnet` não existe.
- `./scripts/check-target-stack.sh` → executou Node check (4/4) e parou com **status 127** no primeiro comando .NET.
- Busca de `godot`, `godot4`, `dotnet`, `psql`, `postgres`, Docker → ausentes. APT sem índices locais.
- Diagnóstico adicional: release Godot GitHub retorna 302 para `objects.githubusercontent.com`/`release-assets.githubusercontent.com` (fora do allowlist), inclusive via API de assets; endpoints Microsoft oficiais `dotnetcli.azureedge.net`, `builds.dotnet.microsoft.com`, `packages.microsoft.com` e o link de instalador oficial `download.microsoft.com` falham `curl` 35 / `SSL_ERROR_SYSCALL`. Nenhum bypass TLS nem fonte não oficial foi usado. Winget não existe neste Linux sandbox; foram consultados manifestos oficiais/WinGet com URL e hashes para fornecer procedimento no Windows.
- Os testes Node validam somente o protótipo, não a stack-alvo.

## Comandos locais reproduzíveis

Instalar os runtimes oficiais compatíveis com os pins acima e as dependências npm; então, na raiz:

```bash
npm ci
npm run check
npm run doctor
dotnet --info
dotnet run --project tests/Shared.Smoke/Shared.Smoke.csproj
npm run test:target:server
npm run test:target:client
npm run test:target
```

O servidor de desenvolvimento pode ser iniciado com `dotnet run --project server/ProjetoRpg.Server.csproj`. Para Godot, abrir projeto `client/` na distribuição 4.7.2 .NET. `npm run doctor` é apenas diagnóstico; `npm run check` só valida Node legado e sintaxe JS.

## Procedimento Windows caso o sandbox continue bloqueado

O guia completo versionado está em [`docs/WINDOWS_SETUP.md`](docs/WINDOWS_SETUP.md). Comandos diretos para clonar a branch publicada e instalar Node, SDK e Godot via WinGet:

```powershell
git clone --branch arena/b2f4e6da-projetorpg --single-branch https://github.com/heldergaraujo2/ProjetoRPG.git ProjetoRPG
Set-Location .\ProjetoRPG
winget install --id OpenJS.NodeJS.22 --exact --version 22.22.3
winget install --id Microsoft.DotNet.SDK.10 --exact --version 10.0.402
winget install --id GodotEngine.GodotEngine.Mono --exact --version 4.7.2
```

A rede permitida neste executor não alcança os hosts binários Microsoft/GitHub Releases. Os manifestos WinGet foram consultados sem baixar binários; eles verificam SHA-256 e apontam para fontes oficiais:

- PowerShell (x64):

```powershell
winget install --id Microsoft.DotNet.SDK.10 --exact --version 10.0.402
winget install --id GodotEngine.GodotEngine.Mono --exact --version 4.7.2
```

O pacote Godot é o Mono x64 oficial; o .NET SDK vem do download Microsoft e inclui runtime 10 para o servidor. Godot 4.7 empacota o runtime necessário ao editor/jogo compilado; **não instale runtime .NET 8 separado**. O cliente/shared `net8.0` podem necessitar do targeting pack 8 durante restore. Tente primeiro o restore do SDK10; somente se o erro reportar targeting pack/ref pack `Microsoft.NETCore.App.Ref` ausente, instale o SDK 8 oficial para fornecer esse targeting pack:

```powershell
winget install --id Microsoft.DotNet.SDK.8 --exact --version 8.0.427
```

Abra um PowerShell novo, confirme versões e, na raiz do repositório:

```powershell
dotnet --info
dotnet --list-sdks
dotnet --list-runtimes
godot --version
npm ci
npm run check
npm run test:target
```

O `global.json` exige SDK `10.0.402` ou patch compatível da banda `10.0.4xx`; `godot --version` deve mostrar 4.7.2 e `mono`. Se WinGet não estiver instalado, utilize as URLs/checksums oficiais nos manifestos [Microsoft .NET SDK 10.0.402](https://github.com/microsoft/winget-pkgs/blob/master/manifests/m/Microsoft/DotNet/SDK/10/10.0.402/Microsoft.DotNet.SDK.10.installer.yaml) e [Godot Mono 4.7.2](https://github.com/microsoft/winget-pkgs/blob/master/manifests/g/GodotEngine/GodotEngine/Mono/4.7.2/GodotEngine.GodotEngine.Mono.installer.yaml); mantenha a verificação de hash/TLS habilitada.

## Próximo passo exato

Os builds limpos de Shared, Server e Client e os testes de destino foram executados com sucesso no Windows em 2026-10-28. Não é necessário reinstalar as toolchains nem repetir os testes iniciais já aprovados.

O próximo objetivo é a Fase 1 — Fundação do servidor: implementar acesso real ao banco, esquema e migrações versionadas, contas e sessões, persistência e testes correspondentes. Acompanhar também a validação do catálogo, o audit log e os testes de carga conforme os critérios de aceite do README. Não declarar a Fase 1 concluída antes de cumprir esses critérios. PostgreSQL/SQLite e gameplay completo ainda não estão comprovados.

**Branch:** `arena/b2f4e6da-projetorpg` (ativa). **Commit de scaffold:** `def86f0`; branch publicada em `origin/arena/b2f4e6da-projetorpg`. **PR aberto:** [#1 — ProjetoRPG: add Godot/.NET scaffold and validation](https://github.com/heldergaraujo2/ProjetoRPG/pull/1). **Fase 0:** concluída em 2026-10-28. **Fase 1:** próximo objetivo, ainda incompleto.

## Validação local confirmada — 2026-10-28

Ambiente Windows do usuário. Esta seção atualiza o estado operacional anterior, que refletia as limitações de toolchain do sandbox Linux em 2026-10-27.

### Ferramentas identificadas

- Node.js 22.23.4.
- .NET SDK 10.0.403, respeitando o `global.json` com roll-forward `latestPatch`.
- Godot 4.7.2 stable Mono.
- O script de smoke do cliente foi executado apontando `GODOT_BIN` para o executável console do Godot instalado localmente.

### Builds limpos

- `dotnet clean` de Shared, Server e Client: sucesso.
- `dotnet restore` dos três projetos: sucesso.
- `dotnet build shared/ProjetoRpg.Shared.csproj --no-restore`: sucesso, target `net8.0`.
- `dotnet build server/ProjetoRpg.Server.csproj --no-restore`: sucesso, target `net10.0`.
- `dotnet build client/ProjetoRpg.Client.csproj --no-restore`: sucesso, target `net8.0`.
- `npm run check`: sintaxe JavaScript aprovada e 4/4 testes do protótipo Node aprovados.

### Testes de destino

- `npm run test:target:shared`: `SHARED_PROTOCOL_SMOKE_OK`.
- `npm run test:target:server`: `SERVER_INTEGRATION_OK`; endpoint ativo, relógio medido em 19,90 Hz nesta execução, validação de catálogo, handshake/ping WebSocket, rejeição de versão incompatível e recusa de catálogo inválido.
- `npm run test:target:client`: `GODOT_CLIENT_SMOKE_OK`; importação/build headless e inicialização da cena Bootstrap.
- `npm run test:target`: os três testes de destino passaram na mesma execução.

Os avisos `MODULE_TYPELESS_PACKAGE_JSON` dos scripts JavaScript foram não bloqueantes. O relógio do servidor mede pulsos; não representa simulação de mundo. A cena Bootstrap confirma inicialização, não gameplay.

### Limites que permanecem

Não foi demonstrada a implementação de contas, registro/login, hash de senhas, sessões autenticadas, personagens persistentes, PostgreSQL/SQLite, migrações ou audit log. Também não estão demonstrados mundo simulado, movimentação multiplayer, combate, inventário, economia ou gameplay completo.

### Próximo objetivo — Fase 1: Fundação do servidor

Implementar configuração e logging robustos, acesso real ao banco, esquema e migrações versionadas, contas/sessões e testes de persistência. Não marcar a Fase 1 como concluída até cumprir os critérios de aceite do README.
