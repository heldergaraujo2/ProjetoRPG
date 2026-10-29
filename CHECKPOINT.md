# ATUALIZAÇÃO OPERACIONAL MAIS RECENTE — 2026-10-29

Esta seção é a referência mais recente para os itens que atualiza abaixo; registros históricos posteriores no documento descrevem tentativas/estados anteriores e não devem sobrescrever estas evidências.

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
