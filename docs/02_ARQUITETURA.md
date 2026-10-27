# Arquitetura operacional do ProjetoRPG

**Autoridade de produto:** [`../README.md`](../README.md) e pacote técnico oficial em [`../rpg-pack/`](../rpg-pack/).
**ADR da stack:** [`adr/ADR-001-stack.md`](adr/ADR-001-stack.md).
**Checkpoint operacional:** [`../CHECKPOINT.md`](../CHECKPOINT.md).

## Reconciliação do checkout

A branch Arena continua `arena/b2f4e6da-projetorpg`, no commit-base `40e8badd43ca28c17662944824a3c25335e22182`; a primeira referência local de `origin/main` estava em `3941e64`. A auditoria executou `git fetch origin main` para atualizar somente a referência. O `origin/main` encontrado, `9ac6d53ecf58cca77fa05295583123bb7ac0f000`, contém README, especificação e `rpg-pack/`, mas não continha `project.godot`, `.csproj`, `.sln` nem implementação Godot/.NET. A inspeção das branches remotas também não revelou outra branch publicada com esses projetos. Nenhum checkout, merge ou troca da branch Arena ocorreu.

Assim, existem duas questões distintas: (1) a branch estava desatualizada em relação aos documentos publicados em `main`; (2) nem essa referência `main` tinha o scaffold Godot/.NET que o usuário relata ter executado no próprio PC. Não é possível atribuir uma causa histórica adicional sem o commit/log/artifact daquele PC. O checkpoint oficial importado informava Fase 0 não iniciada e não trazia resultados de teste. A divergência não demonstra que os testes externos falharam; somente não os comprova.

Os documentos locais anteriores que foram substituídos foram recuperados de blobs Git inacessíveis e arquivados em [`archive/`](archive/RECOVERY.md). Não é possível provar que cada blob correspondia byte a byte à última versão não commitada do working tree.

## Decisão técnica e versões

A arquitetura oficial em `rpg-pack/02_ARQUITETURA.md` propunha Godot 4 C#, .NET 8, PostgreSQL, SQLite para desenvolvimento e TCP/UDP confiável com WebSocket como alternativa inicial. A Fase 0 permite confirmar a proposta por ADR. A decisão registrada em [`adr/ADR-001-stack.md`](adr/ADR-001-stack.md) é:

- **Cliente:** Godot `4.7.2-stable` .NET, C# com TFM `net8.0` e SDK `Godot.NET.Sdk/4.7.2`.
- **Servidor dedicado:** ASP.NET Core, C#, SDK/TFM `.NET 10.0.402` / `net10.0`.
- **Shared:** contratos tipados/serialização em `net8.0`, sem dependência de Godot; biblioteca consumida pelo server net10.
- **Persistência final:** PostgreSQL; SQLite para desenvolvimento/testes quando o provider e migrações estiverem implementados.
- **Transporte inicial:** WebSocket/TCP, protocolo JSON versionado. UDP confiável só após benchmark e ADR.
- **Conteúdo:** JSON versionado em `content/`, recusado no startup se o manifest ou arquivos referenciados forem inválidos.

A decisão usa net8.0 no cliente/shared porque a source tree oficial da tag Godot 4.7.2 declara `GodotSharp` e `GodotTools` com TFM net8.0; o pacote SDK é `Godot.NET.Sdk/4.7.2`. O servidor fica em net10.0 por ciclo de suporte para projeto de longo prazo. A GitHub API oficial reportou o asset Mono Linux x86_64 do Godot `4.7.2-stable` e o SDK .NET `10.0.402`. Em ambiente local será necessário o SDK .NET 10.0.402 e Godot Mono. Godot 4.7 inclui o runtime necessário para executar jogos C#; não é preciso instalar runtime .NET 8 separado. O build do client ainda precisa resolver o targeting pack net8.0 (via restore ou, se necessário, SDK 8 oficial). Isso fundamenta os pins, mas **a integração ainda não foi compilada neste ambiente**; a ADR registra a compatibilidade como pendente.

## Componentes implementados e fronteiras

- **`client/`:** scaffold Godot C# com projeto, cena de diagnóstico e marcador de boot. Não contém gameplay, input de movimento, navegação, render final ou conexão ao servidor.
- **`server/`:** scaffold ASP.NET Core com health endpoints, catálogo de conteúdo no bootstrap, sessão WebSocket com Hello/HelloAck, Ping/Pong, versionamento/ordem básica, e serviço de clock configurado a 20 Hz com counter/histogram OpenTelemetry. O tick apenas conta pulsos: ainda não simula mundo, atores, movimento, persistência ou autoridade de gameplay.
- **`shared/`:** envelopes JSON comuns; limites de tamanho e validação sintática de tipo/versão/sequence.
- **`content/`:** manifest inicial e schema documentado. O servidor verifica manualmente campos obrigatórios, versão, IDs únicos/estáveis, traversal/caminho, existência e JSON de cada arquivo referenciado. O catálogo inicial está vazio. Não há ainda um validador JSON Schema genérico nem catálogo de gameplay.
- **`tests/` e `scripts/`:** smoke do protocolo, integração HTTP/WebSocket/tick/recusa de catálogo inválido e smoke headless Godot. A presença dos scripts não é evidência de sucesso; dependem das toolchains.
- **Protótipo legado (`server.js`, `public/`, `test/`):** preservado e validado separadamente em Node, sem tratá-lo como arquitetura-alvo.

