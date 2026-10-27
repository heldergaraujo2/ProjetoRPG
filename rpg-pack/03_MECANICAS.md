# 03 — MECÂNICAS DO JOGO

Estas mecânicas se inspiram em sistemas clássicos de MMORPG (ver 13 para o que a pesquisa confirmou). Todos os NÚMEROS abaixo são pontos de partida configuráveis em `content/config/*.json`, para ajustar sem recompilar. Nomes são originais.

## 1. Atributos e progressão
- 4 atributos base: **Força (FOR), Agilidade (AGI), Vitalidade (VIT), Energia (ENE)**. Classe especial de comando tem o 5º: **Comando (CMD)**.
- Ao subir de nível o jogador ganha pontos livres (padrão: 5 por nível; classes híbrida e comandante: 7).
- Efeitos (referência de design clássico):
  - FOR: dano físico corpo a corpo; requisito de armas/armaduras pesadas.
  - AGI: precisão, velocidade de ataque, defesa, esquiva; dano de arco/besta; requisito de itens leves.
  - VIT: vida máxima e resistência.
  - ENE: mana máxima, regeneração, dano de magias; requisito de magias.
  - CMD: requisito de skills de comando, limite de membros de guilda, controle de montaria/pet.
- Requisitos de atributo para equipar itens e aprender skills (campos `req` nos dados).
- Fórmulas em `Shared/Formulas` e em `content/config/formulas.json` (coeficientes por classe). Testes unitários obrigatórios.
- **Curva de XP:** função configurável (`xp_to_next(level)`), com fator de bônus/penalidade por diferença de nível entre jogador e monstro. Nível máximo configurável (comece em 200).
- **Respec:** item/NPC que redistribui pontos (custo crescente).
- **Reset/renascimento (opcional, fase tardia):** reinicia nível com bônus permanente. Só implementar se o design aprovar.

## 2. Classes (originais; 5 arquétipos)
| Arquétipo | Papel | Atributos chave |
|---|---|---|
| Guerreiro | Corpo a corpo, tanque/dano | FOR, VIT |
| Mago | Dano mágico em área, frágil | ENE |
| Arqueiro | Dano à distância, buffs, esquiva | AGI, ENE |
| Lâmina Arcana (híbrido) | Físico+mágico | FOR, ENE |
| Comandante | Pet/montaria, buffs de grupo, comando | CMD, FOR |
Cada classe tem: árvore de skills, armas permitidas, coeficientes, evolução de classe (ex.: nível 150 → classe avançada via quest).

## 3. Combate
- Tempo real, alvo selecionado por clique, skills com custo de mana e/ou "estamina de skill" (AG), recarga e alcance.
- Dano = (ataque base entre mín e máx do equipamento + bônus de atributo + bônus de skill) − defesa do alvo, modificado por:
  - taxa de acerto vs. taxa de defesa (esquiva) do alvo;
  - **crítico** (chance base + itens), **dano excelente** (proc raro de dano extra), dano duplo;
  - resistência elemental e tipo de dano (físico, mágico, elemental).
- Estados: veneno, lentidão, congelamento, atordoamento, silêncio, sangramento (com duração, empilhamento e **resistência diminuindo a cada aplicação** para evitar stun-lock).
- Buffs/debuffs com ids e prioridade; servidor aplica e expira.
- Morte PvE: perde XP (percentual do nível), pode perder Zen; itens podem cair apenas sob condições (ver PK). Ressurreição na cidade ou no local com item.
- Mortes e danos são sempre calculados no servidor.

## 4. Monstros, spawn e IA
- Definição em dados: id, nível, vida, ataque, defesa, velocidade, alcance de percepção, comportamento (passivo/agressivo/em grupo), skills, tabela de drop, XP, respawn.
- **Spawn:** pontos e áreas por mapa, com quantidade e tempo de respawn; limite por área para performance.
- IA por máquina de estados: ocioso → patrulha → perseguição → ataque → retorno ("leash"). Aggro por dano/proximidade, tabela de ameaça para bosses.
- **Bosses:** fases por % de vida, skills especiais, mecânicas de arena, respawn por agenda ou evento, drop exclusivo, anúncio global.
- Monstros "elite" aleatórios (variante com bônus e drop melhor).

