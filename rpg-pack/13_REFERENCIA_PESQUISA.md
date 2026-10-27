# 13 — REFERÊNCIA DE PESQUISA (MMORPGs clássicos, resumo parafraseado)

Objetivo: documentar o que se sabe sobre as mecânicas do gênero para inspirar o design. NÃO é fonte de conteúdo para copiar. Os valores variam entre versões e servidores; trate como referência, não como verdade. Marcação: [FATO-FONTE] = visto em fonte consultada; [DESIGN] = decisão deste projeto.

## Atributos
- [FATO-FONTE] Quatro atributos base (força, agilidade, vitalidade, energia); ao subir de nível o jogador distribui 5 pontos livres, e as classes híbrida e de comando ganham 7. Classe de comando tem o atributo extra "comando". Fonte: guias de comunidade (evil-mu.forumgratuit.ro/t86-item-guides-2).
- [FATO-FONTE] Força: dano físico e requisitos de equipamento. Agilidade: precisão, velocidade de ataque, defesa, esquiva e dano de arcos. Vitalidade: vida. Energia: mana, regeneração, dano de skills. Comando: requisitos de skills de comando e limite de membros de guilda.
- [FATO-FONTE] Builds comuns: foco em força, agilidade, vitalidade ou energia conforme a classe (guias de comunidade, cmub.forumgratuit.ro).

## Refino de itens
- [FATO-FONTE] Gema de bênção: +0 a +6, 100%. Gema de alma: até +9, ~50% (maior com sorte); falha reduz nível (regras variam por faixa). Níveis +10 a +15 pelo "goblin do caos" com gema do caos, bênção e alma, chances na faixa de 40–70%. Fonte oficial do jogo: muonline.webzen.com/en/gameinfo/guide/detail/32.
- [FATO-FONTE] Sorte dá +25% de chance de sucesso nos refinos e +5% de crítico (mesma fonte).
- [FATO-FONTE] Gema da vida adiciona/eleva a opção extra (~50%; falha reduz 1 nível da opção).
- [FATO-FONTE] Máquina do caos: combina itens +4 com opção +4 e gemas para criar armas especiais, asas, montarias, convites de evento; falhas podem destruir/rebaixar (strategywiki.org/wiki/Mu_Online/Chaos_Machine).
- [FATO-FONTE] Sistema de soquetes/sementes: itens com encaixes recebem "sementes" de elementos (fogo, água, terra, raio, gelo) que dão bônus específicos (guia oficial detail/34).
- [DESIGN] Neste projeto: refino configurável por `refine.json`, nomes de gemas originais.

## Eventos
- [FATO-FONTE] Arena de ondas: até 10 jogadores, entrada via convite criado com duas peças + gema do caos, faixas de nível, sem PK dentro, mortos voltam à cidade (muonline.webzen.com guide/126).
- [FATO-FONTE] Fortaleza de grupo: máx. 10 por sub-servidor de nível, entrada com capa criada por pergaminho + osso + gema, morrer não perde XP/Zen/itens, sobreviventes ganham gema (guide/116).
- [FATO-FONTE] Battle royale do caos: até 100, taxa de entrada, equipamento padronizado, monstros-armadilha, chão que desmorona, vencedor leva prêmio (strategywiki.org/wiki/Mu_Online/Chaos_Castle).
- [FATO-FONTE] Cerco ao castelo: 1 guilda/aliança dona, até 3 times atacantes, 6 portões e 4 estátuas reparáveis, impostos de lojas, coroa dentro da torre protegida por barreira com 2 interruptores (strategywiki.org/wiki/Mu_Online/Castle_Siege).
- [FATO-FONTE] Versões modernas/mobile incluem guild boss, banquete de guilda, leilão de guilda e dungeons por faixa.

## PvP/PK
- [FATO-FONTE] Sistema com autodefesa (quem é atacado pode matar o agressor em tempo limite) e estados de Assassino/Herói; Herói = matar 3+ assassinos sem acionar autodefesa, reduz perda de XP e drop; itens muito refinados, excelentes e asas nunca caem na morte (guias de comunidade).
- [DESIGN] Este projeto usa estados próprios (ver 10) e proteções anti-abuso.

## Outras referências de gênero úteis para inspirar sistemas
- PK com cores de nome e penalidades escalonadas (diversos MMOs). Punições típicas: restrição de lojas, perda de XP, chance de largar item.
- Combate com skills por classe, estados com resistência decrescente, bosses com fases.

## Lacunas conhecidas (pesquisar só se necessário)
Tabelas exatas de XP por nível, chances de drop por monstro, fórmulas de dano exatas e estatísticas de itens variam por versão e servidor e geralmente não são documentadas oficialmente. Este projeto define as próprias e calibra por simulação (09/10).
