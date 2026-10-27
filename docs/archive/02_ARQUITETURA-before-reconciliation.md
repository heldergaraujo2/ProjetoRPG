# 02 — Arquitetura do ProjetoRPG

**Versão:** 0.1 · **Data da auditoria:** 2026-10-27 · **Decisão:** [`adr/ADR-001.md`](adr/ADR-001.md)

## Contexto e auditoria do estado real

A auditoria encontrou um único commit-base (`40e8bad`, “Initial commit”) e, na árvore de trabalho atual, um protótipo web criado sobre a base original: `server.js`, `public/` (HTML/CSS/Canvas 2D), `public/data/game-data.json`, `ws` e testes Node. Não foram encontrados cenas/recursos Godot, código C#, solução .NET, banco de dados, autenticação, persistência, inventário, navegação por navmesh, contas nem documentação oficial anterior para arquitetura, checkpoint ou ADRs. O `README.md` que havia nesta árvore descrevia somente o protótipo; esta revisão o substitui pelo roadmap oficial consolidado.

O protótipo serve para validar rapidamente a ideia e o fluxo realtime, mas **não** satisfaz a meta 3D, a autoridade/persistência do servidor, os controles definidos nem os critérios de MMORPG. Seus jogadores, criaturas, XP e mapas existem apenas em memória; a interface permite teclado para gameplay. Não afirmar que está pronto para jogadores/produção.

## Stack aprovada

- **Cliente:** Godot 4.x, build .NET/C#; cenas 3D e recursos do motor.
- **Servidor dedicado:** ASP.NET Core sobre .NET LTS; simulação e regras autoritativas.
- **Persistência:** PostgreSQL em produção; SQLite para desenvolvimento, com migrações e transações compatíveis.
- **Realtime:** WebSocket/TLS (WSS) para intenção/estado de sessão; HTTPS para operações de conta/lobby quando aplicável. O servidor valida identidade, versão de protocolo, autorização e conteúdo de cada mensagem.
- **Testes:** xUnit e testes de integração para servidor; testes de cena/recursos e automação de cliente Godot; testes de integração multiplayer com dois clientes. A implementação escolherá os frameworks de engine compatíveis com a versão aprovada.
- **Conteúdo:** definições versionadas/dados validados; caminhos de malha independentes da identidade lógica de atores. Nenhum segredo em configs versionadas.

Esta é uma decisão de destino, não uma descrição do que já está implementado. O protótipo Node é mantido em funcionamento até uma migração testada; nenhuma funcionalidade deve ser migrada por simples reescrita sem critério. A migração é justificada porque o requisito exige runtime 3D Godot/C#, servidor .NET dedicado e persistência relacional.

## Componentes e fronteiras

```text
[Cliente Godot 4 + C#]
  cenas 3D / UI / input / predição visual / interpolação
            | HTTPS (conta/lobby) + WSS (intenção/sessão)
            v
[Servidor dedicado ASP.NET Core + .NET]
  autenticação/autorização / simulação / regras / validação / presença
            | transações e migrações
            v
[PostgreSQL prod | SQLite dev]
  contas / personagens / atributos / inventário / moedas / quests / progresso
```

1. O cliente nunca decide dano, recompensa, custo de teleporte, saldo, inventário, nível, posição persistida ou autorização. Envia intenções com identificador/tempo/sequence quando a fase do protocolo definir.
2. O servidor autentica o principal, valida tamanho/formato/frequência, verifica estado do personagem e calcula o resultado. Apenas o servidor confirma mudanças persistentes.
3. Snapshot realtime entrega estado necessário por região/visibilidade; não publicar dados privados de conta/inventário a outros jogadores.
4. Mudanças de mapa, débito/recompensa, compras/trocas e crafting são transações consistentes; mensagens repetidas devem ser idempotentes ou rejeitadas.
5. O cliente pode suavizar renderização/predizer resposta visual, mas reconcilia com estado autoritativo. Não alterar regra de gameplay por animação local.
6. Mapas e conteúdo têm IDs estáveis, requisitos e pontos de entrada validados no carregamento. Região planejada/inexistente não é destino jogável.
7. Escalabilidade horizontal, sharding, cache/filas e particionamento não serão adicionados antes de métricas que os justifiquem. A primeira arquitetura multiplayer deve deixar a autoridade do estado clara e testar contenção.

