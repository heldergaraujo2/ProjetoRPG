# 08 — ROTEIRO EM FASES

Regras: uma fase só termina quando TODOS os critérios de aceitação passam em teste reproduzível. Ao terminar, gerar `docs/reports/faseNN.md`, atualizar CHECKPOINT.md e só então avançar. Dentro de cada fase, divida em tarefas pequenas (1 sessão = várias tarefas, cada uma com commit).

Estimativa de cota: as fases 0–6 produzem o MVP jogável. Prioridade máxima é chegar lá cedo (fatia vertical) e só então expandir.

---
## FASE 0 — Preparação (meta: base pronta para trabalhar)
Tarefas: confirmar stack (ADR-001) · criar repositório git e estrutura de 02 · script único de build e de testes · CI simples (script local) · esqueleto `content/` com schemas vazios · `docs/` com este pacote · arquivo `ASSETS_LICENCAS.md`.
Entregáveis: projeto compila (servidor + cliente vazios), 1 teste passando em cada.
Aceitação: `build` e `test` rodam do zero em máquina limpa; CHECKPOINT preenchido com comandos.

## FASE 1 — Fundação do servidor
Tarefas: loop de tick · rede básica (sessões, handshake, ping) · serialização e testes de ida e volta · contas (registrar, login com hash seguro) · persistência com migrations · ContentCatalog + schemas + ContentValidator · logs e métricas · AuditLog.
Aceitação: um cliente de teste (console) registra, loga, recebe lista de personagens; conteúdo inválido impede o servidor de subir; 20 Hz estável com 0 jogadores e com 100 bots conectados parados.

## FASE 2 — Cliente base e login
Tarefas: projeto Godot · tela de login · seleção e criação de personagem · conexão com servidor · cena de mundo com chão plano e placeholders · câmera e entrada.
Aceitação: do cliente, criar conta → criar personagem → entrar no mundo e ver o próprio personagem (placeholder).

## FASE 3 — Mundo e movimento
Tarefas: formato de mapa (04) · carregador de mapas no servidor (colisão, navegação) · grade AOI · MoveIntent com validação e correção · cliente com previsão + interpolação · chunks e streaming no cliente · portais e troca de mapa · cidade segura · 2º jogador visível em tempo real.
Aceitação: 2 clientes andam no mesmo mapa e se veem sem teleporte visual; speed hack simulado é corrigido; troca de mapa funciona; 200 bots andando com tick estável.

## FASE 4 — Combate, monstros e skills básicas
Tarefas: fórmulas de combate (Shared, com testes) · monstros em dados · SpawnManager · IA (ocioso/perseguição/ataque/leash) · ataque básico · 3 skills por classe-protótipo · barras de vida/mana · estados (veneno, lentidão) · morte e respawn · XP e nível (curva) · DamageEvent no cliente com números flutuantes.
Aceitação: matar monstro dá XP e sobe nível; monstro persegue, ataca e volta; fórmulas cobertas por testes; simulação determinística de 10.000 combates sem erro.

## FASE 5 — Itens, inventário, drop e lojas
Tarefas: definições de itens em dados · instâncias no banco · inventário em grade + equipamento · requisitos de atributo · durabilidade · DropService e tabelas · itens no chão com posse temporária · NPC de loja (comprar/vender/reparar) · armazém · UI de inventário e equipamento · composição visual de equipamentos (placeholders).
Aceitação: matar monstro → item dropa conforme tabela → pegar → equipar muda stats; teste de 1M de drops dentro da tolerância; impossível duplicar item (teste de estresse com desconexões no meio da ação).

## FASE 6 — Progressão e classes (FIM DO MVP)
Tarefas: atributos e pontos livres · efeitos dos atributos (fórmulas) · 3 classes completas com árvore de skills · aprender skills por nível/atributo · respec · penalidade de morte configurável · UI de atributos e skills · evolução de classe (quest simples).
Aceitação: jogador novo joga ~1 hora seguida com progressão coerente; build de FOR e build de ENE se comportam diferente por testes; relatório do MVP.
**Marco: faça um playtest humano aqui antes de continuar.**

