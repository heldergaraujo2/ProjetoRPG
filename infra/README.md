# PostgreSQL local e validação

A stack de persistência em execução é PostgreSQL via Npgsql `10.0.0`; SQLite continua uma possibilidade futura do ADR, mas não tem provider nem migrações e não está coberto pelo teste. O fluxo automatizado usa Docker e banco descartável.

## Teste de integração completo

Na raiz do repositório, com .NET SDK 10 e Docker Engine/Desktop ativos:

```powershell
npm ci
npm run test:phase1
```

O runner cria container PostgreSQL temporário em porta loopback aleatória, gera senha aleatória em memória, não monta volume, executa migrações/testes e remove o container em `finally`. O log de sucesso registra a versão do PostgreSQL e o image ID resolvido. Para reproduzir exatamente uma execução anterior, forneça a mesma imagem por `PROJETORPG_TEST_POSTGRES_IMAGE` (tag ou digest aprovado). Não use o runner contra banco com dados de jogador.

## Desenvolvimento manual (PowerShell)

Inicie ou use um PostgreSQL de desenvolvimento local e passe conexão pela variável somente na sessão:

```powershell
$env:ConnectionStrings__GameDatabase = 'Host=127.0.0.1;Port=5432;Database=projetorpg;Username=<usuario-local>;Password=<senha-local>;Ssl Mode=Disable'
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet run --project server\ProjetoRpg.Server.csproj -- --db-status
dotnet run --project server\ProjetoRpg.Server.csproj -- --db-migrate
```

Depois, configure `$env:ASPNETCORE_URLS = 'http://127.0.0.1:5050'` e execute `dotnet run --project server\ProjetoRpg.Server.csproj`. Não salve os valores de connection string em arquivos commitados. Em produção use `Ssl Mode=VerifyFull` (ou VerifyCA), secret manager/variáveis gerenciadas, usuário runtime com privilégios mínimos e conexão `ConnectionStrings__MigrationDatabase` separada para executar DDL. O runtime precisa de `CONNECT`, `USAGE` no schema, leitura de `schema_migrations`, SELECT/INSERT/UPDATE/DELETE nas tabelas de contas/sessões/personagens conforme rotas usadas e `INSERT` + `USAGE` na sequência do audit; não deve receber `CREATE`, `ALTER`, `DROP` ou ownership.

`dotnet run --project server\ProjetoRpg.Server.csproj -- --db-rollback --confirm-data-loss` desfaz somente a migração mais recente e a versão inicial apaga tabelas/dados. Só use em ambiente descartável ou após backup restaurável confirmado; não é operação normal de atualização.
