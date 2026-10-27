# 06 — SERVIDOR AUTORITATIVO

## Loop principal
- Tick fixo (padrão 20 Hz). A cada tick: processar entrada de rede → atualizar mundo (IA, movimento, combate, spawns, timers) → enviar snapshots.
- Medir duração do tick; alertar quando passar de 80% do orçamento.
- Dividir o mundo em **shards lógicos por região** que podem rodar em threads diferentes (começar single-thread, particionar quando medição exigir).

## AOI (área de interesse)
- Grade espacial (células ~ 32–64 unidades). Cada entidade pertence a uma célula. Jogador recebe dados das células vizinhas.
- Eventos de entrada/saída da AOI geram Spawn/Despawn.

## Persistência
- Tabelas principais: `accounts`, `characters`, `items` (instâncias), `inventories`/`equipment`, `warehouse`, `quests_progress`, `guilds`, `guild_members`, `friends`, `mail`, `audit_log`, `rankings`.
- Salvar personagem: periodicamente (ex.: 60 s), em eventos importantes (item valioso, troca, refino), e ao sair. Usar transações.
- Item é entidade única com id estável (GUID). Movimentações críticas (trade, drop, venda) são transações atômicas para impedir duplicação.
- Migrations versionadas.

## Contas e sessões
- Registro, login com senha (hash forte: Argon2/bcrypt), limite de tentativas, sessões únicas por conta, token de reconexão curto.
- Contas de GM com papéis (observador, moderador, admin).

## Serviços internos
- `ContentCatalog` (carrega e valida dados), `SpawnManager`, `DropService`, `CombatService`, `ItemService`, `QuestService`, `EventManager`, `GuildService`, `ChatService`, `Scheduler` (eventos por horário), `AuditLog`.

## Admin e operação
- Console de GM + API de administração local. Métricas: jogadores online, tempo de tick, mensagens/s, memória, erros. Exportar para Prometheus/Grafana (opcional).
- Logs estruturados com id de personagem e correlação.
- Backup automático do banco. Procedimento de rollback documentado.

## Carga e escala
- Bots de carga (`Tools/LoadBot`) simulam N jogadores andando e lutando. Definir metas na Fase 13 a partir de medições, não de promessas.