## 5. Itens
- Categorias: armas (espada, machado, maça, lança, arco, besta, cajado, escudo), armaduras (capacete, peitoral, calça, luvas, botas), asas/mochila, anéis, colares, consumíveis (poções, pergaminhos), gemas/materiais, itens de evento, itens de quest, montarias/pets.
- Qualidade/raridade: Comum, Incomum (com opções), **Excelente** (opções especiais aleatórias), **Conjunto/Set** (bônus por peças equipadas), **Lendário/Ancestral**.
- Cada instância de item (no banco) guarda: `item_def_id`, nível de refino (+0..+15), opção extra (nível), sorte (flag), habilidade (flag), opções excelentes (bitmask/lista), soquetes, durabilidade, dono, id único.
- Durabilidade que cai com uso e reparo em NPC pagando Zen.
- Separar **identidade lógica** (stats) de **aparência** (modelo/material), para trocar visual sem mexer no jogo.
- Inventário em grade (ex.: 8x8), baú pessoal (armazém) e baú compartilhado entre personagens da conta.
- Itens "vinculados": ao pegar, ao equipar ou sem vínculo (campo `bind`).

## 6. Refino (melhoria de itens)
Sistema de risco/recompensa, configurável:
- **Gema de Reforço Seguro:** +0 → +6, 100% de sucesso.
- **Gema de Reforço Arriscado:** +0 → +9, ~50% (maior com "sorte"). Falha: reduz nível (configurável: -1 ou volta a +0, por faixa).
- **Máquina de Fusão:** +10 → +15 com várias gemas, chance 40–70% decrescente por nível, custo em moeda. Falha pode destruir ou rebaixar (configurável).
- **Gema de Opção:** adiciona/eleva opção extra (~50%, falha reduz 1 nível da opção).
- Sorte: +chance de refino e +chance de crítico.
- Limites de segurança: itens muito refinados não podem ser largados no chão ao morrer.
- Todas as chances em `content/config/refine.json`. Registrar cada tentativa no log de auditoria.

## 7. Crafting / Fusão
- Receitas em dados (`content/recipes/*.json`): ingredientes (itens com nível mínimo), custo em Zen, chance, resultado, comportamento de falha.
- Exemplos de classes de receita: combinar itens refinados para criar arma especial; criar convite de evento a partir de duas peças + gema; criar asas de nível 1/2/3; criar montaria; criar gema composta.
- Servidor valida tudo; cliente só envia "tentar receita X com estes itens".

## 8. Drop
- Tabelas de drop em dados, referenciadas pelo monstro: `drop_table` com grupos independentes (cada grupo sorteia 0/1 item) + pesos.
- Fatores: nível do monstro vs. nível do item (item só dropa em faixa de nível), raridade, bônus de evento, **multiplicador global** de drop configurável.
- Zen: quantidade em faixa por nível do monstro, com multiplicador.
- Regras de posse: o drop pertence ao autor do dano/party por X segundos, depois fica livre. Itens somem do chão após Y segundos.
- Chance de variantes: item pode nascer com opção, sorte, habilidade ou excelente com probabilidades próprias.
- **Pity timer opcional:** garante drop raro após N tentativas (reduz frustração).
- Telemetria: o servidor registra drops para auditar taxas reais vs. configuradas.

## 9. Economia
- Moeda principal: **Zen** (nome pode mudar). Fontes: drop, venda a NPC, quests. Sumidouros (sinks): reparo, refino, fusão, taxas de loja, resets, taxas de teleporte, impostos de castelo.
- Mercado entre jogadores: troca direta (janela de trade com confirmação dupla) e, mais tarde, casa de leilão.
- Anti-inflação: monitorar Zen total gerado vs. destruído; ajustar sinks por config.
- Log de auditoria de toda transação de valor.

