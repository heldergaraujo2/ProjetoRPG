# PLANO MESTRE — PROJETORPG

Objetivo: construir um MMORPG 3D online original, com cliente, servidor dedicado autoritativo, persistência, mundo compartilhado, combate, progressão, itens, economia, sistemas sociais, PvP, quests, eventos e ferramentas de operação. Este plano organiza as 15 fases oficiais de README.md e rpg-pack/08_FASES.md; não declara funcionalidades implementadas.

## Fontes de verdade
1. README.md: visão, regras e ordem geral.
2. rpg-pack/CHECKPOINT.md: estado operacional e próximo passo exato. É o checkpoint oficial.
3. rpg-pack/CLAUDE.md: protocolo permanente do agente.
4. rpg-pack/08_FASES.md: escopo e aceite macro.
5. Documentos especializados em rpg-pack/: arquitetura, mecânicas, dados, rede, servidor, cliente, testes, PvP, segurança e legalidade.
6. docs/phases/PHASE-NN.md: decomposição complementar.

Conflitos devem ser registrados e resolvidos com evidências e ADR, nunca escolhidos silenciosamente.

## Roadmap oficial
| Fase | Nome | Resultado |
|---|---|---|
| 0 | Preparação | Stack validada, arquitetura registrada, estrutura e build/teste reproduzíveis |
| 1 | Fundação do servidor | Servidor autoritativo, rede, contas, persistência e catálogo validado |
| 2 | Cliente base e login | Cliente conectado, cadastro, login e entrada no mundo |
| 3 | Mundo e movimento | Mapa compartilhado, movimento validado, AOI e sincronização |
| 4 | Combate, monstros e skills | Loop PvE, IA, dano, morte, XP e skills iniciais |
| 5 | Itens, inventário, drop e lojas | Loot, coleta, equipamento, lojas e persistência segura |
| 6 | Progressão e classes | Atributos, níveis, skills e três classes; fim do MVP |
| 7 | Social | Chat, party, amigos, guildas e troca segura |
| 8 | Refino, crafting e opções | Refino, gemas, crafting e opções especiais |
| 9 | PvP, PK e ranking | Regras competitivas, PK, arenas/duelos e ranking |
| 10 | Quests, NPCs e bosses | Motor de quests, NPCs e conteúdo PvE |
| 11 | Eventos | Eventos instanciados, invasões, bosses globais e cerco |
| 12 | Ferramentas de conteúdo | Validador/editor, ferramentas GM e painel administrativo |
| 13 | Performance, segurança e QA | Capacidade medida, segurança e recuperação testadas |
| 14 | Polimento e lançamento | Beta/release reproduzível, operação e licenças verificadas |

Fases 0–6 formam o MVP; ao fim da Fase 6 deve ocorrer playtest humano. Fase 14 é a primeira versão lançável, não o fim da evolução.

## Ciclo obrigatório
Ler README, checkpoint, regras, fase autorizada e documentos relevantes; verificar branch e git status; mapear requisitos para arquivos e testes reais; planejar tarefas pequenas; implementar e integrar preservando trabalho existente; executar build/testes/regressões; corrigir e repetir; registrar evidências em docs/reports/faseNN.md; atualizar rpg-pack/CHECKPOINT.md com histórico append-only; relatar PLANEJADO, IMPLEMENTADO, TESTADO, BLOQUEADO e CONCLUÍDO separadamente; parar ao fim da fase autorizada e aguardar autorização para a próxima.

Dentro da fase liberada, trabalhar autonomamente nas tarefas seguras. Não parar por trivialidades. Se houver bloqueio, concluir trabalho independente e registrar evidências e próximo passo.

## Qualidade e segurança
Servidor é autoridade sobre posição, dano, inventário, economia, drops e recompensas. Mocks/placeholders não provam funcionalidade de produção. Build verde não prova gameplay. Testes não executados nunca são aprovados. Operações econômicas devem ser transacionais, idempotentes e auditáveis. Não versionar segredos, executar operações destrutivas, publicar serviços ou migrar dados reais sem autorização.

## Originalidade
Usar apenas ideias gerais do gênero. Não copiar código, assets, mapas, textos, marcas, nomes próprios ou dados proprietários. Consultar rpg-pack/12_LEGAL_ORIGINALIDADE.md e manter registros de licenças.

## Estado documental
A documentação de planejamento está na branch docs/plan, separada de main. O checkpoint original aponta Fase 0 como NÃO INICIADA. Nenhuma implementação de gameplay foi realizada nesta alteração. Próxima ação: executar a auditoria e validação descritas em docs/phases/PHASE-00.md.
