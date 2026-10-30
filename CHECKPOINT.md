# CHECKPOINT — integração server-authoritative de Coast

**Data desta atualização:** 2026-10-29 (America/Sao_Paulo)
**Branch autorizada:** `arena/b2f4e6da-projetorpg` (`pr_open`)
**Clone de trabalho exclusivo:** `/home/user/ProjetoRPG-integration-20261030`
**Base/remote verificados no início:** `bd55328d3b0261fb673f8cc395c7715ff7f04413`.
**Estado desta etapa:** implementação e documentação em dois commits foram enviadas para a branch autorizada; continuam pendentes compilação/build, integração/runtime e validação visual no Windows. Não marcar como tarefa concluída/jogável até evidência de execução.

## Pedido ativo

Entregar somente o fluxo funcional: **LOGIN → LOBBY COM TODOS OS PERSONAGENS → SELEÇÃO → ENTRAR NO MUNDO → JDUNGEON3D COAST → SPAWN → CAMINHAR COM COLISÕES**, com identidade/ownership/mapa/spawn/movimento validados pelo servidor. Não iniciar combate, inventário, missões, economia ou atributos. Não descrever como multiplayer: ainda não há broadcast/colisão dinâmica entre jogadores.

## Trabalho acrescentado nesta etapa

- `content/manifest.json` aponta `world.coast` e `world.coast.navigation`; mapa/configuração e 1.849 triângulos de NavigationMesh foram derivados do Coast importado no commit upstream `73bea81341846341f302dfa184e33d31733c3fa8`. O ponto `coast.initial` é `(0.148617, 2.25, 51.4204)` e foi geometricamente analisado com raio 0,32. Licenças MIT/avisos originais mantidos em `client/Assets/Worlds/JDungeon3D-Coast/licenses/`.
- `shared/World/WorldContracts.cs` define request de entrada, snapshot e intenção direcional; `ProtocolConstants` passou para v2 e o codec rejeita campos extras na intenção.
- Migration aditiva `0002_world_sessions` guarda sessão de mundo, ownership, mapa/spawn, posição/velocidade, sequência/tick, atividade, expiração e encerramento. `WorldMapCatalog` carrega a malha e valida spawn/raio; `WorldSessionService` autentica ownership, cria/recupera sessão idempotente, limita por `WorldTickState`/tick de 20 Hz, calcula posição somente a partir da direção e persiste estado. `WorldEndpoints` expõe criação/retomada e encerramento; `/ws` requer bearer + `WorldJoin`, revalida a conta a cada intenção, recusa socket concorrente e preserva estado por dez minutos após desconexão.
- Lobby conserva dados reais da API e seleção explícita; o botão de entrada depende de ID presente na lista. `AccountGateway` chama `/api/v1/world/sessions` antes da cena. `WorldGameplay.tscn`/`WorldGameplay.cs` usam cena própria, Coast real, ator selecionado, WASD/setas, câmera, Idle/Walking, snapshots server-authoritative, retry e retorno. `Bootstrap.tscn` continua principal.
- Testes `test/world-gameplay.test.js`, `test/client-*.test.js`, `tests/Shared.Smoke/Program.cs` e `scripts/phase1-integration.js` foram ampliados para mapa/spawn, ownership, idempotência, spawn, join de WS, input inválido/posição arbitrária, speed, parada na borda, disconnect/reconnect/restart/leave. O runner exige PostgreSQL descartável e .NET.
- Atualizados README/arquitetura/contrato/changelog/checkpoint/README do server/client e atribuição/licença Coast.

## Evidência disponível nesta etapa