## Contrato de rede atual

O cliente envia intenções; somente o servidor deverá validar e mutar estado de jogo. O protocolo atual usa JSON UTF-8 via WebSocket, protocolo `1`, limite de frame de 4 KiB, handshake obrigatório `Hello`, `HelloAck`, `Ping`/`Pong` e sequence monotônica por sessão. No scaffold, versão inválida, frame inválido e sequence repetida são recusados/encerram a sessão; o endpoint não oferece autenticação nem estado de jogo. Implementações futuras devem manter o servidor como autoridade para sessão, movimento, combate, loot, inventário, economia, teleporte, progressão e interações.

Clique esquerdo continuará sendo o único comando de movimento com navegação/pathfinding, colisão e sincronização server-side; clique direito solicita ataque sujeito a validação autoritativa. WASD/Espaço não terão gameplay; I/P/M são atalhos de UI. Os atributos canônicos são Fúria, Defesa, Mana, Agilidade e Sabedoria. Esses sistemas não foram implementados no scaffold.

## Segurança, persistência e conteúdo

- Migrations versionadas; nunca apagar banco de jogador para corrigir schema.
- Inventário/economia devem usar operações transacionais/idempotentes.
- Password hashing reconhecido, rate limits e respostas sem enumeração de conta.
- Queries parametrizadas, privilégios mínimos e nenhum segredo em Git/logs.
- Validar tipo, faixas, tamanho, posse, frequência e estado antes de mutação.
- O scaffold não possui contas, hash de senha, PostgreSQL, SQLite, migrations, rate limiting ou autorização de jogo. Sua resposta `health/tick` deixa explícito que `persistenceConfigured=false`.

## Ferramentas e evidência disponível

No sandbox remoto, Node `v22.22.3`/npm `10.9.8` estão disponíveis, mas não `godot`, `godot4`, `dotnet`, `psql`, `postgres` ou Docker; APT sem índices locais. A auditoria de rede confirmou que o endpoint de release Godot no GitHub retorna 302 para `objects.githubusercontent.com` (fora do allowlist); a API de assets também redireciona para `release-assets.githubusercontent.com`, igualmente fora do allowlist. Downloads Microsoft em `dotnetcli.azureedge.net`, `builds.dotnet.microsoft.com` e `packages.microsoft.com` falham TLS com `curl` 35 / `SSL_ERROR_SYSCALL`; sem contornar TLS, não há rota oficial de binário acessível neste sandbox. Nenhum runtime/SDK foi instalado e nenhum build alvo pode ser declarado. Fontes não oficiais não serão usadas.

Com toolchains instaladas localmente, a sequência reproduzível a partir da raiz é:

```bash
npm ci
npm run check                       # protótipo Node + sintaxe dos scripts
npm run doctor                      # diagnóstico de toolchain (não é build)
dotnet --info
dotnet run --project tests/Shared.Smoke/Shared.Smoke.csproj
npm run test:target:server           # build e integração real do ASP.NET Core
npm run test:target:client           # import/build e boot headless no Godot .NET
npm run test:target                  # todos os testes de destino em sequência
```

Para iniciar o servidor de desenvolvimento: `dotnet run --project server/ProjetoRpg.Server.csproj`. O cliente abre com Godot 4.7.2 Mono, projeto `client/`. No Windows, a instalação reprodutível é `winget install --id Microsoft.DotNet.SDK.10 --exact --version 10.0.402` e `winget install --id GodotEngine.GodotEngine.Mono --exact --version 4.7.2`. Godot Mono contém o runtime para executar o jogo; não requer runtime 8 separado. O client usa TFM `net8.0`, então o restore precisa resolver `Godot.NET.Sdk/4.7.2`, `GodotSharp` e o targeting pack 8; só se o restore indicar que o targeting pack não está disponível, instalar `winget install --id Microsoft.DotNet.SDK.8 --exact --version 8.0.427`.

O teste automatizado server inclui o aceite de **recusar startup com manifest de schema desconhecido**. Ainda não há teste de 100 bots, registro/login/personagens, integração de banco, movimento/combat, navegação ou MMORPG multiplayer. O script de integração calcula a taxa média observada numa janela de 1 s e exige 15–25 ticks/s como margem de smoke; ainda não foi executado neste ambiente, portanto não há taxa medida. Isso não demonstra deadline por tick nem world simulation. A Fase 0 permanece incompleta até build/testes de ambos os lados em toolchain real e demais critérios do checkpoint serem comprovados.
