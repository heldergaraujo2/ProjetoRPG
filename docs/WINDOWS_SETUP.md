# Windows — clonar, instalar e validar

Este guia configura o **scaffold atual** para download e testes. Não declara Fase 0 concluída nem transforma a cena Bootstrap/clock em gameplay. O código de cliente/servidor ainda não foi compilado no CI ou neste sandbox porque as toolchains não estavam instaladas.

## 1. Clonar a branch publicada

Abra PowerShell em uma pasta de desenvolvimento e execute:

```powershell
git clone --branch arena/b2f4e6da-projetorpg --single-branch https://github.com/heldergaraujo2/ProjetoRPG.git ProjetoRPG
Set-Location .\ProjetoRPG
git status --short
git log -1 --oneline
```

O clone é Git comum do repositório público; use a branch da PR, não `main`, até as alterações serem integradas. O clone contém o commit mais recente publicado no momento em que você executá-lo.

## 2. Instalar ferramentas oficiais

Em Windows x64 com WinGet, instale as versões pinadas no projeto:

```powershell
winget install --id OpenJS.NodeJS.22 --exact --version 22.22.3
winget install --id Microsoft.DotNet.SDK.10 --exact --version 10.0.402
winget install --id GodotEngine.GodotEngine.Mono --exact --version 4.7.2
```

Abra um novo PowerShell para atualizar o `PATH`, vá à raiz do clone e confirme:

```powershell
node --version
npm --version
dotnet --info
dotnet --list-sdks
dotnet --list-runtimes
godot --version
```

Esperado: Node `v22.22.3` (o `package.json` aceita `>=22.22.3 <23`), SDK .NET `10.0.402` (o `global.json` permite patches da mesma banda `10.0.4xx`) e Godot `4.7.2` com o marcador `.mono`. Se WinGet não estiver disponível, use os downloads oficiais e valide integridade/TLS; os manifestos usados para confirmar os IDs, versões, URLs e hashes WinGet são [Node.js 22.22.3](https://github.com/microsoft/winget-pkgs/blob/master/manifests/o/OpenJS/NodeJS/22/22.22.3/OpenJS.NodeJS.22.installer.yaml), [.NET SDK 10.0.402](https://github.com/microsoft/winget-pkgs/blob/master/manifests/m/Microsoft/DotNet/SDK/10/10.0.402/Microsoft.DotNet.SDK.10.installer.yaml) e [Godot Mono 4.7.2](https://github.com/microsoft/winget-pkgs/blob/master/manifests/g/GodotEngine/GodotEngine/Mono/4.7.2/GodotEngine.GodotEngine.Mono.installer.yaml).

### SDK, runtime e targeting pack — não são a mesma coisa

- Godot 4.7 Mono traz o runtime .NET usado para executar jogos Godot C#. **Não instale um runtime .NET 8 separado** para este projeto.
- O .NET SDK 10 contém compilador/MSBuild e runtime .NET 10 para o servidor `net10.0`.
- O cliente e `shared` têm TFM `net8.0`, conforme os targets oficiais `GodotSharp`/`GodotTools` do Godot 4.7.2. O build precisa do reference/targeting pack .NET 8. Primeiro deixe o restore resolver o pack pelo NuGet. Só se o restore falhar especificamente por falta do pack `Microsoft.NETCore.App.Ref` instale o SDK 8 oficial, por exemplo `winget install --id Microsoft.DotNet.SDK.8 --exact --version 8.0.427`; SDK 8 traz o targeting pack. O `global.json` continuará selecionando o SDK 10 para os projetos.
- O requisito oficial Godot 4.7 é .NET SDK 8 ou posterior; SDK 10 atende a esse mínimo documentado. Ainda falta confirmar o build completo deste repositório em máquina Windows.

## 3. Restaurar dependências do clone

Na raiz do repositório:

```powershell
npm ci
dotnet restore shared\ProjetoRpg.Shared.csproj
dotnet restore server\ProjetoRpg.Server.csproj
dotnet restore client\ProjetoRpg.Client.csproj
```

Dependências conhecidas:

- `ws` do protótipo Node, travado em `package-lock.json` e instalado por `npm ci`.
- `Godot.NET.Sdk/4.7.2`, `GodotSharp` e o targeting pack .NET 8 via restore .NET/NuGet.
- ASP.NET Core vem do shared framework do .NET SDK; não há provider de banco configurado.
- `content/manifest.json`, catálogos iniciais e schema estão versionados. As imagens em `docs/referencias/` também estão incluídas; são referências visuais, não assets transparentes/finais de produção.

Não há submódulos Git, Git LFS ou arquivos obrigatórios exclusivos do Arena. `node_modules`, `.godot`, `bin` e `obj` são artefatos locais ignorados/recriáveis.

## 4. Builds e testes do scaffold alvo

Execute na raiz:

```powershell
npm run check
dotnet build shared\ProjetoRpg.Shared.csproj
dotnet run --project tests\Shared.Smoke\Shared.Smoke.csproj
npm run test:target:server
npm run test:target:client
npm run test:target
```

`npm run test:target` executa, em ordem, smoke de protocolo, integração do servidor e smoke Godot. O smoke do servidor sobe uma porta local temporária, testa `/health/live`, `/health/tick`, `/health/content`, handshake e ping-pong WebSocket, recusa versão/manifest inválidos e mede por 1 segundo a taxa média do clock (aceita 15–25 ticks/s como margem). Essa medição do clock **não** comprova simulação de mundo ou deadline por tick. O smoke do cliente usa o editor Godot headless com `--build-solutions`, inicia a cena Bootstrap e requer `CLIENT_BOOT_OK`.

O teste C# compartilhado deve imprimir `SHARED_PROTOCOL_SMOKE_OK`; os smokes integrados terminam com `SERVER_INTEGRATION_OK` e `GODOT_CLIENT_SMOKE_OK` quando todos os critérios correspondentes passarem. Guarde a saída completa se houver falha e atualize `CHECKPOINT.md`; não marque a Fase 0 concluída só porque um projeto compila.

## 5. Iniciar manualmente (opcional)

Servidor dedicado de desenvolvimento:

```powershell
$env:ASPNETCORE_URLS = 'http://127.0.0.1:5050'
dotnet run --project server\ProjetoRpg.Server.csproj
```

Em outro PowerShell, `Invoke-RestMethod http://127.0.0.1:5050/health/live` verifica o processo e `/health/tick` mostra os pulsos contados. A persistência é explicitamente `false` e o endpoint não autentica jogadores.

Cliente de diagnóstico:

```powershell
godot --path client
```

A cena Bootstrap é somente confirmação de boot; ainda não há login, movimento, combate, conexão ao servidor ou gameplay.

> `npm start` inicia o protótipo legado Node/Canvas em `http://localhost:3000`; não é o cliente Godot nem o servidor ASP.NET Core. `npm run check` valida esse protótipo e a sintaxe dos scripts, não os builds Godot/.NET.

## 6. Banco e limitações atuais

A arquitetura prevê PostgreSQL em produção e SQLite em desenvolvimento, mas ainda não existem providers, migrations, schema de contas/personagens ou serviços de persistência. Não instale/configure banco para validar a Fase 0 e não interprete a ausência de testes de banco como teste aprovado. Essa integração deve entrar na Fase 1, depois dos builds iniciais, com testes reais para cada banco.
