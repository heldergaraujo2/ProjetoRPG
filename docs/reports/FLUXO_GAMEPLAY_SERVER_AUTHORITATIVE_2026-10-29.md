# Implementação do fluxo gameplay server-authoritative — 2026-10-29

## Resultado

Foi escrita no clone de integração uma primeira implementação integrada do fluxo: autenticação/account lobby preexistente, seleção de registro real, entrada REST autenticada, sessão PostgreSQL idempotente, seleção Coast/spawn pelo servidor, canal WebSocket autenticado v2, intents direcionais com tick/speed/navigation validados, cena Godot própria para Coast, ator controlado por snapshots, câmera/animação, reconexão e retorno ao lobby.

**Status honesto:** implementação **não compilada nem executada nesta etapa**. Portanto o critério final “login → seleção → entrar em Coast → caminhar com colisões” ainda **não está comprovado e não pode ser declarado concluído/jogável**. Próximo bloqueio material é toolchain/teste Windows, não escopo adicional.

## Mudanças principais

- `shared/Protocol/ProtocolEnvelope.cs`, `shared/World/WorldContracts.cs`: protocolo v2, payload JSON estrito e tipos de entrada/snapshot.
- `content/manifest.json`, `content/worlds/coast.json`, `content/worlds/coast-navigation.json`, schemas: map/spawn server-side e 1.849 triângulos extraídos da NavigationMesh da Coast fixada.
- `server/Persistence/Migrations/0002_world_sessions.{up,down}.sql`, `MigrationRunner.cs`, `DatabaseCommandRunner.cs`: migration aditiva de sessão.
- `server/World/WorldMapCatalog.cs`, `WorldSessionService.cs`, `WorldEndpoints.cs`, `server/Network/WebSocketEndpoint.cs`, `Program.cs`: catálogo/navegação, owner check, sessão idempotente, persistência/expiração, rotas e WS.
- `client/Scenes/WorldGameplay.tscn`, `client/Scripts/WorldGameplay.cs`, `WorldRealtimeClient.cs`, `CharacterActor.cs`, `CharacterLobby.cs`, `AccountGateway.cs`: entrada autenticada, cena Coast própria, WASD/setas e aplicação de snapshots.
- Testes/guia: `scripts/phase1-integration.js`, `tests/Shared.Smoke/Program.cs`, `test/world-gameplay.test.js`, testes de lobby/auth e `docs/CONTRATO_FUTURO_SESSAO_GAMEPLAY.md`.
- Atualizados `README.md`, `docs/02_ARQUITETURA.md`, `server/README.md`, `client/README.md`, atribuição Coast, `CHECKPOINT.md` e este relatório.

## Contrato atualmente no código

### HTTP

- `POST /api/v1/world/sessions` — Bearer de conta; corpo só `{ characterId }`; valida ownership no servidor. `201` cria, `200` retoma a mesma sessão ativa, `404` não existe/não pertence, `401` auth, `503` mapa/spawn indisponível.
- `DELETE /api/v1/world/sessions/{worldSessionId}` — fecha sessão/velocidade; resposta idempotente `204`.
- `GET /health/world` — status do mapa/spawn autorizado.

### WebSocket

`/ws`, protocolo 2, frame máximo 4KiB. `Hello` → `HelloAck`; `WorldJoin { worldSessionId }` requer Bearer dono e retorna `WorldJoined`; `MoveIntent { directionX,directionZ,inputSequence }` retorna `WorldState` ou `InputRejected`; `Bye` fecha canal. Conta é revalidada a cada movimento.

Nenhuma posição final, delta ou velocidade é aceita do cliente. `WorldTickState` (20Hz) determina o intervalo; velocidade Coast 3,5; diagonais normalizadas; amostras de navmesh a cada ≤0,05 com agente raio 0,32/oito pontos; cada movimento aceito é persistido. Reconnect usa mesma sessão/posição por dez minutos, com tick baseline refeito inclusive após reinício do processo.

### Cliente

`Bootstrap.tscn` permanece main scene. O lobby só habilita entrada para GUID selecionado presente na lista real mais recente. `AccountGateway` obtém resposta autorizada antes de criar `WorldGameplay.tscn`. Essa cena carrega `CoastWorld.tscn`, instancia `CharacterActor.tscn`, usa WASD/setas, recebe posição/velocidade do servidor, orienta câmera/ator e alterna Idle/Walking. Tenta reconexão e oferece retorno/encerramento.

Não é multiplayer: não há broadcast de pares, sincronização multi-player nem colisão dinâmica entre avatares. O servidor testa navegabilidade com malha geométrica; não executa o `PhysicsServer3D` Godot. A correspondência física real entre collider, chão, ator e navmesh ainda requer validação.

## Licença

Coast provém de `jonathaneeckhout/jdungeon3d`, commit `73bea81341846341f302dfa184e33d31733c3fa8`. O cenário, colisores/navegação e avisos/licenças MIT específicos permanecem nos caminhos seletivos em `client/Assets/Worlds/JDungeon3D-Coast/`; geometria server-side é derivada da NavigationMesh. Nenhum Player/script/gameplay upstream foi importado. KayKit continua com suas próprias notices CC0.

## Testes e observações reais

Executados neste Agent:

- `npm test`: 29/29 passaram (Node + asserções estáticas/geometria).
- `npm run check`: 29/29 passou depois de atualizar o runner/protocolo; avisos ESM são não bloqueantes.
- Parser tree-sitter C#: `CSHARP_TREE_SITTER_SYNTAX_OK` (sintaxe, sem semântica/build).
- Verificação geométrica Python do arquivo de navegação: 1.849 triangles; centro e oito pontos do raio inicial encontram superfície em Y=2.25.
- `npm run test:phase1`: bloqueado com código 2 porque `.NET SDK is unavailable`; Docker/PostgreSQL também não estão disponíveis.
- `npm run test:target:client`: bloqueado com código 2 porque Godot 4 .NET/Mono não está disponível.
- Busca PATH/pastas: `dotnet`, `godot`, `godot4`, `csc`, `mcs`, `mono` ausentes.

- `git diff --check`: passou; publicados na branch autorizada `arena/b2f4e6da-projetorpg` os commits `e91cd0d` (implementação) e `fc1cf37` (documentação/checkpoint).

Nada nesta seção prova que C#/Godot compilam, que PostgreSQL aceita migration 2, que endpoints executam ou que a cena renderiza/move em runtime. Sem resultado de build, import, gameplay real ou inspeção visual no Windows nesta etapa.

## Próximas validações obrigatórias

Na cópia descartável `D:\ProjetoRPG-Teste-Lobby`, executar build Shared/server/client; aplicar migration 2 no DB descartável; `npm run test:phase1`; importar/abrir Godot 4.7.2 Mono; provar login, lista/seleção, ownership, idempotência, spawn, WASD/setas, velocidade, barreira/navmesh, câmera/orientação/animação, desconexão/reconnect, encerramento e lobby. Registrar versões, logs, evidências, branch e SHAs. Não acessar `D:\AProjeto RPG Fixo`. Só então atualizar o status para concluído, caso todos os critérios passem.
