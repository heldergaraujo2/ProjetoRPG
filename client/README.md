# Cliente Godot C#

Cliente Godot 4.7.2 Mono, `Godot.NET.Sdk/4.7.2`, TFM `net8.0`, contratos de `shared/`. O servidor ASP.NET Core usa .NET 10.

## Caminho oficial

`Scenes/Bootstrap.tscn` continua a cena principal. Seu `Bootstrap.cs` inicializa `AccountGateway`. O fluxo é login → listagem real de personagens → seleção explícita → solicitação de entrada server-authoritative → cena `WorldGameplay.tscn` com `CoastWorld.tscn` → movimento WASD/setas sincronizado pelo servidor → encerramento e retorno ao salão.

O bearer permanece somente em memória; credenciais/tokens não vão para disco/log. `PROJETORPG_SERVER_URL` aceita HTTPS remoto e HTTP somente loopback de desenvolvimento. Conta e personagem usam APIs existentes; nenhuma classe/aparência é inventada quando `classId` falta.

`CharacterLobby` cria um `SubViewport` independente por personagem retornado, apresenta nome correto, animação Idle disponível, seleção por botão/cartão e destaque. “Entrar no mundo” fica habilitado apenas se o ID selecionado pertence à lista validada mais recente. `AccountGateway` solicita `POST /api/v1/world/sessions` antes de carregar cena e valida o snapshot retornado.

## Gameplay Coast do ProjetoRPG

`WorldGameplay.tscn` e `WorldGameplay.cs` são cena/controlador próprios do ProjetoRPG: carregam Coast importada e `CharacterActor.tscn`, orientam câmera e personagem, fazem Idle/Walking a partir do estado recebido e mostram estados/retry/retorno. WASD/setas transmitem somente `directionX`, `directionZ`, `inputSequence` em `/ws` com bearer e protocolo v2. O cliente nunca determina coordenadas finais nem speed; `CharacterActor` não movimenta localmente nesta cena.

A sessão é individual, com estado persistido pelo servidor e reconexão durante a janela de dez minutos. Isso não é multiplayer: não há broadcast nem colisão entre avatares. A validação server-side usa malha navegável/radius; física final deve ser conferida contra colliders no Godot/Windows.

A cena `CharacterDemo.tscn` continua uma demo local separada (clique esquerdo no plano da arena, sem servidor, navegação ou persistência); ela não é usada como gameplay online. Assets KayKit mantêm atribuições/licença em `Assets/Characters/KayKitAdventurers/`. Coast mantém MIT/avisos upstream em `Assets/Worlds/JDungeon3D-Coast/licenses/` e README.

## Build e execução

```powershell
npm run test:target:client
```

O smoke exige Godot 4.7.2 Mono e toolchain .NET/targeting packs. Abrir `client/project.godot` no editor também importa os GLBs e `CoastWorld.tscn`. Configure o servidor autenticado, aplique migrations e ajuste `PROJETORPG_SERVER_URL`; então teste login/lista/seleção/entrada/teclas/câmera/colisão/retorno. Nesta revisão, o Agent não tem Godot, Godot Mono nem .NET: os testes estáticos não substituem build/import/runtime.

Validação Windows final somente na cópia descartável `D:\ProjetoRPG-Teste-Lobby`. Não acessar nem modificar `D:\AProjeto RPG Fixo`.
