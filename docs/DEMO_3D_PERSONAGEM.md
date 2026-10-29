# Demo 3D local de personagem — estado e execução

## Artefatos e estado da validação — 2026-10-29

Os arquivos reais estão nos caminhos publicados abaixo; não foi necessário mover nem duplicar os GLBs. `SOURCE.md` e `LICENSE.txt` preservam origem e CC0. Cada GLB possui textura PNG embutida, rig de 41 joints e 76 clipes declarados; os hashes e inventário completo estão em [`SOURCE.md`](../client/Assets/Characters/KayKitAdventurers/SOURCE.md).

O teste Node dedicado passou **4/4**, incluindo hashes/conteúdo GLB, caminhos reais, referências externas das cenas, licença e cena principal. Isso é validação estática, não importação Godot. O script do ator agora envia `GD.PushError` legível se o modelo obrigatório estiver ausente ou não instanciar como `Node3D`. Neste executor não há Godot 4 Mono nem .NET SDK; `npm run test:target:client`, import/build C#, F6 e inspeção visual continuam pendentes. `Bootstrap.tscn` permanece como cena principal oficial.

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

## Instruções exatas para Windows — Godot 4.7.2 Mono

### Caminhos reais publicados

Não mova nem copie os modelos. A estrutura já está organizada assim, relativa à raiz do repositório:

- `client/Scenes/CharacterDemo.tscn` — cena completa a abrir e executar.
- `client/Scenes/CharacterActor.tscn` — ator reutilizável instanciado pela demo.
- `client/Assets/Characters/KayKitAdventurers/Barbarian.glb` — jogador local.
- `client/Assets/Characters/KayKitAdventurers/Mage.glb` — NPC local.
- `client/Assets/Characters/KayKitAdventurers/SOURCE.md` e `LICENSE.txt` — origem, hashes, inventário e CC0.

As texturas PNG estão embutidas nos GLBs; não há arquivos de textura externos a localizar. A cena contém chão/colisão, iluminação, câmera de acompanhamento, marcador de destino e instruções locais.

### Conferir o executável e testar pelo PowerShell

Abra PowerShell na raiz do clone ProjetoRPG (a pasta que contém `package.json`, `global.json` e `client`). O caminho informado para o executável é usado literalmente:

```powershell
$repo = (Resolve-Path .).Path
$godot = 'D:\Ferramentas\Godot\4.7.2\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64_console.exe'
$client = Join-Path $repo 'client'
$project = Join-Path $client 'project.godot'
$scene = Join-Path $client 'Scenes\CharacterDemo.tscn'

$requiredFiles = @(
  $godot,
  $project,
  $scene,
  (Join-Path $client 'Scenes\CharacterActor.tscn'),
  (Join-Path $client 'Assets\Characters\KayKitAdventurers\Barbarian.glb'),
  (Join-Path $client 'Assets\Characters\KayKitAdventurers\Mage.glb'),
  (Join-Path $client 'Assets\Characters\KayKitAdventurers\SOURCE.md')
)
foreach ($file in $requiredFiles) {
  if (-not (Test-Path -LiteralPath $file -PathType Leaf)) { throw "Arquivo obrigatório não encontrado: $file" }
}

$godotVersion = (& $godot --version 2>&1 | Out-String).Trim()
$godotVersion
if ($LASTEXITCODE -ne 0 -or $godotVersion -notmatch '^4\.7\.2.*mono') {
  throw "Esperava Godot 4.7.2 Mono; recebido: $godotVersion"
}
$dotnetVersion = (& dotnet --version 2>&1 | Out-String).Trim()
$dotnetVersion
if ($LASTEXITCODE -ne 0 -or $dotnetVersion -notmatch '^10\.0\.') {
  throw "Verifique o SDK .NET requerido por global.json (10.0.402, latestPatch); recebido: $dotnetVersion"
}
node --version
npm --version

# Teste estático de caminhos, hashes/estrutura GLB, cenas, materiais/texturas e licença.
node --test .\test\godot-character-demo.test.js
if ($LASTEXITCODE -ne 0) { throw 'O teste estático da demo falhou.' }

# Importa recursos no editor headless, compila C# e inicia o Bootstrap oficial.
# Exige Godot Mono e o SDK .NET resolvido por global.json.
$env:GODOT_BIN = $godot
npm run test:target:client
if ($LASTEXITCODE -ne 0) { throw 'O smoke Godot/.NET falhou; leia a saída acima.' }
```

