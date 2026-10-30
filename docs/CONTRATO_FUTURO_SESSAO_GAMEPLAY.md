# Contrato de sessão de gameplay — entrada, Coast e movimento

**Estado do código nesta revisão:** implementação acrescentada; build do .NET/Godot e integração PostgreSQL ainda precisam ser executados. Este documento descreve rotas e mensagens existentes no código, não resultados de runtime.

## Fluxo do cliente

`LOGIN → GET /api/v1/characters → seleção explícita → POST /api/v1/world/sessions → CoastWorld.tscn → WorldJoin autenticado → WASD/setas → retorno ao salão`.

A lista continua sendo a resposta real de personagens da conta. O lobby não cria personagens, classes nem aparências; se não houver classe atribuída, usa uma prévia genérica visivelmente indicada. “Entrar no mundo” só está habilitado quando o ID selecionado é um GUID presente na última lista validada pela API e nenhuma operação de lobby está ocupando o cliente.

## Entrada e ownership

### `POST /api/v1/world/sessions`

- **Autorização:** `Authorization: Bearer <token>` de conta válido; `accountId` é sempre derivado no servidor.
- **Corpo:** `{ "characterId": "<GUID>" }`. Nenhum mapa, spawn, posição ou velocidade é aceito.
- **Ownership:** consulta `characters` por `account_id` e `character_id`, com lock da linha do personagem. Ausente ou pertencente a outra conta retorna `404 { "error": "character_not_found" }`.
- **Mapa/spawn:** escolha exclusiva do servidor: `world.coast` e `coast.initial`, lidos de `content/manifest.json`, `content/worlds/coast.json` e `content/worlds/coast-navigation.json`. A cena importada tem marcador correspondente em `(0.148617, 2.25, 51.4204)`. O catálogo valida spawn e raio do agente contra a malha antes de subir.
- **Idempotência:** uma linha ativa por personagem tem índice único parcial. Primeiro ingresso responde `201`; retry ou entrada com sessão válida responde `200` e o mesmo ID/posição. Uma sessão expirada é fechada e cria-se outra. O lock do personagem serializa chamadas concorrentes.
- **Falhas:** sem bearer válido `401`; corpo sem GUID válido `400`; mapa/spawn inválido `503`; personagem inexistente/alheio `404`. Nenhuma escolha local do cliente substitui a resposta do servidor.
- **Resposta:** `WorldSessionSnapshot`: `worldSessionId`, `characterId`, `characterName`, `classId`, `mapId`, `spawnId`, `position`, `velocity`, `movementSpeed`, `serverTick`, `lastInputSequence`, `expiresAt`, `resumed`.

### `DELETE /api/v1/world/sessions/{worldSessionId}`

Autentica a conta e marca a sessão como encerrada (velocidade zero). Respostas são idempotentes `204` inclusive se o ID não pertencer à conta, para não revelar ownership. No retorno, o cliente tenta esse endpoint e sempre volta ao lobby; se a rede falhar, o cliente informa que o encerramento remoto não foi confirmado. Fechamento abrupto não apaga posição; a sessão fica recuperável por dez minutos após a última intenção válida.

## Canal realtime `/ws`

WebSocket com protocolo compartilhado **versão 2**, JSON UTF-8, frame máximo 4 KiB e `sequence` externo monotônico por conexão. O cliente envia `Authorization: Bearer ...` no upgrade.

1. `Hello` → `HelloAck`.
2. `WorldJoin` com `{ "worldSessionId": "..." }` → `WorldJoined` com snapshot atual. A conta do bearer deve ser dona da sessão; uma sessão aceita só um socket ativo. `world_sessionId` vindo do cliente é apenas localizador e nunca prova identidade.
3. `MoveIntent` com `{ "directionX": -1..1, "directionZ": -1..1, "inputSequence": n }` → `WorldState` ou `InputRejected`.
4. `Bye` fecha o canal. Desconectar inesperadamente preserva sessão/posição durante a janela de dez minutos.

A credencial de conta é revalidada a cada intenção. Sessão revogada/expirada, ownership incorreto ou sessão de mundo encerrada é recusada; o servidor finaliza o socket quando não há mais autorização. O servidor não transmite estado de outros jogadores: isto é um fluxo individual autoritativo inicial, **não multiplayer**.

