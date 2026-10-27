# 04 — DADOS EXTENSÍVEIS (como adicionar conteúdo sem mexer no código)

## Regra de ouro
Qualquer coisa que um designer queira adicionar ou balancear deve ser um arquivo em `content/`. O código só interpreta esses dados. Cada tipo tem JSON Schema em `content/schemas/`. A ferramenta `Tools/ContentValidator` valida: ids únicos, referências existentes, faixas numéricas, assets existentes. O servidor RECUSA subir com conteúdo inválido (em produção) e relata todos os erros de uma vez.

## Estrutura
```
content/
  schemas/        *.schema.json
  config/         formulas.json, refine.json, drop_global.json, pvp.json, xp_curve.json
  classes/        guerreiro.json ...
  skills/         <classe>/*.json
  items/          weapons/ armors/ consumables/ materials/ quest/ event/
  item_sets/      *.json
  monsters/       *.json
  bosses/         *.json
  npcs/           *.json
  dialogues/      *.json
  quests/         *.json
  drop_tables/    *.json
  recipes/        *.json
  maps/           <id>/map.json  (+ heightmap, colisão, spawns, portais)
  events/         *.json
  shops/          *.json
  i18n/           pt-BR.json, en.json
  assets_manifest/ catálogo de modelos, texturas, sons, ícones por id
```

## Convenções
- Id: `tipo.categoria.nome_snake` (ex.: `item.weapon.sword_iron_01`, `monster.forest.wolf_gray`).
- Texto exibido vai em `i18n` por chave (`name_key`, `desc_key`), nunca fixo no dado.
- Referência de arte por **asset id** (`visual.model: "asset.model.sword_iron_01"`), resolvido pelo manifest. Trocar o visual = mudar o manifest, não o item.
- `schema_version` em todo arquivo, com migração quando mudar.

## Exemplos
Veja a pasta `data_examples/` (item, monstro, boss, NPC, quest, mapa, drop table, skill, evento, receita). O agente deve criar os schemas a partir deles na Fase 1.

## Hot reload
Comando de GM `/reload content` revalida e aplica. Entidades existentes mantêm referência por id; se o id sumiu, o servidor registra aviso e usa um substituto seguro.

## Mapas extensíveis
- `map.json`: id, nome, tamanho, regras (pvp, nível recomendado, voo permitido), chunks, heightmap (arquivo), malha de caminhada/colisão (arquivo), spawns, portais (destino mapa/ponto), NPCs, pontos de respawn do jogador, clima, música.
- O cliente carrega chunks por proximidade. O servidor carrega colisão e navegação por região.
- Novo mapa = nova pasta + registro automático por varredura. Nenhum `switch` de ids no código.
