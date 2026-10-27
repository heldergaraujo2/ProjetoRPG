# 05 — REDE E PROTOCOLO PRÓPRIO

O protocolo é INVENTADO por nós (não compatível com nenhum jogo existente). Definir em `Shared/Protocol` e documentar aqui a cada mudança.

## Camadas
- **Canal confiável** (TCP ou UDP confiável): login, personagem, inventário, chat, trade, quests, UI.
- **Canal rápido** (UDP não confiável com sequência): movimento, snapshots de entidades, eventos de combate efêmeros.

## Formato
- Cabeçalho: `[tamanho:u16][opcode:u16][seq:u32][payload]`. Serialização binária compacta (MessagePack, protobuf, ou struct manual). Decidir no ADR de rede.
- `PROTOCOL_VERSION` trocado no handshake. Versão incompatível = erro claro ao cliente.
- Limite de tamanho por mensagem e rate limit por opcode, por sessão.

## Princípio: cliente envia INTENÇÃO, servidor envia RESULTADO
Cliente → servidor: "quero mover para X", "usar skill S no alvo T", "usar item do slot N".
Servidor → cliente: posição real, dano aplicado, item criado, inventário atualizado.

## Catálogo inicial de mensagens (nomes de trabalho)
**Sessão:** Hello/Handshake, Login, LoginResult, Logout, Ping/Pong, Kick.
**Personagens:** CharList, CharCreate, CharDelete, CharSelect, EnterWorld.
**Mundo:** EntitySpawn, EntityDespawn, EntitySnapshot (posição/direção/estado), MoveIntent, MoveCorrection, ChangeMap, Teleport.
**Combate:** AttackIntent, SkillIntent, DamageEvent, StatusApplied/Removed, Death, Respawn, CastStart/Cancel.
**Stats:** StatsUpdate, LevelUp, SpendStatPoint, XpUpdate.
**Itens:** InventorySync, ItemAdd/Remove/Update, MoveItem, EquipItem, UnequipItem, UseItem, DropItem, PickupItem, GroundItemSpawn/Despawn, ShopOpen/Buy/Sell, Repair.
**Refino/Craft:** RefineRequest/Result, CraftRequest/Result.
**NPC/Quest:** NpcInteract, DialogueNode, DialogueChoice, QuestOffer/Accept/Progress/Complete.
**Social:** ChatSend/ChatReceive, PartyInvite/Accept/Leave/Update, GuildCreate/Invite/Update, TradeRequest/Offer/Confirm/Cancel, FriendList.
**PvP/PK:** PkStateUpdate, DuelRequest/Accept, ArenaQueue.
**Eventos:** EventAnnounce, EventEnter, EventState, EventResult.
**Admin:** GmCommand (só contas autorizadas).

## Sincronização de movimento
- Cliente prevê o próprio movimento; servidor valida (velocidade máxima, colisão, teleporte ilegal) e envia correção se divergir.
- Outras entidades: interpolação no cliente entre snapshots (buffer ~100 ms).
- Snapshots só de entidades na AOI, com compressão (delta, posições quantizadas).

## Segurança de rede
- Handshake com troca de chave e criptografia (TLS no canal confiável; no UDP, AEAD com chave de sessão).
- Token de sessão com expiração. Anti-replay por `seq`.
- Nunca confiar em tamanhos, índices ou ids vindos do cliente: validar tudo.

## Testes de protocolo
- Teste de ida e volta (serialize/deserialize) para toda mensagem.
- Fuzz básico: bytes aleatórios não derrubam o servidor.
- Teste de compatibilidade de versão.
