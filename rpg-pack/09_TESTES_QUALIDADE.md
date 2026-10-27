# 09 — TESTES E QUALIDADE

## Pirâmide
1. **Unitários** (Shared/Server): fórmulas de dano, XP, drop, refino, validação de dados, serialização.
2. **Integração:** servidor + banco em memória/SQLite, fluxo login → personagem → entrar no mundo → matar monstro → drop → inventário.
3. **Simulação determinística:** semente fixa de RNG; "1 milhão de mortes de monstro" para conferir taxas de drop; "1 milhão de refinos" para conferir chances.
4. **Carga:** LoadBot com N clientes; medir tick, banda, memória.
5. **Cliente:** testes de UI por cena e capturas de tela de referência quando possível.
6. **Segurança:** fuzz de pacotes, tentativas de duplicação de item, speed hack simulado.

## Regras
- Todo bug corrigido ganha um teste que o reproduz.
- RNG injetável (para teste determinístico).
- Validador de conteúdo roda no CI e antes de cada commit relevante.
- Relatório de teste por fase em `docs/reports/faseNN.md` com: o que foi testado, resultados, limites conhecidos.

## Métricas a acompanhar
Tempo de tick (p50/p95/p99), jogadores por servidor, mensagens por segundo, FPS do cliente em cena padrão, uso de memória, taxa real de drop vs. configurada, Zen criado vs. destruído.
