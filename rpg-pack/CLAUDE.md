# CLAUDE.md — REGRAS PERMANENTES (leia no início de toda sessão)

## Projeto
RPG online de mundo aberto, cliente + servidor próprios, inspirado em MECÂNICAS de MMORPGs clássicos (ver 03_MECANICAS.md). Tudo (nome, arte, texto, código) deve ser ORIGINAL. Ver 12_LEGAL_ORIGINALIDADE.md.

## Protocolo de sessão (obrigatório)
1. Ler `CHECKPOINT.md`. Executar o "PRÓXIMO PASSO" indicado lá.
2. Trabalhar em tarefas PEQUENAS (cada uma cabe em poucos arquivos e termina com testes passando).
3. Ao concluir CADA tarefa: rodar testes, `git commit`, e atualizar `CHECKPOINT.md` (estado + próximo passo).
4. A cota de uso pode acabar a qualquer momento sem aviso. Portanto: nunca deixe trabalho importante só na conversa. Tudo que importa vai para arquivos e commits.
5. No fim da sessão (ou se perceber que o limite está perto), crie `checkpoints/AAAA-MM-DD_HHMM.md` usando `templates/SESSAO.md` e atualize `CHECKPOINT.md`.
6. Se algo falhar e não for resolvido, registre em CHECKPOINT.md na seção "PROBLEMAS ABERTOS" com o que já foi tentado.

## Regras de engenharia
- Servidor é AUTORITATIVO: o cliente envia intenções; o servidor valida e decide. Nunca confie no cliente.
- Conteúdo é DADO, não código: itens, monstros, NPCs, quests, mapas, drops, skills vivem em arquivos de dados validados por schema (ver 04). Adicionar conteúdo não deve exigir recompilar o servidor.
- Antes de decisão estrutural (stack, formato de dados, protocolo): escrever um ADR curto em `docs/adr/` usando `templates/ADR.md`.
- Não invente: se um requisito está ambíguo, registre a suposição em CHECKPOINT.md e siga com a opção mais simples e reversível.
- Separe fatos, suposições e pendências. Marque com [FATO], [SUPOSIÇÃO], [PENDENTE].
- Cada módulo tem testes. Nada de "funciona na minha máquina": build reproduzível por um comando (`make build`/script).
- Não faça otimização prematura. Meça primeiro (ver 09).
- Mudanças de protocolo de rede incrementam `PROTOCOL_VERSION` e atualizam 05_REDE.
- Não apague nem reescreva checkpoints históricos. Apenas acrescente.
- Commits pequenos com mensagens claras (`feat:`, `fix:`, `docs:`, `test:`, `refactor:`).

## Economia de cota
- Não releia todos os documentos a cada sessão. Leia CLAUDE.md, CHECKPOINT.md, a fase atual e só o necessário.
- Prefira editar trechos a reescrever arquivos inteiros.
- Use o modelo mais leve para tarefas mecânicas e o mais forte para arquitetura e depuração difícil.

## Definição de PRONTO (para qualquer tarefa)
Código + testes passando + documentação mínima + commit + CHECKPOINT atualizado.
