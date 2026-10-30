## Estado operacional mais recente — 2026-10-30 (em andamento)

Branch de trabalho: `arena/b2f4e6da-projetorpg`; clone limpo `/home/user/ProjetoRPG-integration-20261030`. Workspace oficial `/home/user/ProjetoRPG` contém diferenças locais não reconciliadas; não stagear em bloco. Clone JDungeon3D: `/home/user/jdungeon3d-source-20261030`, commit `73bea81341846341f302dfa184e33d31733c3fa8`.

- **Lobby:** cartão SubViewport para cada personagem real retornado pela API, seleção/destaque individual e ator Idle; nenhum personagem inventado. `classId` null/desconhecido usa modelo genérico rotulado; aparência não é persistida. Validação de campos/IDs reforçada.
- **Mapa:** Coast importada seletivamente em `client/Assets/Worlds/JDungeon3D-Coast/` (GLB, colisão estática, NavigationMesh, água e notices MIT). Player/ComponentList/respawn/Kelp/Mushroomdog/sistemas de gameplay foram excluídos. `Bootstrap.tscn` preservada.
- **Entrada no mundo:** NÃO implementada. Sem endpoint, sessão, ownership na entrada, mapa no servidor, spawn autorizado, snapshot, autenticação realtime, idempotência, expiração/reconexão ou movimento server-authoritative. Botão desabilitado; Coast não está no fluxo.
- **Testes Agent:** `npm ci` passou; `npm run check` **24/24 testes Node passou**; `git diff --check` passou. São checks estáticos e um smoke do protótipo Node, não build C#/Godot nem gameplay. `godot`, `godot4` e `dotnet` ausentes.
- **Windows:** não acessível ao agente. Usar somente `D:\ProjetoRPG-Teste-Lobby`, verificar árvore/branch antes de fast-forward; nunca acessar `D:\AProjeto RPG Fixo`. Comandos em `docs/reports/IMPLEMENTACAO_LOBBY_COAST_2026-10-30.md`.
- **Publicação:** SHA remoto será preenchido após revisão e push somente na branch autorizada.

Detalhes: `docs/reports/IMPLEMENTACAO_LOBBY_COAST_2026-10-30.md`, `docs/AVALIACAO_MAPAS_3D_2026-10-29.md` e `docs/CONTRATO_FUTURO_SESSAO_GAMEPLAY.md`.

---

# CHECKPOINT — AJUSTE VISUAL DO LOBBY — 2026-10-29

## Atualização publicada para teste Windows

Branch: `arena/b2f4e6da-projetorpg`.

- Removidos os pilares cilíndricos e as esferas decorativas que apareciam à frente/ao redor do personagem na captura visual do usuário.
- Mantida a pequena base cilíndrica de exposição.
- Câmera reposicionada para `(0, 3.15, 10.6)`, mirando `(0, 1.3, 0)`, FOV 30: enquadramento frontal e levemente elevado, mais próximo para dar destaque ao modelo.
- Escala da instância de prévia ampliada de 1.35 para 1.5. Os arquivos GLB originais não foram modificados.
- Subtítulo do cabeçalho recebeu largura mínima de 340 px, alinhamento à direita e quebra automática desativada para evitar texto vertical.
- Botões inferiores receberam larguras mínimas explícitas: 240 px para “Entrar no mundo” (continua desabilitado) e 170 px para “Trocar conta / sair”.
- Nenhuma alteração no servidor, banco, autenticação, DTOs, contrato de personagens ou `client/project.godot`.

## Evidências e limites

- O usuário confirmou que a revisão anterior compilou com sucesso em `D:\ProjetoRPG-Teste-Lobby` usando `dotnet build .\client\ProjetoRpg.Client.csproj`.
- A captura posterior mostrou que o GLB carrega, mas oito pilares/esferas encobrem a silhueta e o subtítulo do cabeçalho está estreito a ponto de quebrar verticalmente.
- **Esta nova revisão ainda não foi compilada nem executada no Windows.** A compilação e a inspeção visual devem ser feitas pelo usuário na pasta de teste. O agente não tem acesso direto ao caminho local `D:\ProjetoRPG-Teste-Lobby`; a publicação foi feita somente na branch remota de teste.
- Preservar a pasta original `D:\AProjeto RPG Fixo`. Não sobrescrever `client/project.godot`, arquivos `.import` ou `.uid` locais.

## Próximo passo para o usuário

Na pasta `D:\ProjetoRPG-Teste-Lobby`, executar `git fetch origin` e restaurar somente `client/Scripts/CharacterLobby.cs` a partir de `origin/arena/b2f4e6da-projetorpg`; em seguida compilar com `dotnet build .\client\ProjetoRpg.Client.csproj`, abrir o editor Godot 4.7.2 Mono e rodar a cena. Se o resultado visual estiver bom, testar login, lista, seleção, criação, atualização e logout. Reportar a saída do build e uma nova captura. Não considerar a entrada no mundo implementada: o botão permanece deliberadamente desabilitado.

---

# VALIDAÇÃO WINDOWS — CHECK/BUILD/INTEGRAÇÃO FASE 1 APROVADOS — 2026-10-29

## Evidência enviada pelo usuário

Na cópia descartável `D:\\ProjetoRPG-Teste-Lobby`, após avançar por fast-forward para o commit remoto `183d9ae2c482b67b3bd54f108aa8ce6a2409973c`, o usuário executou:

- `npm run check`: **21/21 testes passaram, 0 falharam**. Inclui testes do contrato de conta/lobby, catálogo de dados e smoke do protótipo legado HTTP/WebSocket; não equivale a teste visual da cena nem a chamadas reais disparadas pelo cliente Godot.
- `dotnet build .\\server\\ProjetoRpg.Server.csproj`: **sucesso**, Shared `net8.0` e Server `net10.0`.
- `npm run test:phase1`: **`PHASE1_INTEGRATION_OK`** usando Docker Engine `29.8.2` e imagem PostgreSQL `18.6` com digest `sha256:10a5c69a83035ab5239f5620d708520e8b28f5aa1f2b20143e6024532edadfb5`. O runner informou sucesso em aplicar/rollback/reaplicar migrations; readiness versus liveness; configuração validada; registro/login; erros genéricos de credencial; hash de senha ASP.NET Identity; sessões hashadas, revogáveis e expirantes; ownership/persistência de personagens; audit events e smoke WebSocket.
- O cliente já havia passado anteriormente em `dotnet build .\\client\\ProjetoRpg.Client.csproj --no-restore` e `npm run test:target:client`, com `GODOT_CLIENT_SMOKE_OK` em Godot `4.7.2.stable.mono.official.ed1daf0bf`.
- O Node do ambiente é `v24.20.0`, fora da faixa declarada no `package.json` (`>=22.22.3 <23`); os avisos `MODULE_TYPELESS_PACKAGE_JSON` são não bloqueantes. Registrar a divergência de versão, mas não misturar uma alteração de configuração Node com esta validação.

**Conclusão permitida:** a suíte Node, a compilação do servidor e a integração automatizada PostgreSQL passaram na cópia Windows de teste; o smoke headless do cliente também passou anteriormente. **Ainda pendente:** teste visual do lobby e execução dos fluxos de cadastro/login/listagem/seleção/criação/logout do cliente Godot contra o servidor local. Não há gameplay/MMORPG jogável: entrada no mundo continua desabilitada e não há sessão de mundo/spawn/movimento autoritativos implementados.

**Próximo passo:** seguir a seção 6 de `docs/WINDOWS_SETUP.md` com PostgreSQL descartável. Como `npm run test:phase1` usa banco/container efêmero, não executar rollback manual no ambiente do usuário. Depois iniciar o servidor local de desenvolvimento, configurar `PROJETORPG_SERVER_URL=http://127.0.0.1:5050` apenas no processo/editor de teste, abrir `client/project.godot` com Godot 4.7.2 Mono e validar visualmente o lobby e os fluxos descritos no guia. Não salvar senha, token ou connection string em arquivos versionados.

---

# VALIDAÇÃO WINDOWS — BUILD DO CLIENTE E SMOKE GODOT APROVADOS — 2026-10-29

## Evidência enviada pelo usuário

