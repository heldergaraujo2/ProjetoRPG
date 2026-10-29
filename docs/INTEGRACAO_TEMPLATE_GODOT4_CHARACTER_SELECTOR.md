# Integração do template Godot 4 — seletor 3D e lobby

Data: 2026-10-29  
Projeto: ProjetoRPG  
Branch de trabalho: `arena/b2f4e6da-projetorpg`

## Pedido e objetivo

O usuário autorizou integrar o projeto público [MacdonaldRobinson/godot4-3d-characters](https://github.com/MacdonaldRobinson/godot4-3d-characters) para aproveitar a apresentação 3D de seleção de personagens e o lobby, mantendo o backend e o cliente C# do ProjetoRPG.

## Auditoria da fonte

- Repositório público: `MacdonaldRobinson/godot4-3d-characters`.
- README declara seletor de personagens, multiplayer, seletor de mundo, câmera, controlador de jogador, controles móveis e lobby/chat/seleção de mapa.
- Estrutura observada: `character-selector/`, `lobby/`, `characters/`, `assets/`, `animations/`, `camera-controller/`, `auto-loads/`, `addons/`, `game/`, entre outras.
- Linguagem predominante: GDScript. Não é um plugin isolado nem uma cena única.
- `project.godot` da fonte declara configuração/features 4.3, main scene `res://game/Game.tscn`, autoloads `NetworkState` e `GameState`, plugin Terrain3D e input actions próprias.
- Licença do código: MIT (copyright 2022 emirthab). Ao copiar/adaptar código, manter o aviso de copyright e o texto da licença. Licenças de modelos, texturas, áudio e addons devem ser verificadas separadamente antes de distribuir esses arquivos.
- README apresenta capturas das seções Character Selector e Lobby, mas não documenta interfaces de integração com o backend do ProjetoRPG.

## Compatibilidade com o ProjetoRPG

O ProjetoRPG atual é um cliente Godot 4.7.2 Mono/C#, com `client/project.godot`, cena principal Bootstrap, autenticação e gerenciamento de personagens em C#, e preview 3D isolado em `CharacterLobby.cs`.

**Não é seguro substituir `client/project.godot`, a cena Bootstrap, autoloads, scripts de conta, API, DTOs ou o fluxo de personagens pelo projeto de origem.** O template tem outra arquitetura, outro ponto de entrada e sistemas de rede próprios. Misturar os dois projetos diretamente também pode importar addons e configurações desnecessárias.

## Plano de integração aprovado

1. Baixar o template original em uma pasta temporária fora de `client/` e inspecionar as cenas reais do seletor e do lobby, scripts anexados, recursos referenciados e dependências transitivas.
2. Identificar o menor subgrafo de cenas/recursos que produz o ambiente e a apresentação de personagem; não importar o multiplayer, chat, seleção de mapas, login, autoloads ou rede do template.
3. Preservar os avisos MIT e fazer inventário/licença individual de cada recurso artístico e addon efetivamente copiado.
4. Adaptar a cena visual selecionada para o projeto Godot 4.7.2 Mono, preferencialmente como cena independente sob `client/Scenes/ImportedLobby/`, sem substituir `CharacterLobby.tscn` nem `Bootstrap.tscn`.
5. Criar uma ponte C# mínima: a cena importada recebe o preview/modelo selecionado e não possui autoridade sobre conta/personagem. A seleção oficial continua vindo de `GET /api/v1/characters`; criação continua em `POST /api/v1/characters`; logout continua no gateway existente.
6. Manter “Entrar no mundo” desabilitado enquanto não existir endpoint/sessão/spawn autoritativos de gameplay.
7. Executar build C#, importação e boot headless com Godot 4.7.2 Mono; depois validar visualmente no Windows e testar login, listagem, seleção, criação, refresh e logout.
8. Só então promover o ambiente importado para a cena principal do lobby. Se a cena de origem for incompatível ou tiver dependências excessivas, portar apenas a composição visual, mantendo os modelos e scripts locais.

## Estado desta execução

- Auditoria documental da fonte e da licença MIT concluída.
- Compatibilidade arquitetural avaliada; substituição integral descartada.
- **A cena de origem ainda não foi copiada nem adaptada.** O conteúdo das cenas internas e seus recursos transitivos ainda precisa ser inspecionado a partir do clone/arquivo do template.
- **Não foi feito build nem teste visual desta integração.** Não declarar integração funcional concluída.
- A máquina Windows do usuário não é acessível diretamente pelo agente nesta sessão. O trabalho local de download/importação e validação exige a pasta de teste do usuário ou um ambiente de execução com Godot 4.7.2 Mono.

## Procedimento seguro no Windows

Executar em PowerShell, somente na pasta de teste. O clone temporário não deve ficar dentro de `client/`:

```powershell
Set-Location D:\ProjetoRPG-Teste-Lobby
git status --short
git clone --depth 1 https://github.com/MacdonaldRobinson/godot4-3d-characters.git ..\godot4-3d-characters-review
```

Não copiar diretórios em bloco. Após a inspeção dos arquivos de cena e dependências, selecionar somente os recursos necessários. Preservar `client/project.godot`, `.import`, `.uid`, Bootstrap, gateway e scripts de autenticação. Se houver alterações locais não commitadas, interromper antes de qualquer restauração/substituição.

## Fontes

- Template: https://github.com/MacdonaldRobinson/godot4-3d-characters
- README e capturas: https://github.com/MacdonaldRobinson/godot4-3d-characters/blob/main/README.md
- Licença: https://github.com/MacdonaldRobinson/godot4-3d-characters/blob/main/LICENSE


## Complemento da auditoria direta das cenas de origem — 2026-10-29

A inspeção foi aprofundada por leitura direta dos arquivos reais do repositório de origem, sem presumir que o clone local já exista.

### Arquivos examinados
- `character-selector/CharacterSelector.tscn` e `CharacterSelector.gd`.
- `lobby/Lobby.tscn` e `Lobby.gd`.
- `characters/character.tscn`, `character.gd`, `character_stats.gd` e `characters/kaya/kaya.tscn`.
- `ui/Theme.tres`, `project.godot`, `README.md` e `LICENSE`.

### Descobertas verificadas
- A cena `CharacterSelector.tscn` não é um salão medieval pronto: é uma composição compacta com um `CSGBox3D` como plataforma, câmera próxima, iluminação simples, dois personagens e formulário inferior de nome/seleção.
- O seletor usa `CharacterSelector.gd` (GDScript), a classe `Character`, `CharacterStats`, recursos Kaya/Clara, Theme e a integração global `GameState`. O botão Select renomeia o personagem de demonstração e chama `GameState.game.load_lobby()`; não conversa com a API de contas/personagens do ProjetoRPG.
- A cena `Lobby.tscn` também não é um cenário de fantasia detalhado: usa um `CSGBox3D`, câmera, iluminação e UI para host/join ENet, seleção de mapas e início de partida. `Lobby.gd` herda `MultiplayerScene` e depende de `NetworkState`, `GameState`, RPCs e cenas de mapa do projeto de origem.
- A cena base `characters/character.tscn` tem dependências transitivas extensas: personagem Clara, árvore/animações, quatro bibliotecas GLB de animação, controlador de câmera, barra de vida, áudio de morte, efeito de level-up, scripts de combate/interação e recursos adicionais. A variante Kaya também depende desses sistemas e de seu FBX.
- A seleção de origem contém estatísticas de demonstração embutidas (nível/vida/XP), que não podem ser apresentadas como dados reais do ProjetoRPG.
- O repositório de origem informa aproximadamente 826.527 KB (cerca de 807 MiB) e linguagem predominante GDScript. Um clone shallow ainda pode baixar uma quantidade grande de dados; não executar clone integral automaticamente.
- A licença MIT do código foi lida diretamente; preservação do aviso Copyright (c) 2022 emirthab e do texto MIT é obrigatória para trechos de código reutilizados. A licença do código não comprova licença dos modelos, animações, áudio, texturas ou addons. Esses recursos não foram aprovados para cópia/distribuição.

### Decisão técnica refinada
Não importar as cenas originais nem suas dependências transitivas como estão. Isso duplicaria estado de conta/personagem/rede, introduziria GDScript e sistemas de gameplay que o cliente não usa e arriscaria exibir estatísticas fictícias. A auditoria documental e estática já identificou que o template não atende ao requisito de “cenário pronto de seleção de personagens” no sentido visual esperado: as duas cenas são plataformas simples, não um salão medieval/MMORPG.

A implementação atual do ProjetoRPG em C# e os modelos KayKit Adventurers licenciados separadamente continuam sendo a base mais segura. A próxima etapa é uma decisão de integração de escopo mínimo: portar apenas ideias de interação/câmera que tragam benefício mensurável, ou selecionar outro cenário 3D com licença de assets comprovada. Não copiar assets deste template até a licença individual de cada pacote estar identificada.

### Estado e limites
- Leitura estática das cenas/scripts de origem: concluída.
- Licença MIT do código: confirmada.
- Clone local em `D:\godot4-3d-characters-review`: não confirmado; o agente não acessa o disco Windows.
- Nenhum arquivo do template foi copiado para `client/`.
- Nenhum build, importação Godot ou teste visual desta integração foi executado.
- O plano de clone integral foi substituído por inspeção seletiva; não executar clone de aproximadamente 807 MiB sem necessidade.

### Próxima ação no Windows
Não é necessário clonar o repositório inteiro para esta decisão. Primeiro verificar, sem modificar, se o diretório temporário existe:
```powershell
$review = 'D:\godot4-3d-characters-review'
if (Test-Path $review) {
  Get-ChildItem -Force $review | Select-Object -First 20 Name,Mode,Length
  if (Test-Path (Join-Path $review '.git')) {
    git -C $review status --short
    git -C $review log -1 --oneline
  }
} else {
  'CLONE_AINDA_NAO_EXISTE'
}
```
Não sobrescrever diretório existente. Como a inspeção GitHub já confirmou que a cena de origem é simples e acoplada, só fazer clone seletivo se uma revisão de recursos específicos for necessária.


## Atualização de implementação — 2026-10-29

Foi publicado um primeiro port do **palco visual** do Character Selector em `client/Scenes/ImportedLobby/TemplateCharacterSelectorStage.tscn`, controlado por `client/Scripts/TemplateCharacterSelectorStage.cs`. A cena instancia os modelos KayKit existentes (Barbarian/Mage), permite alternar com as setas e inclui palco, câmera e iluminação. README e aviso MIT também foram adicionados.

**Escopo:** porta a experiência visual básica de seleção. Não importa o sistema Multiplayer do template, que depende de GameState/NetworkState/ENet/MultiplayerScene e mundos próprios incompatíveis com a arquitetura atual. A tela principal `CharacterLobby.tscn` continua preservada e conectada à conta. A nova cena é isolada e ainda não foi validada por build/execução local.

Próximo passo: atualizar `D:\ProjetoRPG-Teste-Lobby`, compilar o cliente e abrir `res://Scenes/ImportedLobby/TemplateCharacterSelectorStage.tscn` no Godot 4.7.2 Mono. Não atualizar a pasta original.
