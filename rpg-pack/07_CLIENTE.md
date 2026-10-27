# 07 — CLIENTE (engine proposta: Godot 4)

## Responsabilidades
Renderizar, tocar áudio, capturar entrada, mostrar UI, prever movimento local, interpolar entidades remotas. NENHUMA regra de jogo decisiva.

## Módulos
- `Net`: conexão, handshake, fila de mensagens, reconexão.
- `World`: carregamento de chunks por proximidade, terreno, objetos estáticos, água, névoa, iluminação.
- `Entities`: jogadores, monstros, NPCs, itens no chão; ciclo de vida guiado por Spawn/Despawn do servidor.
- `Animation`: máquina de estados de animação por tipo de entidade; animações por id (idle, andar, atacar, skill, morrer).
- `Equipment`: composição visual do personagem (partes/slots) a partir de asset ids. Trocar equipamento = trocar malha/material do slot.
- `UI`: framework de janelas independentes da lógica.
- `Audio`, `Input`, `Config`, `Assets` (carregador por asset id com cache e validação).

## UI (100% própria)
Janelas: HUD (vida/mana/AG/XP, barra de skills, alvo, minimapa), inventário, equipamento, atributos, skills, loja, armazém, party, guilda, chat, quests, mapa-múndi, configurações, seleção de personagem, login, eventos, ranking.
Requisitos: temas, escala por resolução (UI em unidades relativas), atalhos configuráveis, arrastar e soltar, tooltips ricos (comparação de itens), acessibilidade (tamanho de fonte, daltonismo).
Arquitetura: componentes reutilizáveis + "view model" ligado a eventos do servidor (UI não chama lógica, só reage a estado).

## Mundo aberto no cliente
- Chunks de ~64–128 m carregados em anel ao redor do jogador, carga assíncrona, descarga por distância.
- LOD de modelos e terreno, culling por frustum, instancing de vegetação/objetos repetidos.
- Orçamento de memória e de draw calls medido por cena de teste.

## Assets
- Pipeline: importar → validar → registrar no manifest. Formato recomendado: glTF/GLB para modelos, PNG/KTX2 para texturas, OGG para áudio.
- Fase inicial: usar placeholders (cubos/cápsulas coloridos) para testar sistemas ANTES de ter arte final.
- Licenças: só assets originais ou com licença livre registrada em `ASSETS_LICENCAS.md`.

## Teste de cliente
- Cena de teste "sandbox" sem servidor para UI e renderização.
- Modo headless para testes automatizados de lógica de cliente (net, parsing).