## FASE 7 — Social
Tarefas: chat (global/região/party/guilda/sussurro) · party com divisão de XP e regras de loot · amigos/bloqueio · troca segura entre jogadores · guilda (criar, convidar, cargos, chat, armazém).
Aceitação: troca nunca duplica nem perde item mesmo com queda de conexão; testes de concorrência de party; moderação básica (silenciar).

## FASE 8 — Refino, crafting e opções
Tarefas: refino +0..+15 com gemas (config `refine.json`) · opção extra e sorte · itens excelentes (opções aleatórias) · conjuntos com bônus · receitas em dados · UI de refino e fusão · log de auditoria de cada tentativa.
Aceitação: simulação de 1M de refinos bate com as chances configuradas; falhas aplicam a regra certa; receitas novas só por dados.

## FASE 9 — PvP, PK e balanceamento (ver 10)
Tarefas: zonas de PvP · estados PK e autodefesa · penalidades e redenção · duelos · arenas e fila · ranking · normalização de PvP (coeficientes separados do PvE) · limites anti-one-shot e anti-stun-lock · telemetria de balance.
Aceitação: bots de classes diferentes em duelos simulados em massa não passam de faixas de vitória aceitáveis (ex.: nenhuma classe >58% ou <42% no mesmo nível/equipamento comparável); PK segue a tabela de estados.

## FASE 10 — Quests, NPCs e bosses
Tarefas: motor de quests por grafo · diálogos em árvore · tipos de quest (matar, coletar, entregar, escolta, diária) · NPCs de serviço · bosses com fases e tabela de ameaça · bosses globais com respawn agendado e anúncio · quest de evolução de classe.
Aceitação: criar uma quest nova e um boss novo somente com arquivos de dados e vê-los funcionar sem recompilar.

## FASE 11 — Eventos
Tarefas: motor genérico de eventos (estados, gatilhos, objetivos, recompensas) · instâncias com limite de jogadores · evento 1 (fortaleza de grupo) · evento 2 (arena de ondas) · evento 3 (battle royale) · evento 4 (2 times) · scheduler por horário · convites craftados · depois: cerco ao castelo (registro de guildas, portões, estátuas, coroa, impostos).
Aceitação: cada evento roda do início ao fim com bots; recompensas corretas; falha de um jogador não quebra a instância; cerco tem teste de carga com dezenas de bots.

## FASE 12 — Ferramentas de conteúdo
Tarefas: ContentEditor (web ou desktop) para itens, monstros, NPCs, quests, drops, mapas simples · validação ao salvar · hot reload · painel de GM · gerador de relatórios de balanceamento · importador de assets.
Aceitação: um designer não técnico cria item + monstro + drop + quest pela ferramenta e testa no jogo em minutos.

## FASE 13 — Performance, segurança e QA
Tarefas: perfil do servidor e do cliente · metas de carga baseadas em medição (LoadBot) · particionamento de regiões se necessário · otimizações medidas (culling, LOD, instancing, carga assíncrona) · auditoria de segurança (11) · testes de caos (quedas, lag, pacotes duplicados) · backups e restauração testados.
Aceitação: relatório de desempenho reproduzível; metas definidas e batidas; checklist de segurança 100%.

## FASE 14 — Polimento e lançamento
Tarefas: arte final e UI final · áudio · tutorial/onboarding · clima e ciclo dia-noite · localização (pt-BR/en) · telemetria de balanceamento · beta fechado · correções · instalador e launcher com atualização.
Aceitação: beta com jogadores reais sem bugs críticos abertos.

---
## Regras de ouro do roteiro
1. Fatia vertical primeiro: ver um personagem andar, lutar, dropar e evoluir (Fases 0–6) antes de sistemas avançados.
2. Cada fase termina com relatório + CHECKPOINT atualizado.
3. Escopo novo vai para a "Lista de Ideias" no CHECKPOINT; só entra no roteiro por decisão explícita.
4. Se uma fase ficar enorme, quebre em sub-fases (ex.: 4a monstros, 4b skills, 4c estados).
