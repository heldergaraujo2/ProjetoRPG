# Demo 3D local de personagem — estado e execução

## Recuperação dos artefatos — 2026-10-29

Os artefatos reais da demonstração foram recuperados do workspace e publicados no repositório: `CharacterDemo.tscn`, `CharacterActor.tscn`, scripts C# do ator/câmera, KayKit Barbarian/Mage em GLB, licença CC0 e `SOURCE.md` com origem, hashes e inventário dos clipes. Os dois GLBs contêm PNG embutido, rig de 41 joints e 76 animações cada; hashes e nomes completos são registrados no arquivo de origem/licença.

O teste Node dedicado passou 3/3 e as referências estáticas das cenas resolvem. Isso não valida o importador/renderizador Godot. `npm run test:target:client` permanece bloqueado (código 2) pela ausência do executável Godot 4 .NET neste ambiente; build C#, abertura com F6 e inspeção visual não foram executados. `Bootstrap.tscn` permanece como cena principal oficial.

## Escopo

Esta cena é uma demonstração isolada, fora da cena inicial `client/Scenes/Bootstrap.tscn`. O comportamento é intencionalmente local e descartável: o jogador da demo pode caminhar para o ponto indicado pelo clique esquerdo, e um NPC visualmente diferente permanece como ator de teste. A cena não envia movimento/ataques ao servidor, não altera estado persistente, não implementa navegação de jogo online, combate, IA ou progressão.

- **Ator controlável:** KayKit Barbarian, com idle/caminhada/corrida conforme a velocidade.
- **NPC:** KayKit Mage, parado em idle, visualmente distinto e sem interação persistente.
- **Clique esquerdo:** define destino no plano da arena e move o ator local.
- **Clique direito:** sem função nesta demo; ataque ainda não está integrado e não é simulado.
- **WASD/Espaço:** não são adicionados como controles de gameplay.
- **I/P:** não são inventário/painel nesta cena; tais interfaces não estão integradas ao servidor.
- **Cena principal oficial:** permanece `Bootstrap.tscn`; não foi substituída.
- **Modelos, licença, hashes e inventário dos clipes:** [`client/Assets/Characters/KayKitAdventurers/SOURCE.md`](../client/Assets/Characters/KayKitAdventurers/SOURCE.md).

## Abrir a cena

No Windows, informe a pasta **real** que contém os executáveis, descubra os binários instalados, confira a edição/versão pela própria saída de `--version` e valide .NET/Node. Este ambiente Linux não consegue acessar a instalação Windows; não presuma o caminho do exemplo nem instale nada automaticamente.

```powershell
$godotDir = Read-Host 'Pasta real que contém os executáveis Godot Mono 4.7.2'
if (-not (Test-Path -LiteralPath $godotDir -PathType Container)) { throw "Pasta não encontrada: $godotDir" }
$godotCandidates = Get-ChildItem -LiteralPath $godotDir -File -Filter '*mono*win64*.exe'
$godotCandidates | Select-Object FullName, Length, LastWriteTime
$godot = $godotCandidates | Where-Object { $_.Name -notmatch 'console' } | Select-Object -First 1
if (-not $godot) { $godot = $godotCandidates | Select-Object -First 1 }
if (-not $godot) { throw 'Nenhum executável mono win64 foi encontrado nesta pasta.' }
$godotExe = $godot.FullName
$version = & $godotExe --version
$version
if ($LASTEXITCODE -ne 0 -or $version -notmatch '^4\.7\.2.*mono') {
  throw "Esperava Godot 4.7.2 Mono; encontrado: $version"
}
node --version
npm --version
dotnet --list-sdks
$dotnetVersion = & dotnet --version
if ($LASTEXITCODE -ne 0 -or $dotnetVersion -notmatch '^10\.') {
  throw "Esperava .NET SDK 10; encontrado: $dotnetVersion"
}
$env:GODOT_BIN = $godotExe
npm run test:target:client
& $godotExe --path (Join-Path (Get-Location) 'client') --editor
```

No editor, aguarde o primeiro import dos GLBs; selecione `client/Scenes/CharacterDemo.tscn` e use **F6 / Run Current Scene**. Não use F5 para testar a demo: F5 continua iniciando o projeto pela cena Bootstrap original.

### Tentativa de validação deste ambiente — 2026-10-28

- O executor é Linux x86_64 (`uname -a`), não Windows; não existem mounts `/mnt/c` ou `/mnt/d`, e os caminhos Windows do projeto não são acessíveis. Não é possível verificar aqui a existência, o caminho ou a edição do executável instalado no Windows.
- `node --version` / `npm --version`: `v22.22.3` / `10.9.8`. `GODOT_BIN` está unset; `command -v godot`, `godot4`, `dotnet` e `opencode` não encontrou executáveis. `dotnet --version` terminou com código 127 (`dotnet: command not found`).
- `npm run test:target:client` foi executado e terminou com **código 2**: `TARGET_TEST_BLOCKED: Godot 4 .NET executable is unavailable. Install the official matching Mono build and rerun npm run test:target:client.` O script foi lido antes da execução; sem Godot ele sai antes de importar ou alterar o projeto. Não houve build/import/boot.
- O checkpoint anterior registra uma validação Windows do usuário com Godot 4.7.2 stable Mono, .NET SDK 10.0.403 e Node 22.23.4; isso é histórico reportado, não uma verificação repetida do caminho/binário nesta missão.
- **F6 e verificação visual não executados.** Modelos, texturas, câmera, movimento, animações, escala/orientação e erros de execução continuam sem validação visual. O caminho do executável e a saída atual das versões precisam ser confirmados no próprio Windows usando os comandos acima.

