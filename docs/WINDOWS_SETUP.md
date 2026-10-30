# Windows — build, testar e validar gameplay Coast

Este guia é para a branch `arena/b2f4e6da-projetorpg` e a integração de sessão Coast. No estado desta revisão, o código foi acrescentado mas ainda não foi compilado/importado ou comprovado em runtime. Faça o teste final somente na cópia descartável `D:\ProjetoRPG-Teste-Lobby`; **não acessar nem modificar `D:\AProjeto RPG Fixo`**.

## 1. Clonar e confirmar branch

Use uma pasta temporária de teste:

```powershell
git clone --branch arena/b2f4e6da-projetorpg --single-branch https://github.com/heldergaraujo2/ProjetoRPG.git D:\ProjetoRPG-Teste-Lobby
Set-Location D:\ProjetoRPG-Teste-Lobby
git status --short --branch
git log -1 --oneline
```

O clone deverá estar limpo e na branch citada. Não executar `reset`, `clean`, checkout destrutivo ou sincronização automática na cópia.

## 2. Toolchain

Com WinGet e os manifests oficiais compatíveis:

```powershell
winget install --id OpenJS.NodeJS.22 --exact --version 22.22.3
winget install --id Microsoft.DotNet.SDK.10 --exact --version 10.0.402
winget install --id GodotEngine.GodotEngine.Mono --exact --version 4.7.2
```

Abra um novo PowerShell e confirme `node --version`, `npm --version`, `dotnet --info`, `godot --version` (`4.7.2` com `.mono`). Godot Mono inclui runtime C# para executar; o cliente/shared têm TFM `net8.0`, e o servidor TFM `net10.0`. Faça restore pelo SDK .NET; só instale SDK 8 se restore apontar falta do targeting pack `Microsoft.NETCore.App.Ref`.

## 3. Restore e testes automatizados

Da raiz:

```powershell
npm ci
npm run check
dotnet restore shared\ProjetoRpg.Shared.csproj
dotnet restore server\ProjetoRpg.Server.csproj
dotnet restore client\ProjetoRpg.Client.csproj
dotnet build shared\ProjetoRpg.Shared.csproj
dotnet build server\ProjetoRpg.Server.csproj
npm run test:phase1
npm run test:target:client
```

`npm run test:phase1` cria banco PostgreSQL descartável em Docker e inclui migrações/auth/ownership, sessão idempotente, spawn, WS auth, input inválido/posição arbitrária, velocidade, caminhada até borda navigável, desconexão/reconexão/reinício e encerramento. Requer Docker Desktop/Engine, .NET SDK 10 e rede NuGet. Nenhum banco real/de jogador deve ser apontado a esse runner.

`npm run check` e `npm test` são Node/estáticos/geometria; não compilam C# nem importam Godot. `npm run test:target:client` usa Godot 4.7.2 Mono headless para import/build e Bootstrap. Build/import não provam gameplay visual.

## 4. Banco local descartável e servidor

Use PostgreSQL vazio/descartável ou o container isolado de `test:phase1`. Defina segredo somente na sessão do PowerShell; não salve em arquivo versionado:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:ASPNETCORE_URLS = 'http://127.0.0.1:5050'
$env:ConnectionStrings__GameDatabase = 'Host=127.0.0.1;Port=5432;Database=projetorpg_test;Username=<usuario-local>;Password=<senha-local>;Ssl Mode=Disable'
dotnet run --project server\ProjetoRpg.Server.csproj -- --db-status
dotnet run --project server\ProjetoRpg.Server.csproj -- --db-migrate
dotnet run --project server\ProjetoRpg.Server.csproj
```

Confirme `pending=[]`, `/health/ready` HTTP 200, `/health/world` com `world.coast`/`coast.initial`, `/health/tick` com `configuredRateHz=20`. A migration `0002_world_sessions` é aditiva; rollback apaga world sessions e requer `--confirm-data-loss`, somente em banco descartável/backup confirmado. O boot não migra banco automaticamente.

## 5. Godot e fluxo de aceitação manual

Em outra janela, aponte o cliente somente ao servidor loopback:

```powershell
$env:PROJETORPG_SERVER_URL = 'http://127.0.0.1:5050'
godot --editor --path .\client
```

Importe/compile, abra `client/project.godot` e mantenha `Scenes/Bootstrap.tscn` como main scene. Execute o projeto e valide, visualmente e com logs:

1. Login de conta real; lista de personagens é exatamente a resposta API, em cartões 3D independentes, com nomes corretos e Idle; selecionar personagem e conferir destaque.
2. Botão “Entrar no mundo” desabilitado sem seleção, habilitado com seleção; clicar solicita POST. Confirmar que personagem da outra conta/inexistente não entra e que sessão repetida retoma mesmo ID/posição.
3. Coast carregada de verdade via `CoastWorld.tscn`; confirmar terreno/texturas/água/luz, câmera, personagem correspondente, spawn autorizado e chão/navmesh.
4. WASD/setas caminham; Idle/Walking e orientação coerentes; medir que não excede speed do servidor; confirmar que navmesh/colisor param o ator na borda e em obstáculo. Nenhum movimento local especulativo pode permanecer após o servidor parar de enviar state.
5. Encerrar conexão abruptamente e reconectar dentro de dez minutos; posição autoritativa persiste. Testar reinício do servidor/sessão se disponível.
6. “Voltar ao salão” envia DELETE, encerra sessão, devolve ao lobby e permite selecionar/entrar novamente. Simular rede indisponível/auth expirada e confirmar mensagem/retry/retorno seguro.
7. Registrar capturas da tela, logs sem tokens/credenciais, versões exatas de toolchain, comandos e status de cada teste.

Se qualquer passo falhar, não declarar sucesso; registrar erro reproduzível e corrigir. A malha server-side é uma aproximação geométrica e não roda o `PhysicsServer3D`: confirmar correspondência física com a Coast importada é obrigatório.

## 6. Protocolo e limites

`POST /api/v1/world/sessions` recebe apenas `characterId` sob Bearer; server seleciona Coast/spawn. `DELETE /api/v1/world/sessions/{id}` encerra. `/ws` usa versão 2: `Hello`, `WorldJoin`, `WorldJoined`, `MoveIntent` (direção + sequência), `WorldState`/`InputRejected`, `Bye`. Nenhuma posição final, velocidade ou delta arbitrário vem do cliente.

É a primeira sessão individual autoritativa, **não multiplayer**: não existem broadcasts de outros jogadores ou colisões dinâmicas entre avatares. Combate, inventário, missões/atributos não fazem parte desse escopo.
