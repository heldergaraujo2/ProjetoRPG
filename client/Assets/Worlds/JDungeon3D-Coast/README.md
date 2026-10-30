# JDungeon3D Coast — integração seletiva

## Proveniência e licença

- Repositório de origem: <https://github.com/jonathaneeckhout/jdungeon3d>
- Commit fixado/auditado: `73bea81341846341f302dfa184e33d31733c3fa8`
- O projeto declara licença MIT. Os textos originais foram preservados em `licenses/` e devem acompanhar qualquer redistribuição deste conteúdo.
- Avisos específicos preservados: Coast, pinheiro e pacotes Celtic Barn, Blacksmith, House e Tavern. A cena de colisões do modelo referencia tavern, pier, barn, 58 pinheiros, casas, blacksmith e o plano; os avisos correspondentes foram copiados sem alteração.

## Recursos incluídos

- `CoastModel.glb`: cenário combinado de Coast.
- `CoastModel.tscn`: wrappers de colisão estática retirados da cena de origem; contém 68 `StaticBody3D` e 68 `CollisionShape3D`.
- `CoastWorld.tscn`: adaptação local que conserva luz, ambiente, shader/plano de água, `NavigationRegion3D`/NavigationMesh e modelo com colisões. O ponto `SpawnReference` está em `(0.148617, 2.25, 51.4204)`. A auditoria estática confirma que x/z cai em um triângulo da NavigationMesh em y=2.25; a posição ainda precisa de inspeção visual/física no Godot 4.7.2.
- `Water.gdshader`: shader de água referenciado pela cena Coast.

Os arquivos de cena foram adaptados para os paths `res://Assets/Worlds/JDungeon3D-Coast/` e a cena Coast foi filtrada/recomposta; não é uma cópia executável do projeto de origem.

## Exclusões intencionais

Não foram importados o Player nem seus scripts/componentes, ComponentList, respawn, inimigos Mushroomdog, itens Kelp, inventário, combate, câmera, movimentação, multiplayer ou outros sistemas do JDungeon3D. Não copiar `Coast.tscn` integralmente: ela contém referências diretas a esses sistemas. `Bootstrap.tscn` continua sendo a cena principal do ProjetoRPG.

O cenário e a navegação são conteúdo importado, não evidência de runtime. Esta pasta ainda não implementa a entrada autenticada de mundo, simulação autoritativa, ator do ProjetoRPG na Coast, câmera de jogo, movimento com colisão/pathfinding, streaming ou reconexão. O botão de entrada permanece bloqueado até esses contratos serem implementados e testados.

## Importação no Godot

Abra `client/project.godot` no Godot 4.7.2 Mono. O editor deve importar o GLB e validar `CoastWorld.tscn` sem recurso externo ausente. Confirme visualmente escala, texturas, céu/água, colisões, NavigationMesh e o marcador de spawn; depois teste caminho e colisão usando uma cena de teste do ProjetoRPG. Esta validação não foi possível no Agent porque Godot/Godot Mono não estão instalados no sandbox.
