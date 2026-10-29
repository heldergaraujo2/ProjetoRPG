# Cliente Godot C#

O cliente continua apoiado em Godot 4.7.2-stable .NET com `Godot.NET.Sdk/4.7.2` e TFM `net8.0`, compartilhando contratos com `shared/net8.0`. O servidor usa .NET 10 separadamente.

## Cenas existentes

- `Scenes/Bootstrap.tscn` continua sendo a cena principal configurada no projeto (`run/main_scene`) e o diagnóstico oficial de inicialização. Não foi substituída.
- `Scenes/CharacterActor.tscn` é um ator C# reutilizável que carrega um modelo GLB, procura o `AnimationPlayer` importado e oferece movimento local apenas quando `PlayerControlled` está ativado.
- `Scenes/CharacterDemo.tscn` é uma demo 3D isolada, aberta com **F6 / Run Current Scene** no editor. Usa KayKit Barbarian para o jogador de teste e Mage como NPC local visualmente distinto. Clique esquerdo define destino no plano da arena; o botão direito não faz nada. Não existe navegação real, combate, IA ou integração de rede/persistência. Não adiciona WASD/Espaço como gameplay; I/P continuam fora desta demo.
- Assets e atribuições/licença são documentados em [`Assets/Characters/KayKitAdventurers/SOURCE.md`](Assets/Characters/KayKitAdventurers/SOURCE.md). O procedimento de execução e a pesquisa de compatibilidade MCP/OpenCode estão em [`../docs/DEMO_3D_PERSONAGEM.md`](../docs/DEMO_3D_PERSONAGEM.md).

A demo **não** conclui nem substitui a Fase 2 (login/cliente-servidor) ou a Fase 3 (mundo navegável, colisões/navegação e movimento sincronizado/autoritativo). O seu movimento não altera estado de servidor ou personagem persistido.

## Validação

A validação do cliente Godot deve ser feita num Windows com Godot 4.7.2 Mono e SDK .NET compatível. A partir da raiz do repo:

```powershell
npm run test:target:client
```

Esse smoke importa/compila e executa o projeto pela cena oficial Bootstrap. Para verificar também o protótipo visual, abra o projeto no editor e execute `Scenes/CharacterDemo.tscn` com F6, conforme o procedimento em `docs/DEMO_3D_PERSONAGEM.md`. O sandbox atual não dispõe de Godot Mono nem .NET SDK; importação, build C#, execução e inspeção visual da nova cena permanecem pendentes. Os testes Node de asset/cenas validam hashes, estrutura declarada dos GLBs e referências estáticas; não provam importação/renderização Godot.