O `global.json` na raiz fixa SDK .NET `10.0.402` com `rollForward: latestPatch`; execute os comandos a partir da raiz para que o SDK correto seja resolvido. O smoke `npm run test:target:client` deve terminar com `GODOT_CLIENT_SMOKE_OK`. Ele cobre importação headless/build C# e inicialização da cena Bootstrap — **não executa a demo nem aprova sua aparência**. Se quiser também a suíte geral do repositório, com as dependências Node já instaladas, rode `npm run check`; ela não substitui o smoke Godot nem o teste visual.

### Importação inicial e abertura do projeto

A primeira importação dos GLBs acontece quando o editor varre/importa os arquivos do projeto; mantenha os dois `.glb` nos caminhos acima. Você pode abrir o projeto por qualquer um destes meios:

1. No Godot Project Manager, escolha **Import** e selecione exatamente `$project` (isto é, `<raiz do clone>\client\project.godot`); confirme o diretório `client`.
2. Ou, na mesma janela PowerShell, execute:

   ```powershell
   & $godot --editor --path $client
   ```

Na primeira abertura, aguarde a varredura/importação terminar e a atividade de importação sumir. Se o editor solicitar build das assemblies C#, use **Build** e aguarde sucesso. Não feche o editor enquanto ainda processa os GLBs. Depois, no painel **FileSystem**, abra `Scenes/CharacterDemo.tscn` (caminho absoluto calculado em `$scene`). Para executar a cena atualmente aberta, use **F6 / Run Current Scene**. **Não use F5** para testar a demo: F5 usa `run/main_scene` e deve continuar abrindo `Scenes/Bootstrap.tscn`.

### Output, debugger e erros de recurso

Com a cena aberta e durante a importação/execução, confira o painel inferior **Output** e as abas de **Debugger** (especialmente **Errors** e **Warnings**). Resolva primeiro erros de parse/importação, referências `res://`, build C# e recursos ausentes; a mensagem do Godot no Output deve indicar o caminho que falhou. O `CharacterActor` também emite `GD.PushError` explicando quando `ModelScene` está ausente ou não instancia como `Node3D`. Não apague `.godot`, caches ou arquivos de origem para contornar erros; registre a mensagem, versão do Godot e SDK e preserve os arquivos.

### Critérios: importação concluída não é validação visual

**Importação/build concluídos** significa que a varredura terminou sem erros de import, o teste `npm run test:target:client` foi aprovado (`GODOT_CLIENT_SMOKE_OK`) e o Bootstrap iniciou com `CLIENT_BOOT_OK`. Isso prova a etapa headless indicada, mas não prova que a cena CharacterDemo abriu ou que modelos/texturas estão visualmente corretos.

**Validação visual da demo** só deve ser marcada depois de abrir `client/Scenes/CharacterDemo.tscn` no editor, executar F6 e confirmar na janela do jogo que Barbarian e Mage estão visíveis com materiais/texturas, câmera e escala adequadas; clicar no chão move o Barbarian; repouso/caminhada/corrida aparecem quando aplicáveis; o Mage permanece NPC local; e Output/Debugger não mostram erros pertinentes. Registre o resultado real (e, se possível, captura de tela). Sem essa execução manual, mantenha a validação visual como pendente. A demo não implementa combate, multiplayer, navegação de jogo ou persistência.

### Tentativa de validação deste ambiente — 2026-10-28

- O executor é Linux x86_64 (`uname -a`), não Windows; não existem mounts `/mnt/c` ou `/mnt/d`, e os caminhos Windows do projeto não são acessíveis. Não é possível verificar aqui a existência, o caminho ou a edição do executável instalado no Windows.
- `node --version` / `npm --version`: `v22.22.3` / `10.9.8`. `GODOT_BIN` está unset; `command -v godot`, `godot4`, `dotnet` e `opencode` não encontrou executáveis. `dotnet --version` terminou com código 127 (`dotnet: command not found`).
- `npm run test:target:client` foi executado e terminou com **código 2**: `TARGET_TEST_BLOCKED: Godot 4 .NET executable is unavailable. Install the official matching Mono build and rerun npm run test:target:client.` O script foi lido antes da execução; sem Godot ele sai antes de importar ou alterar o projeto. Não houve build/import/boot.
- O checkpoint anterior registra uma validação Windows do usuário com Godot 4.7.2 stable Mono, .NET SDK 10.0.403 e Node 22.23.4; isso é histórico reportado, não uma verificação repetida do caminho/binário nesta missão.
- **F6 e verificação visual não executados.** Modelos, texturas, câmera, movimento, animações, escala/orientação e erros de execução continuam sem validação visual. O caminho do executável informado pelo usuário está registrado acima; a existência do binário, sua versão atual, o SDK resolvido e a execução visual ainda precisam ser confirmados no próprio Windows usando os comandos acima.

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
