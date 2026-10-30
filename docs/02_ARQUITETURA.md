# Arquitetura operacional do ProjetoRPG

**Stack aprovada:** [`adr/ADR-001-stack.md`](adr/ADR-001-stack.md).
**Estado de gameplay:** [`CONTRATO_FUTURO_SESSAO_GAMEPLAY.md`](CONTRATO_FUTURO_SESSAO_GAMEPLAY.md).
**Checkpoint/validação:** [`../CHECKPOINT.md`](../CHECKPOINT.md).

## Componentes executáveis

- **Cliente `client/`:** Godot 4.7.2 Mono, C# `net8.0`; `Bootstrap.tscn` segue cena principal e inicializa autenticação. O cliente conversa com APIs reais de conta/personagem, mostra todos os registros recebidos em previews 3D independentes, exige seleção válida e solicita entrada a `/api/v1/world/sessions`. O fluxo de jogo carrega `CoastWorld.tscn`, instancia `CharacterActor.tscn` para o personagem autorizado, conecta `/ws`, envia somente intenções WASD/setas e aplica snapshots produzidos pelo servidor. Retorno encerra sessão e volta ao lobby.
- **Servidor `server/`:** ASP.NET Core/.NET 10, Npgsql/PostgreSQL. Contas, sessões bearer, personagens/ownership, migrations, audit, catálogo de conteúdo e WorldTickService 20 Hz. `WorldSessionService` autoriza Coast/spawn, persiste sessão, valida input e grava posição autoritativa.
- **Shared `shared/`:** contratos JSON e codec estrito de envelopes/payloads, protocolo WebSocket v2 e DTOs de sessão/movimento, sem dependência de Godot.
- **Conteúdo `content/`:** manifest `0.2.0` registra world Coast e malha de navegação triangular. O startup verifica IDs/paths e o catálogo de mundo valida triângulos, velocidade, raio e spawn.
- **Assets `client/Assets/Worlds/JDungeon3D-Coast/`:** cena/mesh/texturas/colisores selecionados da origem MIT JDungeon3D, commit `73bea81341846341f302dfa184e33d31733c3fa8`; licenças e avisos preservados. Não incluímos gameplay upstream. A malha server-side é derivada da NavigationMesh fixada.

O fluxo implementado é de jogador/sessão individual real, não multiplayer: não há broadcast de pares, instâncias distribuídas nem colisão dinâmica entre avatares. A API e o servidor não aceitam posição final arbitrária do cliente. O servidor não executa o PhysicsServer3D: usa navmesh e amostras circulares para autorizar espaço caminhável; alinhamento real com colisores Godot ainda exige import/build/teste visual e físico.

## Entrada autoritativa

`POST /api/v1/world/sessions` recebe somente `characterId` sob Bearer válido. A identidade é extraída de `AuthService`, e `WorldSessionService` trava a linha do personagem, confirma ownership, escolhe `world.coast`/`coast.initial` no catálogo e cria ou recupera a sessão com unicidade parcial por personagem. Primeira entrada `201`, recuperação idempotente `200`; mapa ou spawn indisponível `503`; personagem ausente/alheio `404`; sessão de conta inválida `401`. `DELETE /api/v1/world/sessions/{id}` encerra de maneira idempotente (`204`) sem divulgar ownership.

O estado inicial contém session/character IDs, nome/classe efetivos do banco, mapa/spawn autorizados, posição/velocidade server-side, walk speed, tick, última sequência aceita, expiração e flag de retomada. O cliente só carrega a cena e o personagem após receber e validar esse snapshot.

## Protocolo WebSocket v2

Canal `/ws`, JSON UTF-8, máximo de 4 KiB, protocolo 2 e sequência monotônica externa. `Hello`/`HelloAck` abre o protocolo; `WorldJoin { worldSessionId }` requer Bearer da conta dona; `WorldJoined` retorna estado atual; `MoveIntent { directionX, directionZ, inputSequence }` envia direção, não coordenadas; `WorldState` retorna resultado autoritativo; `InputRejected` reporta recusa; `Bye` encerra. A credencial é revalidada a cada input e o registry impede duas conexões simultâneas para o mesmo ID de sessão naquele processo. Desconexão abrupta mantém posição por dez minutos para reconexão.

O `WorldTickService` mede ticks a 20 Hz. Uma intenção pode avançar no máximo dois ticks, com `walkSpeed=3.5` vindo do catálogo; diagonais normalizadas; sequência/tick/frequência validados. Cada movimento aceito grava posição/velocidade/atividade/expiração em PostgreSQL. Navegação server-side consulta a malha triangular fixada, subdivide passos até 0,05 e amostra oito pontos em raio mínimo 0,32; fora da superfície, o ator para. Falhas não modificam posição nem aceitam delta/velocidade/teleporte enviado pelo cliente.

## Persistência e operações

- `0001_initial` contém contas, account sessions, personagens e audit; `0002_world_sessions` é aditiva e grava sessões, propriedade, estado, ticks, expiração e encerramento. Checksums, advisory lock, transações e rollback explícito mantidos. Migration 2 deve ser aplicada antes de novas rotas entrarem em operação.
- Contas usam `PasswordHasher<TUser>`, tokens opacos persistem apenas como SHA-256; autorização é por ownership em queries parametrizadas.
- O boot não migra DB automaticamente. `--db-status`, `--db-migrate` e rollback com confirmação explícita continuam. `/health/live`, `/health/database`, `/health/ready`, `/health/tick`, `/health/content` e `/health/world` medem etapas distintas.
- Audit não grava senha, token ou username. Não registrar connection strings/secrets.

## Limites e validação

- O navmesh do servidor é uma cópia de geometria derivada de conteúdo versionado; o runtime Godot continua necessário para importar Coast, confirmar terreno, texturas, spawn, colisores e câmera. Mover servidor não simula colisão física arbitrária, players de outros servidores nem corpos dinâmicos.
- Sessões persistem através de reinício; registry de websocket é local ao processo. Sem sincronização multiplayer ou escalabilidade horizontal.
- Movimento client-side está desabilitado no personagem da cena de gameplay; somente os snapshots de `WorldState` controlam root/rotação e animações Idle/Walking.
- `npm test`/`npm run check` são testes Node/estáticos e geométricos; não compilam C#/Godot. `npm run test:phase1` exige SDK .NET e Docker/PostgreSQL; `npm run test:target` exige .NET/Godot Mono.
- Validação final visual/física exige Windows e cópia descartável `D:\ProjetoRPG-Teste-Lobby`. Não usar `D:\AProjeto RPG Fixo`. Reportar separadamente builds, import, integração, gameplay e inspeção Windows; ausência de toolchain não é sucesso.

## Sequência de validação Windows

```powershell
npm ci
npm run check
npm run test:phase1
dotnet build shared\ProjetoRpg.Shared.csproj
dotnet build server\ProjetoRpg.Server.csproj
npm run test:target
```

Configure PostgreSQL descartável, defina `ConnectionStrings__GameDatabase` (e `ConnectionStrings__MigrationDatabase` em Production), rode `--db-status` e `--db-migrate`, depois inicie servidor e abra `client/project.godot` em Godot 4.7.2 Mono. No client configure `PROJETORPG_SERVER_URL` para URL local HTTPS ou HTTP loopback. No Windows descartável comprovar o fluxo completo e guardar evidências antes de declarar pronto.
