# ProjetoRPG — MMORPG de fantasia sombria

**Estado real:** protótipo online 2D em Canvas/Node.js; ainda não é um MMORPG 3D, não tem contas nem persistência. A arquitetura-alvo é cliente Godot 4 com C#, servidor dedicado .NET/C# e PostgreSQL (SQLite no desenvolvimento). Consulte [`CHECKPOINT.md`](CHECKPOINT.md) antes de continuar.

Este README e os documentos ligados abaixo são a referência do projeto. O escopo aqui foi consolidado a partir da missão fornecida em 2026-10-27, pois a auditoria não encontrou README de roadmap, CHECKPOINT ou arquitetura anteriores. Funcionalidades marcadas como planejadas não devem ser descritas como prontas.

## Visão e princípios

- MMORPG 3D online, com mundo compartilhado e regiões expansíveis.
- Servidor autoritativo: cliente envia intenções; servidor valida estado, combate, progressão, inventário, moedas e permissões.
- Persistência de contas, personagens e progresso. PostgreSQL para produção; SQLite para desenvolvimento local.
- Arte original/licenciada. Placeholders geométricos são aceitos durante implementação, identificados como temporários e substituíveis por malhas.
- Implementação incremental, seguindo as Fases 0–14 e critérios de aceite. Não declarar uma fase concluída só porque compila.
- Integridade, operação multiplayer, segurança, desempenho medido e documentação atualizada fazem parte do produto.

## Arquitetura e decisões

- [`docs/02_ARQUITETURA.md`](docs/02_ARQUITETURA.md) — componentes, protocolo, dados e limites.
- [`docs/adr/ADR-001.md`](docs/adr/ADR-001.md) — decisão de stack e justificativa.
- [`CHECKPOINT.md`](CHECKPOINT.md) — fase, estado, bloqueios e próximo comando de trabalho.
- [`docs/CHANGELOG.md`](docs/CHANGELOG.md) — histórico verificável.

**Stack aprovada para implementação:** Godot 4.x com .NET para o cliente; ASP.NET Core/.NET LTS para o servidor dedicado; PostgreSQL em produção e SQLite em desenvolvimento. O protótipo Node/Canvas atual é preservado como evidência e referência temporária, não é a arquitetura final. A auditoria em 2026-10-27 encontrou Node 22/npm 10, mas não encontrou Godot, .NET SDK, PostgreSQL client/server nem Docker no ambiente; por isso build/teste reais da stack-alvo ainda são bloqueados até provisionar essas ferramentas.

## Fases e critérios de aceite

Uma fase só passa a **concluída** quando os critérios aplicáveis estão implementados, testados e registrados em `CHECKPOINT.md`.

### Fase 0 — Preparação
- Auditar repositório e ferramentas sem apagar trabalho válido.
- Confirmar stack, registrar ADR-001, estruturar pastas, documentar versões e comandos reproduzíveis.
- Testes iniciais do cliente e servidor; registrar limitações do ambiente.
- **Aceite:** decisões, layout, comandos e resultados verificáveis; bloqueios explicitados.

### Fase 1 — Fundação do servidor
- Solução .NET, configuração segura, logs/health, persistência PostgreSQL/SQLite e migrações.
- Modelo inicial de conta/personagem, validação de protocolo, limites de mensagem e sessões.
- **Aceite:** teste de integração sobe servidor e banco, cria/le/reverte dados e rejeita requests inválidos; nenhum segredo em código.

### Fase 2 — Cliente base e login
- Projeto Godot C#, fluxo real de carregamento, cadastro/login via servidor, expiração/revogação de sessão.
- Lobby: criação de classe/nome, seleção de personagens persistidos e entrada no mundo.
- **Aceite:** fluxo completo em cliente novo e tratamento testado de credenciais erradas, desconexão, timeout e duplicação concorrente.

### Fase 3 — Mundo e movimento
- Cliente 3D e cena de mundo, navegação/collision, mapas orientados a dados, lista de regiões e teleporte server-side.
- Clique esquerdo move até o chão por navegação real; **W/A/S/D/Espaço não executam gameplay**.
- Teleporte verifica mapa, nível, Coins e permissões; débito e troca de mapa são atômicos.
- **Aceite:** movimento autoritativo e sincronizado em pelo menos dois clientes; obstáculos/limites respeitados; destinos indisponíveis claramente marcados.

### Fase 4 — Combate, monstros e habilidades
- Clique direito seleciona/confirma inimigo e solicita ataque; servidor verifica alvo, alcance, estado, cooldown e linha de visão aplicável.
- IA, vida/mana, dano, morte, respawn, ataques/habilidades e efeitos de gameplay reais.
- **Aceite:** testes de regras e multiplayer provam estado/dano produzido no servidor, não animação local.

### Fase 5 — Itens, inventário, drops e lojas
- Inventário funcional, grade, slots/equipamentos, seleção/inspeção, raridade, quantidades, empilhar/separar, consumíveis e feedback.
- Drops, coleta, lojas, compra/venda, capacidade e persistência validadas no servidor.
- **Aceite:** testes de concorrência/abuso impedem duplicação, saldo negativo e quantidade inválida; UI apresenta os dados reais.
- Referências visuais do usuário: [`docs/referencias/`](docs/referencias/README.md).

