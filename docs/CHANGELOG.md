# Changelog

## 2026-10-28 — Validação Windows da fundação PostgreSQL (Fase 1 em andamento)

- Confirmados no Windows `npm run check` (4/4 testes Node), `npm run test:target`, build do servidor .NET e `npm run test:phase1` com PostgreSQL descartável.
- Corrigido o runner de integração: `psql -q` mantém a saída do valor retornado limpa; o teste de expiração ajusta `created_at` e `expires_at` respeitando a constraint da tabela.
- Adicionado `server/packages.lock.json`, com Npgsql `10.0.0` e referência ao projeto compartilhado. `dotnet restore server\\ProjetoRpg.Server.csproj --locked-mode` passou.
- Os avisos Node `MODULE_TYPELESS_PACKAGE_JSON` e conversão LF/CRLF pelo Git foram não bloqueantes.
- Fase 1 permanece EM ANDAMENTO: falta executar o teste separado de 100 WebSockets ociosos e revisar integralmente os critérios de aceitação. Esse teste não equivale a gameplay ou carga real de jogadores.
- Commits publicados na branch `arena/b2f4e6da-projetorpg`: `e731795` (correção do runner) e `2f39387` (lockfile).

> Nota histórica: a seção seguinte registra o estado inicial no sandbox Linux, antes da validação posterior no Windows.

## Histórico inicial de implementação — bloqueios no sandbox Linux (2026-10-28)

- Sincronizado o workspace com o HEAD remoto inicial `99ab3d742427c04043109304ae0b87622b9f3769`; o stash/backup pré-sync preserva a cópia antiga. Não houve force-push.
- Commits de implementação: `74784ee75fa8a5f8ba56a7f8d930d9db2d7e907e` (`feat(server): add PostgreSQL account and character foundation`) e `04efa0e18d744ccfc0567e606470338906613297` (runner Docker ignora connection string de migration herdada, mantendo o teste restrito ao banco efêmero). Adicionado Npgsql `10.0.0` (fonte oficial confirma TFMs net8/net9/net10), config obrigatória por environment, JSON logging, respostas seguras, rate limiting e health live/database/ready.
- Criadas migrações SQL com constraints/FKs/índices/checksums, runner transacional com advisory lock, status e rollback explicitamente destrutivo; nenhuma migration automática no boot.
- Adicionados registro/login usando ASP.NET Core Identity PasswordHasher, token aleatório persistindo somente SHA-256, expiração/revogação, respostas de login genéricas, personagens por owner sem classe fictícia e audit events sem credenciais.
- `npm run test:phase1` cria PostgreSQL descartável para teste integrado de migração/config/banco/auth/sessão/ownership/restart/audit/catalog/WebSocket. `npm run load:parked` prepara 100 WebSockets ociosos, sem reivindicar gameplay ou capacidade real.
- `npm ci`: passou. `npm run check`: passou, 4/4 testes Node. Parse estático JSON/XML e `git diff --check`: passaram.
- `npm run doctor`: status 1; .NET/Godot/PostgreSQL/Docker ausentes no sandbox. `npm run test:phase1`: bloqueado antes da execução pelo SDK .NET ausente. Nenhum build C#, restore Npgsql, teste PostgreSQL ou load test foi executado; `server/packages.lock.json` ainda precisa ser produzido/confirmado após restore no Windows.
- Fase 1 segue EM ANDAMENTO; os smoke tests Fase 0 de Windows permanecem confirmados pelo usuário e não foram repetidos.

## 2026-10-28 — Builds limpos e testes de destino aprovados (Fase 0 concluída)

- Toolchain Windows validada: Node.js 22.23.4, .NET SDK 10.0.403 e Godot 4.7.2 stable Mono.
- Limpeza, restore e builds de Shared (`net8.0`), Server (`net10.0`) e Client (`net8.0`) concluídos com sucesso.
- `npm run check`: sintaxe aprovada e 4/4 testes Node aprovados.
- `npm run test:target`: Shared, integração HTTP/WebSocket do servidor e import/build/boot headless do cliente Godot passaram.
- Avisos `MODULE_TYPELESS_PACKAGE_JSON` foram não bloqueantes. A medição de clock não equivale a simulação do mundo; o boot da cena não equivale a gameplay.
- Fase 0 concluída conforme os critérios de preparação e testes iniciais do README. Fase 1 é o próximo objetivo e continua incompleta: banco, migrações, autenticação e persistência ainda precisam ser implementados.

## 2026-10-27 — Download Windows e validação de publicação

- Confirmado workspace limpo em HEAD `4ba8a0a`, branch publicada e PR #1 apontando para o mesmo commit antes desta atualização.
- Versionado `docs/WINDOWS_SETUP.md` com clone Git da branch, comandos WinGet para Node/.NET/Godot, restore/build/testes, inventário de dependências e limitações; README e checkpoint apontam para o guia.
- Revalidada a ausência de submódulos/Git LFS e de dependências Arena; assets de referência e lockfile estão versionados.
- Builds do cliente/servidor permanecem não executados por falta de toolchain no sandbox.

## 2026-10-27 — Compatibilidade e bloqueio de toolchain (Fase 0 pendente)

