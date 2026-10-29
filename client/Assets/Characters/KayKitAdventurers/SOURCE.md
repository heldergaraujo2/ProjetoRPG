# KayKit — Character Pack: Adventurers 1.0

- **Criador/distribuidor:** Kay Lousberg (KayKit).
- **Página de origem do pacote:** https://kaylousberg.itch.io/kaykit-adventurers
- **Repositório de distribuição consultado:** https://github.com/KayKit-Game-Assets/KayKit-Character-Pack-Adventures-1.0
- **Commit fixado/baixado:** `672074b73ba276876a19e8816ecdc5241817ab47` (2023-09-16; branch `main` no momento da obtenção).
- **Data de obtenção:** 2026-10-28.
- **Licença original preservada ao lado dos modelos:** [`LICENSE.txt`](LICENSE.txt), CC0 1.0 Universal. O texto original autoriza uso pessoal, educacional e comercial; atribuição a Kay Lousberg é opcional, mas recomendada.
- **Arquivo de origem examinado:** `KayKit-Character-Pack-Adventures-1.0-672074b73ba2.zip` fora do repositório, SHA-256 `00777a7b9811a7c7f8dd3fda4e56604f6a93b33673d64557153f354d677b47af`. O ZIP passou `testzip()` sem corrupção e lista 80 entradas. Contém GLB e arquivos glTF; não contém FBX. Somente os dois GLBs usados nesta demo e a licença foram copiados para o projeto; não foram incluídos caches, FBX, ferramentas ou modelos não utilizados.

## Modelos incorporados

| Arquivo | Uso nesta demo | SHA-256 | Dados inspecionados no GLB |
|---|---|---|---|
| `Barbarian.glb` | Personagem local jogável | `cefc311a0e10c7858b6141f5ada7e33268727564fb8ac1347aab97d000669cc6` | glTF 2.0, esqueleto `Rig` com 41 joints, uma imagem PNG embutida, 76 animações nomeadas |
| `Mage.glb` | NPC local, aparência distinta | `cf898585da33fab50c724d31605fb931eb2912e6d2280092141e98ca81ad507d` | glTF 2.0, esqueleto `Rig` com 41 joints, uma imagem PNG embutida, 76 animações nomeadas |

Os dois arquivos são aproximadamente 3,5 MiB cada; as texturas estão incorporadas no GLB. A inspeção do contêiner e do JSON interno confirmou malhas/skinning, a imagem embutida e as animações abaixo. **Isso não substitui a importação pelo editor Godot**, bloqueada neste ambiente sem Godot 4.7.2 .NET.

## Nomes de animação declarados nos GLBs

Os dois GLBs contêm o mesmo conjunto de 76 nomes. A cena usa `Idle`, `Walking_A` e `Running_A`; o NPC toca `Idle`. O controlador verifica a presença dos clipes e não falha fatalmente se um importador futuro não os expuser.

```text
1H_Melee_Attack_Chop, 1H_Melee_Attack_Slice_Diagonal,
1H_Melee_Attack_Slice_Horizontal, 1H_Melee_Attack_Stab,
1H_Ranged_Aiming, 1H_Ranged_Reload, 1H_Ranged_Shoot, 1H_Ranged_Shooting,
2H_Melee_Attack_Chop, 2H_Melee_Attack_Slice, 2H_Melee_Attack_Spin,
2H_Melee_Attack_Spinning, 2H_Melee_Attack_Stab, 2H_Melee_Idle,
2H_Ranged_Aiming, 2H_Ranged_Reload, 2H_Ranged_Shoot, 2H_Ranged_Shooting,
Block, Block_Attack, Block_Hit, Blocking, Cheer, Death_A, Death_A_Pose,
Death_B, Death_B_Pose, Dodge_Backward, Dodge_Forward, Dodge_Left, Dodge_Right,
Dualwield_Melee_Attack_Chop, Dualwield_Melee_Attack_Slice,
Dualwield_Melee_Attack_Stab, Hit_A, Hit_B, Idle, Interact,
Jump_Full_Long, Jump_Full_Short, Jump_Idle, Jump_Land, Jump_Start,
Lie_Down, Lie_Idle, Lie_Pose, Lie_StandUp, PickUp, Running_A, Running_B,
Running_Strafe_Left, Running_Strafe_Right, Sit_Chair_Down, Sit_Chair_Idle,
Sit_Chair_Pose, Sit_Chair_StandUp, Sit_Floor_Down, Sit_Floor_Idle,
Sit_Floor_Pose, Sit_Floor_StandUp, Spellcast_Long, Spellcast_Raise,
Spellcast_Shoot, Spellcasting, T-Pose, Throw, Unarmed_Idle,
Unarmed_Melee_Attack_Kick, Unarmed_Melee_Attack_Punch_A,
Unarmed_Melee_Attack_Punch_B, Unarmed_Pose, Use_Item, Walking_A,
Walking_B, Walking_Backwards, Walking_C
```

O README do pacote informa 75 animações para o bundle; a contagem efetivamente feita no JSON dos GLBs selecionados é 76 por arquivo. Esta divergência está registrada em vez de assumir o número do anúncio.