### Fase 6 — Progressão e classes
- Experiência, nível, pontos distribuíveis e painel P com dados persistidos.
- Cinco atributos canônicos: **Fúria, Defesa, Mana, Agilidade, Sabedoria**. Revisar os dados legados FOR/AGI/VIT/ENE/CMD, se aparecerem; mapear/substituir em documentação, nunca manter sistemas paralelos.
- Definir e documentar fórmulas, limites e diferenças entre classes; implementar estatísticas derivadas e testar cada atributo em gameplay.
- **Aceite:** distribuição transacional, sem pontos negativos/excedentes, refletida em combate e persistência.

### Fase 7 — Social
- Chat e presença persistentes/validados conforme escopo; party, convites, grupos, guildas e trocas seguras entre jogadores.
- **Aceite:** permissões e operações concorrentes testadas; sessão/logout não deixam estado inconsistente.

### Fase 8 — Refino, crafting e opções
- Receitas, materiais, refino, taxas/resultados definidos, opções do cliente e persistência.
- **Aceite:** consumo/recompensa atômicos no servidor e testes para falhas/repetição.

### Fase 9 — PvP, PK e ranking
- Regras por área, PK, consequências, ranking derivado de dados confiáveis e ferramentas de moderação.
- **Aceite:** combate PvP e cálculos validados pelo servidor; não aceitar ranking submetido pelo cliente.

### Fase 10 — Quests, NPCs e bosses
- Diálogos, objetivos, estado de quests, recompensas, bosses e encontros multiplayer.
- **Aceite:** progresso/recompensa persistem, são idempotentes e dependem de objetivos realmente concluídos.

### Fase 11 — Eventos
- Eventos temporizados/configuráveis, participação e recompensas integradas ao mundo persistente.
- **Aceite:** início/fim, reconexão e elegibilidade testados no servidor.

### Fase 12 — Ferramentas de conteúdo
- Validação/importação de mapas, NPCs, itens, criaturas, quests e eventos; erros legíveis e versionamento de conteúdo.
- **Aceite:** conteúdo inválido é detectado antes de entrar no servidor; adicionar região não exige duplicar lógica do cliente.

### Fase 13 — Performance, segurança e QA
- Profiling cliente/servidor, testes de carga, limites de frequência/bytes, observabilidade, recuperação e threat review.
- **Aceite:** metas de concorrência e FPS definidas e medidas em hardware/ambiente declarados; regressões e riscos documentados.

### Fase 14 — Polimento e lançamento
- Arte/animação/áudio licenciados, acessibilidade, localização, UX, infraestrutura de produção, backups e runbooks.
- Tela de carregamento de fantasia sombria com arte original/licenciada (sem falsificar progresso); estado real, timeout, erro e reconexão.
- **Aceite:** checklist de lançamento, smoke/load/security tests e documentação de operação aprovados.

## Controles obrigatórios

- **Botão esquerdo:** mover por navmesh até ponto válido, respeitando colisões, obstáculos, limites e sincronização do servidor.
- **Botão direito:** atacar alvo válido; validação autoritativa conforme Fase 4.
- **W, A, S, D e Espaço:** sem qualquer ação de gameplay; remover mapeamentos na engine, scripts e configurações, não apenas esconder da UI.
- **I:** inventário. **P:** painel do personagem. **M:** lista de mapas. Estas são exceções exclusivamente para interfaces.
- **Enter/Esc** podem manter funções de interface não relacionadas a gameplay, desde que não conflitem com o contrato acima.

## Atributos e dados de jogo

Não foi encontrado sistema de atributos anterior na auditoria desta revisão. Os cinco atributos acima são o conjunto canônico futuro. Fórmulas exatas, cap e progressão devem ser definidos antes de implementar a distribuição de pontos na Fase 6, versionados em documentação e testes. O servidor é a fonte de verdade para estatísticas, custo de recurso e resultado de ação.

## Desenvolvimento local — protótipo atual

O código funcional existente antes da migração usa Node.js 22, npm 10 e `ws` 8.18; isto executa **somente o protótipo 2D**, não o cliente Godot nem a arquitetura final:

```bash
npm ci
npm run doctor   # detecta as ferramentas alvo; retorna erro enquanto faltarem
npm run check
npm start
```

O servidor de protótipo usa `PORT` (padrão 3000), aceita dois navegadores para verificar a sincronização efêmera e escuta `0.0.0.0`. `npm run check` executa syntax-check e testes Node. `npm run doctor` é diagnóstico de presença, não substitui build nem confirma que Godot suporta C#; não há build Godot/.NET reproduzível neste ambiente até instalar/verificar os SDKs adequados.

## Segurança e conteúdo

Nunca guardar senhas em texto puro, secrets em Git/logs, nem confiar no cliente para dano, Coins, loot, teleporte ou inventário. Usar hashing de senhas reconhecido, transações/constraints no banco, validação de payload, rate limits e logs sem credenciais. Manter origem/licença de assets em documentação; assets anexados pelo usuário são referências visuais, não automaticamente texturas finais.