## Movimento autoritativo e colisões

- A intenção não contém posição final, `deltaTime`, velocidade ou teleport. JSON com campo extra não previsto é recusado pelo codec estrito.
- A autoridade calcula tudo no servidor. `WorldTickService` avança o `WorldTickState` a 20 Hz; no máximo uma intenção é aplicada por tick e o intervalo efetivo é limitado a 1–2 ticks (até 0,1 s), sem catch-up arbitrário.
- `walkSpeed=3.5` unidades/s vem do catálogo, direção diagonal é normalizada, magnitude por eixo é validada, sequência monotônica é persistida e cada estado aceito salva posição/velocidade/atividade em PostgreSQL.
- O servidor amostra a NavigationMesh triangular derivada da NavigationMesh incluída na Coast fixada, em passos de até 0,05 unidade. O corpo tem raio autoritativo mínimo de 0,32 e oito amostras periféricas: movimento para fora da área navegável para na borda. Desníveis incompatíveis também são recusados.
- O cliente Godot instancia `CharacterActor.tscn` com a aparência ilustrativa da mesma classe efetivamente recebida, mas não processa movimento local. WASD/setas enviam intenção; só `WorldState` reposiciona o ator, orienta e troca Idle/Walking. A câmera segue o ator e o cliente mantém botão de reconexão e retorno ao lobby.
- A malha do servidor valida navegabilidade/raio sobre dados importados; ela não executa o `PhysicsServer3D` do Godot. Verificar no Godot/Windows a correspondência visual e física entre colisores da Coast, NavigationMesh, cápsula do personagem e spawn é obrigatório antes de declarar colisão final comprovada.

## Persistência, desconexão e limites atuais

Migration `0002_world_sessions` é aditiva e guarda ID de sessão, conta/personagem, mapa/spawn, posição/velocidade, sequência/tick, atividade, expiração e encerramento. Entradas idempotentes recuperam o mesmo estado; movimento aceita somente comandos associados a sessão/account autenticados; retorno explícito encerra; desconexão abrupta preserva por dez minutos; reinício do processo não remove a posição persistida. Sessões não são sincronizadas entre servidores de aplicação: o registry de socket ativo é local ao processo. Ainda não há múltiplos jogadores visíveis, descoberta/instâncias, colisão dinâmica entre avatares, combate, inventário, missões, streaming ou autoridade sobre atributos não implementados.

## Origem/licença de Coast

O cenário continua limitado ao conteúdo selecionado de `jonathaneeckhout/jdungeon3d`, commit `73bea81341846341f302dfa184e33d31733c3fa8`. A atribuição MIT e avisos de assets importados permanecem em `client/Assets/Worlds/JDungeon3D-Coast/licenses/` e no README do asset. `content/worlds/coast-navigation.json` é extração geométrica de navegação desse mesmo commit; não foram portados Player, scripts ou gameplay do upstream. `Bootstrap.tscn` continua como cena principal ProjetoRPG.

## Validação exigida para aceitar a entrega

No Agent, os testes Node/estáticos e análise geométrica do JSON não compilam C# nem importam cena Godot. Rodar `npm run test:phase1` prova somente se o teste de integração realmente completa com SDK .NET, Docker/PostgreSQL descartável e servidor. Rodar `npm run test:target` prova builds/import/headless disponíveis, mas não substitui inspeção visual/física.

No Windows, exclusivamente na cópia descartável `D:\ProjetoRPG-Teste-Lobby`: instalar/importar com Godot 4.7.2 Mono, compilar Shared/Server/Client, migrar banco descartável, testar login→lista completa→seleção→entrada autorizada, recusa por ownership, sessão duplicada/retomada, Coast, spawn, WASD/setas, bloqueio na borda/colisor, orientação, câmera, Idle/Walking, desconexão/reconexão e retorno ao lobby. Registre screenshots/logs/SHAs reais. Não usar `D:\AProjeto RPG Fixo`. Até esses testes serem executados e registrados, o gameplay permanece **implementado no código, não validado como build/jogável**.
