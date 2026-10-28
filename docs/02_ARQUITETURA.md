# Arquitetura operacional do ProjetoRPG

**Autoridade de produto:** [`../README.md`](../README.md) e pacote técnico oficial em [`../rpg-pack/`](../rpg-pack/).
**ADR da stack:** [`adr/ADR-001-stack.md`](adr/ADR-001-stack.md).
**Checkpoint operacional:** [`../CHECKPOINT.md`](../CHECKPOINT.md).

## Reconciliação do checkout

Na auditoria inicial de 2026-10-27, a branch Arena estava no commit-base `40e8badd43ca28c17662944824a3c25335e22182`; naquela ocasião, a referência `origin/main` encontrada era `9ac6d53ecf58cca77fa05295583123bb7ac0f000` e não continha scaffold Godot/.NET. Esse relato é histórico, não o estado atual. Na retomada de 2026-10-28, o workspace foi comparado e atualizado por fast-forward até o HEAD remoto `99ab3d742427c04043109304ae0b87622b9f3769`, preservando divergências antigas em backup/stash. A implementação parcial desta Fase 1 está no commit local `74784ee75fa8a5f8ba56a7f8d930d9db2d7e907e`; confira o checkpoint para estado de publicação e validação.

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
- O scaffold da primeira publicação não possuía contas, hash, banco, migrations, rate limiting ou autorização; a implementação parcial de Fase 1 agora usa PostgreSQL/Npgsql, hash ASP.NET Identity, sessões opacas, persistência/autorização básica de personagem e audit log. SQLite segue sem provider. `/health/live` é independente do banco; `/health/database` consulta `SELECT 1`; `/health/ready` exige conexão e migration atual. A resposta `health/tick` informa apenas `databaseConnectionConfigured`, não comprova DB saudável.

## Ferramentas e evidência disponível

No sandbox remoto, Node `v22.22.3`/npm `10.9.8` estão disponíveis, mas não `godot`, `godot4`, `dotnet`, `psql`, `postgres` ou Docker; APT sem índices locais. Downloads oficiais binários falharam por allowlist/TLS sem qualquer bypass. Isso bloqueia builds/testes da **implementação de Fase 1 desta sessão**. Separadamente, o usuário confirmou os builds Shared/Server/Client e smoke tests da Fase 0 em Windows em 2026-10-28; essa evidência está registrada no checkpoint e não foi repetida nesta sessão. Fontes não oficiais não serão usadas.

Com toolchains instaladas localmente, a sequência reproduzível a partir da raiz é:

```powershell
npm ci
npm run check                       # protótipo Node + sintaxe dos scripts
npm run doctor                      # diagnóstico de toolchain (não é build)
dotnet build server\ProjetoRpg.Server.csproj
npm run test:phase1                 # Docker/PostgreSQL descartável + auth/schema/persistência
npm run test:target:client          # import/build e boot headless no Godot .NET
npm run test:target                 # shared + server/PostgreSQL + client
```

O servidor exige `ConnectionStrings__GameDatabase`, valida a configuração e não aplica migrations no boot. Execute `dotnet run --project server/ProjetoRpg.Server.csproj -- --db-status`, depois `-- --db-migrate`, e então inicie o app. `/health/live` mede processo, `/health/database` faz `SELECT 1` e `/health/ready` exige schema atualizado. Com Godot 4.7.2 Mono e SDK .NET 10.0.402 disponíveis, o client `net8.0` pode requerer targeting pack 8; tente restore antes de instalar SDK 8 oficial. Godot Mono contém o runtime do jogo.

A implementação parcial inclui Npgsql/PostgreSQL, uma migration inicial reversível com confirmação explícita, registro/login via `PasswordHasher<TUser>`, sessões opacas com expiração/hash/revogação, personagens sem classe atribuída enquanto o catálogo real está vazio, ownership, audit log e smoke completo em container. Esses builds/testes da Fase 1 **não foram executados nesta sessão**: o sandbox não tem .NET SDK nem Docker. `npm run load:parked` prepara 100 conexões WebSocket ociosas por 60 s, mas não foi executado e não representa usuários autenticados nem gameplay. Fase 0 está concluída conforme testes Windows confirmados pelo usuário; Fase 1 continua em andamento e incompleta.
