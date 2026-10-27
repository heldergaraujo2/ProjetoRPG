# 01 — VISÃO E ESCOPO

## Resumo
RPG online de ação em mundo aberto, com combate em tempo real (clicar/atacar), farm de monstros, itens com refino e opções, drop rico, PvP equilibrado, eventos instanciados, guildas, quests e bosses. Servidor autoritativo e conteúdo 100% extensível por dados.

## Pilares
1. **Loop viciante de progressão:** matar, dropar, refinar, evoluir.
2. **Extensibilidade total:** novo item/NPC/monstro/boss/quest/mapa/evento = novos arquivos de dados, sem mexer no núcleo.
3. **PvP justo:** escolha de build importa mais que "quem gastou mais"; regras de PvP separadas das de PvE.
4. **Mundo aberto:** regiões contínuas com streaming, sem telas de carregamento entre áreas vizinhas (instâncias só para eventos e masmorras).
5. **Identidade própria:** nomes, arte, mundo e lore originais.

## Escopo mínimo (MVP jogável = fim da Fase 6)
Login, 1 personagem, 1 região com 2 cidades pequenas, 15 monstros, 1 boss, ~80 itens, inventário, equipamento, drop, loja, níveis, pontos de atributo, 3 classes, party, chat.

## Escopo completo (fim da Fase 14)
Todas as mecânicas de 03_MECANICAS.md, ferramentas de conteúdo, eventos, PvP ranqueado, guilda com cerco ao castelo.

## NÃO fazer
- Não copiar nomes, arte, textos, sons, mapas, modelos ou código de nenhum jogo existente.
- Não reaproveitar arquivos de cliente de nenhum jogo comercial.
- Não implementar monetização que dê vantagem de combate (pay-to-win).
- Não fazer lógica de jogo no cliente.
- Não construir engine própria; usar engine existente (ver 02).

## Critério de sucesso
Um jogador novo consegue: criar conta, criar personagem, andar pelo mundo, matar monstros, pegar drops, equipar, subir de nível, distribuir pontos, entrar em party, vencer um boss e disputar PvP, tudo estável com N jogadores simultâneos (meta N definida na Fase 13 por medição).
