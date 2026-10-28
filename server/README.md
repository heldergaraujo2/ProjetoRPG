# Servidor ASP.NET Core/.NET — Fase 1 em andamento

O servidor continua sendo fundação, não um MMORPG jogável. A Fase 0 foi confirmada pelo usuário no Windows (builds limpos e smokes registrados em `CHECKPOINT.md`); as mudanças de Fase 1 abaixo ainda não foram compiladas nesta sessão.

## Configuração e execução local

A conexão PostgreSQL é obrigatória e só vem de configuração externa. Não grave connection strings, `.env`, senhas ou tokens no Git. Em PowerShell, defina a variável apenas na sessão local:

```powershell
$env:ConnectionStrings__GameDatabase = 'Host=127.0.0.1;Port=5432;Database=projetorpg;Username=projetorpg_dev;Password=<senha-local>;Ssl Mode=Disable'
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:ASPNETCORE_URLS = 'http://127.0.0.1:5050'
dotnet run --project server\ProjetoRpg.Server.csproj -- --db-status
dotnet run --project server\ProjetoRpg.Server.csproj -- --db-migrate
dotnet run --project server\ProjetoRpg.Server.csproj
```

`ConnectionStrings__GameDatabase`, `Auth__SessionLifetimeMinutes` (5–10.080; padrão 720) e `Auth__RequestsPerMinute` (1–600; padrão 12) são nomes de variáveis de ambiente/configuração. Em Production, `GameDatabase` exige TLS com `SslMode=VerifyFull` ou `VerifyCA`. O comando de migração em Production também exige `ConnectionStrings__MigrationDatabase`, uma conexão distinta com permissões DDL; o servidor usa somente a conexão runtime.

## Migrações PostgreSQL

O provider é Npgsql `10.0.0`, fonte oficial cuja release declara alvos `net8.0`, `net9.0` e `net10.0`; o servidor continua em `net10.0`. A migração `server/Persistence/Migrations/0001_initial.{up,down}.sql` cria contas, sessões, personagens e eventos de auditoria com PKs, FKs, constraints e índices. `schema_migrations` guarda versão, nome e checksums das migrações. O runner usa lock advisory do PostgreSQL, transação e checksum normalizado para LF; reaplicar é idempotente e edição de migração aplicada é recusada.

```powershell
dotnet run --project server\ProjetoRpg.Server.csproj -- --db-status
dotnet run --project server\ProjetoRpg.Server.csproj -- --db-migrate
```

O app **não migra automaticamente** no startup. Faça backup antes de qualquer mudança de schema. O comando `--db-rollback` só desfaz a última migração e exige `--confirm-data-loss`; a migration `down` inicial apaga todas as tabelas de domínio. Use rollback somente em banco descartável/vazio ou após backup restaurável confirmado. Não é uma rotina de downgrade segura para dados de jogadores.

Para um banco PostgreSQL local de desenvolvimento, pode-se iniciar um container descartável com Docker ou usar uma instalação local; mantenha credenciais apenas em variáveis de ambiente. O teste automatizado abaixo cria seu próprio container temporário sem volume e o remove ao terminar.

## Saúde e erros

- `GET /health/live`: processo HTTP ativo; não depende do banco.
- `GET /health/database`: executa `SELECT 1` no PostgreSQL; 200 só quando a conexão funciona.
- `GET /health/ready`: exige banco acessível e migração atual; responde 503 distinguindo indisponibilidade, schema pendente ou inválido.
- Erros não tratados retornam JSON genérico com `traceId`; exceções de DB não expõem connection string/SQL parâmetros. Console usa logging JSON estruturado. Rate limit fixo por IP é aplicado a cadastro/login.

## Contas, sessões e personagens implementados (ainda não validados por build)

- `POST /api/v1/accounts/register` aceita username ASCII de 3–32 caracteres e senha de 12–128 caracteres. Cadastro usa `PasswordHasher<TUser>` do ASP.NET Core Identity (hash versionado PBKDF2); nunca grava senha em texto puro. Cadastro duplicado responde igual ao novo para evitar enumeração.
- `POST /api/v1/sessions` retorna token bearer opaco aleatório de 256 bits. Só SHA-256 do token é persistido; expiração vem do relógio do banco, revogação é via logout. Username inexistente e senha incorreta retornam a mesma resposta.
- `DELETE /api/v1/sessions/current` revoga a sessão atual (idempotente).
- `GET/POST /api/v1/characters`, `GET/PATCH /api/v1/characters/{id}` operam sempre sob `account_id` autenticado; personagem alheio é indistinguível de inexistente. Nome é normalizado e único.
- Não existe classe implementada no catálogo de produção (o manifest atual está vazio); personagem pode ser persistido sem classe. Um `classId` só é aceito quando existe uma entrada `kind: class` real e validada. Não há gameplay/classificação fictícia nesta fase.
- Audit log guarda tipos e IDs de conta, sem campos de username, senha, token, IP ou payload.

## Validação de Fase 1

```powershell
npm ci
npm run check
npm run test:phase1
```

`npm run test:phase1` compila/restaura o servidor e cria PostgreSQL descartável `postgres:18-alpine` com credencial aleatória temporária, sem volume persistente. Testa configuração ausente, banco indisponível, readiness/liveness, migração e rollback seguro, cadastro/login, mensagens de erro equivalentes, hash, expiração/revogação, isolamento e persistência de personagem após reiniciar o servidor, auditoria, conteúdo inválido e protocolo WebSocket. Docker Desktop/Engine e SDK .NET 10 são necessários. O script informa a versão e image ID resolvida; use `PROJETORPG_TEST_POSTGRES_IMAGE` para fornecer um tag/digest aprovado pela sua organização.

Teste separado de 100 conexões WebSocket paradas:

```powershell
$env:PROJETORPG_LOAD_URL = 'http://127.0.0.1:5050'
$env:PROJETORPG_LOAD_CLIENTS = '100'
$env:PROJETORPG_LOAD_DURATION_SECONDS = '60'
npm run load:parked
```

Esse script exige servidor migrado e PostgreSQL acessível. Mede somente handshake/conexões WebSocket ociosas: não autentica contas, cria jogadores, executa simulação ou prova capacidade de produção. O código não foi executado nesta sessão por falta de .NET/Docker.

`npm run check` permanece separado e testa apenas o protótipo Node (4 testes) e sintaxe dos scripts; não compila o server nem cobre PostgreSQL.
