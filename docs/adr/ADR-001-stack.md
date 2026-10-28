# ADR-001 — Stack e fronteiras do ProjetoRPG

- **Estado:** Aceita; builds iniciais Windows da Fase 0 confirmados em 2026-10-28. A nova persistência/autenticação da Fase 1 ainda aguarda build/testes de integração.
- **Data:** 2026-10-27.
- **Contexto de origem:** `rpg-pack/02_ARQUITETURA.md` propunha Godot 4 C#, .NET 8, PostgreSQL, SQLite local e TCP + UDP confiável (com WebSocket como alternativa inicial); a Fase 0 do `rpg-pack/08_FASES.md` manda confirmar a proposta.
- **Relacionado:** [`../02_ARQUITETURA.md`](../02_ARQUITETURA.md), [`../../README.md`](../../README.md), [`../../CHECKPOINT.md`](../../CHECKPOINT.md).

## Contexto da divergência de repositório — auditoria histórica de 2026-10-27

A branch ativa `arena/b2f4e6da-projetorpg` estava então no commit inicial `40e8bad`; sua referência `origin/main` estava desatualizada em `3941e64`. Após `git fetch origin main` (sem checkout ou merge), foi encontrado `origin/main` commit `9ac6d53`, com README e pacote `rpg-pack` oficial, mas sem projetos/código Godot/.NET. Esses documentos foram importados para o working tree. Naquele ponto não havia logs/artifacts locais dos testes reportados no PC. Em 2026-10-28 o usuário confirmou os builds/smokes Windows da Fase 0; CHECKPOINT.md registra essa evidência reportada e separa-a dos novos testes de Fase 1 ainda não executados.

## Opções consideradas

1. Manter Node/Canvas como stack final. Rejeitada: não cumpre cliente 3D Godot/C# nem servidor .NET requerido.
2. Usar o stack proposto literalmente com .NET 8. Rejeitada para novo desenvolvimento de longo prazo: a auditoria ocorre em 2026-10-27 e o ciclo de suporte do .NET 8 está no fim; piná-lo agora cria migração imediata de runtime.
3. Godot 4 .NET com TFM `net8.0` (alinhado ao GodotSharp dessa release) + ASP.NET Core/.NET 10 LTS para o servidor; shared `net8.0`; PostgreSQL em produção e SQLite no desenvolvimento; iniciar realtime com WebSocket/TCP e avaliar UDP confiável somente após medir. **Escolhida.**

## Decisão

- Cliente em **Godot 4.7.2-stable .NET**, C# com TFM **`net8.0`**. A source tree oficial da release declara `GodotSharp` e `GodotTools` como `net8.0`; o projeto cliente deve acompanhar o runtime alvo do engine, e não forçar `net10.0` sem teste.
- Servidor dedicado **ASP.NET Core/.NET 10 SDK 10.0.402**, TFM `net10.0`, C#.
- Biblioteca `shared/` em `net8.0`, para envelope/versionamento e DTOs portáveis sem dependência do Godot; o servidor net10 consome a biblioteca compatível net8.
- **PostgreSQL** como banco de produção; **SQLite** para desenvolvimento e testes locais, com migrações versionadas.
- **WebSocket/TCP** como único transporte da fundação e primeiro fluxo realtime. Só decidir por UDP depois de benchmark/requisito, conforme alternativa inicial permitida no protocolo.
- Catálogos JSON versionados em `content/`; schemas/validação no startup do servidor.
- Preservar `server.js`, `public/` e `test/` como protótipo de referência durante a migração, sem tratá-los como implementação final.

