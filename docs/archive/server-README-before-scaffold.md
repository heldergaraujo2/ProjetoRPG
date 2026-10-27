# Servidor dedicado — estrutura alvo

Reservado ao servidor ASP.NET Core/.NET LTS aprovado em [ADR-001](../docs/adr/ADR-001.md). Deve ser autoridade de autenticação, regras online e persistência. Não há solução/projeto C# compilável nesta revisão: o SDK `dotnet` não está instalado. A primeira entrega de Fase 1 deve adicionar health checks, configuração segura, testes e persistência/migrações sem confiar no cliente.
