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
