# PACOTE DE PROJETO — RPG ONLINE DE MUNDO ABERTO (inspirado nas mecânicas de MMORPGs clássicos)

Este pacote é o "cérebro" do projeto. Foi feito para o Claude Code (Opus 5.5) ler, entender a estratégia e retomar o trabalho em qualquer sessão sem se perder.

## Como o humano usa este pacote
1. Crie uma pasta vazia para o projeto (ex.: `D:\MeuRPG`) e extraia este ZIP dentro dela.
2. Abra o terminal nessa pasta e rode `claude` (Claude Code).
3. Cole o "PROMPT DE INÍCIO DE SESSÃO" de `14_PROMPTS.md`. É sempre o mesmo, em toda sessão nova.
4. Quando a janela de uso acabar, nada se perde: o estado está em `CHECKPOINT.md` + git.
5. Na próxima janela, cole o mesmo prompt de início. O agente retoma de onde parou.

## Como o agente (Claude) deve usar este pacote
Leia nesta ordem, SEMPRE, no início de cada sessão:
1. `CLAUDE.md` (regras permanentes)
2. `CHECKPOINT.md` (onde paramos, próximo passo exato)
3. O arquivo da fase atual em `08_FASES.md`
4. Só então os documentos de apoio necessários para a tarefa (não leia tudo se não precisar; economiza cota)

## Mapa dos arquivos
| Arquivo | Conteúdo |
|---|---|
| CLAUDE.md | Regras permanentes e protocolo de sessão |
| CHECKPOINT.md | Estado atual, decisões, próximo passo. ATUALIZAR SEMPRE |
| 01_VISAO_E_ESCOPO.md | O que é o jogo, pilares, o que NÃO fazer |
| 02_ARQUITETURA.md | Stack proposta, módulos, estrutura de pastas |
| 03_MECANICAS.md | Todas as mecânicas de jogo (stats, combate, itens, drop, PvP, eventos...) |
| 04_DADOS_EXTENSIVEIS.md | Como adicionar itens, NPCs, monstros, bosses, quests, mapas sem mexer no código |
| 05_REDE_E_PROTOCOLO.md | Protocolo próprio cliente-servidor |
| 06_SERVIDOR.md | Detalhes do servidor autoritativo |
| 07_CLIENTE.md | Detalhes do cliente, UI, mundo aberto |
| 08_FASES.md | Roadmap completo em fases com critérios de aceitação |
| 09_TESTES_QUALIDADE.md | Estratégia de testes |
| 10_PVP_BALANCEAMENTO.md | PvP equilibrado, PK, ranking |
| 11_SEGURANCA.md | Anti-cheat, validações, conta |
| 12_LEGAL_ORIGINALIDADE.md | Regras para não copiar conteúdo protegido |
| 13_REFERENCIA_PESQUISA.md | O que se sabe sobre os MMORPGs clássicos, com fontes |
| 14_PROMPTS.md | Prompts prontos para cada situação |
| ASSETS_LICENCAS.md / THIRD_PARTY.md | Registro de licenças de arte e bibliotecas |
| templates/ | Modelos de ADR, log de sessão |
| data_examples/ | Exemplos JSON de item, monstro, NPC, quest, mapa, drop, skill |
| checkpoints/ | Histórico de sessões (um arquivo por sessão) |

## Aviso honesto sobre expectativas
- O agente escreve código, ferramentas, testes e documentação. Ele NÃO cria arte 3D, animações e sons de qualidade profissional. Use assets próprios, comprados ou de bibliotecas com licença livre (ver 12_LEGAL_ORIGINALIDADE.md).
- Um MMORPG completo leva muitas sessões. Este pacote existe para tornar isso possível em pequenos passos verificáveis.
- Os números de balanceamento são pontos de partida. Precisam de testes de jogo reais.