## 10. Mundo aberto
- Mundo dividido em **regiões** conectadas, carregadas em **chunks** no cliente (streaming), com teletransporte rápido entre cidades.
- Tipos: cidade segura (sem PvP), campos de caça, masmorras instanciadas, arenas, áreas de PvP livre.
- Clima/ciclo dia-noite opcionais (Fase 14).
- Cada região define: nível recomendado, regras de PvP, spawns, pontos de teleporte, música.

## 11. Social
- **Party:** até 5–10 membros, divisão de XP com bônus por tamanho, regras de loot (livre, alternado, por dono), chat de party.
- **Guilda:** criar, convidar, cargos, limite de membros (pode depender de CMD do mestre), chat, aliança, rivalidade, armazém, brasão.
- **Chat:** global, região, party, guilda, sussurro, comércio. Filtros, rate limit, silenciar.
- **Amigos/bloqueio.**
- **Troca (trade):** janela segura, ambos confirmam, servidor move os itens atomicamente.

## 12. PvP e PK (resumo; detalhes em 10)
- Zonas: segura / PvP livre / duelo / arena / guerra de guilda.
- **PK (matar jogador neutro):** estados Normal → Agressor → Assassino, com cores de nome, penalidades crescentes (restrição de lojas, mais perda de XP, chance de largar equipamento) e redenção por tempo ou pagamento.
- **Autodefesa:** quem foi atacado pode revidar sem virar PK por um tempo.
- **Herói/Defensor:** recompensa por matar assassinos.

## 13. Eventos (instanciados; servidor roda os scripts)
Modelos de evento (nomes originais; baseados nos tipos clássicos):
1. **Invasão de Fortaleza (tipo "castelo de sangue"):** grupo pequeno (≤10) entra com convite, cumpre objetivos em tempo limite (matar chefe de portão, levar item, derrotar chefe final), recompensa por desempenho. Morte sem perda de XP.
2. **Arena de Ondas (tipo "praça do demônio"):** até 10 jogadores, ondas infinitas de monstros, pontuação por kills, ranking de recompensa. Sem PK.
3. **Castelo do Caos (battle royale):** até ~100 jogadores, equipamento padronizado, o chão desmorona por etapas, o último vence. Armadilhas, taxa de entrada.
4. **Templo das Ilusões (2 times):** objetivo de capturar relíquia, times balanceados por nível, sem perda de itens.
5. **Cerco ao Castelo (guerra de guildas):** 1 dono, atacantes se registram, portões e estátuas reparáveis, torres, coroa a capturar; dono recebe impostos de lojas e controla taxas.
6. **Eventos de mundo:** invasão aleatória de monstros, boss global, hora da recompensa dobrada, caça ao tesouro.
- Entrada via convite craftado, taxa ou nível. Restrições de nível por faixa (`level_band`). Limite de jogadores por instância. Regras: pode ou não usar teleporte, pet, asas, poções.
- Motor de eventos genérico: `Evento = estados + gatilhos + objetivos + recompensas` descrito em dados (ver 04), com ganchos de script quando preciso.

## 14. Quests e NPCs
- Tipos de quest: matar N, coletar N, entregar, falar com, escoltar, explorar, cadeia, diária, repetível, evolução de classe.
- Quest = grafo de estados em dados: pré-requisitos, passos, recompensas (XP, Zen, itens, pontos de atributo, skills).
- NPCs: loja, quest, serviço (reparo, armazém, fusão, teleporte, reset), decorativo. Diálogos em árvore com condições.

## 15. Habilidades (skills)
- Dados: id, classe, nível mínimo, requisitos de atributo, custo, recarga, alcance, tipo (alvo, área, buff, passiva, invocação), fórmula de dano, efeitos de estado, animação/efeito (id visual).
- Skills aprendidas por nível/atributo/livro. Árvore por classe, com evolução por nível de skill.

## 16. Montaria e pets
- Montaria: velocidade, bônus, equipável; pet: auxilia com buff, coleta, ataque. Comandante controla pet de combate.

## 17. Comandos de GM e administração
- Spawn de item/monstro, teleporte, banir/silenciar, anunciar, recarregar conteúdo, mudar multiplicador de XP/drop, iniciar/encerrar evento, ver logs. Tudo auditado.
