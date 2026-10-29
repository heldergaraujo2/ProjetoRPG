# Port visual do Character Selector — ProjetoRPG

Este diretório contém uma adaptação isolada do conceito de palco 3D do projeto
[godot4-3d-characters](https://github.com/MacdonaldRobinson/godot4-3d-characters).

## O que foi portado

- Palco 3D com base compacta, câmera frontal elevada e iluminação principal/preenchimento.
- Alternância entre os modelos Barbarian e Mage já versionados no ProjetoRPG.
- Navegação por teclado: setas esquerda/direita.
- Implementação em C# compatível com o cliente Godot 4.7.2 Mono, sem dependência dos autoloads,
  classes de rede ou multiplayer do projeto de demonstração original.

## Abrir a cena

No Godot, abra `client/project.godot` e execute:
`res://Scenes/ImportedLobby/TemplateCharacterSelectorStage.tscn`.

## Limites importantes

Esta é uma cena isolada de demonstração visual e não substitui a tela principal de lobby.
A tela principal continua sendo `res://Scenes/CharacterLobby.tscn`, ligada à API de conta
e personagens. A seleção visual desta cena não cria personagens, não altera a conta e não
inicia uma sessão de mundo. O botão de entrada no mundo permanece desabilitado até existir
um fluxo autoritativo no servidor.

O template original possui dependências próprias de GameState, NetworkState, MultiplayerScene,
ENet, cenas de mundo e addons. Essas dependências não foram copiadas porque não pertencem à
arquitetura atual do ProjetoRPG.

## Créditos e licença

O código original consultado está sob MIT, Copyright (c) 2022 emirthab. Esta adaptação é
independente e limitada ao palco/ideia de seleção visual. Consulte
`LICENSE-MIT-ORIGINAL.txt` para o aviso MIT do projeto de origem.

Os modelos Barbarian/Mage usados aqui são os assets KayKit Adventurers já presentes no
ProjetoRPG; suas licenças e procedência estão documentadas junto aos próprios assets.