- `npm test`: **29/29 passaram** (testes Node legados e testes estáticos/geometria; não compilam C# nem importam Godot).
- `npm run check`: **passou, 29/29**, após atualizar o runner/protocolo; avisos ESM são não bloqueantes.
- Parse C# via tree-sitter (sintaxe somente): `CSHARP_TREE_SITTER_SYNTAX_OK` para todos os `.cs`; não é compilação nem type-check.
- Análise estática do Coast: 1.849 triângulos; centro e oito amostras do spawn estão em superfície navegável na origem extraída. Não prova execução Godot ou colisão física real.
- `git diff --check`: passou antes dos commits; push normal para `origin/arena/b2f4e6da-projetorpg` concluído. Commits: `e91cd0d` (implementação) e `fc1cf37` (documentação/checkpoint). A árvore estava limpa após o push; nenhum commit em main/force-push.
- `npm run test:phase1`: **bloqueado, código 2**, `.NET SDK is unavailable`; Docker também indisponível. Nenhum build, migration PostgreSQL ou execução HTTP/WebSocket do servidor novo foi observada.
- `npm run test:target:client`: **bloqueado, código 2**, executável Godot 4 .NET ausente. Nenhum build C#/Godot, import, abrir cena ou gameplay visual foi observado.
- `dotnet`, `godot`, `godot4`, `csc`, `mcs`, `mono`, Docker e PostgreSQL não estão disponíveis no Agent.
- Validação Windows em `D:\ProjetoRPG-Teste-Lobby` ainda não executada/não é acessível ao Agent. Não usar `D:\AProjeto RPG Fixo`.

## Próximas ações — não pular

1. Na cópia descartável Windows, compilar Shared/server/client com versões da ADR; executar `npm run test:phase1` em Docker/PostgreSQL descartável e corrigir erros reais. Aplicar migration `0002` apenas ao DB de teste.
2. Abrir/importar Godot 4.7.2 Mono e validar cena de gameplay/Coast e fluxo com servidor real. Provar login, lista completa/seleção, autorização/ownership, entrada/idempotência, spawn, WASD/setas, bloqueio de borda/obstáculos, câmera/orientação/Idle/Walking, disconnect/reconnect, leave e retorno ao lobby. Capturar evidências/logs/versões/SHAs reais.
3. Só então declarar o critério não negociável satisfeito. Distinguir teste Agent, build .NET, import Godot, execução automatizada e inspeção visual Windows.
4. Correções futuras devem ser commits revisáveis somente em `arena/b2f4e6da-projetorpg`; nunca trocar branch, push para main, force-push ou alterar PR sem pedido explícito.

## Preservação e workspaces

- Todo o trabalho novo desta etapa está no clone `/home/user/ProjetoRPG-integration-20261030`, com cwd/paths explícitos. Não escrever em `/home/user/ProjetoRPG` durante esta tarefa.
- Histórico de preservação: etapa anterior teve escrita acidental no workspace oficial `/home/user/ProjetoRPG`, com estado local desconhecido e arquivos locais possivelmente sobrescritos. Não tentar restaurar/limpar/sincronizar às cegas; não sobrescrever nele. Nenhuma escrita foi feita nele nesta etapa.
- Não alterar/reverter `.uid`/`.import` desconhecidos, não executar `reset`, `clean`, checkout destrutivo, stage amplo, sincronização automática ou operações indiscriminadas.
- Nunca usar `D:\AProjeto RPG Fixo`; só a cópia descartável autorizada `D:\ProjetoRPG-Teste-Lobby`.

## Histórico resumido

- Branch já publicou lobby account-backed, importação seletiva de Coast e documentação parcial antes desta etapa (commits prévios `0bff34d`, `a1cb78d`, `bd55328` na linha de integração). Entrada e gameplay ainda não existiam quando esta etapa começou.
- O checkpoint anterior de 698 linhas foi preservado em `docs/archive/CHECKPOINT-pre-gameplay-2026-10-29.md`; estados antigos são apenas históricos.
- Este checkpoint substitui relatos antigos de “botão bloqueado/nenhum world endpoint” quanto ao **código atual do clone**, mas não transforma implementação não compilada em evidência executável.
- A fonte de critérios do usuário é a sessão Arena; o contrato e README detalham o corte atual e seus limites. Fases futuras do MMORPG seguem fora desta tarefa.
