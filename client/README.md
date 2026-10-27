# Cliente Godot C#

Scaffold executável pretendido em Godot 4.7.2-stable .NET com o SDK `Godot.NET.Sdk/4.7.2` e TFM `net8.0` (alvo do GodotSharp dessa release); compila contra `shared/net8.0`. O servidor usa .NET 10 separado. A cena Bootstrap é diagnóstico de inicialização, não gameplay, protótipo de MMORPG nem critério de Fase 0 concluído.

Validação a partir da raiz do repositório, quando as ferramentas estiverem instaladas:

```bash
dotnet --version
godot --version
npm run test:target:client
```

O teste compila/importa o projeto no editor headless e exige o marcador `CLIENT_BOOT_OK` ao executar a cena. A saída do teste ainda não está disponível nesta sessão porque `godot` e `dotnet` não estão instalados. O cliente não implementa input, movimento, pathfinding, rede, login ou gameplay.
