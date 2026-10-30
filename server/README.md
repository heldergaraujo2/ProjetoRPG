# Servidor ASP.NET Core/.NET — sessões de mundo Coast

O servidor implementa auth PostgreSQL, personagens account-backed, entrada idempotente em Coast e movimento authoritativo pelo canal WebSocket. É um jogador/sessão individual e **não** um servidor multiplayer validado. O código de C#/Godot e testes PostgreSQL nesta revisão ainda precisam de build/runtime real; verifique o checkpoint antes de tratar como pronto.

## Configuração local

A conexão vem de configuração externa; não armazene `.env`, senhas, bearer ou connection strings em Git/logs.

```powershell
$env:ConnectionStrings__GameDatabase = 'Host=127.0.0.1;Port=5432;Database=projetorpg;Username=projetorpg_dev;Password=<senha-local>;Ssl Mode=Disable'
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:ASPNETCORE_URLS = 'http://127.0.0.1:5050'
dotnet run --project server\ProjetoRpg.Server.csproj -- --db-status
dotnet run --project server\ProjetoRpg.Server.csproj -- --db-migrate
dotnet run --project server\ProjetoRpg.Server.csproj
```

Production requer TLS e `ConnectionStrings__MigrationDatabase` separado para DDL; o runtime usa privilégios mínimos. O app não migra schema automaticamente.

## Migrações

- `0001_initial`: contas/sessões de conta/personagens/auditoria.
- `0002_world_sessions`: tabela aditiva de sessões de mundo e estado autoritativo. Confirme `pending=[]` antes de iniciar gameplay.
- O runner usa advisory lock, transação e checksums; `--db-rollback` exige `--confirm-data-loss`. `0002` deve ser revertida somente em ambiente descartável/backup verificado, pois remove world sessions.

## Rotas

| Método/rota | Autorização | Comportamento |
|---|---|---|
| `POST /api/v1/accounts/register` | auth rate limit | Cadastro sem enumeração |
| `POST /api/v1/sessions` | auth rate limit | Login, token opaco |
| `DELETE /api/v1/sessions/current` | bearer | Revoga sessão de conta |
| `GET/POST /api/v1/characters` | bearer | Lista/cria registros da conta |
| `GET/PATCH /api/v1/characters/{id}` | bearer | Ownership obrigatório |
| `POST /api/v1/world/sessions` | bearer | Recebe apenas `characterId`; 201 novo / 200 retomado; o servidor confere ownership e decide mapa/spawn |
| `DELETE /api/v1/world/sessions/{id}` | bearer | Marca a sessão como encerrada; 204 idempotente |
| `GET /ws` | bearer para gameplay | WebSocket protocol 2; `Hello`, `WorldJoin`, `MoveIntent`, `WorldState`, `Bye` |

Map/spawn liberados: `world.coast` / `coast.initial`, registrados em `content/manifest.json`. O mapa não é selecionável pelo cliente. Posição inicial é configurada no JSON e validada contra 1.849 triângulos derivados da NavigationMesh importada do upstream pinado; o cliente recebe somente o snapshot gerado pelo servidor.

O movimento envia `directionX/Z` e `inputSequence`, nunca posição final, velocidade ou delta. `WorldTickState` fornece a base 20 Hz; o servidor limita um input por tick, no máximo dois ticks por passo, speed 3.5 u/s, normaliza diagonais, verifica sequência/ownership e amostra malha com agente de raio 0.32. Cada estado aceito é persistido por transação PostgreSQL. Desconexão mantém sessão recuperável por 10 min; `WorldJoin` atualiza lease/tick baseline para reconexão mesmo após reinício de processo. O token de conta é revalidado em cada intenção.

A validação por NavigationMesh é uma aproximação server-side; não executa colisão de física 3D. O teste final deve verificar a correspondência com colisores estáticos/ator em Godot/Windows. Não há broadcast, colisão dinâmica entre jogadores nem escala horizontal.

## Saúde e diagnóstico

- `/health/live`: processo.
- `/health/database`: query real `SELECT 1`.
- `/health/ready`: DB e migração atual.
- `/health/tick`: pulso do serviço 20 Hz.
- `/health/content`: versão e quantidade de entries.
- `/health/world`: disponibilidade do map/spawn Coast.

## Integração automatizada

```powershell
npm ci
npm run check
npm run test:phase1
```

`test:phase1` compila server e inicia PostgreSQL descartável em Docker. Inclui migrations, auth/ownership, entrada 201/200, spawn autorizado, WS join autorizado/recusado, velocidade/inputs inválidos/posição arbitrária, caminhada até limite de navegação, desconexão, retomada, reinício e encerramento. Requer SDK .NET 10 e Docker. O script não foi executado nesta sessão porque o Agent não tem `dotnet` nem Docker; esses comportamentos são critérios escritos, não alegações de sucesso.
