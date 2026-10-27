# 11 — SEGURANÇA E ANTI-CHEAT

## Regras
- Servidor autoritativo para tudo que importa.
- Validação de TODA entrada: tipos, faixas, existência de ids, posse do item, distância, recarga, mana.
- Rate limit por sessão e por opcode. Desconexão e log para abuso.
- Movimento: velocidade máxima, aceleração, colisão, teleporte só por mecanismos autorizados.
- Combate: alcance, linha de visão (se aplicável), recarga, custo.
- Itens: transações atômicas, id único, nenhuma criação de item vinda do cliente.
- Economia: auditoria de transações, alertas para saltos anormais de Zen/itens.
- Contas: hash forte, bloqueio por tentativas, sessão única, 2FA opcional, recuperação segura.
- Rede: TLS/AEAD, token de sessão, anti-replay.
- Banco: queries parametrizadas, privilégios mínimos, backups testados.
- Ferramentas de GM: papéis, log imutável, confirmação em ações destrutivas.
- Detecção heurística: velocidade de ataque impossível, padrões de bot (movimento perfeitamente regular), múltiplas contas do mesmo IP em eventos (configurável).
- Conteúdo de usuário (nomes, chat): filtros e limites.
- Dados pessoais: coletar o mínimo; seguir leis aplicáveis (ex.: LGPD) caso lance publicamente.

## Checklist de segurança (Fase 13)
Fuzz de rede · teste de duplicação de itens · teste de replay · injeção de SQL · limites de recursos (conexões, memória) · rotação de chaves · logs sem dados sensíveis · plano de resposta a incidentes.