## Dados e segurança

- Separar conta, sessão e personagem; não enviar hash/segredo ao cliente.
- Senhas: algoritmo de derivação de senha reconhecido, salt único e parâmetros atualizáveis; nunca texto puro.
- Usar constraints (unidade de nome, saldo não negativo, slots únicos etc.) e transações no armazenamento. Usar IDs/versões de operação para evitar repetição de compras, criação e troca.
- Chaves e URLs de produção entram por secret store/ambiente; verificar configuração na inicialização e redigir segredos de logs.
- Validar mensagens em DTOs limitados; rate limit para auth, chat e ações; rejeitar números NaN/infinitos, negativos indevidos, IDs desconhecidos e versão incompatível.
- Definir política de retenção, backup/restauração, logs sem dados sensíveis e plano de incidentes antes do lançamento.

## Cliente, movimento e renderização

A Fase 2 cria shell/login/lobby; Fase 3 migra para cena de mundo 3D e navegação real (NavigationRegion/NavigationAgent ou equivalente aprovado), colisões e limites. Clique esquerdo define objetivo em ponto caminhável; autoridade server-side verifica posição/destino/velocidade e sincroniza. W/A/S/D/Espaço não são gameplay, nem por input map nem por fallback em script. Botão direito solicita ataque na Fase 4; range/LOS/estado/cooldown são verificados pelo servidor.

Modelos, animações, efeitos e layout são carregados como apresentação dos IDs lógicos de classe/criatura/NPC. Manter placeholders reconhecíveis enquanto a arte não estiver pronta; documentar procedência/licença. As referências visuais anexadas estão em `docs/referencias/`.

## Estado de build e ambiente

Na auditoria de 2026-10-27:

- Disponível: Node `v22.22.3`, npm `10.9.8` (confirmados nesta sessão).
- Não encontrado no PATH: Godot/Godot .NET, `dotnet`, `psql`, `postgres`, Docker.
- O único build/execução possível atualmente é o protótipo npm descrito no README. `npm run doctor` detecta presença do Godot 4/.NET e ferramentas opcionais de banco, mas não prova suporte C# nem substitui build. Não dizer que uma solução C#/Godot ou banco foi compilado/testado até executar com os binários reais.

A versão estável mais recente consultada via GitHub API durante a auditoria foi Godot `4.7.2-stable` e .NET SDK `10.0.402`. Antes de fixar isso nos arquivos de solução, verificar compatibilidade do Godot .NET SDK, runtime/target .NET e disponibilidade de versões no ambiente. Reproduzir as versões exatas no `global.json`, projeto Godot e CI quando a ferramenta for provisionada; não rastrear versões móveis (`latest`) em build de release.

## Pastas oficiais

- `client/` — projeto Godot, cenas, scripts, recursos e testes de cliente (estrutura alvo; implementação ainda não existe).
- `server/` — solução ASP.NET Core, domínio, API/realtime, persistência e testes (estrutura alvo; implementação ainda não existe).
- `infra/` — compose/serviços locais, migrações e configurações não secretas de desenvolvimento (estrutura alvo; ainda sem stack executável).
- `docs/adr/` — decisões arquiteturais imutáveis por nova revisão.
- `docs/referencias/` — material visual fornecido pelo usuário e notas de procedência.
- raiz `public/`, `server.js`, `test/` — protótipo web temporário mantido até migração.

## Metas não assumidas sem medição

“FPS máximo” não é meta numérica verificável. Definir cenário/hardware, resolução, perfil gráfico, número de entidades/clientes, p50/p95 e limites de frame-time no planejamento da Fase 13. Medir servidor (tick, CPU, memória, tráfego e latência) e cliente (FPS/stutter) em builds exportados; não usar preview de desenvolvimento como benchmark final.
