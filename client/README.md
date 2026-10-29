# Cliente Godot C#

O cliente continua apoiado em Godot 4.7.2-stable .NET com `Godot.NET.Sdk/4.7.2` e TFM `net8.0`, compartilhando contratos com `shared/net8.0`. O servidor usa .NET 10 separadamente.

## Cenas existentes

- `Scenes/Bootstrap.tscn` continua sendo a cena principal configurada no projeto (`run/main_scene`) e o diagnóstico oficial de inicialização. Não foi substituída.
- `Scenes/CharacterActor.tscn` é um ator C# reutilizável que carrega um modelo GLB, procura o `AnimationPlayer` importado e oferece movimento local apenas quando `PlayerControlled` está ativado.
- `Scenes/CharacterDemo.tscn` é uma demo 3D isolada, aberta com **F6 / Run Current Scene** no editor. Usa KayKit Barbarian para o jogador de teste e Mage como NPC local visualmente distinto. Clique esquerdo define destino no plano da arena; o botão direito não faz nada. Não existe navegação real, combate, IA ou integração de rede/persistência. Não adiciona WASD/Espaço como gameplay; I/P continuam fora desta demo.
- Assets e atribuições/licença são documentados em [`Assets/Characters/KayKitAdventurers/SOURCE.md`](Assets/Characters/KayKitAdventurers/SOURCE.md). O procedimento de execução e a pesquisa de compatibilidade MCP/OpenCode estão em [`../docs/DEMO_3D_PERSONAGEM.md`](../docs/DEMO_3D_PERSONAGEM.md).

A demo **não** conclui nem substitui a Fase 2 (login/cliente-servidor) ou a Fase 3 (mundo navegável, colisões/navegação e movimento sincronizado/autoritativo). O seu movimento não altera estado de servidor ou personagem persistido.

## Incremento de Fase 2 — conta e personagens (parcial)

`Bootstrap.tscn` continua sendo a cena principal. Seu `Bootstrap.cs` agora inicia a interface `AccountGateway`, que usa `HTTPRequest` contra as APIs reais do servidor: cadastro (`POST /api/v1/accounts/register`), login (`POST /api/v1/sessions`), lista/criação de personagens (`GET/POST /api/v1/characters`) e logout (`DELETE /api/v1/sessions/current`). O endereço é `PROJETORPG_SERVER_URL`; sem variável, usa `http://127.0.0.1:5050` para desenvolvimento local. HTTP fora de loopback é recusado; endpoints remotos exigem HTTPS.

O cadastro respeita a resposta genérica 202 (não revela existência da conta); o login exige bearer token bem-formado e não expirado. O token permanece apenas em memória, não é mostrado nem escrito em disco/log. Senhas são mascaradas e apagadas depois de cadastro/login. O servidor continua responsável por autenticação, ownership, unicidade e validação; a criação solicita personagem sem classe, já que o catálogo real segue vazio. A interface trata falhas HTTP, rate limit e sessão expirada, mantém caminhos de nova tentativa e permite atualizar a lista.

**Limite deliberado:** este incremento conecta identidade e personagens persistidos, mas ainda não entra em mundo, não instancia personagem 3D selecionado e não move/sincroniza entidades. Não é multiplayer nem gameplay online; a demo `CharacterDemo.tscn` segue isolada. A aceitação oficial da Fase 2 permanece aberta até haver teste Godot/servidor real e entrada em mundo com personagem placeholder. Fase 1 também continua em andamento conforme `CHECKPOINT.md`.

## Validação

A validação do cliente Godot deve ser feita num Windows com Godot 4.7.2 Mono e SDK .NET compatível. A partir da raiz do repo:

```powershell
npm run test:target:client
```

Esse smoke importa/compila e executa o projeto pela cena oficial Bootstrap. Para verificar também o protótipo visual, abra o projeto no editor e execute `Scenes/CharacterDemo.tscn` com F6, conforme o procedimento em `docs/DEMO_3D_PERSONAGEM.md`. `node --test test/client-account-flow.test.js test/godot-character-demo.test.js` cobre apenas contratos/referências estáticas e assets; não executa a interface, servidor HTTP, importação/renderização Godot ou build C#. O sandbox atual não dispõe de Godot Mono nem .NET SDK; importação, build C#, teste real do fluxo e inspeção visual continuam pendentes.