Portanto não declarar a demo aberta/importada/buildada com sucesso até realizar essa validação no Windows.

## Compatibilidade MCP Godot / OpenCode (pesquisa, sem instalação)

### Candidato principal para avaliação futura

Foi inspecionado o repositório [tugcantopaloglu/godot-mcp](https://github.com/tugcantopaloglu/godot-mcp), no commit `88f0fbc69a08225d4fdec32f921209e127f52395` (2026-10-09). O README do código-fonte declara MIT, Node.js >=18, Godot 4.4+ e teste com Godot 4.7; descreve recursos .NET/C# e transporte MCP local por stdio. Isso é compatibilidade **declarada pelo projeto**, não teste independente com Godot 4.7.2 Mono ou com este repositório. O MCP não é necessário para a demo e não foi instalado, executado ou habilitado.

**Atenção ao empacotamento:** `package.json`/README do código-fonte consultado identificam a versão 3.1.0 e SDK MCP `^1.32.1`, mas o registro npm consultado em 2026-10-28 lista `@tugcantopaloglu/godot-mcp` como versão mais recente **1.0.0**, publicada em 2026-03-07. O tarball npm foi baixado apenas para inspeção (não instalado/executado) e contém versão 1.0.0 com SDK MCP 0.6.0; não foi demonstrado que corresponda ao commit-fonte acima. Não recomendar `npx @tugcantopaloglu/godot-mcp` enquanto essa divergência não for resolvida. Se houver autorização futura para testar, preferir primeiro um checkout fixado no SHA acima, inspecionar dependências e alterações, instalar localmente em diretório isolado, executar testes próprios e validar com o executável Godot Mono real.

A leitura estática do `src/index.ts`, `src/security.ts`, `src/scripts/mcp_interaction_server.gd`, `scripts/build.js` e `tests/security.test.ts` no commit fixado confirmou detalhes importantes:

- `StdioServerTransport` é usado para o MCP local. `GODOT_MCP_ALLOWED_DIRS` é **opcional**: sem a variável, a lista vazia permite qualquer raiz de projeto. Quando configurada, a validação resolve caminhos/symlinks; a suite de teste-fonte contém casos para traversal/symlink e rejeição de projetos fora da raiz (não foi executada nesta auditoria).
- `run_project` valida a raiz, encerra o processo Godot iniciado anteriormente pelo próprio MCP, injeta automaticamente `mcp_interaction_server.gd` no projeto e edita `project.godot` para registrar um autoload antes de iniciar Godot em modo debug. Na saída normal, o código tenta remover a linha, o script e UID que ele próprio inseriu; interrupção abrupta pode deixar alterações residuais. Logo, mesmo um teste runtime altera temporariamente arquivos do projeto. `launch_editor` inicia Godot sem esse passo de injeção.
- O bridge GDScript abre TCP em `127.0.0.1:9090`; `game_eval` encaminha código GDScript arbitrário para o jogo. Ele executa com a permissão do processo Godot, sem sandbox de sistema operacional.
- O `package.json` não declara script `postinstall`; `prepare` compila TypeScript e executa um script local que copia três scripts GDScript e marca o entrypoint como executável. Isso descreve os scripts visíveis do pacote, mas não equivale a auditar todas as dependências transitivas. Não houve instalação, `npm audit`, teste MCP, nem execução de Godot.

O MCP anuncia amplo controle (157 ferramentas no README consultado), incluindo criação/edição de arquivos e cenas, execução/encerramento do Godot e interação runtime. Esses recursos agem com as permissões da conta local. Se houver autorização futura para avaliá-lo, restringir a raiz a `client`, revisar diff/backups de `project.godot` e arquivos antes/depois, preferir inicialmente somente operações de leitura/edição headless estritamente necessárias e não habilitar `game_eval`/runtime sem motivo. A variável de raiz reduz a área de impacto, mas não torna código não confiável seguro.

### Esboço de configuração OpenCode, não ativado

OpenCode não foi encontrado no `PATH` deste ambiente; versão/compatibilidade local não verificadas. Não foi criado nem editado arquivo de configuração. O exemplo abaixo é apenas um modelo para eventual avaliação manual: substituir os caminhos, usar um checkout de fonte revisado e **manter `enabled: false` até haver autorização e validação**. Os nomes/caminhos não são descobertos nem garantidos por esta missão.

```json
{
  "mcp": {
    "godot-local": {
      "type": "local",
      "command": [
        "node",
        "C:\\CAMINHO\\REVISADO\\godot-mcp\\build\\index.js"
      ],
      "environment": {
        "GODOT_PATH": "C:\\CAMINHO\\Godot_v4.7.2-stable_mono_win64.exe",
        "GODOT_MCP_ALLOWED_DIRS": "C:\\CAMINHO\\ProjetoRPG\\client"
      },
      "enabled": false
    }
  }
}
```

A configuração está deliberadamente fora do repositório e não concede permissões. Executável Godot, pacote npm, instalação local, runtime bridge e teste real continuam pendentes de disponibilidade/autorização.
