# 02 — ARQUITETURA (PROPOSTA INICIAL — confirmar com ADR-001 na Fase 0)

## Stack proposta
| Camada | Proposta | Motivo |
|---|---|---|
| Cliente | **Godot 4 (C#)** | Gratuita, leve, 3D suficiente, projeto em texto (bom para o agente editar), exporta para Windows/Linux |
| Servidor | **.NET 8 (C#)**, processo próprio | Mesma linguagem do cliente: dá para compartilhar uma biblioteca `Shared` (protocolo, tipos, fórmulas) |
| Banco | **PostgreSQL** (SQLite no desenvolvimento) | Persistência confiável de contas, personagens, itens |
| Rede | **TCP** para login/chat/transações + **UDP confiável (ex.: ENet/LiteNetLib)** para movimento/combate | Menor latência no combate. Alternativa simples: só TCP/WebSocket no começo |
| Dados | **JSON** validado por JSON Schema (ou YAML) | Legível, editável por ferramentas, validável |
| Testes | xUnit (servidor/shared) + testes de simulação headless | Combate e economia testáveis sem cliente |

Alternativas válidas (registrar no ADR): Unity (C#, ecossistema grande), Unreal (pesada demais para começar), servidor em Go/Rust (mais desempenho, perde o código compartilhado). O agente deve avaliar rapidamente, decidir e seguir. Não gaste a sessão inteira nisso.

## Módulos (cada um com responsabilidade clara e testes)
```
/ (raiz do projeto)
  CLAUDE.md, CHECKPOINT.md, docs/ (este pacote + adr/)
  src/
    Shared/        tipos, enums, protocolo, fórmulas de combate, validação de schemas
    Server/
      Core/        loop de tick, scheduler, logging
      Net/         sessões, serialização, rate limit
      Accounts/    login, personagens
      World/       mapas, regiões, grade espacial (AOI), spawn
      Entities/    jogadores, monstros, NPCs
      Combat/      dano, skills, buffs, status
      Items/       itens, inventário, drop, refino, crafting
      Quests/      motor de quests
      Social/      party, guilda, chat, troca
      Events/      eventos instanciados, cerco
      Persistence/ repositórios, migrations
      Admin/       comandos de GM, métricas
    Client/ (projeto Godot)
      Net/ World/ Entities/ UI/ Audio/ Config/
    Tools/
      ContentValidator/  valida todos os dados
      ContentEditor/     editor web ou desktop simples (Fase 12)
      LoadBot/           bots para teste de carga
  content/            DADOS DO JOGO (itens, monstros, npcs, quests, mapas, drops, skills, eventos)
  tests/
```

## Princípios
- **Data-driven:** `content/` é carregado no start do servidor e pode ser recarregado a quente (comando de GM). O cliente baixa/lê o mesmo catálogo para exibir nomes, ícones, descrições.
- **Autoridade no servidor:** posição, dano, drop, inventário, economia.
- **Tick fixo:** servidor roda a 20 Hz (50 ms) como ponto de partida. Medir e ajustar.
- **AOI (área de interesse):** grade espacial; cada jogador só recebe entidades próximas.
- **Identificadores:** todo conteúdo tem `id` string estável (ex.: `item.sword.iron_01`). Nunca usar índices numéricos que mudam. O banco guarda ids string/GUID.
- **Versionamento:** `PROTOCOL_VERSION` e `CONTENT_VERSION`.
- **Observabilidade:** logs estruturados, métricas de tick, de rede e de jogadores online desde cedo.
