# 14 — PROMPTS PRONTOS (copiar e colar no Claude Code)

## A) PROMPT DE INÍCIO DE SESSÃO (use SEMPRE que abrir uma sessão nova)
```
Leia CLAUDE.md e CHECKPOINT.md deste projeto. Resuma em até 10 linhas onde paramos. 
Depois execute o "PRÓXIMO PASSO EXATO" em tarefas pequenas. 
Ao concluir cada tarefa: rode os testes, faça commit e atualize o CHECKPOINT.md.
Se o limite de uso estiver perto, atualize o CHECKPOINT.md e crie o arquivo de sessão em checkpoints/ imediatamente.
Não releia documentos que não forem necessários.
```

## B) PROMPT DA FASE 0 (primeira vez)
```
Estamos na FASE 0. Leia 02_ARQUITETURA.md e 08_FASES.md (seção Fase 0).
1) Escreva docs/adr/ADR-001-stack.md confirmando ou ajustando a stack proposta (decida rápido, com justificativa curta).
2) Inicialize o git e crie a estrutura de pastas.
3) Crie scripts de build e teste únicos e um teste "hello" para servidor e cliente.
4) Preencha em CHECKPOINT.md os comandos reais de build/teste/execução e o próximo passo exato.
Faça commits pequenos.
```

## C) PROMPT PARA AVANÇAR DE FASE
```
A fase atual parece concluída. Verifique os critérios de aceitação da fase em 08_FASES.md um por um, 
execute os testes e gere docs/reports/faseNN.md com resultados e limitações. 
Se algum critério falhar, liste o que falta e corrija antes de avançar. 
Só quando tudo passar, marque a fase como CONCLUÍDA no CHECKPOINT.md e defina o primeiro passo da próxima fase.
```

## D) PROMPT PARA ADICIONAR CONTEÚDO
```
Adicione [descreva: ex. 5 espadas novas, o monstro X, o boss Y, uma quest Z] usando APENAS arquivos em content/ (sem alterar código). 
Siga os exemplos de data_examples/ e rode o ContentValidator. Mostre como testar no jogo.
```

## E) PROMPT PARA CORRIGIR BUG
```
Reproduza este bug: [descrição]. Escreva primeiro um teste que falha, depois corrija, depois rode toda a suíte. 
Commit e atualize CHECKPOINT.md se mudar algo relevante.
```

## F) PROMPT QUANDO A SESSÃO ANTERIOR TRAVOU NO MEIO
```
A sessão anterior foi interrompida. Rode git status e git log -5, compare com o CHECKPOINT.md, 
verifique se há trabalho não commitado, rode os testes e descreva o estado real. 
Corrija inconsistências entre código e CHECKPOINT.md antes de continuar.
```

## G) PROMPT DE BALANCEAMENTO
```
Rode a simulação de duelos em massa (bots, semente fixa) para todas as classes e gere tabela de taxa de vitória. 
Proponha ajustes SOMENTE em content/config/pvp.json e formulas.json, rode de novo e compare antes e depois.
```

## H) PROMPT DE REVISÃO DE SEGURANÇA
```
Faça uma revisão de segurança seguindo 11_SEGURANCA.md: procure qualquer lugar onde o servidor confia no cliente, 
possibilidade de duplicar item, falta de rate limit e validação de ids. Liste achados por gravidade e corrija os críticos com testes.
```