Na cópia descartável `D:\\ProjetoRPG-Teste-Lobby`, com commit remoto `9291f1b` (correções C# já presentes), o usuário executou:

- `dotnet build .\\client\\ProjetoRpg.Client.csproj --no-restore`: **sucesso**, Shared `net8.0` e Client `net8.0` compilados sem erros.
- `npm run test:target:client`, com `GODOT_BIN` apontando para Godot `4.7.2.stable.mono.official.ed1daf0bf`: **`GODOT_CLIENT_SMOKE_OK`**; importação/build headless e inicialização da cena Bootstrap passaram.
- Node reportado anteriormente no mesmo ambiente: `v24.20.0`, npm `11.19.0`; `npm ci` instalou dependências com aviso `EBADENGINE`, pois o projeto requer Node `>=22.22.3 <23`. Aviso do `MODULE_TYPELESS_PACKAGE_JSON` durante o smoke foi não bloqueante.

Após executar o editor/build, `git status --short` mostrou quatro metadados `.glb.import/.png.import` modificados e dois arquivos `.uid` não rastreados. Esses arquivos permanecem apenas na cópia Windows de teste; não foram commitados nem descartados. Inspecionar diffs antes de decidir se são metadados gerados/necessários.

**Conclusão permitida:** o cliente compila e o smoke headless de Bootstrap passa nesta máquina. **Não concluído:** validação visual do lobby, login contra backend real, lista/seleção/criação/logout com PostgreSQL, e gameplay. Próximo passo é confirmar Docker disponível e seguir a seção 6 de `docs/WINDOWS_SETUP.md` para PostgreSQL descartável e servidor local; configurar `PROJETORPG_SERVER_URL=http://127.0.0.1:5050` apenas no processo de teste e abrir `client/project.godot` pelo editor Godot 4.7.2 Mono.

---

# CORREÇÃO APÓS PRIMEIRO BUILD DO LOBBY — 2026-10-29

## Evidência do teste Windows e correções publicadas

O primeiro build do cliente na cópia Windows `D:\\ProjetoRPG-Teste-Lobby` falhou: Shared `net8.0` compilou, mas o Client apresentou 13 erros em `AccountGateway.cs` e `CharacterLobby.cs`. As causas identificadas foram uso de `Separation` como propriedade C# inexistente nos containers e referências ambíguas a `HttpClient.Method` entre Godot e .NET.

Foram publicadas na branch `arena/b2f4e6da-projetorpg` as seguintes correções:
- `0be873f8b9b2f0baf428d1b195c8e3c3da843e12`: qualifica os métodos HTTP como `Godot.HttpClient.Method` e substitui os inicializadores de espaçamento no gateway por `AddThemeConstantOverride("separation", ...)`.
- `52acf76c734a4adb6832eea23b54cdb5e3e5b2af`: ajusta os seis containers do lobby para configurar o espaçamento por `AddThemeConstantOverride("separation", ...)`.

A leitura posterior dos arquivos remotos confirmou cinco chamadas HTTP qualificadas, seis overrides de espaçamento e nenhuma ocorrência restante de `Separation =` nos dois scripts. Isso é verificação estática dos arquivos publicados, **não** prova de compilação. As correções ainda precisam ser compiladas e testadas no Windows; não declarar o lobby funcional até esses testes passarem.

O Windows reportou Node.js `24.20.0` e npm `11.19.0`, enquanto `package.json` exige Node `>=22.22.3 <23`. `npm ci` concluiu com aviso de engine incompatível. Antes dos testes Node finais, usar Node `22.22.3` e reinstalar dependências com `npm ci`.

---

# ATUALIZAÇÃO OPERACIONAL MAIS RECENTE — 2026-10-29 — Lobby preparado para validação Windows

Esta seção descreve o conteúdo preparado para a ponte Arena.ai → GitHub → Windows. Ela não declara o lobby funcional: o build/import Godot e a validação visual continuam pendentes.

## Estado dos três ambientes

- **Arena.ai — workspace oficial identificado:** `/home/user/ProjetoRPG`, branch local `arena/b2f4e6da-projetorpg`, HEAD observado `40e8badd43ca28c17662944824a3c25335e22182`. O checkout contém o README modificado e o projeto como arquivos não rastreados; não fiz checkout, reset, clean, stash nem stage em bloco. O snapshot pré-alteração está preservado em `/home/user/ProjetoRPG-preserved-20261029-205352.tar.gz` (SHA-256 `c50706901b1b0d3d72d92ccb50f18e253a30d6f18a68de224c6fee64d86fbe40`).
- **Cópia de integração Git:** `/home/user/ProjetoRPG-clean-20261029-205359`, branch `arena/b2f4e6da-projetorpg`, base `5feb171abfc776f080c58c324d154e1a39fb948c`, confirmada novamente contra `git ls-remote` antes da integração. Foi usada para alinhar e publicar somente arquivos revisados, pois o checkout oficial antigo tem numerosos arquivos não rastreados e não pode ser atualizado por fast-forward sem reconciliação destrutiva.
- **Comparação seletiva:** os cinco arquivos novos de cena/ambiente/script/DTO/teste do lobby eram byte-idênticos entre os dois workspaces. `AccountGateway.cs` e `test/client-account-flow.test.js` divergiam. O gateway oficial foi comparado integralmente e adotado na cópia de integração; acrescentei apenas `lobby.SetBusy(_busy)` ao abrir o lobby, para manter os controles ocupados enquanto a lista inicial é consultada. O teste oficial, mais completo, foi preservado e ajustado para verificar as responsabilidades que passaram à cena do lobby. Nenhum arquivo não relacionado foi incluído em bloco; a entrega é limitada aos nove caminhos listados nesta atualização.
- O documento separado [`docs/CONTRATO_FUTURO_SESSAO_GAMEPLAY.md`](docs/CONTRATO_FUTURO_SESSAO_GAMEPLAY.md) registra requisitos futuros sem inventar rota, spawn, estado inicial ou valores. A mesma seleção de alterações revisadas é mantida no workspace oficial e na cópia de integração.
- **Windows:** não foi acessado pelo agente. Esta sessão não confirma a existência do Godot/SDK no PC; essa validação depende da execução local descrita abaixo.

## Revisão de fluxo e limites funcionais

- **Cadastro:** preserva `POST /api/v1/accounts/register` e a resposta genérica `202`; senha mascarada e limpa depois da resposta.
- **Login:** preserva `POST /api/v1/sessions`; só aceita `201` com Bearer/expiração válidos. O token fica em memória. A cena do lobby só é aberta após a sessão válida; falha ao carregar a cena limpa a sessão local e retorna à autenticação.
- **Listagem/seleção:** usa `GET /api/v1/characters` autenticado, preserva seleção ao atualizar quando o ID continua na lista e apresenta apenas campos retornados pelo servidor. A figura 3D é genérica e não representa atributos, aparência ou estado de gameplay.
- **Criação:** usa `POST /api/v1/characters`, exige confirmação `201` e corpo parseável antes de limpar o formulário/atualizar a lista. A classe é explicitamente `null`, pois o catálogo de classes continua indisponível.
- **Logout/troca:** tenta `DELETE /api/v1/sessions/current`, limpa o token/usuário/senha no cliente mesmo se a revogação remota falhar e remove a cena do lobby.
- **Autoridade:** cadastro/login/personagens permanecem ligados às rotas existentes do servidor. A seleção não cria sessão de mundo. O botão **“Entrar no mundo” permanece desabilitado**; não existe entrada, spawn, estado inicial nem movimento de gameplay implementados no servidor. O lobby não deve ser descrito como multiplayer/gameplay funcional.

## Validação executada no ambiente de desenvolvimento

- Na cópia de integração baseada no branch remoto, `npm run check`: **21/21 testes Node passaram**. São 20 verificações estáticas e 1 smoke comportamental HTTP/WebSocket do protótipo Node legado; nenhum executa o `AccountGateway` C#, requests reais do cliente Godot ou a cena do lobby.
- No workspace oficial do Agent, `npm run check`: **28/28 testes Node passaram**. Os sete adicionais exercitam o harness local de clientes estacionados com fixtures; esse conjunto local não foi incluído na entrega seletiva e não equivale a teste de carga real contra o servidor publicado. Também não valida o cliente Godot.
- `git diff --check`: passou.
- O diagnóstico desta sessão encontrou Debian 12 x86_64, Node `v22.22.3` e npm `10.9.8`; não encontrou executáveis Godot/Godot Mono nem .NET nas localizações acessíveis pesquisadas. O `npm run doctor` reporta Godot Mono, SDK .NET e runtime .NET 10 ausentes.
- A tentativa anterior de `npm run test:target:client` foi bloqueada com código 2 antes de import/build por ausência do Godot .NET. Não repeti tentativas de compilação sem a toolchain. **Não houve build C#, importação Godot, boot real do Bootstrap nem validação visual nesta revisão.** O smoke headless também não provaria a aparência do lobby.

## Requisitos e comandos de validação no Windows

- `client/ProjetoRpg.Client.csproj`: `Godot.NET.Sdk/4.7.2`, TFM `net8.0`; `shared` também `net8.0`.
- `global.json`: SDK `10.0.402`, `rollForward: latestPatch` (SDK selecionado deve ser esse patch ou patch compatível da banda 10.0.4xx). Godot deve ser **4.7.2 Mono**; o runner aceita genericamente 4.x Mono, por isso a versão exata deve ser conferida antes. Godot inclui runtime para executar jogos. O build precisa de targeting pack .NET 8; primeiro tente o restore normal e só investigue SDK 8 se o erro disser especificamente que falta `Microsoft.NETCore.App.Ref`. Não instale runtime .NET 8 separado.

Na raiz do checkout que contém esta revisão, confirme ferramentas/versões, restaure/compile e execute o import/build/boot headless:

```powershell
$godot = (Get-Command godot -ErrorAction SilentlyContinue).Source
if (-not $godot) { $godot = Read-Host 'Caminho completo do executável console Godot 4.7.2 Mono' }
$godotVersion = (& $godot --version 2>&1 | Out-String).Trim()
if ($LASTEXITCODE -ne 0 -or $godotVersion -notmatch '^4\.7\.2.*mono') { throw "Godot inesperado: $godotVersion" }
$dotnetVersion = (& dotnet --version 2>&1 | Out-String).Trim()
if ($LASTEXITCODE -ne 0 -or $dotnetVersion -notmatch '^10\.0\.4\d{2}$') { throw "SDK incompatível com global.json: $dotnetVersion" }
dotnet --list-sdks
dotnet --list-runtimes
node --version
npm --version
npm ci
dotnet restore .\client\ProjetoRpg.Client.csproj
dotnet build .\client\ProjetoRpg.Client.csproj --no-restore
$env:GODOT_BIN = $godot
npm run test:target:client
if ($LASTEXITCODE -ne 0) { throw 'Smoke Godot/.NET falhou; preserve a saída completa.' }
```

O resultado do smoke deve incluir `GODOT_CLIENT_SMOKE_OK` e o boot deve incluir `CLIENT_BOOT_OK`. Para a validação visual do **lobby** (não apenas do Bootstrap), siga também a seção 6 de [`docs/WINDOWS_SETUP.md`](docs/WINDOWS_SETUP.md): preparar PostgreSQL descartável, definir `ConnectionStrings__GameDatabase` somente no ambiente local, aplicar `--db-status`/`--db-migrate` e iniciar o servidor em `http://127.0.0.1:5050`. Configure `PROJETORPG_SERVER_URL` para esse loopback no processo/editor Godot, abra `client/project.godot`, rode pelo F5 e autentique uma conta de teste. Confira importação/compilação, layout/câmera/luz/figura genérica, listagem, seleção/prévia, criação confirmada pelo servidor, refresh, logout/troca e ausência de erros no Output/Debugger. Não use produção nem registre senhas/tokens ou grave connection strings em arquivos commitados. A cena `CharacterDemo.tscn` é independente e não deve substituir a cena principal.

---

# REGISTRO DE RECUPERAÇÃO INICIAL — 2026-10-29 (histórico)

Este registro descreve a retomada inicial, antes da integração seletiva e da preparação do pacote Windows. Os fatos atuais constam na seção operacional mais recente acima.

## Preservação e estado Git

- Branch correta: `arena/b2f4e6da-projetorpg`, status informado `pr_open`. HEAD local permanece `40e8badd43ca28c17662944824a3c25335e22182`. Após `git fetch` somente para ler o remoto, `origin/arena/b2f4e6da-projetorpg` aponta para `5feb171abfc776f080c58c324d154e1a39fb948c`; o branch local está 26 commits atrás. Não fiz fast-forward, pull, commit ou push.
- O working tree tem `README.md` modificado e 131 arquivos não rastreados: 112 byte-idênticos ao remoto, 12 divergentes e 7 locais exclusivos. Arquivos do lobby, APIs, projeto, testes e docs seguem presentes no workspace; não descartei nenhum.
- O stash solicitado `c3efbd5a35c03469bbe9f16f8321157ff90a288a` **não está presente** em `git stash list`; `git cat-file` não encontra o objeto. A inspeção somente leitura dos reflogs não encontrou referência ao stash. `git fsck --full --unreachable --no-reflogs` reportou 20 blobs inalcançáveis, mas todos têm SHA-1 idêntico a arquivos já presentes no workspace (incluindo README, AccountGateway, lobby, testes e documentos); não há commit/tree de stash recuperável nem conteúdo distinto a restaurar. Não alterei objetos ou referências. A busca por arquivos `.bak`, `.backup`, `.patch`, `.tar`, `.tar.gz` ou com `snapshot` no nome (excluindo `.git` e `node_modules`) não encontrou backups dentro do workspace. Não consultei diretórios externos. Este checkout não permite confirmar ou restaurar backups fora dele.

### Inventário do working tree

Capturado com `git status --porcelain=v1 --untracked-files=all`; nada está staged. Única alteração rastreada: `M README.md`. Há **131 arquivos `??`**; 112 têm conteúdo idêntico ao remoto atual, 12 diferem do remoto e 7 são exclusivos locais. Lista integral (paths relativos à raiz):

```text
.gitignore
.nvmrc
CHECKPOINT.md
ProjetoRpg.sln
client/Assets/Characters/KayKitAdventurers/Barbarian.glb
client/Assets/Characters/KayKitAdventurers/Barbarian.glb.import
client/Assets/Characters/KayKitAdventurers/Barbarian_barbarian_texture.png
client/Assets/Characters/KayKitAdventurers/Barbarian_barbarian_texture.png.import
client/Assets/Characters/KayKitAdventurers/LICENSE.txt
client/Assets/Characters/KayKitAdventurers/Mage.glb
client/Assets/Characters/KayKitAdventurers/Mage.glb.import
client/Assets/Characters/KayKitAdventurers/Mage_mage_texture.png
client/Assets/Characters/KayKitAdventurers/Mage_mage_texture.png.import
client/Assets/Characters/KayKitAdventurers/SOURCE.md
client/ProjetoRpg.Client.csproj
client/README.md
client/Scenes/Bootstrap.tscn
client/Scenes/CharacterActor.tscn
client/Scenes/CharacterDemo.tscn
client/Scenes/CharacterLobby.tscn
client/Scenes/CharacterLobbyEnvironment.tres
client/Scripts/AccountApiClient.cs
client/Scripts/AccountApiClient.cs.uid
client/Scripts/AccountGateway.cs
client/Scripts/AccountGateway.cs.uid
client/Scripts/Bootstrap.cs
client/Scripts/Bootstrap.cs.uid
client/Scripts/CharacterActor.cs
client/Scripts/CharacterActor.cs.uid
client/Scripts/CharacterLobby.cs
client/Scripts/CharacterSummary.cs
client/Scripts/DemoCamera.cs
client/Scripts/DemoCamera.cs.uid
client/project.godot
content/manifest.json
content/schemas/manifest.schema.json
docs/02_ARQUITETURA.md
docs/AVALIACAO_MAPAS_3D_2026-10-29.md
docs/CHANGELOG.md
docs/DEMO_3D_PERSONAGEM.md
docs/LOAD_TESTING.md
docs/WINDOWS_SETUP.md
docs/adr/ADR-001-stack.md
docs/archive/02_ARQUITETURA-before-reconciliation.md
docs/archive/ADR-001-before-version-audit.md
docs/archive/README-before-official-import.md
docs/archive/RECOVERY.md
docs/archive/client-README-before-scaffold.md
docs/archive/gitignore-before-scaffold.txt
docs/archive/infra-README-before-scaffold.md
docs/archive/package-before-target-scaffold.json
docs/archive/server-README-before-scaffold.md
docs/referencias/README.md
docs/referencias/inventario-elfico-referencia.png
docs/referencias/painel-atributos-elfico-referencia.png
global.json
infra/README.md
package-lock.json
package.json
public/app.js
public/assets/mesh-manifest.json
public/data/game-data.json
public/index.html
public/styles.css
rpg-pack/00_LEIA-ME_PRIMEIRO.md
rpg-pack/01_VISAO_E_ESCOPO.md
rpg-pack/02_ARQUITETURA.md
rpg-pack/03_MECANICAS.md
rpg-pack/04_DADOS_EXTENSIVEIS.md
rpg-pack/05_REDE_E_PROTOCOLO.md
rpg-pack/06_SERVIDOR.md
rpg-pack/07_CLIENTE.md
rpg-pack/08_FASES.md
rpg-pack/09_TESTES_QUALIDADE.md
rpg-pack/10_PVP_BALANCEAMENTO.md
rpg-pack/11_SEGURANCA.md
rpg-pack/12_LEGAL_ORIGINALIDADE.md
rpg-pack/13_REFERENCIA_PESQUISA.md
rpg-pack/14_PROMPTS.md
rpg-pack/ASSETS_LICENCAS.md
rpg-pack/CHECKPOINT.md
rpg-pack/CLAUDE.md
rpg-pack/THIRD_PARTY.md
rpg-pack/data_examples/boss.json
rpg-pack/data_examples/drop_table.json
rpg-pack/data_examples/event.json
rpg-pack/data_examples/item_weapon.json
rpg-pack/data_examples/map.json
rpg-pack/data_examples/monster.json
rpg-pack/data_examples/npc.json
rpg-pack/data_examples/pvp_config.json
rpg-pack/data_examples/quest.json
rpg-pack/data_examples/recipe.json
rpg-pack/data_examples/refine_config.json
rpg-pack/data_examples/skill.json
rpg-pack/templates/ADR.md
rpg-pack/templates/SESSAO.md
scripts/check-target-stack.sh
scripts/check-toolchain.js
scripts/load-parked-clients.js
scripts/phase1-integration.js
scripts/target-godot-smoke.js
scripts/target-server-smoke.js
server/Accounts/AccountEndpoints.cs
server/Accounts/AuthService.cs
server/Characters/CharacterService.cs
server/Configuration/ServerConfiguration.cs
server/Content/ContentCatalog.cs
server/Network/WebSocketEndpoint.cs
server/Persistence/DatabaseCommandRunner.cs
server/Persistence/MigrationRunner.cs
server/Persistence/Migrations/0001_initial.down.sql
server/Persistence/Migrations/0001_initial.up.sql
server/Program.cs
server/ProjetoRpg.Server.csproj
server/README.md
server/Services/WorldTickService.cs
server/Services/WorldTickState.cs
server/appsettings.json
server.js
server/packages.lock.json
shared/ProjetoRpg.Shared.csproj
shared/Protocol/ProtocolEnvelope.cs
test/client-account-flow.test.js
test/client-character-lobby.test.js
test/game-data.test.js
test/godot-character-demo.test.js
test/load-parked-clients.test.js
test/prototype-smoke.test.js
tests/Shared.Smoke/Program.cs
tests/Shared.Smoke/Shared.Smoke.csproj
```

## Revisão e correções locais

- Revisei `Bootstrap.tscn`, `AccountGateway`, `AccountApiClient`, `CharacterLobby.tscn`, ambiente `.tres`, DTO, endpoints, serviços e testes. `GET/POST /api/v1/characters`, sessão Bearer e logout são contratos reais. Não encontrei endpoint autoritativo de entrada no mundo; o botão continua desabilitado.
- Encontrei um erro de tipos C# entre `AccountGateway` e `AccountApiClient`: as chamadas usavam `HTTPClient`/`HTTPRequest`, enquanto a API .NET do Godot usa `HttpClient.Method`/`HttpRequest.Result`. Alinhei o gateway à assinatura já presente no transporte e à documentação oficial Godot C#.
- Corrigi também caminhos de falha: se a cena do lobby não carregar, limpa a sessão local e preserva a tela de login; erros excepcionais ao carregar a lista encerram o estado de loading; logout esconde o lobby imediatamente antes de enfileirar sua liberação e limpa usuário/senha da tela de autenticação, evitando exibir dados da conta anterior durante a troca.
- Reforcei verificações estáticas para tipos C#, ordem `201 → parse → refresh`, remoção do preview anterior, falha de carregamento, limpeza de sessão e ausência de rota world. Essas verificações leem texto/arquivos: **não** são teste C# nem simulação de UI/API.
- Arquivos editados nesta retomada: `client/Scripts/AccountGateway.cs`, `test/client-account-flow.test.js`, `test/client-character-lobby.test.js`, `client/README.md`, `README.md`, `docs/02_ARQUITETURA.md`, `docs/CHANGELOG.md`, `docs/WINDOWS_SETUP.md`, `docs/referencias/README.md` e `CHECKPOINT.md`. `AccountApiClient.cs` foi inspecionado como referência e não alterado.

## Contratos ausentes para entrada no mundo

`docs/02_ARQUITETURA.md` agora registra os componentes necessários sem alegar que já existam: validar Bearer e ownership do `characterId`, resolver mundo/instância e spawn no servidor, criar sessão de mundo de forma atômica/idempotente, retornar estado inicial autoritativo, definir canal realtime autenticado e cobrir sessão expirada/ownership/concorrência/reconexão. `WebSocketEndpoint` atual só valida handshake/protocolo. Nenhum path/resposta de entrada foi implementado ou presumido.

## Referências e ambiente de validação

- As fontes MU externas e a documentação Godot solicitadas fora de `github.com` não puderam ser abertas porque os hosts não pertencem à allowlist de saída deste sandbox. As URLs e estados individuais estão em [`docs/referencias/README.md`](docs/referencias/README.md); os equivalentes oficiais `godotengine/godot-docs` branch `4.7` foram usados quando disponíveis. O tutorial e a referência da classe [`HttpRequest`](https://github.com/godotengine/godot-docs/blob/4.7/classes/class_httprequest.rst) documentam `HttpRequest`, `HttpClient.Method` e `(long)HttpRequest.Result.Success`, confirmando a nomenclatura para a API 4.7; isso motivou a correção.
- `npm ci --ignore-scripts --no-audit --no-fund` já havia passado; não foi necessário reinstalar dependências nesta etapa. Após adicionar testes estáticos para versão do SDK/tipos HTTP, falha ao abrir cena e logout com erro de revogação, `npm test`: **28/28** e `npm run check`: **passou**, incluindo os mesmos **28/28**. `git diff --check` passou; a verificação adicional de whitespace passou nos 13 arquivos de código/docs revisados/editados, inclusive untracked. Os testes são Node/estáticos (mais teste do harness de carga com servidor fixture), não compilação nem integração do cliente Godot.
- `npm run doctor`: status 1, sem Godot Mono, SDK .NET, PostgreSQL e Docker. `dotnet build client/ProjetoRpg.Client.csproj`: status 127 (`dotnet: command not found`). `npm run test:target:client`: status 2 antes de importar/buildar, porque não há Godot 4 .NET. `npm run test:phase1`: status 2 no preflight por SDK .NET ausente, antes de criar container. Nenhuma integração com servidor/banco real foi executada.
- **Inspeção visual:** não realizada; sem Godot não houve câmera, iluminação, escala, interação, importação ou conferência de liberação em runtime. Não declarar lobby validado em execução.

## Próximo passo exato

1. A verificação local de recuperação não encontrou stash nem backup distinto recuperável; manter este worktree intacto. Se um backup verificável vier a ficar acessível no workspace autorizado, registrar localização e conteúdo antes de qualquer uso. Não sincronizar: o branch está 26 commits atrás e há 12 arquivos divergentes.
2. Em Windows com Godot 4.7.2 Mono e SDK .NET 10.0.402, executar `dotnet restore client\ProjetoRpg.Client.csproj`, `dotnet build client\ProjetoRpg.Client.csproj` e `npm run test:target:client`; depois abrir o projeto no editor e validar a checklist de [`docs/WINDOWS_SETUP.md`](docs/WINDOWS_SETUP.md).
3. Com PostgreSQL realmente descartável e servidor local, testar login válido/inválido, lista vazia/erro/retry, seleção e descarte do preview, criação válida/inválida/duplicada, refresh após 201 e logout/troca de conta. Não usar produção.
4. Manter entrada no mundo desabilitada até contrato e implementação server-side autoritativos. Atualizar evidências depois da execução real; não declarar fases concluídas por testes estáticos.

---

# REGISTRO OPERACIONAL ANTERIOR — 2026-10-29 (histórico)

Esta seção registra fatos da etapa anterior e é mantida como histórico.

## Sincronização Windows → GitHub

- O usuário confirmou que o commit local foi enviado com sucesso para `arena/b2f4e6da-projetorpg`.
- Commit publicado: [`48a552a1f1ce67e92abdcfcb17fc32df2a4464ac`](https://github.com/heldergaraujo2/ProjetoRPG/commit/48a552a1f1ce67e92abdcfcb17fc32df2a4464ac), mensagem `fix: validate Godot account client build`.
- O commit inclui 13 arquivos: correções C# em `AccountApiClient.cs` e `AccountGateway.cs`, ajuste do teste de licença para normalizar CRLF/LF, metadados `.uid` e arquivos/metadados importados dos modelos Barbarian e Mage.
- A saída do Git confirmou o push de `a8ce4c9` para `48a552a`. A branch de trabalho permaneceu `arena/b2f4e6da-projetorpg`; `main` não foi alterada e PR #1 continua aberta. Os avisos LF/CRLF dos arquivos .import/.uid foram apenas avisos de conversão; `git diff --cached --check` passou antes do commit.

## Evidências de validação no Windows

- `npm run check`: **12/12 testes passaram**, sem falhas.
- `dotnet build .\client\ProjetoRpg.Client.csproj --no-restore --verbosity quiet`: **sucesso**.
- `npm run test:target`: **sucesso** para os três grupos de teste (protocolo compartilhado, integração do servidor e smoke do cliente Godot), com `GODOT_BIN` apontado para Godot 4.7.2 Mono instalado no Windows.
- `npm run test:target:client`: **sucesso**; importação/build headless e inicialização da cena Bootstrap confirmados.
- Importação Godot headless e inicialização headless do projeto: código de saída 0. Saída de inicialização informou `CLIENT_BOOT_OK godot=4.7.2-stable (official) protocol=1` e `CLIENT_ACCOUNT_FLOW_READY`.
- A integração de Fase 1 com PostgreSQL descartável também passou nesta sessão; o resultado `PHASE1_INTEGRATION_OK` cobriu migrations, configuração, health/readiness, registro/login, hash de senha, sessões com hash/expiração/revogação, ownership/persistência de personagens, audit e smoke WebSocket.
- `npm run test:target` não equivale a validação de gameplay ou carga de jogadores; o teste de 100 WebSockets ociosos segue pendente.

## Teste manual do fluxo de conta no cliente

- Foi usado um banco PostgreSQL descartável separado, em container `projetorpg-e2e-manual`, porta local `55433`, e servidor de teste em `http://127.0.0.1:5050`. Os containers preexistentes `projetorpg-loadtest-20261029124805` e `nexo-postgres-1` foram preservados e não devem ser interrompidos.
- `GET /health/live` respondeu HTTP 200. No Godot GUI, o usuário confirmou que a interface de login/cadastro abriu, conseguiu criar uma conta e conectar; em seguida, apareceu a tela de criar/selecionar personagem.
- Ao selecionar o personagem, a interface informou que o servidor ainda não oferece entrada no mundo nem movimento e que nenhum estado de mundo foi criado. Esse comportamento é a limitação real atual, não uma falha de autenticação.
- **Não declarar multiplayer/gameplay funcional.** Ainda faltam sessão de entrada em mundo autoritativa, estado de mundo, spawn de personagem e movimento validado/sincronizado pelo servidor. A Fase 2 continua aberta e a Fase 3 ainda não foi entregue.

## Próximo passo exato

1. O agente construtor deve implementar a entrada real no mundo com autoridade do servidor, sem simular multiplayer no cliente e sem alterar o fluxo de conta que já passou na validação.
2. Definir e testar contrato de entrada em mundo: personagem pertencente à conta autenticada, sessão/mundo autoritativos, spawn validado e retorno de estado inicial; cobrir falhas de ownership, sessão expirada e mundo indisponível.
3. Em seguida, implementar movimento autorizado/validado pelo servidor e sincronização entre pelo menos dois clientes, com testes de integração reproduzíveis.
4. Executar separadamente o teste de 100 WebSockets ociosos para fechar a pendência de carga da Fase 1; esse teste não substitui a validação de gameplay.
5. Manter o mapa externo JDungeon3D Coast isolado até terminar a inspeção de scripts, importação, colisores e navegabilidade descrita na seção de avaliação de mapas. Não alterar `Bootstrap.tscn` ou importar sistemas de terceiros sem revisão.
6. Preservar a branch `arena/b2f4e6da-projetorpg`, não fazer merge em `main` e manter este `CHECKPOINT.md` atualizado em cada handoff.

---

# CHECKPOINT — continuidade do ProjetoRPG

**Atualizado:** 2026-10-29
**Fase oficial atual:** 1 — Fundação do servidor
**Estado:** Fase 0 concluída. Fase 1 implementada parcialmente; build do servidor, restore em locked-mode e testes de integração PostgreSQL confirmados no Windows em 2026-10-28. Fase 1 permanece EM ANDAMENTO, pendente da revisão integral dos critérios e do teste de carga separado. A demo 3D isolada usa assets CC0 verificados; nesta retomada, `npm run test:target:client` ficou bloqueado no executor Linux (código 2, Godot ausente). Import/build e inspeção visual continuam pendentes, sem alterar o estado oficial das Fases 2/3.
**Branch ativa:** `arena/b2f4e6da-projetorpg` (base original `40e8badd43ca28c17662944824a3c25335e22182`; não trocada). PR #1 está aberta contra `main`.
**Checkpoint oficial importado:** `rpg-pack/CHECKPOINT.md` é histórico; este arquivo na raiz é a única fonte operacional.

## Avaliação de mapas externos — 2026-10-29

- **Pesquisa estática registrada:** comparação de Slavic World Free, JDungeon3D Coast, Godot Adventure Game e TRINITY-WAR em [`docs/AVALIACAO_MAPAS_3D_2026-10-29.md`](docs/AVALIACAO_MAPAS_3D_2026-10-29.md). Os três repositórios GitHub foram clonados fora do projeto em `/tmp/ProjetoRPG-map-review.Cu1Ni7`; nenhum asset foi copiado.
- **Melhor candidato preliminar:** JDungeon3D Coast, por ser cenário 3D construído com construções, colisores estáticos e NavigationMesh, com MIT no repositório e no conteúdo Coast. Godot 4.7.2 Mono ainda não abriu/importou/rodou a cena; a integração permanece pendente e sem aprovação final.
- **Limite do executor:** `godot`, `godot4` e `dotnet` não estão disponíveis. A análise confirmou estrutura/recursos por inspeção estática, não compatibilidade, renderização, walkability ou comportamento em runtime. itch.io continua inacessível, então licença/conteúdo de Slavic World Free são desconhecidos.
- **Sem mudanças de produto:** não foram alterados Bootstrap, cenas/personagens, autenticação, servidor ou movimento; não foi criada cena demo de mapa. O mapa potencial contém sistemas de jogo de terceiros que não devem ser importados. Mantidas pendentes as validações Godot visuais do Barbarian, a carga de 100 WebSockets e as fases do roadmap.
- **Próximo passo exato:** com Godot 4.7.2 Mono, abrir/importar o clone temporário e montar `MapProbe.tscn` sem scripts, instanciando apenas `CoastModel.tscn` com câmera/luz de teste; não executar `Coast.tscn`/jogo externo antes de revisar os scripts que referencia. Inspecionar erros, materiais e colisores; testar NavigationMesh seletivamente numa cena isolada. Só após inspeção visual e prova local de caminhabilidade com o personagem/controlador próprio decidir sobre cópia seletiva e demo em `client/Assets/Worlds/JDungeon3D-Coast/`; preservar `Bootstrap.tscn` e a autoridade futura do servidor. A comparação deve ser apresentada antes de alterar assets/cenas do projeto.


## Cliente de conta/personagens integrado ao servidor — 2026-10-29

- **Incremento real:** `Bootstrap.tscn` continua sendo a cena principal e passa a iniciar `AccountGateway` via `Bootstrap.cs`. O cliente implementa cadastro/login/lista/criação de personagem/logout via HTTP contra rotas já existentes no ASP.NET Core. Endpoint configurável por `PROJETORPG_SERVER_URL`, fallback `http://127.0.0.1:5050`; HTTP é permitido só para loopback e servidores remotos exigem HTTPS. Cadastro respeita resposta genérica 202; personagem é criado sem classe (catálogo segue vazio); a lista vem da conta autenticada e pode ser atualizada.
- **Segurança/limites:** senha mascarada e apagada após envio; bearer fica somente em memória, não é exibido, gravado ou logado; servidor segue autoridade para autenticação, ownership, unicidade e validação. UI trata erros HTTP/rede, 429 e sessão expirada com nova tentativa. O cliente não entra em mundo, não instancia o personagem escolhido em 3D e não movimenta/sincroniza entidades. Não é multiplayer ou gameplay online. `CharacterDemo.tscn`, atores, GLBs, licença e procedência não foram alterados; `Bootstrap.tscn` continua configurado como cena principal.
- **Validação neste executor:** `npm ci` instalou a dependência já declarada (`ws`), sem vulnerabilidades; `npm run check` passou **19/19** testes Node e verificações de sintaxe. O teste novo de conta valida apenas contratos/referências estáticas; GLBs/cenas também são verificados estaticamente. `git diff --cached --check` passou para a entrega preparada. `npm run test:target:client` foi executado e ficou bloqueado com código 2 porque Godot 4 .NET não está disponível. Não foi possível importar/compilar C#, executar a interface nem fazer uma chamada real cliente→servidor neste executor. Não declarar Fase 2 concluída.
- **Estado oficial e pendências:** Fase 1 permanece EM ANDAMENTO (teste real separado de 100 WebSockets ociosos ainda pendente); a Fase 2 também permanece aberta, pois falta validar o fluxo no Godot contra servidor/banco de teste e entregar entrada em mundo. Nenhuma fase foi marcada como concluída. A validação visual adiada da orientação do Barbarian continua pendente e não foi usada como bloqueio.
- **Próximo passo exato:** no Windows com a toolchain já referida, executar `npm run test:target:client`; subir servidor migrado com banco descartável/de teste; definir `PROJETORPG_SERVER_URL` se diferente do fallback; validar cadastro→login→criar/listar/selecionar personagem→logout e respostas para credenciais inválidas, nome duplicado, sessão expirada, servidor indisponível/retry; não usar banco de produção. Depois, implementar a entrada de mundo e movimento autoritativo da Fase 3 com teste de integração de dois clientes. A carga ociosa de 100 segue independente.

## Orientação e sincronização do movimento 3D — 2026-10-29

- Corrigi o sintoma reportado no Windows: Barbarian tocava `Walking_A` mas o visual parecia andar de costas. Inspeção direta do GLB: geometria facial de `Barbarian_Head` aponta para local **-Z**; nós `Rig`/`root` não possuem yaw de 180° no repouso. `Walking_A` é in-place: root sem movimento X/Z e sem yaw animado. Portanto o clipe não altera o rumo do ator.
- O código mantém `offset = destino - posição`, normaliza a direção, usa-a no `Velocity` e chama `LookAt(GlobalPosition + direction, Vector3.Up)`. Em Godot, `LookAt` com o default `use_model_front=false` orienta o -Z local ao alvo. Não inverti o vetor nem mudei o destino ou a colisão.
- Adicionei `VisualYawCorrectionDegrees` ao ator e apliquei 180° somente ao nó visual importado do Barbarian; `CharacterBody3D` segue controlando orientação/movimento e retém a colisão. O Player da demo usa 180°; o NPC Mage é explicitamente 0° e fica inalterado. O giro não toca nas animações.
- Testes estáticos atualizados para conferir correção por ator, rota visual isolada, rumo LookAt e velocidade baseada no mesmo vetor posição→destino, além de parada/idle. `node --test test/godot-character-demo.test.js` passou 4/4; `node --check` e `git diff --check` passaram. `npm run test:target:client` continua bloqueado com código 2 por ausência de Godot 4 .NET neste executor.
- **Limite:** a correção ainda precisa de validação visual no Windows 4.7.2 Mono. Não afirmar o problema resolvido até abrir `CharacterDemo.tscn` com F6, confirmar que o Barbarian encara/caminha em direção ao destino e para, e verificar Output/Debugger. A cena principal permanece Bootstrap; Mage/carga/DB/servidor não foram alterados.

## Recuperação e publicação da demo 3D — 2026-10-29

- Após buscar `origin/arena/b2f4e6da-projetorpg`, recuperei do workspace os artefatos reais da demo: `CharacterDemo.tscn`, `CharacterActor.tscn`, `CharacterActor.cs`, `DemoCamera.cs`, dois GLBs KayKit, licença CC0, auditoria `SOURCE.md`, README do cliente e teste Node dedicado. A publicação foi preparada como continuação do branch remoto `3396fce`; arquivos locais não relacionados não foram incluídos.
- SHA-256 conferidos: Barbarian `cefc311a0e10c7858b6141f5ada7e33268727564fb8ac1347aab97d000669cc6`; Mage `cf898585da33fab50c724d31605fb931eb2912e6d2280092141e98ca81ad507d`; licença `ae322141814056dda0deea7540d74c41d87aee1da319977cd1bd84ee5a923629`. O parse do cabeçalho/JSON glTF confirmou arquivos completos, rig de 41 joints, PNG embutido e 76 animações por GLB; origem, CC0 e inventário permanecem junto dos modelos em `SOURCE.md`.
- Verificação estática confirmou que todas as referências externas das duas cenas resolvem para arquivos existentes. `project.godot` ainda aponta para `Scenes/Bootstrap.tscn`, e `Bootstrap.tscn`/script/configuração foram comparados por hash com o branch remoto sem diferença. A demo segue isolada; nenhum arquivo de banco, servidor de carga ou gameplay foi incluído.
- `node --test test/godot-character-demo.test.js`: **3/3 passou**. `git diff --check`: passou antes desta atualização documental. `npm run check` foi tentado, mas terminou com **código 1**: 6 testes passaram e 2 não iniciaram por dependência `ws` ausente (`test/load-parked-clients.test.js` e `test/prototype-smoke.test.js`); não instalei dependências nem alterei o teste de carga.
- `npm run test:target:client` foi reexecutado e terminou com **código 2**, `TARGET_TEST_BLOCKED: Godot 4 .NET executable is unavailable`. Não houve importação Godot, build C#, execução de cena, F6 ou inspeção visual nesta VM. Não afirmar validação de renderização ou compatibilidade prática concluída.
- Próximo passo: em Windows com Godot 4.7.2 Mono e SDK .NET compatível, executar `npm run test:target:client`, abrir `client` no editor e rodar `Scenes/CharacterDemo.tscn` com F6; registrar import/build e avaliação visual dos modelos, câmera, escala e animações.

## Tentativa de validação e estado do executor — 2026-10-28

- Reconfirmei branch `arena/b2f4e6da-projetorpg`, HEAD `766b316ff550e4f630877705e695b0de5307e127`; working tree já continha alterações locais rastreadas e arquivos novos (incluindo atividade de carga anterior), sem arquivos staged. Nenhum estado foi descartado; não houve stage/commit/push/merge/reset/troca de branch.
- O executor é Linux x86_64, não Windows. Não existem mounts `/mnt/c` ou `/mnt/d`; `GODOT_BIN` está unset e `command -v` não encontra `godot`, `godot4`, `dotnet` ou `opencode`. Node/npm locais: `v22.22.3` / `10.9.8`. `dotnet --version` não encontrou comando (código shell 127).
- `npm run test:target:client` foi realmente executado; retorno do script **2**, com `TARGET_TEST_BLOCKED: Godot 4 .NET executable is unavailable. Install the official matching Mono build and rerun npm run test:target:client.` Inspecionei o runner: sem Godot ele termina antes de importar/buildar ou escrever no projeto. A falha é ausência da ferramenta neste executor, não um erro confirmado da cena/C#; nenhum código foi mudado em resposta.
- `npm run doctor` retornou **1** e reportou Godot Mono, .NET SDK e runtime .NET 10 ausentes; PostgreSQL/psql e Docker também ausentes. Essa execução confirma o estado deste executor, não do Windows.
- O checkpoint histórico da validação do usuário registra Windows com Godot `4.7.2 stable Mono`, SDK `.NET 10.0.403` e Node `22.23.4`; não tenho acesso a esse Windows nesta sessão e não verifiquei novamente executável, caminho ou versão. Não apresentar esse registro histórico como confirmação atual.
- Não foi possível executar o smoke de import/build/Bootstrap, abrir o editor, rodar F6 ou inspecionar modelo/textura/câmera/movimento/animações/escala/orientação. A demonstração segue sem validação visual.
- Após atualizar a documentação: `npm run check` passou (sintaxe JS + `npm test`, **14/14**), e `git diff --check` passou. Permanecem os avisos Node não bloqueantes `MODULE_TYPELESS_PACKAGE_JSON` nos dois testes ESM. Esses testes não substituem o smoke Godot bloqueado acima.
- **Próximo passo exato:** no Windows acessível, verificar caminho e `--version` do binário Mono, `dotnet --list-sdks` e Node/npm; executar `npm run test:target:client`, abrir `client` e rodar `CharacterDemo.tscn` com F6. Registrar saídas e resultados visuais; corrigir somente falhas reproduzidas.

## Validação operacional atual — 2026-10-28

Esta seção atualiza o estado operacional. As referências a bloqueios de SDK, Docker ou testes não executados mais adiante são registros históricos de tentativas no sandbox Linux, não o estado atual do Windows.

- `npm run check`: passou, com verificações de sintaxe e 4/4 testes Node.
- `npm run test:target`: passou, incluindo smoke do protocolo compartilhado, integração HTTP/WebSocket do servidor e import/build/boot headless do cliente Godot.
- `dotnet build server\\ProjetoRpg.Server.csproj --no-restore`: passou; Shared `net8.0` e Server `net10.0`, sem erros.
- `npm run test:phase1`: passou com PostgreSQL descartável, validando migrations, configuração, health/readiness, registro/login, hash de senha, expiração/revogação de sessões, ownership/persistência de personagens, audit e WebSocket.
- `dotnet restore server\\ProjetoRpg.Server.csproj --locked-mode`: passou. O lockfile `server/packages.lock.json` foi publicado no repositório.
- `scripts/phase1-integration.js` foi corrigido para usar `psql -q` e expirar a sessão de teste ajustando `created_at` e `expires_at` de forma compatível com a constraint. O teste de integração passou após a correção.
- `git diff --check`: passou. O aviso LF/CRLF é uma notificação de conversão de finais de linha pelo Git, não uma falha.

A Fase 1 continua EM ANDAMENTO até a revisão integral dos critérios de aceitação. O teste separado de 100 WebSockets ociosos ainda precisa ser executado; ele não comprova gameplay nem carga real de jogadores autenticados. SQLite e gameplay completo permanecem não implementados/não comprovados.

## Retomada segura e estado atual da Fase 1 — 2026-10-28

- **Base remota confirmada antes da implementação:** `99ab3d742427c04043109304ae0b87622b9f3769`, branch `arena/b2f4e6da-projetorpg`, PR #1 aberta. Essa base descendia do workspace antigo `40e8badd43ca28c17662944824a3c25335e22182`.
- Auditoria do working tree antigo: README modificado localmente já era byte-idêntico ao remoto; 85 arquivos locais não rastreados também eram idênticos; divergiam somente CHECKPOINT, CHANGELOG e smoke Godot, todos mais antigos. Nenhum arquivo local exclusivo foi encontrado. O estado anterior completo foi preservado em stash `pre-sync workspace snapshot 2026-10-28 before 99ab3d7` e `/home/user/ProjetoRPG-pre-sync-20261028.tar.gz`; atualização foi fast-forward, sem force-push/reset destrutivo. Esse backup não está no repositório.
- Commits da implementação desta atualização: `74784ee75fa8a5f8ba56a7f8d930d9db2d7e907e` (`feat(server): add PostgreSQL account and character foundation`) e `04efa0e18d744ccfc0567e606470338906613297` (isolamento do runner contra connection string de migração herdada). Adicionado provider PostgreSQL Npgsql `10.0.0`; source oficial da tag v10 confirma TFMs net8.0/net9.0/net10.0. Client/shared permanecem net8.0; server permanece net10.0. `RestorePackagesWithLockFile` está ligado; `server/packages.lock.json` foi gerado no Windows, validado com restore em `--locked-mode` e publicado na branch.
- Implementados e validados no Windows: config env obrigatória/validação, TLS do PostgreSQL verificado em Production, migration runner transacional/checksum/advisory lock e rollback com confirmação, PostgreSQL accounts/sessions/characters/audit, PasswordHasher ASP.NET Identity, bearer token aleatório armazenado somente como SHA-256, expiração/revogação, autorização por `account_id`, readiness/database/live e validação reforçada de IDs/path traversal no catálogo.
- Personagem é persistido sem classe atribuída porque `content/manifest.json` não contém classes reais. `classId` só funciona quando houver classe real no catálogo; não adicionar uma classe de mentira. Não há gameplay implementado.
- **Validação histórica do sandbox Linux:** `npm run doctor` e `npm run test:phase1` ficaram bloqueados naquele ambiente por falta de toolchain. A validação posterior no Windows está registrada na seção `Validação operacional atual` acima: build, restore em locked-mode e integração PostgreSQL passaram.
- Runner automatizado `npm run test:phase1` usa PostgreSQL Docker efêmero, migrações reversíveis, configuração, health, auth/sessões, autorização/persistência após restart, audit e catálogo. `npm run load:parked` prepara 100 conexões WebSocket ociosas; não autentica nem simula gameplay e ainda não foi executado.
- **Próximo passo exato:** executar separadamente o teste de 100 WebSockets ociosos via `npm run load:parked` com o servidor e banco de teste configurados; registrar resultados e revisar todos os critérios do README antes de declarar a Fase 1 concluída.

## Causa confirmada da divergência

1. A branch desta sessão permaneceu no commit-base `40e8bad`; a primeira referência local de `origin/main` estava em `3941e64`.
2. `git fetch origin main` atualizou **somente a referência** para `origin/main` `9ac6d53ecf58cca77fa05295583123bb7ac0f000`. Não ocorreu checkout, merge, rebase ou mudança de branch.
3. A árvore desse `main` continha o README e pacote oficial `rpg-pack/`, mas não tinha `project.godot`, `.csproj`, `.sln` ou implementação Godot/.NET. A busca por branches remotas também não revelou outra branch publicada com esses projetos.
4. Na inspeção inicial, ainda não havia evidência local verificável dos testes feitos no PC. Isso foi esclarecido em 2026-10-28: o usuário confirmou no Windows os builds limpos de Shared, Server e Client e os testes `npm run check` e `npm run test:target`. Os resultados estão registrados na seção **Validação local confirmada** abaixo. A observação anterior sobre ausência de evidências descreve somente o estado da inspeção inicial e não deve ser interpretada como estado atual.

## Fontes e documentos reconciliados

- `rpg-pack/` foi importado de `origin/main` como fonte documental oficial; `rpg-pack/CHECKPOINT.md` foi marcado como histórico.
- `README.md`, `docs/02_ARQUITETURA.md`, `docs/adr/ADR-001-stack.md`, `docs/WINDOWS_SETUP.md`, `docs/CHANGELOG.md` e este checkpoint foram atualizados para distinguir planejamento, implementação e evidência; o guia Windows contém clone, instalação, restore, execução e testes.
- Stack-alvo: Godot `4.7.2-stable` .NET / `Godot.NET.Sdk/4.7.2` com client/shared `net8.0` (targets oficiais GodotSharp/GodotTools); servidor ASP.NET Core/SDK+TFM `.NET 10.0.402` / `net10.0`; PostgreSQL produção, SQLite desenvolvimento e WebSocket/TCP inicial. Requer Godot Mono e SDK .NET 10.0.402. A documentação oficial Godot 4.7 diz que Godot traz runtime para rodar jogos e exige SDK .NET separado; 4.5+ permite SDK 8 ou posterior. Não é necessário instalar runtime 8 separado; o client `net8.0` requer targeting pack 8 no build (restore primeiro; SDK 8 oficial somente se necessário). O servidor usa runtime 10 do SDK. **Validação atualizada em 2026-10-28:** os builds e testes de integração HTTP/WebSocket foram executados com sucesso no Windows, conforme a seção de validação local abaixo. As notas históricas do sandbox Linux mais adiante registram tentativas anteriores e bloqueadas naquele ambiente, não os resultados atuais.
- O README, arquitetura, ADR e readmes preexistentes de client/server/infra foram recuperados de blobs Git inacessíveis e preservados em `docs/archive/`. Como existem snapshots próximos diferentes, não é possível garantir que sejam byte a byte a última versão não commitada do working tree; ver `docs/archive/RECOVERY.md`.

## Estado real por componente

### Já existia e foi preservado

- Protótipo cliente HTML/CSS/Canvas 2D em `public/` e servidor Node.js/WebSocket em `server.js`.
- Dados e testes de protótipo em `public/data/` e `test/`; pacote npm com `ws`.
- Referências visuais preexistentes, não convertidas em assets de produção.

### Scaffold criado nesta etapa — builds e smoke tests confirmados

- `global.json`, `ProjetoRpg.sln`, `shared/ProjetoRpg.Shared.csproj`, contratos JSON/versionamento e `tests/Shared.Smoke/`.
- `server/ProjetoRpg.Server.csproj`: ASP.NET Core, endpoints live/tick/content, catálogo JSON validado no startup, loop-clock que conta pulsos a 20 Hz, métricas counter/histogram e WebSocket `Hello`/`HelloAck` + `Ping`/`Pong`, com validação simples de versão e sequence.
- `client/ProjetoRpg.Client.csproj`, `client/project.godot`, cena/script Bootstrap e teste headless. A cena apenas confirma inicialização; não é gameplay/MMORPG.
- `content/manifest.json` e `content/schemas/manifest.schema.json`; `scripts/target-server-smoke.js`, `scripts/target-godot-smoke.js`, `scripts/check-target-stack.sh` e comandos npm separados.
- O catálogo inicial está vazio. O loader atual valida manualmente manifest, IDs únicos/estáveis, existência/path traversal e JSON dos arquivos referenciados; ainda não é validador JSON Schema genérico.

### Não implementado / não comprovado

- Contas/registro/login/hash/sessões/personagens/PostgreSQL/migration/audit foram compilados e testados na validação Windows registrada acima; SQLite não foi implementado.
- Simulação de mundo: o serviço 20 Hz atual só conta ticks e emite métrica; não atualiza atores/movimento nem prova precisão de deadline.
- Teste de carga 0/100 ainda não executado (há script WebSocket ocioso); movimentação autorizada, navegação/pathfinding, colisão, sincronização, combate, loot, inventário, economia, atributos, teleporte, quests ou interações não estão implementados.
- O cliente não implementa input. Mantêm-se os requisitos: clique esquerdo é o único input de movimento; clique direito solicita ataque validado pelo servidor; W/A/S/D/Espaço não são gameplay; I/P/M são atalhos de UI; atributos canônicos Fúria, Defesa, Mana, Agilidade e Sabedoria. Não foram encontrados/introduzidos sistemas paralelos FOR/AGI/VIT/ENE/CMD.

## Matriz oficial da Fase 0

- [x] Ler/importar a especificação oficial e registrar ADR/stack justificada.
- [x] Estruturar diretórios alvo, solução/projetos, content/schema e scripts locais de build/teste.
- [x] Criar runner local `scripts/check-target-stack.sh` para executar verificações em sequência.
- [x] Criar arquivos mínimos de cliente Godot C# e servidor ASP.NET Core.
- [x] Compilar limpo cliente e servidor com versões instaladas e compatíveis.
- [x] Executar smoke da cena Godot e integração HTTP/WebSocket do servidor.
- [x] Conferir os builds reais e registrar as evidências de execução no checkpoint.
- [x] Concluir a Fase 0 após os builds e testes iniciais passarem; Fase 1 permanece incompleta.

## Trabalho seguro iniciado em direção à Fase 1 — snapshot anterior à implementação atual

Na confirmação dos smokes iniciais de 2026-10-28, o código possuía somente serialização/versionamento, handshake/ping, clock de 20 Hz, métricas e recusa de conteúdo inválido. Esse snapshot comprovou Fase 0, não persistência. Desde então, a seção **Validação operacional atual** registra os builds e testes Windows confirmados; os bloqueios citados em seções históricas referem-se ao sandbox Linux. Catálogos de gameplay, teste de 100 conexões executado e aceite completo da Fase 1 permanecem pendentes.

## Testes e comandos — resultados históricos do sandbox Linux (2026-10-27)

Executado a partir da raiz `/home/user/ProjetoRPG`:

- `npm run check` → **passou**, syntax-check dos scripts + 4/4 testes do protótipo Node. Durante a primeira tentativa, `node --test` auto-descobriu scripts chamados `test-*.js`; eles foram renomeados para `target-*-smoke.js` e o comando final passou com 4/4.
- `npm run doctor` → **status 1 esperado**. Reportou Node `v22.22.3` / npm `10.9.8`; Godot Mono, SDK .NET e runtime .NET 10 ausentes; PostgreSQL/Docker ausentes.
- `npm run test:target:shared` → **status 127**, `dotnet: not found`; nenhum teste C# executou.
- `npm run test:target:server` → **status 2**, `TARGET_TEST_BLOCKED`; não houve build nem teste HTTP/WebSocket do novo ASP.NET Core.
- `npm run test:target:client` → **status 2**, `TARGET_TEST_BLOCKED`; Godot Mono não está instalado, não houve import/build/boot da cena.
- `npm run test:target` → **status 127**, a sequência parou ao tentar o teste compartilhado porque `dotnet` não existe.
- `./scripts/check-target-stack.sh` → executou Node check (4/4) e parou com **status 127** no primeiro comando .NET.
- Busca de `godot`, `godot4`, `dotnet`, `psql`, `postgres`, Docker → ausentes. APT sem índices locais.
- Diagnóstico adicional: release Godot GitHub retorna 302 para `objects.githubusercontent.com`/`release-assets.githubusercontent.com` (fora do allowlist), inclusive via API de assets; endpoints Microsoft oficiais `dotnetcli.azureedge.net`, `builds.dotnet.microsoft.com`, `packages.microsoft.com` e o link de instalador oficial `download.microsoft.com` falham `curl` 35 / `SSL_ERROR_SYSCALL`. Nenhum bypass TLS nem fonte não oficial foi usado. Winget não existe neste Linux sandbox; foram consultados manifestos oficiais/WinGet com URL e hashes para fornecer procedimento no Windows.
- Os testes Node validam somente o protótipo, não a stack-alvo.

## Comandos locais reproduzíveis

Instalar os runtimes oficiais compatíveis com os pins acima e as dependências npm; então, na raiz:

```bash
npm ci
npm run check
npm run doctor
dotnet --info
dotnet run --project tests/Shared.Smoke/Shared.Smoke.csproj
npm run test:target:server
npm run test:target:client
npm run test:target
```

O servidor de desenvolvimento pode ser iniciado com `dotnet run --project server/ProjetoRpg.Server.csproj`. Para Godot, abrir projeto `client/` na distribuição 4.7.2 .NET. `npm run doctor` é apenas diagnóstico; `npm run check` só valida Node legado e sintaxe JS.

## Procedimento Windows — referência histórica de preparação

O guia completo versionado está em [`docs/WINDOWS_SETUP.md`](docs/WINDOWS_SETUP.md). Comandos diretos para clonar a branch publicada e instalar Node, SDK e Godot via WinGet:

```powershell
git clone --branch arena/b2f4e6da-projetorpg --single-branch https://github.com/heldergaraujo2/ProjetoRPG.git ProjetoRPG
Set-Location .\ProjetoRPG
winget install --id OpenJS.NodeJS.22 --exact --version 22.22.3
winget install --id Microsoft.DotNet.SDK.10 --exact --version 10.0.402
winget install --id GodotEngine.GodotEngine.Mono --exact --version 4.7.2
```

A rede permitida neste executor não alcança os hosts binários Microsoft/GitHub Releases. Os manifestos WinGet foram consultados sem baixar binários; eles verificam SHA-256 e apontam para fontes oficiais:

- PowerShell (x64):

```powershell
winget install --id Microsoft.DotNet.SDK.10 --exact --version 10.0.402
winget install --id GodotEngine.GodotEngine.Mono --exact --version 4.7.2
```

O pacote Godot é o Mono x64 oficial; o .NET SDK vem do download Microsoft e inclui runtime 10 para o servidor. Godot 4.7 empacota o runtime necessário ao editor/jogo compilado; **não instale runtime .NET 8 separado**. O cliente/shared `net8.0` podem necessitar do targeting pack 8 durante restore. Tente primeiro o restore do SDK10; somente se o erro reportar targeting pack/ref pack `Microsoft.NETCore.App.Ref` ausente, instale o SDK 8 oficial para fornecer esse targeting pack:

```powershell
winget install --id Microsoft.DotNet.SDK.8 --exact --version 8.0.427
```

Abra um PowerShell novo, confirme versões e, na raiz do repositório:

```powershell
dotnet --info
dotnet --list-sdks
dotnet --list-runtimes
godot --version
npm ci
npm run check
npm run test:target
```

O `global.json` exige SDK `10.0.402` ou patch compatível da banda `10.0.4xx`; `godot --version` deve mostrar 4.7.2 e `mono`. Se WinGet não estiver instalado, utilize as URLs/checksums oficiais nos manifestos [Microsoft .NET SDK 10.0.402](https://github.com/microsoft/winget-pkgs/blob/master/manifests/m/Microsoft/DotNet/SDK/10/10.0.402/Microsoft.DotNet.SDK.10.installer.yaml) e [Godot Mono 4.7.2](https://github.com/microsoft/winget-pkgs/blob/master/manifests/g/GodotEngine/GodotEngine/Mono/4.7.2/GodotEngine.GodotEngine.Mono.installer.yaml); mantenha a verificação de hash/TLS habilitada.

## Próximo passo exato — registro histórico revisado em 2026-10-28

Os builds limpos de Shared, Server e Client e os testes de destino foram confirmados pelo usuário no Windows em 2026-10-28; não é necessário repeti-los para esta mudança. A lista de tarefas de Fase 1 abaixo foi parcialmente iniciada nesta sessão; o estado atual e o próximo passo executável estão na seção **Retomada segura e estado atual da Fase 1** no início deste checkpoint.

Persistência PostgreSQL, migrations, registro/login, sessões, ownership de personagem, audit log e runner de teste real foram adicionados ao código, mas não validados por build devido à falta de SDK .NET/Docker neste sandbox. PostgreSQL/SQLite e gameplay completos não devem ser declarados concluídos.

**Branch:** `arena/b2f4e6da-projetorpg`. **Commit de scaffold:** `def86f0`; PR #1 permanece aberta contra `main`. A implementação foi publicada nesta branch com a limitação de validação explícita; builds, restore e integração PostgreSQL continuam pendentes e a Fase 1 não está concluída.

## Validação local confirmada — 2026-10-28

Ambiente Windows do usuário. Esta seção atualiza o estado operacional anterior, que refletia as limitações de toolchain do sandbox Linux em 2026-10-27.

### Ferramentas identificadas

- Node.js 22.23.4.
- .NET SDK 10.0.403, respeitando o `global.json` com roll-forward `latestPatch`.
- Godot 4.7.2 stable Mono.
- O script de smoke do cliente foi executado apontando `GODOT_BIN` para o executável console do Godot instalado localmente.

### Builds limpos

- `dotnet clean` de Shared, Server e Client: sucesso.
- `dotnet restore` dos três projetos: sucesso.
- `dotnet build shared/ProjetoRpg.Shared.csproj --no-restore`: sucesso, target `net8.0`.
- `dotnet build server/ProjetoRpg.Server.csproj --no-restore`: sucesso, target `net10.0`.
- `dotnet build client/ProjetoRpg.Client.csproj --no-restore`: sucesso, target `net8.0`.
- `npm run check`: sintaxe JavaScript aprovada e 4/4 testes do protótipo Node aprovados.

### Testes de destino

- `npm run test:target:shared`: `SHARED_PROTOCOL_SMOKE_OK`.
- `npm run test:target:server`: `SERVER_INTEGRATION_OK`; endpoint ativo, relógio medido em 19,90 Hz nesta execução, validação de catálogo, handshake/ping WebSocket, rejeição de versão incompatível e recusa de catálogo inválido.
- `npm run test:target:client`: `GODOT_CLIENT_SMOKE_OK`; importação/build headless e inicialização da cena Bootstrap.
- `npm run test:target`: os três testes de destino passaram na mesma execução.

Os avisos `MODULE_TYPELESS_PACKAGE_JSON` dos scripts JavaScript foram não bloqueantes. O relógio do servidor mede pulsos; não representa simulação de mundo. A cena Bootstrap confirma inicialização, não gameplay.

### Limites após Fase 0 e status atualizado para Fase 1

Snapshot histórico anterior à validação da Fase 1 no Windows: a confirmação inicial da Fase 0 cobria build/smoke, não banco. Depois, contas, hash, sessões, personagens, PostgreSQL, migrations e audit log foram compilados e testados conforme a seção **Validação operacional atual** no topo. SQLite continua sem provider. Também não estão demonstrados mundo simulado, movimentação multiplayer, combate, inventário, economia ou gameplay completo.

### Próximo objetivo — Fase 1: validação e endurecimento

Gerar e versionar `server/packages.lock.json` com restore real; compilar o server; executar `npm run test:phase1` com Docker/PostgreSQL; corrigir falhas; então executar o teste separado de 100 conexões ociosas. Depois implementar/revisar grants mínimos de DB, rate limiting distribuído conforme necessidade, revogação em massa/rotação quando o produto exigir, retenção de audit, backup/restore e documentação de operação. Não marcar Fase 1 concluída até cobrir integralmente os critérios do README.


---

# AUDITORIA DIRETA DO TEMPLATE GODOT 4 — 2026-10-29

## Resultado desta retomada

- Acesso ao repositório oficial confirmado: `heldergaraujo2/ProjetoRPG`; permissão GitHub de push confirmada; branch `arena/b2f4e6da-projetorpg` localizada.
- Lidos o checkpoint, changelog, documento de integração e os arquivos atuais do lobby C# e suas cenas.
- O documento `docs/INTEGRACAO_TEMPLATE_GODOT4_CHARACTER_SELECTOR.md` já estava publicado e foi usado como ponto de partida; a auditoria abaixo aprofundou as cenas reais da fonte.
- Fonte examinada: `MacdonaldRobinson/godot4-3d-characters`, branch `main`; código MIT, Copyright (c) 2022 emirthab. O tamanho reportado pelo GitHub é 826.527 KB (~807 MiB).
- Arquivos-fonte lidos: CharacterSelector.tscn/.gd, Lobby.tscn/.gd, characters/character.tscn, character.gd, character_stats.gd, characters/kaya/kaya.tscn, ui/Theme.tres, project.godot, README.md e LICENSE.

## Descoberta que altera o plano

O template não entrega o cenário medieval/MMORPG visual esperado. O Character Selector é uma plataforma CSG simples com dois personagens e formulário; o Lobby é outra plataforma simples com UI host/join ENet e seletor de mapas. A cena do personagem traz dependências extensas (Character/CharacterStats, animações GLB, câmera, health bar, áudio, efeitos, scripts de combate/interação). Os scripts chamam GameState/NetworkState e usam estado/estatísticas de demonstração. Importar essas cenas literalmente duplicaria sistemas e poderia mostrar dados fictícios.

**Decisão:** não copiar as cenas originais nem o projeto inteiro. Não clonar os ~807 MiB sem necessidade. Não copiar modelos/animações/sons/texturas/addons enquanto as licenças individuais não forem comprovadas. Considerar somente portar um comportamento de câmera/seleção se houver ganho claro, mantendo C# e API autoritativa do ProjetoRPG; em paralelo, avaliar cenário 3D alternativo com licenças explícitas.

## O que está e não está feito

- Auditoria estática das cenas e scripts da fonte: concluída.
- Licença MIT do código: confirmada.
- Integração/cópia de assets: NÃO realizada.
- Clone local em `D:\godot4-3d-characters-review`: não confirmado, pois o agente não acessa o Windows.
- Build/import Godot/teste visual após integração: NÃO executados.
- Arquitetura de conta/personagens/rede do ProjetoRPG: preservada nesta sessão.

## Próxima tarefa
Verificar no Windows se o clone temporário existe, sem sobrescrever nem apagar nada. Como a inspeção remota já foi suficiente para rejeitar a importação integral, só clonar seletivamente se for necessário inspecionar um recurso específico. Depois decidir entre portar somente uma interação/câmera útil ou selecionar um cenário de lobby realmente adequado e com licenças de assets verificáveis. Manter o botão “Entrar no mundo” desabilitado até haver operação autoritativa de gameplay.



# PORT VISUAL DO CHARACTER SELECTOR — PUBLICADO NO GITHUB — 2026-10-29

- Adicionada a cena isolada `client/Scenes/ImportedLobby/TemplateCharacterSelectorStage.tscn` e o controlador C# `client/Scripts/TemplateCharacterSelectorStage.cs`.
- O palco usa os GLBs Barbarian/Mage já existentes, base compacta, câmera frontal elevada, iluminação e alternância por setas esquerda/direita.
- Incluídos README e aviso MIT original em `client/Scenes/ImportedLobby/`.
- Este é um port visual independente, não a importação integral do demo: não traz autoloads, ENet, MultiplayerScene, mundos ou addons do template. A tela principal `res://Scenes/CharacterLobby.tscn` e a API de conta/personagens permanecem preservadas.
- A nova cena é executável isoladamente para avaliação visual. Entrada no mundo continua desabilitada.
- **Validação pendente no PC:** atualizar a branch, compilar o cliente e abrir a cena no Godot 4.7.2 Mono. Esta publicação não equivale a build/teste visual confirmado.


# RETOMADA DO PORT VISUAL — AUDITORIA REMOTA — 2026-10-29

## Verificação do GitHub

- Repositório: `heldergaraujo2/ProjetoRPG`.
- Branch consultada: `arena/b2f4e6da-projetorpg`.
- HEAD remoto confirmado pela API de refs: `b3b6f37481f64607e7e8c2fe625a300345f0b885` (`docs: record 3D character selector port and test status`).
- A cadeia de commits foi verificada: controlador `549a3d9`, cena `363ab5c`, README `59d1804`, licença `5510a15`, checkpoint `c6f5308`, changelog `48c12a4` e documentação de integração `b3b6f37`. Os arquivos do port estão presentes na branch.
- Arquivos conferidos: `client/Scripts/TemplateCharacterSelectorStage.cs`, `client/Scenes/ImportedLobby/TemplateCharacterSelectorStage.tscn`, README e licença da pasta ImportedLobby, `client/Scripts/CharacterLobby.cs`, `client/Scenes/CharacterLobby.tscn`, `CHECKPOINT.md`, `docs/CHANGELOG.md`, `docs/INTEGRACAO_TEMPLATE_GODOT4_CHARACTER_SELECTOR.md`, `docs/WINDOWS_SETUP.md` e `README.md`.

## Auditoria do port publicado

- O controlador C# carrega os GLBs locais Barbarian/Mage, usa `ui_left/ui_right`, aplica escala 1.5 e atualiza o Label3D.
- A cena contém WorldEnvironment, base cilíndrica, câmera, luz direcional, luz de preenchimento e Label3D. A cena está isolada e não possui dependência de GameState/NetworkState/ENet.
- O lobby principal continua sendo `CharacterLobby.tscn`; seu script continua construindo a prévia por conta própria e ligado ao fluxo de personagens. A cena importada ainda NÃO foi conectada ao lobby principal.
- O código remoto foi inspecionado estaticamente. Nenhum build foi executado pelo agente e não há evidência de que o usuário já tenha atualizado a cópia Windows ou executado a cena. Não declarar compilação, importação ou validação visual do novo port.
- Nenhum arquivo local do Windows foi acessado. Nenhuma alteração foi feita em `D:\\AProjeto RPG Fixo` ou em `D:\\ProjetoRPG-Teste-Lobby`.

## Próxima ação obrigatória — atualizar a cópia descartável

No PowerShell, executar primeiro a verificação segura de branch e working tree, seguida de fetch e fast-forward apenas se a branch correta estiver limpa. Se o script interromper por alterações locais ou branch divergente, não descartar nem sobrescrever nada; compartilhar a saída para análise.

```powershell
Set-Location D:\\ProjetoRPG-Teste-Lobby
$expected = 'arena/b2f4e6da-projetorpg'
$current = (git branch --show-current).Trim()
if ($LASTEXITCODE -ne 0 -or $current -ne $expected) { throw "Branch inesperada: '$current'. Nenhuma atualização realizada." }
$status = git status --porcelain
if ($LASTEXITCODE -ne 0) { throw 'Não foi possível consultar git status.' }
if ($status) { $status; throw 'Há alterações locais. Pare aqui; não fazer merge nem descarte.' }
git fetch origin
if ($LASTEXITCODE -ne 0) { throw 'git fetch falhou.' }
git merge --ff-only origin/arena/b2f4e6da-projetorpg
if ($LASTEXITCODE -ne 0) { throw 'Fast-forward não foi possível. Preserve o estado e reporte.' }
git log -1 --oneline
git status --short
```

Depois do fast-forward, executar `dotnet build .\\client\\ProjetoRpg.Client.csproj`. Só após build sem erros abrir a cena `res://Scenes/ImportedLobby/TemplateCharacterSelectorStage.tscn` no Godot 4.7.2 Mono e enviar captura/erros do console. A integração ao lobby principal fica bloqueada até essa validação isolada.

> **Arquivo histórico preservado ao atualizar o checkpoint da raiz em 2026-10-29.** Estados de branch/workspace/runtime descritos acima são fotografias da etapa anterior e não substituem `CHECKPOINT.md` atual.