A lista oficial do GitHub API reportou release Godot `4.7.2-stable` com build `mono_linux_x86_64` de 107,698,034 bytes e .NET SDK `10.0.402`. O código-fonte oficial da tag 4.7.2 declara `GodotSharp` e `GodotTools` com target `net8.0` e declara o pacote `Godot.NET.Sdk` como `4.7.2`; por isso o client/shared usam net8.0. O backend fica em net10.0 por ciclo de suporte. A documentação oficial da linha 4.7 diz que Godot inclui o runtime necessário para executar jogos compilados, mas não o compilador/MSBuild; exige Godot .NET e .NET SDK, e Godot 4.5+ aceita SDK .NET 8 ou posterior. Assim, **não é necessário instalar runtime .NET 8 separado para executar editor/jogo**. O SDK .NET 10.0.402 satisfaz a versão mínima documentada e inclui runtime 10 para o servidor; o build do client resolve o targeting/reference pack `net8.0` via restore (SDK 8 oficial somente se necessário). Os builds iniciais da stack cruzada foram confirmados pelo usuário no Windows em 2026-10-28; os novos arquivos de persistência/auth da Fase 1 ainda aguardam build e integração PostgreSQL. Fontes primárias: [pré-requisitos C# da documentação Godot 4.7](https://github.com/godotengine/godot-docs/blob/4.7/tutorials/scripting/c_sharp/c_sharp_basics.rst), [GodotSharp.csproj na tag 4.7.2-stable](https://github.com/godotengine/godot/blob/4.7.2-stable/modules/mono/glue/GodotSharp/GodotSharp/GodotSharp.csproj), [GodotTools.csproj](https://github.com/godotengine/godot/blob/4.7.2-stable/modules/mono/editor/GodotTools/GodotTools.csproj) e [Godot.NET.Sdk.csproj](https://github.com/godotengine/godot/blob/4.7.2-stable/modules/mono/editor/Godot.NET.Sdk/Godot.NET.Sdk.csproj).

## Consequências

### Positivas
- Stack alinhado ao requisito 3D e ao mesmo ecossistema C# nas duas camadas.
- .NET LTS mais adequado ao período de desenvolvimento do que a proposta .NET 8 quase no fim do suporte.
- WebSocket reduz a complexidade operacional/protocolar da primeira fase; uma futura mudança de transporte exige medição e ADR/versionamento.
- Banco relacional permite transações/constraints necessárias para economia e itens; SQLite acelera desenvolvimento.

### Custos e riscos
- Godot Mono e .NET SDK precisam ser instalados separadamente. O Godot contém seu runtime para executar jogos; o SDK .NET 10 também fornece runtime para o servidor. O client `net8.0` precisa do targeting pack 8 durante o build, que pode vir do restore ou do SDK 8 oficial se o restore não resolver. Esta sessão não tem Godot nem dotnet.
- Os downloads oficiais foram investigados pela API, mas HEAD/download binário no sandbox encerrou TLS; não foi possível instalar por causa do ambiente remoto/allowlist.
- PostgreSQL/SQLite e seus providers ainda precisam ser integrados e testados; nenhum acesso a banco está comprovado.
- A migração do protótipo não deve remover conteúdo/fluxos até haver substituição e testes.

## Como validar

1. Executar `dotnet --info`, `dotnet --list-runtimes` e `godot --version`; confirmar SDK 10.0.402, runtime 10, Godot Mono e restore do targeting pack net8.0 para o client.
2. Executar build limpo do projeto shared, servidor e projeto Godot.
3. Rodar smoke do servidor por HTTP/WebSocket e boot da cena cliente headless.
4. Adicionar testes com SQLite e PostgreSQL quando os providers/serviços forem configurados.
5. Se .NET 10 não for aceito pelo Godot .NET 4.7.2, não alterar em silêncio: avaliar a menor alternativa ainda suportada em novo ADR.

## Aditivo de implementação — Fase 1 (2026-10-28)

- O servidor começa com **PostgreSQL apenas**, por Npgsql `10.0.0`; SQLite permanece uma possibilidade arquitetural não implementada e não é alegado como provider compatível. Desenvolvimento/teste usa PostgreSQL local ou um container descartável.
- A release oficial Npgsql `v10.0.0` (`https://github.com/npgsql/npgsql/releases/tag/v10.0.0`) declara TFM `net8.0;net9.0;net10.0` em `src/Npgsql/Npgsql.csproj`; isso confirma compatibilidade de pacote com o TFM server `net10.0` sem alterar o stack Godot/client/shared. Package reference está fixada em `10.0.0`; NuGet lockfile ainda precisa ser gerado após restore real.
- Migrações explícitas e transacionais são aplicadas com `--db-migrate`, não automaticamente no boot. `--db-rollback` exige confirmação explícita pois a migration `down` inicial apaga dados. Produção separa runtime connection/read-only schema health da conexão de migração com DDL e exige TLS validado.
- Este aditivo descreve código criado, não resultado validado: a compilação/restore e integração PostgreSQL desta implementação não foram executadas no sandbox, sem SDK .NET/Docker.
