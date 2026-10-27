# Infraestrutura e banco — pendente

O projeto ainda não tem compose, PostgreSQL, SQLite provider, migrações ou CI. Não existe schema de contas/personagens validado. PostgreSQL permanece alvo de produção; SQLite permanece alternativa local de desenvolvimento, conforme [`docs/adr/ADR-001-stack.md`](../docs/adr/ADR-001-stack.md). Não declarar persistência instalada/testada até adicionar configuração segura, migrações e integração real com ambos os modos previstos.
