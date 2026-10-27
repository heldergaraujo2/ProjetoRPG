# Servidor ASP.NET Core/.NET

Scaffold inicial de servidor dedicado C# (`net10.0`) com health endpoints, loop de clock 20 Hz (não é simulação de mundo), catálogo JSON carregado no startup e endpoint WebSocket versionado de handshake/ping. Build/test:

```bash
dotnet --info
dotnet run --project server/ProjetoRpg.Server.csproj
npm run test:target:server
```

O teste de integração compila a aplicação, testa health/tick/catalog, handshake e ping/pong, rejeição de versão e recusa de catálogo inválido. Ainda não existem contas/login, banco, migrações, autenticação, jogadores ou regras persistidas; `health/tick` reporta `persistenceConfigured=false`. Não é servidor MMORPG pronto para produção. Toolchain oficial não estava disponível nesta sessão, portanto não há build/teste C# comprovado.
