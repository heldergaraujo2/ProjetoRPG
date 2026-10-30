# JDungeon3D Coast — integração seletiva no ProjetoRPG

## Origem, versão e atribuição

- Projeto de origem: <https://github.com/jonathaneeckhout/jdungeon3d>
- Commit pinado e auditado: `73bea81341846341f302dfa184e33d31733c3fa8`
- Licença declarada pelo upstream: MIT. Arquivos de licença e avisos de conteúdo selecionado foram preservados em `licenses/`; mantenha-os em qualquer distribuição.
- Avisos específicos incluem Coast, pinheiro e pacotes Celtic Barn, Blacksmith, House e Tavern. A cena de colisões referencia tavern, pier, barn, 58 pinheiros, casas, blacksmith e o plano.
- `content/worlds/coast-navigation.json` extrai somente triângulos/vertices da NavigationMesh desse commit para validação de movimento no servidor. Não copia código ou gameplay upstream.

## Conteúdo selecionado

- `CoastModel.glb`: cenário Coast combinado.
- `CoastModel.tscn`: wrappers locais de colisão estática; 68 `StaticBody3D` e 68 `CollisionShape3D`.
- `CoastWorld.tscn`: cena local que conserva modelo com colisões, `NavigationRegion3D`/NavigationMesh, luz, ambiente, água e marcador `SpawnReference`.
- `Water.gdshader`: shader de água referenciado pela cena.
- Spawn de cena e servidor: `(0.148617, 2.25, 51.4204)`, IDs `world.coast` / `coast.initial`. O catálogo server-side confirma o centro e oito amostras de raio do agente sobre a malha.

O ProjetoRPG instancia `CoastWorld.tscn` dentro de sua própria cena `WorldGameplay.tscn`, usa `CharacterActor.tscn`, câmera local e snapshots de estado do servidor. `Bootstrap.tscn` permanece cena de entrada.

## Conteúdo não trazido

Não foram importados Player/scripts/componentes, `ComponentList`, respawn, inimigos Mushroomdog, itens Kelp, inventário, combate, câmera, movimento ou multiplayer do JDungeon3D. Coast é cenário, não pacote de gameplay upstream. Cliente/server ProjetoRPG possuem seus próprios scripts de movimento e contrato de sessão.

## Estado de validação

O servidor consulta a malha triangular importada e impede avanço além da área caminhável com raio mínimo 0.32; Godot instancia o ator com cápsula e aplica apenas o snapshot server-authoritative. A prova real de alinhamento entre navmesh, chão, spawn, câmera e colisores requer abrir/importar a cena no Godot 4.7.2 Mono e testar em Windows. No Agent nesta etapa não há Godot, Godot Mono nem .NET SDK: JSON/geometria e testes Node são estáticos, não prova de gameplay executável.

Abra `client/project.godot` no Godot 4.7.2 Mono em `D:\ProjetoRPG-Teste-Lobby` (cópia descartável). Não execute teste nem altere `D:\AProjeto RPG Fixo`.