- Inspecionados os pins e TFM reais: client `net8.0`/Godot.NET.Sdk 4.7.2, shared `net8.0`, server e smoke `net10.0`, SDK 10.0.402 em `global.json`.
- Verificados os fontes oficiais Godot 4.7: Godot inclui runtime de C# para executar jogos; SDK separado é necessário para compilar; SDK 8 ou posterior é compatível com Godot 4.5+. Corrigida a instrução anterior que exigia runtime 8 externo; targeting pack net8 só deve ser instalado se restore não o resolver.
- Buscados executáveis e pastas de instalação, sem Godot/dotnet; APT sem índices. Carga oficial via GitHub API confirmou redirect de asset para host fora do allowlist. Hosts Microsoft oficiais testados com TLS normal retornam `curl` 35 (`SSL_ERROR_SYSCALL`). Não contornado TLS.
- Consultados manifestos WinGet e registrados comandos oficiais/versionados no checkpoint para Windows. Nenhuma toolchain foi instalada nesta VM.
- `npm run check` passou com 4/4 testes Node; doctor e três comandos target bloqueados como detalhado em `CHECKPOINT.md`. Integração SQLite/PostgreSQL não existe no scaffold, então não foi alegada nem testada.
- Commit `def86f0` publicado na branch ativa; PR #1 aberto para `main`. Fase 0 continua pendente de builds/execuções reais.

## 2026-10-27 — Reconciliação e scaffold de destino (Fase 0 incompleta)

- Comparada a branch Arena `40e8bad` com `origin/main` `9ac6d53`; `main` continha documentação oficial, mas também não continha projetos Godot/.NET. Sem checkout, merge ou mudança de branch.
- Importada a especificação oficial em `rpg-pack/`, atualizados README/arquitetura e ADR: Godot 4.7.2 .NET/client `net8.0`, servidor .NET 10.0.402 (`net10.0`). Compatibilidade integrada permanece pendente de build real.
- Recuperadas versões anteriores de README, arquitetura, ADR e readmes dos diretórios a partir de blobs Git inacessíveis; preservadas sob `docs/archive/`, com ressalva de que não se pode provar que cada snapshot era a última versão não commitada.
- Criado scaffold `shared/`, ASP.NET Core com loop-clock 20 Hz, health/content endpoints e WebSocket handshake/ping, cliente Godot com cena de boot diagnóstico, manifest/schema e smoke scripts. Esses componentes são fundação parcial, não gameplay nem Fase 0 concluída.
- `npm run check` executado neste sandbox: 4/4 testes Node do protótipo passaram; sintaxe dos novos scripts passou.
- `npm run doctor` status 1 por ausência de Godot/.NET. Smoke shared retornou 127 (`dotnet: not found`); smokes server/client retornaram status 2 e `TARGET_TEST_BLOCKED`. Nenhum build C#, teste de integração do novo servidor, boot Godot, banco ou carga foi executado.
- Revisada a documentação oficial Godot 4.7: Godot empacota o runtime para executar jogos, mas exige .NET SDK para compilação; SDK 8 ou posterior é aceito. Corrigida a suposição anterior de que seria necessário instalar runtime .NET 8 separado. O client continua em `net8.0`; verificar targeting pack no restore.
- Diagnóstico da rede: asset GitHub Godot redireciona para `objects.githubusercontent.com`/`release-assets.githubusercontent.com` fora do allowlist; hosts Microsoft testados retornam TLS `curl 35`. Registrados comandos WinGet verificados por manifest para instalação Windows oficial; sem bypass de TLS nem instalação neste sandbox.
- O smoke do server foi ampliado para medir taxa média de ticks por 1 s (tolerância 15–25 Hz), sem confundir clock com simulação; não foi executado por falta do SDK.
- Fase 0 permanece incompleta. Próximo passo: provisionar toolchain oficial compatível e executar `npm run test:target`; corrigir erros reais; então completar persistência/autenticação de Fase 1 e só avançar mediante critérios.

## 2026-10-27 — Auditoria e preparação (Fase 0 em andamento)

- Auditada a única base `40e8bad`; confirmada ausência de roadmap/checkpoint/arquitetura anteriores e ferramentas Godot/.NET/DB no PATH.
- Registrada em ADR-001 a stack de destino solicitada, preservando o protótipo web durante a migração futura.
- README refeito como fonte do roadmap Fases 0–14; arquitetura, checkpoint e changelog adicionados.
- Adicionado smoke test que sobe o protótipo em porta livre, valida HTTP/cliente estático, conecta dois clientes, confere presença no snapshot comum e chat compartilhado.
- Adicionado `npm run check` (syntax checks + testes) e documentados comandos executáveis do protótipo.
- Guardadas as duas imagens enviadas como referências visuais; checkerboard está rasterizado (RGB sem alpha).
- Smoke test cobre dois clientes, presença e chat; o join ignora destino `mapId` fornecido pelo cliente e fixa spawn inicial no servidor.
- Adicionado `npm run doctor`, diagnóstico reproduzível de ferramentas alvo; saída desta sessão confirma Godot/.NET ausentes (falha esperada, não build).
- **Não implementado nesta etapa:** autenticação, persistência, cliente 3D, servidor .NET, inventário, atributos distribuíveis ou controles mouse; fases de gameplay permanecem pendentes.
