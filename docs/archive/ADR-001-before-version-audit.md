# ADR-001 — Stack do cliente 3D, servidor dedicado e persistência

- **Estado:** Aceita para a arquitetura-alvo; implantação bloqueada por ausência da toolchain no ambiente auditado.
- **Data:** 2026-10-27.
- **Decisores/requisito:** missão do ProjetoRPG fornecida pelo proprietário do repositório.
- **Relacionado:** [`../02_ARQUITETURA.md`](../02_ARQUITETURA.md), [`../../CHECKPOINT.md`](../../CHECKPOINT.md).

## Contexto

A árvore continha somente um protótipo web: cliente HTML/Canvas 2D, servidor Node/WebSocket, estado em memória e testes simples de dados. Isso é útil para demonstração, mas não satisfaz os requisitos explícitos de cliente 3D, mundo/navegação de engine, serviço dedicado C#, autenticação e progresso persistentes. O dono do projeto propôs Godot 4/C#, servidor .NET/C# e PostgreSQL, com SQLite opcional no desenvolvimento; esses requisitos são coerentes com as fronteiras necessárias e não há requisito que exija manter o runtime web.

A sessão tem Node 22/npm, mas não tem Godot, .NET SDK, PostgreSQL ou Docker disponíveis no PATH. Assim, a decisão de arquitetura é registrada agora; versões de SDK/projetos e qualquer implementação .NET/Godot não serão declaradas validadas antes de executar build/test reais.

## Decisão

1. Usar **Godot 4 com suporte .NET/C#** para cliente desktop 3D e interfaces.
2. Usar **ASP.NET Core sobre uma versão LTS do .NET** em processo servidor dedicado. Servidor é autoridade de login/autorização, simulação de gameplay, inventário/economia e progresso.
3. Usar **PostgreSQL em produção** e **SQLite para desenvolvimento local**, mantendo migrações e semântica transacional coerentes.
4. Usar HTTPS para operações de conta/lobby e WebSocket seguro (WSS) para realtime, conforme desenho do protocolo nas Fases 1–3.
5. Manter o protótipo Node/Canvas existente, sem apagá-lo, até haver substituição funcional e testes de equivalência pertinentes. É referência/prova do loop realtime, não código de autoridade da arquitetura final.
6. Não adotar serviços de cache, broker, clusters ou outros componentes distribuídos sem evidência de profiling e capacidade alvo.

GitHub API consultada em 2026-10-27 reportou `4.7.2-stable` para Godot e `v10.0.402` para .NET SDK. Esses valores são referência para avaliação, não pin de build: verificar compatibilidade específica do Godot .NET SDK e runtime/TFM após instalar as ferramentas; então fixar as versões reprodutíveis.

## Consequências positivas

- Godot oferece cena, navegação, colisão, importação de malhas e renderização 3D coerentes com a meta.
- C# pode ser usado nas duas camadas com tipos/contratos claros; o processo dedicado desacopla lifecycle de clientes.
- Banco relacional oferece constraints e transações para contas, moedas, inventário e operações concorrentes.
- SQLite facilita testes e desenvolvimento sem servidor de banco, sem substituir Postgres no teste de produção.

## Custos e riscos

- Migração do protótipo e disponibilidade de arte 3D elevam o custo inicial.
- Manter clientes Godot e builds de servidor requer provisionar .NET/Godot, CI e runners adequados; esses binários não estão nesta sessão.
- SQLite e PostgreSQL têm diferenças: testes de integração contra Postgres continuam obrigatórios antes de concluir fases de persistência.
- Serviço authoritative exige definir protocolo, clock/tick, interesse de região, reconexão, rate limits e modelos de concorrência; não copiar as simplificações do Node prototype.

## Rejeitadas

- **Manter Node/Canvas como produto final:** rejeitada porque não entrega cliente Godot 3D/C# nem os requerimentos de runtime/arquitetura expressos.
- **Armazenamento apenas em memória ou JSON:** rejeitado para progresso multiplayer persistente e economia concorrente.
- **Microserviços/mensageria desde o início:** rejeitados por aumentar complexidade operacional sem requisito de carga comprovado; começar com servidor dedicado modular e banco relacional.

## Verificação / reconsideração

Após provisionar as ferramentas, criar projetos mínimos Godot .NET e ASP.NET Core, executar builds limpos, testes cliente/servidor e teste de conexão com SQLite/PostgreSQL; registrar versões exatas e CI. Reconsiderar a decisão se um bloqueio técnico/compatibilidade objetivo for demonstrado, por novo ADR (não editar silenciosamente este registro).
